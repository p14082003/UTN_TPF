using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Negocio
{
    public static class InstiRedApi
    {
        private static readonly HttpClient cliente = new HttpClient(new HttpClientHandler
        {
            AllowAutoRedirect = false
        })
        {
            Timeout = TimeSpan.FromSeconds(15)
        };

        public static async Task<Respuesta> PostAsync(string ruta, object datos, bool leerCuerpo = false)
        {
            var json = new JavaScriptSerializer();

            using (var request = CrearRequest(HttpMethod.Post, ruta))
            {
                request.Content = new StringContent(json.Serialize(datos), Encoding.UTF8, "application/json");

                using (HttpResponseMessage respuesta = await cliente.SendAsync(request))
                {
                    string cuerpo = null;
                    if (leerCuerpo || respuesta.StatusCode == HttpStatusCode.BadRequest)
                    {
                        cuerpo = await respuesta.Content.ReadAsStringAsync();
                    }

                    string mensaje = null;
                    if (respuesta.StatusCode == HttpStatusCode.BadRequest)
                    {
                        mensaje = LeerMensajeApi(json, cuerpo);
                    }

                    return new Respuesta { Estado = respuesta.StatusCode, Mensaje = mensaje, Cuerpo = cuerpo };
                }
            }
        }

        public static async Task<T> GetAsync<T>(string ruta, bool permitirNoEncontrado = false) where T : class
        {
            using (var request = CrearRequest(HttpMethod.Get, ruta))
            using (HttpResponseMessage respuesta = await cliente.SendAsync(request))
            {
                if (permitirNoEncontrado && respuesta.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }

                respuesta.EnsureSuccessStatusCode();
                string cuerpo = await respuesta.Content.ReadAsStringAsync();
                T datos = new JavaScriptSerializer().Deserialize<T>(cuerpo);
                if (datos == null)
                {
                    throw new InvalidOperationException("La API de InstiRed devolvió una respuesta vacía.");
                }

                return datos;
            }
        }

        public static async Task<HttpResponseMessage> DescargarDocumentoPostulacionAsync(int id)
        {
            using (var request = CrearRequest(HttpMethod.Get, "archivos/PostulacionesAlumnos/documento/" + id))
            {
                return await cliente.SendAsync(request);
            }
        }

        public static async Task<List<ImagenInstitucion>> ListarImagenesAsync()
        {
            var imagenes = await GetAsync<List<ImagenInstitucion>>("ent/Imagenes");
            return imagenes.Where(imagen => !string.IsNullOrWhiteSpace(imagen.UrlImagen)
                && Uri.IsWellFormedUriString(imagen.UrlImagen, UriKind.Relative)).ToList();
        }

        private static string ObtenerUrl(string ruta)
        {
            string urlBase = ConfigurationManager.AppSettings["InstiRedApiUrl"];
            if (string.IsNullOrWhiteSpace(urlBase))
            {
                throw new InvalidOperationException("Falta configurar el acceso a la API de InstiRed.");
            }

            return urlBase.Trim().TrimEnd('/') + "/" + ruta.TrimStart('/');
        }

        private static HttpRequestMessage CrearRequest(HttpMethod metodo, string ruta)
        {
            string apiKey = ConfigurationManager.AppSettings["InstiRedApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException("Falta configurar el acceso a la API de InstiRed.");
            }

            var request = new HttpRequestMessage(metodo, ObtenerUrl(ruta));
            request.Headers.Add("X-Api-Key", apiKey.Trim());
            return request;
        }

        private static string LeerMensajeApi(JavaScriptSerializer json, string cuerpo)
        {
            const string mensajeGenerico = "Revisá los datos ingresados e intentá nuevamente.";
            try
            {
                var error = json.Deserialize<ErrorApi>(cuerpo);
                if (error != null && !string.IsNullOrWhiteSpace(error.message))
                {
                    return error.message;
                }
            }
            catch (ArgumentException)
            {
                return mensajeGenerico;
            }
            catch (InvalidOperationException)
            {
                return mensajeGenerico;
            }

            return mensajeGenerico;
        }

        public class Respuesta
        {
            public HttpStatusCode Estado { get; set; }
            public string Mensaje { get; set; }
            public string Cuerpo { get; set; }
        }

        public class DocumentoPostulacion
        {
            public int Id { get; set; }
            public int IdPostulacionAlumnos { get; set; }
            public string Descripcion { get; set; }
        }

        public class ImagenInstitucion
        {
            public int Id { get; set; }
            public int IdInstitucion { get; set; }
            public string UrlImagen { get; set; }
            public string Descripcion { get; set; }
            public string UrlPublica => ObtenerUrl("publico/Instituciones/imagen/" + Id);
        }

        private class ErrorApi
        {
            public string message { get; set; }
        }
    }
}
