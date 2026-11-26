using Negocio;
using Presentacion.Auth;
using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Web.UI;

namespace Presentacion
{
    public partial class FormularioAdministradorAdicional : System.Web.UI.Page
    {
        private UsuarioSesion usuario;

        protected void Page_Init(object sender, EventArgs e)
        {
            ViewStateUserKey = Session.SessionID;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            usuario = UsuarioSesion.Obtener(Session);
            if (usuario == null || usuario.IdTipoUsuario != UsuarioSesion.AdministradorInstitucion)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (usuario.IdInstitucion.GetValueOrDefault() <= 0)
            {
                Response.Redirect("~/FormularioInstitucion.aspx");
                return;
            }

            Response.Cache.SetNoStore();
        }

        protected void btnAgregar_Click(object sender, EventArgs e)
        {
            lblMensaje.Visible = false;

            // Validaciones básicas
            if (string.IsNullOrWhiteSpace(txtNombre.Text) ||
                string.IsNullOrWhiteSpace(txtApellido.Text) ||
                string.IsNullOrWhiteSpace(txtDocumento.Text) ||
                string.IsNullOrWhiteSpace(txtFechaNacimiento.Text) ||
                string.IsNullOrWhiteSpace(txtTelefono.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text) ||
                string.IsNullOrWhiteSpace(txtDomicilio.Text))
            {
                MostrarMensaje("Completá todos los campos del formulario.", false);
                return;
            }

            string documento = txtDocumento.Text.Trim();
            if (documento.Length < 7 || documento.Length > 8 || documento.StartsWith("0") ||
                documento.Any(c => c < '0' || c > '9'))
            {
                MostrarMensaje("Ingresá un DNI de 7 u 8 dígitos, sin puntos y sin empezar con cero.", false);
                return;
            }

            if (!EmailValido(txtEmail.Text.Trim()))
            {
                MostrarMensaje("Ingresá un correo electrónico válido.", false);
                return;
            }

            if (!DateTime.TryParse(txtFechaNacimiento.Text, out DateTime fecha) ||
                fecha.Year < 1000 || fecha.Date > DateTime.Today)
            {
                MostrarMensaje("Ingresá una fecha de nacimiento válida.", false);
                return;
            }

            if (txtNombre.Text.Trim().Length > 50 || txtApellido.Text.Trim().Length > 50 ||
                txtEmail.Text.Trim().Length > 100 || txtDomicilio.Text.Trim().Length > 100 ||
                txtTelefono.Text.Trim().Length > 30)
            {
                MostrarMensaje("Revisá la longitud de los datos ingresados.", false);
                return;
            }

            RegisterAsyncTask(new PageAsyncTask(AgregarAdministradorAsync));
        }

        private async Task AgregarAdministradorAsync()
        {
            var registro = new
            {
                idUsuarioActor = usuario.IdUsuario,
                nombre = txtNombre.Text.Trim(),
                apellido = txtApellido.Text.Trim(),
                documento = txtDocumento.Text.Trim(),
                domicilio = txtDomicilio.Text.Trim(),
                fechaNacimiento = txtFechaNacimiento.Text,
                email = txtEmail.Text.Trim(),
                telefono = txtTelefono.Text.Trim()
            };

            try
            {
                var respuesta = await InstiRedApi.PostAsync("sp/CrearAdministradorInstitucionAdicional", registro, leerCuerpo: true);

                if (respuesta.Estado == HttpStatusCode.Created)
                {
                    LimpiarCampos();
                    try
                    {
                        AdministradorApi administrador = new JavaScriptSerializer().Deserialize<AdministradorApi>(respuesta.Cuerpo);
                        if (administrador != null && EnviarCorreo(administrador.Email, administrador.Nombre, administrador.Apellido, administrador.Clave))
                        {
                            MostrarMensaje("Administrador agregado correctamente. Se enviaron sus credenciales por correo electrónico.", true);
                            ClientScript.RegisterStartupScript(this.GetType(), "Redirección", "setTimeout(() => window.location.href='GestionarAdministradores.aspx',3000);", true);
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.Warn("FormularioAdministradorAdicional", "El administrador se creó, pero no se pudieron obtener las credenciales para el correo.", ex);
                    }
                    MostrarMensaje("El administrador se creó, pero no pudimos enviar sus credenciales. Para obtener una nueva contraseña, puede usar «¿Olvidaste tu contraseña?» en el inicio de sesión. No vuelvas a registrar la cuenta.", false);
                    return;
                }

                MostrarMensaje(respuesta.Mensaje ?? "No se pudo completar el alta. Intentá nuevamente más tarde.", false);
            }
            catch (Exception ex)
            {
                Trace.Warn("FormularioAdministradorAdicional", "Error al registrar el administrador en la API.", ex);
                MostrarMensaje("No se pudo confirmar el alta del administrador. Intentá nuevamente más tarde.", false);
            }
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/GestionarAdministradores.aspx");
        }

        // Deja el formulario vacío (se mantiene el mensaje de resultado)
        private void LimpiarCampos()
        {
            txtNombre.Text = "";
            txtApellido.Text = "";
            txtDocumento.Text = "";
            txtFechaNacimiento.Text = "";
            txtTelefono.Text = "";
            txtEmail.Text = "";
            txtDomicilio.Text = "";
        }

        private static bool EmailValido(string email)
        {
            try
            {
                return new MailAddress(email).Address == email;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private void MostrarMensaje(string texto, bool ok)
        {
            lblMensaje.Text = Server.HtmlEncode(texto);
            lblMensaje.CssClass = ok ? "alert theme-success" : "alert theme-danger";
            lblMensaje.Visible = true;
        }

        private bool EnviarCorreo(string email, string nombre, string apellido, string clave)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                Trace.Warn("FormularioAdministradorAdicional", "No se pudo enviar el correo: el administrador no tiene un email registrado.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(apellido) || string.IsNullOrWhiteSpace(clave))
            {
                Trace.Warn("FormularioAdministradorAdicional", "No se pudo enviar el correo: faltan datos del administrador.");
                return false;
            }

            string asunto = "Cuenta de Administrador Creada - InstiRed";
            string cuerpo = CuerpoCredencialesAdministrador(email, nombre, apellido, clave);

            try
            {
                EmailService emailService = new EmailService();
                emailService.armarCorreo(email, asunto, cuerpo);
                emailService.enviarEmail();
                return true;
            }
            catch (Exception ex)
            {
                Trace.Warn("FormularioAdministradorAdicional", "Error al enviar el correo con las credenciales del administrador.", ex);
                return false;
            }
        }

        private string CuerpoCredencialesAdministrador(string email, string nombre, string apellido, string clave)
        {
            string administrador = Server.HtmlEncode(nombre + " " + apellido);
            email = Server.HtmlEncode(email);
            clave = Server.HtmlEncode(clave);

            string cuerpo = $@"
                <html>
                    <body style=""margin:0; padding:0; background-color:#F7F5FC; font-family:'Segoe UI', Roboto, Helvetica, Arial, sans-serif;"">
                        <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color:#F7F5FC;"">
                            <tr>
                                <td align=""center"" style=""padding:32px 16px;"">
                                    <table role=""presentation"" width=""600"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""width:100%; max-width:600px;"">
                                        <tr>
                                            <td align=""center"" bgcolor=""#373A72"" style=""background-color:#373A72; border-radius:15px 15px 0 0; padding:32px 24px;"">
                                                <span style=""font-size:36px; font-weight:700; color:#FFFFFF;"">InstiRed</span>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td bgcolor=""#FFFFFF"" style=""background-color:#FFFFFF; border:1px solid #E4E0F0; border-top:none; border-radius:0 0 15px 15px; padding:40px 36px;"">
                                                <h1 style=""margin:0 0 16px 0; font-size:22px; color:#373A72;"">¡Tu cuenta fue creada!</h1>

                                                <p style=""font-size:15px; line-height:1.6; color:#373A72;"">
                                                    Hola, <strong>{administrador}</strong>. Se creó correctamente tu cuenta de administrador en InstiRed.
                                                </p>

                                                <p style=""font-size:15px; line-height:1.6; color:#373A72;"">
                                                    Para ingresar a la plataforma, utilizá las siguientes credenciales:
                                                </p>

                                                <div style=""background-color:#F7F5FC; border:1px solid #E4E0F0; border-radius:10px; padding:20px; margin:24px 0;"">
                                                    <p style=""margin:0 0 12px 0; font-size:14px; color:#373A72;"">
                                                        <strong>Correo electrónico:</strong><br/>
                                                        {email}
                                                    </p>
                                                    <p style=""margin:0; font-size:14px; color:#373A72;"">
                                                        <strong>Contraseña:</strong><br/>
                                                        <span style=""font-size:20px; font-weight:700; letter-spacing:1px; color:#373A72;"">{clave}</span>
                                                    </p>
                                                </div>

                                                <p style=""font-size:15px; line-height:1.6; color:#373A72;"">
                                                    Ingresá a InstiRed utilizando tu correo electrónico y la contraseña proporcionada. Te recomendamos cambiarla después de iniciar sesión.
                                                </p>

                                                <hr style=""border:none; border-top:1px solid #E4E0F0; margin:24px 0 20px 0;"">

                                                <p style=""font-size:13px; line-height:1.5; color:#8D91D5;"">
                                                    Este es un correo automático. Por favor, no respondas a esta dirección.
                                                </p>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>
                        </table>
                    </body>
                </html>";

            return cuerpo;
        }

        private class AdministradorApi
        {
            public string Nombre { get; set; }
            public string Apellido { get; set; }
            public string Email { get; set; }
            public string Clave { get; set; }
        }
    }
}