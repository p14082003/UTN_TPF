using System;
using Negocio;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web.UI;

namespace Presentacion
{
    public partial class RegistroResponsable : Page
    {
        protected void btnRegistrar_Click(object sender, EventArgs e)
        {
            RegisterAsyncTask(new PageAsyncTask(RegistrarAsync));
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Login.aspx");
        }

        private async Task RegistrarAsync()
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text) ||
                string.IsNullOrWhiteSpace(txtApellido.Text) ||
                string.IsNullOrWhiteSpace(txtDocumento.Text) ||
                string.IsNullOrWhiteSpace(txtFechaNacimiento.Text) ||
                string.IsNullOrWhiteSpace(txtTelefono.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text) ||
                string.IsNullOrWhiteSpace(txtDomicilio.Text) ||
                string.IsNullOrWhiteSpace(txtClave.Text) ||
                string.IsNullOrWhiteSpace(txtClaveRepetida.Text))
            {
                MostrarResultado("Completá todos los campos del formulario.", false);
                return;
            }

            string documento = txtDocumento.Text.Trim();
            if (documento.Length > 8 || documento.StartsWith("0") ||
                documento.Any(c => c < '0' || c > '9'))
            {
                MostrarResultado("Ingresá un DNI de hasta 8 dígitos, sin puntos y sin empezar con cero.", false);
                return;
            }

            string email = txtEmail.Text.Trim();
            if (!EmailValido(email))
            {
                MostrarResultado("Ingresá un correo electrónico válido.", false);
                return;
            }

            string clave = txtClave.Text;
            if (clave.Length < 8 || clave.Length > 20 ||
                !clave.Any(char.IsUpper) || !clave.Any(char.IsLower) ||
                !clave.Any(char.IsDigit) || !clave.Any(c => char.IsPunctuation(c) || char.IsSymbol(c)))
            {
                MostrarResultado("La contraseña debe tener entre 8 y 20 caracteres e incluir una mayúscula, una minúscula, un número y un símbolo.", false);
                return;
            }

            if (clave != txtClaveRepetida.Text)
            {
                MostrarResultado("Las contraseñas no coinciden.", false);
                return;
            }

            var registro = new
            {
                nombre = txtNombre.Text.Trim(),
                apellido = txtApellido.Text.Trim(),
                documento = documento,
                fechaNacimiento = txtFechaNacimiento.Text,
                telefono = txtTelefono.Text.Trim(),
                email = email,
                domicilio = txtDomicilio.Text.Trim(),
                clave = clave
            };

            try
            {
                var respuesta = await InstiRedApi.PostAsync("/sp/RegistrarResponsable", registro);
                if (respuesta.Estado == HttpStatusCode.Created)
                {
                    MostrarResultado("Responsable registrado correctamente.", true);
                    btnRegistrar.Enabled = false;
                    return;
                }

                if (respuesta.Estado == HttpStatusCode.BadRequest)
                {
                    MostrarResultado(respuesta.Mensaje, false);
                    return;
                }
            }
            catch (InvalidOperationException)
            {
                MostrarResultado("El registro no está disponible. Intentá nuevamente más tarde.", false);
                return;
            }
            catch (HttpRequestException)
            {
                MostrarResultado("No se pudo conectar con la API. Intentá nuevamente más tarde.", false);
                return;
            }
            catch (TaskCanceledException)
            {
                MostrarResultado("La API tardó demasiado en responder. Intentá nuevamente más tarde.", false);
                return;
            }

            MostrarResultado("No se pudo completar el registro. Intentá nuevamente más tarde.", false);
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

        private void MostrarResultado(string mensaje, bool exito)
        {
            pnlResultado.Visible = true;
            pnlResultado.CssClass = exito ? "alert alert-success mb-3" : "alert alert-danger mb-3";
            litResultado.Text = mensaje;
        }

    }
}
