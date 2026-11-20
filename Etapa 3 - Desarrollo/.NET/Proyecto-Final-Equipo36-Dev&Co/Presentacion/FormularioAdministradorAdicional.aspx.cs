using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web.UI;
using Negocio;
using Presentacion.Auth;

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
                string.IsNullOrWhiteSpace(txtDomicilio.Text) ||
                string.IsNullOrWhiteSpace(txtClave.Text) ||
                string.IsNullOrWhiteSpace(txtClaveRepetida.Text))
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

            string clave = txtClave.Text;
            if (clave.Length < 8 || clave.Length > 20 ||
                !clave.Any(char.IsUpper) || !clave.Any(char.IsLower) ||
                !clave.Any(char.IsDigit) || !clave.Any(c => char.IsPunctuation(c) || char.IsSymbol(c)))
            {
                MostrarMensaje("La contraseña debe tener entre 8 y 20 caracteres e incluir una mayúscula, una minúscula, un número y un símbolo.", false);
                return;
            }

            if (clave != txtClaveRepetida.Text)
            {
                MostrarMensaje("Las contraseñas no coinciden.", false);
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
                clave = txtClave.Text,
                email = txtEmail.Text.Trim(),
                telefono = txtTelefono.Text.Trim()
            };

            try
            {
                var respuesta = await InstiRedApi.PostAsync("sp/CrearAdministradorInstitucionAdicional", registro);
                if (respuesta.Estado == HttpStatusCode.Created)
                {
                    LimpiarCampos();
                    MostrarMensaje("Administrador agregado correctamente. Ya puede iniciar sesión con su correo y contraseña.", true);
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
            Response.Redirect("~/GestionarPostulacionAlumno.aspx");
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
            txtClave.Text = "";
            txtClaveRepetida.Text = "";
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
            lblMensaje.CssClass = ok ? "d-block mb-3 fg-success" : "d-block mb-3 fg-danger";
            lblMensaje.Visible = true;
        }
    }
}
