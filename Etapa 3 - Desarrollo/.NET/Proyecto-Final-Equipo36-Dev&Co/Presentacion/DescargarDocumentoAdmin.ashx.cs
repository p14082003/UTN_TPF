using Negocio;
using Presentacion.Auth;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;

namespace Presentacion
{
    public class DescargarDocumentoAdmin : HttpTaskAsyncHandler, IReadOnlySessionState
    {
        public override async Task ProcessRequestAsync(HttpContext context)
        {
            HttpResponse respuesta = context.Response;
            respuesta.TrySkipIisCustomErrors = true;
            respuesta.Cache.SetNoStore();

            if (!UsuarioSesion.TieneRol(context.Session, UsuarioSesion.DevCo))
            {
                respuesta.StatusCode = 403;
                return;
            }

            string tipo = context.Request.QueryString["tipo"];
            if (!int.TryParse(context.Request.QueryString["id"], out int id) || id <= 0 ||
                (tipo != "constancia-fiscal" && tipo != "habilitacion" && tipo != "constancia-laboral"))
            {
                respuesta.StatusCode = 404;
                return;
            }

            try
            {
                using (var archivo = await InstiRedApi.DescargarDocumentoAdminAsync(id, tipo))
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
                context.Trace.Warn("DescargarDocumentoAdmin", "Error al descargar el documento de la API.", ex);
                respuesta.StatusCode = 502;
            }
        }
    }
}
