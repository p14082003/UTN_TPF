using Negocio;
using Presentacion.Auth;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;

namespace Presentacion
{
    public class DescargarDocumentoAlumno : HttpTaskAsyncHandler, IReadOnlySessionState
    {
        public override async Task ProcessRequestAsync(HttpContext context)
        {
            HttpResponse respuesta = context.Response;
            respuesta.TrySkipIisCustomErrors = true;
            respuesta.Cache.SetNoStore();

            var usuario = UsuarioSesion.Obtener(context.Session);
            if (usuario == null || usuario.IdTipoUsuario != UsuarioSesion.AdministradorInstitucion ||
                usuario.IdInstitucion.GetValueOrDefault() <= 0)
            {
                respuesta.StatusCode = 403;
                return;
            }

            if (!int.TryParse(context.Request.QueryString["id"], out int id) || id <= 0)
            {
                respuesta.StatusCode = 404;
                return;
            }

            try
            {
                var documento = await InstiRedApi.GetAsync<InstiRedApi.DocumentoPostulacion>(
                    "ent/DocumentosPostulacion/" + id, permitirNoEncontrado: true);
                if (documento == null)
                {
                    respuesta.StatusCode = 404;
                    return;
                }

                var postulacion = await InstiRedApi.GetAsync<PostulacionApi>(
                    "vw/PostulacionesAlumnos/" + documento.IdPostulacionAlumnos, permitirNoEncontrado: true);
                if (postulacion == null || postulacion.IdInstitucion != usuario.IdInstitucion)
                {
                    respuesta.StatusCode = 404;
                    return;
                }

                using (HttpResponseMessage archivo = await InstiRedApi.DescargarDocumentoPostulacionAsync(id))
                {
                    if (archivo.StatusCode == HttpStatusCode.NotFound)
                    {
                        respuesta.StatusCode = 404;
                        return;
                    }

                    archivo.EnsureSuccessStatusCode();
                    respuesta.Headers["X-Content-Type-Options"] = "nosniff";
                    respuesta.Headers["Content-Disposition"] = archivo.Content.Headers.ContentDisposition?.ToString() ?? "attachment";
                    respuesta.ContentType = archivo.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
                    await archivo.Content.CopyToAsync(respuesta.OutputStream);
                }
            }
            catch (Exception ex)
            {
                context.Trace.Warn("DescargarDocumentoAlumno", "Error al descargar el documento de la API.", ex);
                respuesta.StatusCode = 502;
            }
        }

        private class PostulacionApi
        {
            public int IdInstitucion { get; set; }
        }
    }
}
