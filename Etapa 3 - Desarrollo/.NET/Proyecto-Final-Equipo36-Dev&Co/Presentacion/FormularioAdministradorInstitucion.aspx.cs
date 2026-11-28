using Negocio;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Presentacion
{
    public partial class FormularioAdministradorInstitucion : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Form.Enctype = "multipart/form-data";
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            RegisterAsyncTask(new PageAsyncTask(RegistrarAsync));
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Login.aspx");
        }

        private async Task RegistrarAsync()
        {
            try
            {
                Validar();

                using (var formulario = new MultipartFormDataContent())
                {
                    formulario.Add(new StringContent(txtNombre.Text.Trim()), "Nombre");
                    formulario.Add(new StringContent(txtApellido.Text.Trim()), "Apellido");
                    formulario.Add(new StringContent(txtDocumento.Text.Trim()), "Documento");
                    formulario.Add(new StringContent(txtDomicilio.Text.Trim()), "Domicilio");
                    formulario.Add(new StringContent(txtFechaNacimiento.Text), "FechaNacimiento");
                    formulario.Add(new StringContent(txtEmail.Text.Trim()), "Email");
                    formulario.Add(new StringContent(txtTelefono.Text.Trim()), "Telefono");
                    formulario.Add(new StringContent(txtClave.Text), "Clave");
                    formulario.Add(new StringContent(txtNombreInstitucion.Text.Trim()), "NombreInstitucion");
                    formulario.Add(new StringContent(txtCuit.Text.Trim()), "CuitInstitucion");
                    AgregarArchivo(formulario, fuConstanciaFiscal, "ConstanciaFiscal");
                    AgregarArchivo(formulario, fuHabilitacion, "Habilitacion");
                    AgregarArchivo(formulario, fuConstanciaLaboral, "ConstanciaLaboral");

                    var respuesta = await InstiRedApi.PostAsync("sp/RegistrarAdministradorInstitucion", formulario, leerCuerpo: true);
                    if (respuesta.Estado == HttpStatusCode.Created)
                    {
                        btnGuardar.Enabled = false;
                        var registro = new JavaScriptSerializer().Deserialize<ResultadoRegistro>(respuesta.Cuerpo);
                        if (registro == null || string.IsNullOrWhiteSpace(registro.CodigoSeguimiento))
                        {
                            MostrarResultado("La solicitud se registró, pero la respuesta no incluyó el código de seguimiento. Consultá con Dev&Co.", true);
                            return;
                        }

                        MostrarResultado("Tu solicitud quedó pendiente de revisión. Guardá el código de seguimiento " +
                            registro.CodigoSeguimiento + ". Podrás iniciar sesión cuando Dev&Co apruebe tu solicitud.", true);

                        // Enviar al mail el codigo de seguimiento
                        EnviarCorreo(txtEmail.Text.Trim(), txtNombre.Text.Trim(), txtApellido.Text.Trim(), txtNombreInstitucion.Text.Trim(), registro.CodigoSeguimiento);
                        ClientScript.RegisterStartupScript(this.GetType(), "Redirección", "setTimeout(() => window.location.href='Login.aspx',3000);", true);
                        return;
                    }

                    if (respuesta.Estado == HttpStatusCode.BadRequest)
                    {
                        MostrarResultado(respuesta.Mensaje, false);
                        return;
                    }
                    if (respuesta.Estado == HttpStatusCode.RequestEntityTooLarge)
                    {
                        MostrarResultado("Los archivos superan el tamaño permitido. El máximo es de 5 MB por documento.", false);
                        return;
                    }
                }

                MostrarResultado("No se pudo completar el preregistro. Intentá nuevamente más tarde.", false);
            }
            catch (ArgumentException ex)
            {
                MostrarResultado(ex.Message, false);
            }
            catch (InvalidOperationException)
            {
                MostrarResultado("El preregistro no está disponible. Intentá nuevamente más tarde.", false);
            }
            catch (HttpRequestException)
            {
                MostrarResultado("No se pudo conectar con la API. Intentá nuevamente más tarde.", false);
            }
            catch (TaskCanceledException)
            {
                MostrarResultado("La API tardó demasiado en responder. No se pudo confirmar el resultado del preregistro.", false);
            }
        }

        private void Validar()
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text) ||
                string.IsNullOrWhiteSpace(txtApellido.Text) ||
                string.IsNullOrWhiteSpace(txtDocumento.Text) ||
                string.IsNullOrWhiteSpace(txtDomicilio.Text) ||
                string.IsNullOrWhiteSpace(txtFechaNacimiento.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text) ||
                string.IsNullOrWhiteSpace(txtTelefono.Text) ||
                string.IsNullOrWhiteSpace(txtClave.Text) ||
                string.IsNullOrWhiteSpace(txtClaveRepetida.Text) ||
                string.IsNullOrWhiteSpace(txtNombreInstitucion.Text) ||
                string.IsNullOrWhiteSpace(txtCuit.Text))
            {
                throw new ArgumentException("Completá todos los campos del formulario.");
            }

            if (txtNombre.Text.Trim().Length > 50 || txtApellido.Text.Trim().Length > 50 ||
                txtDomicilio.Text.Trim().Length > 100 || txtEmail.Text.Trim().Length > 100 ||
                txtTelefono.Text.Trim().Length > 30 || txtNombreInstitucion.Text.Trim().Length > 100)
            {
                throw new ArgumentException("Revisá la longitud de los datos ingresados.");
            }

            string documento = txtDocumento.Text.Trim();
            if (documento.Length < 7 || documento.Length > 8 || documento.StartsWith("0") ||
                documento.Any(c => c < '0' || c > '9'))
            {
                throw new ArgumentException("Ingresá un DNI de 7 u 8 dígitos, sin puntos y sin empezar con cero.");
            }

            if (!DateTime.TryParse(txtFechaNacimiento.Text, out DateTime fecha) ||
                fecha.Year < 1000 || fecha.Date > DateTime.Today)
            {
                throw new ArgumentException("Ingresá una fecha de nacimiento válida.");
            }

            string email = txtEmail.Text.Trim();
            if (!EmailValido(email))
            {
                throw new ArgumentException("Ingresá un correo electrónico válido.");
            }

            string clave = txtClave.Text;
            if (clave.Length < 8 || clave.Length > 20 ||
                !clave.Any(char.IsUpper) || !clave.Any(char.IsLower) ||
                !clave.Any(char.IsDigit) || !clave.Any(c => char.IsPunctuation(c) || char.IsSymbol(c)))
            {
                throw new ArgumentException("La contraseña debe tener entre 8 y 20 caracteres e incluir una mayúscula, una minúscula, un número y un símbolo.");
            }
            if (clave != txtClaveRepetida.Text)
            {
                throw new ArgumentException("Las contraseñas no coinciden.");
            }

            string cuit = txtCuit.Text.Trim();
            if (cuit.Length != 11 || cuit.Any(c => c < '0' || c > '9'))
            {
                throw new ArgumentException("Ingresá un CUIT de 11 dígitos, sin guiones.");
            }

            ValidarArchivo(fuConstanciaFiscal, "la constancia fiscal");
            ValidarArchivo(fuHabilitacion, "la habilitación");
            ValidarArchivo(fuConstanciaLaboral, "la constancia laboral");
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

        private static void ValidarArchivo(FileUpload archivo, string nombre)
        {
            if (!archivo.HasFile)
            {
                throw new ArgumentException("Adjuntá " + nombre + ".");
            }
            if (archivo.PostedFile.ContentLength > 5 * 1024 * 1024)
            {
                throw new ArgumentException("El archivo de " + nombre + " no puede superar los 5 MB.");
            }

            string extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            if (extension != ".pdf" && extension != ".jpg" && extension != ".jpeg" && extension != ".png")
            {
                throw new ArgumentException("El archivo de " + nombre + " debe ser PDF, JPG o PNG.");
            }
        }

        private static void AgregarArchivo(MultipartFormDataContent formulario, FileUpload archivo, string nombre)
        {
            formulario.Add(new StreamContent(archivo.PostedFile.InputStream), nombre, Path.GetFileName(archivo.FileName));
        }

        private void MostrarResultado(string mensaje, bool exito)
        {
            pnlMensaje.Visible = true;
            pnlMensaje.CssClass = exito ? "alert theme-success mb-3" : "alert theme-danger mb-3";
            if (!exito)
            {
                mensaje += " Volvé a seleccionar los tres archivos antes de enviar nuevamente.";
            }
            litMensaje.Text = mensaje;
        }

        private void EnviarCorreo(string email, string nombre, string apellido, string institucion, string codigoSeguimiento)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                Trace.Warn("FormularioAdministradorInstitucion", "No se pudo enviar el correo: el administrador no tiene un email registrado.");
                return;
            }

            if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(apellido) || string.IsNullOrWhiteSpace(institucion) || string.IsNullOrWhiteSpace(codigoSeguimiento))
            {
                Trace.Warn("FormularioAdministradorInstitucion", "No se pudo enviar el correo: faltan datos de la postulación.");
                return;
            }

            string asunto = "Bienvenido a InstiRed - Solicitud recibida";
            string cuerpo = CuerpoCodigoSeguimiento(nombre, apellido, institucion, codigoSeguimiento);

            try
            {
                EmailService emailService = new EmailService();
                emailService.armarCorreo(email, asunto, cuerpo);
                emailService.enviarEmail();
            }
            catch (Exception ex)
            {
                Trace.Warn("FormularioAdministradorInstitucion", "Error al enviar el correo con el código de seguimiento", ex);
            }
        }

        private string CuerpoCodigoSeguimiento(string nombre, string apellido, string institucion, string codigoSeguimiento)
        {
            string nombreAdministrador = nombre + " " + apellido;

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
                                                <h1 style=""margin:0 0 16px 0; font-size:22px; color:#373A72;"">¡Bienvenido! Recibimos tu solicitud</h1>

                                                <p style=""font-size:15px; line-height:1.6; color:#373A72;"">
                                                    Hola, <strong>{nombreAdministrador}</strong>. Recibimos correctamente tu solicitud de registro como administrador de la institución <strong>{institucion}</strong>.
                                                </p>

                                                <p style=""font-size:15px; line-height:1.6; color:#373A72;"">
                                                    Tu solicitud quedó pendiente de revisión por parte de Dev&Co. Para consultar su estado, conservá el siguiente código de seguimiento:
                                                </p>

                                                <div style=""background-color:#F7F5FC; border:1px solid #E4E0F0; border-radius:10px; padding:20px; text-align:center; margin:24px 0;"">
                                                    <p style=""margin:0 0 8px 0; font-size:13px; color:#8D91D5;"">CÓDIGO DE SEGUIMIENTO</p>
                                                    <span style=""font-size:24px; font-weight:700; letter-spacing:1px; color:#373A72;"">{codigoSeguimiento}</span>
                                                </div>

                                                <p style=""font-size:15px; line-height:1.6; color:#373A72;"">
                                                    Recordá que podrás iniciar sesión en InstiRed una vez que Dev&Co apruebe tu solicitud.
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

        private class ResultadoRegistro
        {
            public string CodigoSeguimiento { get; set; }
        }
    }
}
