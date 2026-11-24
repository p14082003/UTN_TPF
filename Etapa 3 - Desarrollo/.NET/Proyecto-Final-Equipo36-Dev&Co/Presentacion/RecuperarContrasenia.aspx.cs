using Negocio;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Presentacion
{
    public partial class RecuperarContrasenia : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Cache.SetNoStore();
        }

        protected void btnEnviarCredenciales_Click(object sender, EventArgs e)
        {
            lblMensaje.Visible = false;
            RegisterAsyncTask(new PageAsyncTask(RecuperarClaveAsync));
        }

        private async Task RecuperarClaveAsync()
        {
            string email = txtEmail.Text.Trim();
            try
            {
                if (email.Length == 0 || email.Length > 100 || new MailAddress(email).Address != email)
                {
                    MostrarMensaje("Ingresá un correo electrónico válido de hasta 100 caracteres.", false);
                    return;
                }
            }
            catch (FormatException)
            {
                MostrarMensaje("Ingresá un correo electrónico válido.", false);
                return;
            }

            btnEnviarCredenciales.Enabled = false;
            try
            {
                var respuesta = await InstiRedApi.PostAsync("sp/RecuperarClave", new { email }, leerCuerpo: true);
                if (respuesta.Estado == HttpStatusCode.BadRequest)
                {
                    MostrarMensaje(respuesta.Mensaje ?? "No se pudo recuperar la cuenta con ese correo.", false);
                    return;
                }
                if (respuesta.Estado != HttpStatusCode.OK)
                {
                    MostrarMensaje("No pudimos confirmar el cambio de contraseña. Volvé a solicitar la recuperación.", false);
                    return;
                }

                var credenciales = new JavaScriptSerializer().Deserialize<CredencialesRecuperacion>(respuesta.Cuerpo);
                if (credenciales == null || string.IsNullOrWhiteSpace(credenciales.Email) ||
                    string.IsNullOrWhiteSpace(credenciales.NuevaClaveGenerada))
                {
                    MostrarMensaje("No pudimos obtener las credenciales para el envío. Volvé a solicitar la recuperación.", false);
                    return;
                }
                if (!envioEmailRecuperacionClave(credenciales.Nombre, credenciales.Apellido,
                    credenciales.Email, credenciales.NuevaClaveGenerada))
                {
                    MostrarMensaje("La contraseña se actualizó, pero no pudimos enviar el correo. Volvé a solicitarlo: se generará otra contraseña. Solo sirve la clave de la última solicitud.", false);
                    return;
                }

                MostrarMensaje("Se envió un correo con tu nueva contraseña. Si hiciste varias solicitudes, usá la clave de la última solicitud.", true);
                ClientScript.RegisterStartupScript(GetType(), "redirigir", "setTimeout(function(){ window.location='Login.aspx'; }, 3000);", true);
            }
            catch (Exception ex)
            {
                Trace.Warn("RecuperarContrasenia", "No se pudo completar la recuperación de contraseña.", ex);
                MostrarMensaje("No pudimos confirmar la recuperación. Volvé a solicitarla: cada intento genera otra contraseña.", false);
            }
            finally
            {
                btnEnviarCredenciales.Enabled = true;
            }
        }

        private bool envioEmailRecuperacionClave(String nombre, string apellido, string emailUsuario, string claveNueva)
        {
            try
            {
                EmailService email = new EmailService();
                string nombreCompleto = HttpUtility.HtmlEncode((nombre + " " + apellido).Trim());
                string saludo = nombreCompleto == "" ? "¡Hola!" : "¡Hola " + nombreCompleto + "!";
                string correo = HttpUtility.HtmlEncode(emailUsuario);
                string clave = HttpUtility.HtmlEncode(claveNueva);
                string cuerpo = $@"
                    <html>
                      <body style='font-family: Arial, sans-serif; background-color:#f5f5f5; padding:20px; color:#000;'>
                        <div style='max-width:600px; margin:auto; background:#fff; border:1px solid #ddd; border-radius:8px; padding:20px;'>
                          <h2 style='text-align:center;'>{saludo} Aquí tienes tus datos de acceso para acceder a InstiRed</h2>
                          <p>Tu usuario para ingresar es: 
                            <b>{correo}</b>
                          </p>
                          <p>Tu nueva contraseña generada es:</p>
                          <p style='font-size:20px; font-weight:bold; text-align:center; margin:15px 0;'>{clave} </p>
                          <p>Si hiciste varias solicitudes, solo sirve la contraseña de la última solicitud.</p>
                          <p style='text-align:center;'> Por seguridad, te sugerimos que al ingresar a tu cuenta la actualices por una nueva contraseña.</p>
                          <hr style='border:none; border-top:1px solid #eee; margin:20px 0;' />
                          <p style='font-size:12px; text-align:center;'>
                            Este mensaje fue generado automáticamente por <b>InstiRed</b>.<br/>
                            Por favor, no respondas a este correo.
                          </p>
                        </div>
                      </body>
                    </html>";

                email.armarCorreo(emailUsuario,"Recupero de datos de acceso - InstiRed",cuerpo);
                email.enviarEmail();
                return true;


            }catch (Exception ex)
            {
                
                Trace.Warn("RecuperarContrasenia", "Error al enviar email.", ex);
                return false;
            }  

        }

        private void MostrarMensaje(string texto, bool ok)
        {
            lblMensaje.Text = HttpUtility.HtmlEncode(texto);
            lblMensaje.CssClass = ok ? "alert alert-success d-block mb-4" : "alert alert-danger d-block mb-4";
            lblMensaje.Visible = true;
        }

        private class CredencialesRecuperacion
        {
            public string Email { get; set; }
            public string Nombre { get; set; }
            public string Apellido { get; set; }
            public string NuevaClaveGenerada { get; set; }
        }
    }
}
