using System;
using System.Configuration;
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

        public static async Task<Respuesta> PostAsync(string ruta, object datos)
        {
            string urlBase = ConfigurationManager.AppSettings["InstiRedApiUrl"];
            string apiKey = ConfigurationManager.AppSettings["InstiRedApiKey"];
            if (string.IsNullOrWhiteSpace(urlBase) || string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException("Falta configurar el acceso a la API de InstiRed.");
            }

            string url = urlBase.TrimEnd('/') + "/" + ruta.TrimStart('/');
            var json = new JavaScriptSerializer();

            using (var request = new HttpRequestMessage(HttpMethod.Post, url))
            {
                request.Headers.Add("X-Api-Key", apiKey.Trim());
                request.Content = new StringContent(json.Serialize(datos), Encoding.UTF8, "application/json");

                using (HttpResponseMessage respuesta = await cliente.SendAsync(request))
                {
                    string mensaje = null;
                    if (respuesta.StatusCode == HttpStatusCode.BadRequest)
                    {
                        string cuerpo = await respuesta.Content.ReadAsStringAsync();
                        mensaje = LeerMensajeApi(json, cuerpo);
                    }

                    return new Respuesta { Estado = respuesta.StatusCode, Mensaje = mensaje };
                }
            }
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
        }

        private class ErrorApi
        {
            public string message { get; set; }
        }
    }
}
