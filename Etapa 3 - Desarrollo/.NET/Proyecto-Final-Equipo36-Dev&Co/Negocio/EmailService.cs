using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Negocio
{
    public class EmailService
    {
        private MailMessage email;
        private SmtpClient server;

        public EmailService()
        {
            server = new SmtpClient();
            server.Credentials = new NetworkCredential("testingprogramacionutnfrpg@gmail.com", "jvxg dnvs puov pggm"); // gmail creado para el programa, clave generada para esta app
            server.EnableSsl = true;
            server.Port = 587;
            server.Host = "smtp.gmail.com";
        }

        public void armarCorreo(string emailDestino, string asunto, string cuerpo)
        {
            email = new MailMessage();
            email.From = new MailAddress("notificaciones@instired.com");
            email.To.Add(emailDestino);
            email.Subject = asunto;
            email.IsBodyHtml = true;
            email.Body = cuerpo;

        }

        public void enviarEmail()
        {
            try
            {
                server.Send(email);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }

    // VALIDAR USUARIO

    //<html>
    //    <body style = ""margin:0; padding:0; background-color:#F7F5FC; font-family:'Segoe UI', Roboto, Helvetica, Arial, sans-serif;"">

    //        <!-- Texto de vista previa -->
    //        <div style = ""display:none; max-height:0; overflow:hidden; opacity:0; color:#F7F5FC; font-size:1px; line-height:1px;"">
    //            Confirmá tu correo para activar tu cuenta en InstiRed.
    //        </div>

    //        <table role = ""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color:#F7F5FC;"">
    //            <tr>
    //                <td align = ""center"" style=""padding:32px 16px;"">

    //                    <table role = ""presentation"" width=""600"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""width:100%; max-width:600px;"">

    //                        <!-- Encabezado -->
    //                        <tr>
    //                            <td align = ""center"" bgcolor=""#373A72"" style=""background-color:#373A72; border-radius:15px 15px 0 0; padding:28px 24px;"">
    //                            <span style = ""font-size:28px; font-weight:700; letter-spacing:0.5px; color:#FFFFFF;"">InstiRed</span>
    //                            </td>
    //                        </tr>

    //                        <!-- Tarjeta -->
    //                        <tr>
    //                            <td bgcolor = ""#FFFFFF"" style=""background-color:#FFFFFF; border:1px solid #E4E0F0; border-top:none; border-radius:0 0 15px 15px; padding:40px 36px;"">

    //                                < h1 style=""margin:0 0 16px 0; font-size:22px; line-height:1.3; font-weight:700; color:#373A72;"">
    //                                    ¡Hola, Bernardo!
    //                                </h1>

    //                                <p style = ""margin:0 0 16px 0; font-size:15px; line-height:1.6; color:#373A72;"">
    //                                    Gracias por registrarte en<strong>InstiRed</strong>.Para activar tu cuenta y empezar a usar la plataforma, necesitamos validar tu correo electrónico.
    //                                </p>

    //                                <p style = ""margin:0 0 28px 0; font-size:15px; line-height:1.6; color:#373A72;"">
    //                                    Hacé clic en el botón para continuar:
    //                                </p>

    //                                <!-- Botón -->
    //                                <table role = ""presentation"" cellpadding=""0"" cellspacing=""0"" border=""0"" align=""center"" style=""margin:0 auto 28px auto;"">
    //                                    <tr>
    //                                        <td align = ""center"" bgcolor=""#373A72"" style=""background-color:#373A72; border-radius:15px;"">
    //                                            <a href = ""{{{{url_validacion}}}}"" target = ""_blank"" style=""display:inline - block; padding: 14px 36px; font - size:16px; font - weight:600; color:#FFFFFF; text-decoration:none; border-radius:15px; border:1px solid #373A72;"">
    //                                                Validar mi cuenta
    //                                            </ a >
    //                                        </ td >
    //                                    </ tr >
    //                                </ table >

    //                                < p style = ""margin: 0 0 6px 0; font - size:14px; line - height:1.5; color:#373A72;"">
    //                                    Si el botón no funciona, hacé clic en este enlace:
    //                                </ p >
    //                                < p style = ""margin: 0 0 28px 0; font - size:14px; line - height:1.5; word -break:break-all; "" >
    //                                    < a href = ""{ { { { url_validacion} } } } "" target = ""_blank"" style=""color:#8D91D5; text-decoration:none;"">{{{{url_validacion}}}}</a>
    //                                </ p >
    //                                < hr style = ""border: none; border - top:1px solid #E4E0F0; margin:0 0 20px 0;"">

    //                                <p style=""margin:0; font - size:13px; line - height:1.5; color:#8D91D5;"">
    //                                    Si no creaste una cuenta en InstiRed, ignorá este mensaje: no se realizará ningún cambio.
    //                                </p>

    //                            </td>
    //                        </tr>

    //                        <!-- Pie -->
    //                        <tr>
    //                            <td align=""center"" style=""padding:24px 16px 0 16px; font - size:12px; line - height:1.5; color:#8D91D5;"">
    //                                Este es un correo automático, por favor no respondas a esta dirección.<br>
    //                                &copy; 2026  InstiRed.Todos los derechos reservados.
    //                            </td>
    //                        </tr>

    //                    </table>

    //                </td>
    //            </tr>
    //        </table>

    //    </body>
    //</html>

    // --------------------------------------------------------------------------------------------------------------------------------------------------------------------

    // CLAVE TEMPORAL

    //<html>
    //    <body style = ""margin:0; padding:0; background-color:#F7F5FC; font-family:'Segoe UI', Roboto, Helvetica, Arial, sans-serif;"">

    //    <!-- Texto de vista previa(preheader) -->
    //    <div style = ""display:none; max-height:0; overflow:hidden; opacity:0; color:#F7F5FC; font-size:1px; line-height:1px;"">
    //        Usá tu clave temporal para ingresar a InstiRed y crear una nueva contraseña.
    //    </div>

    //    <table role = ""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color:#F7F5FC;"">
    //        <tr>
    //            <td align = ""center"" style=""padding:32px 16px;"">

    //            <table role = ""presentation"" width=""600"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""width:100%; max-width:600px;"">

    //                <!-- Encabezado -->
    //                <tr>
    //                    <td align = ""center"" bgcolor=""#373A72"" style=""background-color:#373A72; border-radius:15px 15px 0 0; padding:28px 24px;"">
    //                        <span style = ""font-size:28px; font-weight:700; letter-spacing:0.5px; color:#FFFFFF;"">InstiRed</span>
    //                    </td>
    //                </tr>

    //                <!-- Tarjeta -->
    //                <tr>
    //                    <td bgcolor = ""#FFFFFF"" style=""background-color:#FFFFFF; border:1px solid #E4E0F0; border-top:none; border-radius:0 0 15px 15px; padding:40px 36px;"">
    //                        < h1 style=""margin:0 0 16px 0; font-size:22px; line-height:1.3; font-weight:700; color:#373A72;"">
    //                            Tu clave temporal
    //                        </h1>

    //                    <p style = ""margin:0 0 16px 0; font-size:15px; line-height:1.6; color:#373A72;"">
    //                        Hola, {{{{nombre}}}}. Recibimos una solicitud para restablecer el acceso a tu cuenta de <strong>InstiRed</strong>.
    //                    </p>

    //                    <p style=""margin:0 0 20px 0; font - size:15px; line - height:1.6; color:#373A72;"">
    //                      Usá la siguiente clave temporal para iniciar sesión:
    //                    </ p >


    //                    < !--Clave temporal-- >
    //                    < table role = ""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""margin:0 0 24px 0; "" >
    //                        < tr >
    //                            < td align = ""center"" bgcolor=""#F7F5FC"" style=""background-color:#F7F5FC; border:1px solid #E4E0F0; border-radius:15px; padding:22px 16px;"">
    //                                <span style=""font-family:'Courier New', Courier, monospace; font - size:30px; font - weight:700; letter - spacing:6px; color:#373A72;"">{{{{clave_temporal}}}}</span>
    //                            </ td >
    //                         </ tr >
    //                    </ table >

    //                    < !--Botón-- >
    //                    < table role = ""presentation"" cellpadding=""0"" cellspacing=""0"" border=""0"" align=""center"" style=""margin:0 auto 28px auto; "" >
    //                        < tr >
    //                            < td align = ""center"" bgcolor=""#373A72"" style=""background-color:#373A72; border-radius:15px;"">
    //                                <a href=""{{{{url_login}}}}"" target = ""_blank"" style=""display:inline - block; padding: 14px 36px; font - size:16px; font - weight:600; color:#FFFFFF; text-decoration:none; border-radius:15px; border:1px solid #373A72;"">
    //                                    Iniciar sesión
    //                                </ a >
    //                            </ td >
    //                        </ tr >
    //                    </ table >

    //                    < !--Indicaciones-- >
    //                    < table role = ""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""margin:0 0 24px 0; "" >
    //                        < tr >
    //                            < td bgcolor = ""#F7F5FC"" style=""background-color:#F7F5FC; border:1px solid #E4E0F0; border-radius:15px; padding:16px 20px; font-size:14px; line-height:1.6; color:#373A72;"">
    //                                < strong > Tené en cuenta:</ strong >< br >
    //                                    &bull; Al ingresar, se te va a pedir que definas una contraseña nueva.<br>
    //                                    &bull; No compartas esta clave con nadie. El equipo de InstiRed nunca te la va a pedir.
    //                            </td>
    //                        </tr>
    //                    </table>
    //                    <hr style=""border:none; border - top:1px solid #E4E0F0; margin:0 0 20px 0;"">

    //                    <p style=""margin:0; font - size:13px; line - height:1.5; color:#8D91D5;"">
    //                        Si no solicitaste este cambio, ignorá este mensaje: tu contraseña actual seguirá funcionando. Si recibís este correo sin haberlo pedido en varias ocasiones, comunicate con el soporte de InstiRed.
    //                    </p>

    //                  </td>
    //                </tr>

    //                <!-- Pie -->
    //                <tr>
    //                    <td align=""center"" style=""padding:24px 16px 0 16px; font - size:12px; line - height:1.5; color:#8D91D5;"">
    //                        Este es un correo automático, por favor no respondas a esta dirección.<br>
    //                        &copy; 2026 InstiRed.Todos los derechos reservados.
    //                    </td>
    //                </tr>

    //              </table>

    //                </td>
    //            </tr>
    //        </table>

    //    </body>
    //</html>
}
