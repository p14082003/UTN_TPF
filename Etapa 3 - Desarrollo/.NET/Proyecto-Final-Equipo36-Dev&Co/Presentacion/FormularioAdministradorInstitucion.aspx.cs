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
            pnlMensaje.CssClass = exito ? "alert alert-success mb-3" : "alert alert-danger mb-3";
            if (!exito)
            {
                mensaje += " Volvé a seleccionar los tres archivos antes de enviar nuevamente.";
            }
            litMensaje.Text = mensaje;
        }

        private class ResultadoRegistro
        {
            public string CodigoSeguimiento { get; set; }
        }
    }
}
