using Presentacion.Auth;
using System;
using System.Linq;
using System.Net.Mail;

namespace Presentacion
{
    public partial class FormularioAdministradorAdicional : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Acceso: permitir solo administrador de institución
            if (!UsuarioSesion.TieneRol(Session, UsuarioSesion.AdministradorInstitucion))
            {
                Response.Redirect("~/Login.aspx");
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

            // API: POST a sp CrearAdministradorInstitucionAdicional (idUsuarioActor, nombre, apellido, documento, domicilio, fechaNacimiento, clave, email, telefono)
            //   idUsuarioActor: el administrador logueado, debe salir de la sesión. El SP lo usa para saber a qué institución se vincula el nuevo administrador y para verificar que quien lo crea sea administrador activo.
            //   Respuesta ok: debe devolver el idAdministrador generado

            MostrarMensaje("Administrador agregado correctamente.", true);
            LimpiarCampos();
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
            lblMensaje.Text = texto;
            lblMensaje.CssClass = ok ? "d-block mb-3 fg-success" : "d-block mb-3 fg-danger";
            lblMensaje.Visible = true;
        }
    }
}
