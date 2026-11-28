using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Negocio
{
    public class NotificacionEmail
    {
        public string Destinatario { get; set; }
        public string NombreDestinatario { get; set; }
        public string Asunto { get; set; }
        public string Titulo { get; set; }
        public string Mensaje { get; set; }
        public IDictionary<string, string> Datos { get; set; } = new Dictionary<string, string>();
        public string Indicaciones { get; set; }
        public string TextoAccion { get; set; }
        public string UrlAccion { get; set; }

        // La operación de negocio ya fue confirmada por la API antes de llamar a este método.
        public bool IntentarEnviar(Action<Exception> registrarError)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Destinatario))
                {
                    throw new ArgumentException("No hay un correo registrado para enviar la notificación.");
                }

                EmailService servicio = new EmailService();
                servicio.armarCorreo(Destinatario.Trim(), Asunto, CrearCuerpo());
                servicio.enviarEmail();
                return true;
            }
            catch (Exception ex)
            {
                registrarError?.Invoke(ex);
                return false;
            }
        }

        private string CrearCuerpo()
        {
            StringBuilder detalle = new StringBuilder();
            foreach (var dato in Datos)
            {
                if (!string.IsNullOrWhiteSpace(dato.Value))
                {
                    detalle.Append("<p style=\"margin:10px 0;\"><strong>")
                        .Append(Texto(dato.Key)).Append(":</strong> ").Append(Texto(dato.Value)).Append("</p>");
                }
            }

            string saludo = string.IsNullOrWhiteSpace(NombreDestinatario)
                ? "Hola." : "Hola, <strong>" + Texto(NombreDestinatario) + "</strong>.";
            string accion = "";
            if (!string.IsNullOrWhiteSpace(TextoAccion) && !string.IsNullOrWhiteSpace(UrlAccion))
            {
                Uri url;
                if (!Uri.TryCreate(UrlAccion, UriKind.Absolute, out url) ||
                    (url.Scheme != Uri.UriSchemeHttps && url.Scheme != Uri.UriSchemeHttp))
                    throw new ArgumentException("El enlace del correo debe ser una dirección HTTP o HTTPS absoluta.");

                accion = $@"<table role=""presentation"" cellpadding=""0"" cellspacing=""0"" style=""margin:24px 0;""><tr>
<td style=""background-color:#373A72;border-radius:12px;text-align:center;""><a href=""{Texto(UrlAccion)}"" style=""display:inline-block;padding:14px 28px;color:#FFFFFF;font-weight:700;text-decoration:none;"">{Texto(TextoAccion)}</a></td>
</tr></table>";
            }
            return $@"<!DOCTYPE html>
<html lang=""es""><body style=""margin:0;padding:0;background-color:#F7F5FC;font-family:'Segoe UI',Roboto,Helvetica,Arial,sans-serif;"">
<table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#F7F5FC;"">
<tr><td align=""center"" style=""padding:32px 16px;"">
<table role=""presentation"" width=""600"" cellpadding=""0"" cellspacing=""0"" style=""width:100%;max-width:600px;"">
<tr><td align=""center"" style=""background-color:#373A72;border-radius:15px 15px 0 0;padding:32px 24px;"">
<span style=""font-size:36px;font-weight:700;color:#FFFFFF;"">InstiRed</span></td></tr>
<tr><td style=""background-color:#FFFFFF;border:1px solid #E4E0F0;border-top:none;border-radius:0 0 15px 15px;padding:32px 24px;color:#373A72;font-size:15px;line-height:1.6;"">
<h1 style=""margin:0 0 16px;font-size:22px;"">{Texto(Titulo)}</h1>
<p>{saludo}</p><p>{Texto(Mensaje)}</p>
<div style=""background-color:#F7F5FC;border:1px solid #E4E0F0;border-radius:10px;padding:16px;"">{detalle}</div>
<p>{Texto(Indicaciones)}</p>
{accion}
<hr style=""border:none;border-top:1px solid #E4E0F0;margin:24px 0 20px;"">
<p style=""font-size:13px;color:#8D91D5;"">Este es un correo automático. Por favor, no respondas a esta dirección.</p>
</td></tr></table></td></tr></table></body></html>";
        }

        private static string Texto(string valor)
        {
            return WebUtility.HtmlEncode(valor ?? "").Replace("\r\n", "\n").Replace("\n", "<br>");
        }
    }
}
