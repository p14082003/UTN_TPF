using System;
using Negocio;
using System.Collections.Generic;
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
                    btnRegistrar.Enabled = false;
                    bool bienvenidaEnviada = EnviarBienvenida(email, registro.nombre + " " + registro.apellido);
                    MostrarResultado(bienvenidaEnviada
                        ? "Tu cuenta se creó correctamente. Ya podés iniciar sesión."
                        : "Tu cuenta se creó correctamente, pero no pudimos enviar el correo de bienvenida. Ya podés iniciar sesión con tu correo y contraseña; no vuelvas a registrarte.", bienvenidaEnviada);
                    if (bienvenidaEnviada)
                        ClientScript.RegisterStartupScript(this.GetType(), "Redirección", "setTimeout(() => window.location.href='Login.aspx',3000);", true);
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

        private bool EnviarBienvenida(string email, string nombre)
        {
            var bienvenida = new NotificacionEmail
            {
                Destinatario = email,
                NombreDestinatario = nombre.Trim(),
                Asunto = "¡Bienvenido a InstiRed!",
                Titulo = "Tu próximo paso empieza en InstiRed",
                Mensaje = "¡Gracias por sumarte! Tu cuenta de responsable ya está lista para acompañarte en la búsqueda de una institución educativa.",
                Datos = new Dictionary<string, string>
                {
                    { "Tu cuenta", email },
                    { "Encontrá tu institución", "Explorá el mapa y compará propuestas, idiomas, accesibilidad y vacantes." },
                    { "Registrá a tus alumnos", "Agregá a quienes tenés a cargo. Si estudiás vos, también podés registrarte como autogestor." },
                    { "Seguí tus postulaciones", "Presentá la documentación y consultá las respuestas desde Mis postulaciones." }
                },
                Indicaciones = "Ingresá con el correo y la contraseña que elegiste al registrarte. ¡Nos alegra acompañarte en esta nueva etapa!",
                TextoAccion = "Empezar en InstiRed",
                UrlAccion = "https://instired.nummis.com.ar/Login.aspx"
            };
            return bienvenida.IntentarEnviar(ex => Trace.Warn("RegistroResponsable", "La cuenta se creó, pero no se pudo enviar la bienvenida.", ex));
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
            pnlResultado.CssClass = exito ? "alert theme-success mb-3" : "alert theme-danger mb-3";
            litResultado.Text = mensaje;
        }

    }
}
