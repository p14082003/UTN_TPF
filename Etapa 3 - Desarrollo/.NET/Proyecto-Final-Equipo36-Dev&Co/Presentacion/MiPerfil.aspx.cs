using Presentacion.Auth;
using System;
using System.Linq;
using System.Web.UI.WebControls;

namespace Presentacion
{
    public partial class MiPerfil : System.Web.UI.Page
    {
        private UsuarioSesion usuario;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Acceso: cualquier usuario logueado (responsable, administrador de institución o Dev&Co)
            usuario = UsuarioSesion.Obtener(Session);
            if (usuario == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            Response.Cache.SetNoStore();
            lblMensaje.Visible = false;

            if (!IsPostBack)
            {
                CargarPerfil();
            }
        }

        // ---------- CARGA ----------

        private void CargarPerfil()
        {
            bool esDevCo = usuario.IdTipoUsuario == UsuarioSesion.DevCo;
            bool esAdmin = usuario.IdTipoUsuario == UsuarioSesion.AdministradorInstitucion;

            // Dev&Co no tiene datos personales ni teléfonos (solo cuenta y contraseña)
            pnlDatosPersonales.Visible = !esDevCo;
            pnlTelefonos.Visible = !esDevCo;
            pnlInstitucion.Visible = esAdmin;

            string nombreCompleto = esDevCo ? "Equipo Dev&Co"
                                  : ((usuario.Nombre ?? "") + " " + (usuario.Apellido ?? "")).Trim();
            litNombreCompleto.Text = nombreCompleto;
            litRol.Text = esDevCo ? "Equipo Dev&Co"
                        : esAdmin ? "Administrador de institución"
                        : "Padre, madre o tutor";
            litEmail.Text = ValorOGuion(usuario.Email);

            if (esDevCo) return;

            // API: GET del perfil del usuario logueado (vista o SP de perfil por idUsuario)
            //   Debe devolver: nombre, apellido, documento, fechaNacimiento, domicilio, teléfonos (id + número)
            //   y, si es administrador, el nombre de la institución. NUNCA la clave.

            // Datos de prueba mientras no esté conectado a la API:
            litNombre.Text = ValorOGuion(usuario.Nombre);
            litApellido.Text = ValorOGuion(usuario.Apellido);
            litDocumento.Text = "35887625";
            litFechaNacimiento.Text = "14/05/1990";
            litDomicilio.Text = "San Martín 1234, Godoy Cruz";
            litInstitucion.Text = "Colegio San Martín";

            CargarTelefonos();
        }

        private void CargarTelefonos()
        {
            // API: teléfonos del dueño (tipoDueno Administrador o Responsable, según el rol)
            // Datos de prueba:
            var telefonos = new[]
            {
                new { Id = 1, Numero = "2613333333" }
            };

            rptTelefonos.DataSource = telefonos;
            rptTelefonos.DataBind();

            // Los botones "Quitar" solo se ven en modo edición
            foreach (RepeaterItem item in rptTelefonos.Items)
            {
                item.FindControl("btnQuitarTelefono").Visible = pnlTelefonosEditar.Visible;
            }

            // Máximo 2 teléfonos: con 2 no se muestra el campo para agregar
            pnlAgregarTelefono.Visible = rptTelefonos.Items.Count < 2;
        }


        // ---------- TELÉFONOS ----------

        protected void btnEditarTelefonos_Click(object sender, EventArgs e)
        {
            pnlTelefonosEditar.Visible = true;
            btnEditarTelefonos.Visible = false;
            CargarTelefonos();
        }

        protected void btnListoTelefonos_Click(object sender, EventArgs e)
        {
            pnlTelefonosEditar.Visible = false;
            btnEditarTelefonos.Visible = true;
            txtTelefono.Text = "";
            CargarTelefonos();
        }

        protected void btnAgregarTelefono_Click(object sender, EventArgs e)
        {
            string telefono = txtTelefono.Text.Trim();

            if (telefono.Length == 0)
            {
                MostrarMensaje("Ingresá el teléfono que querés agregar.", false);
                return;
            }

            if (rptTelefonos.Items.Count >= 2)
            {
                MostrarMensaje("Ya tenés 2 teléfonos. Quitá uno para agregar otro.", false);
                return;
            }

            if (!telefono.Any(char.IsDigit))
            {
                MostrarMensaje("El teléfono debe tener números.", false);
                return;
            }

            // API: POST sp/AgregarTelefono con { idUsuarioActor = usuario.IdUsuario, tipoDueno = "Administrador" o "Responsable",
            //   idDueno = usuario.IdAdministradorInstitucion o usuario.IdResponsable, telefono }
            //   Si da error, mostrar el mensaje con MostrarMensaje(mensaje, false)
            // Después recargar la lista con CargarTelefonos()

            txtTelefono.Text = "";
            MostrarMensaje("Teléfono agregado correctamente.", true);
        }

        protected void rptTelefonos_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "Quitar") return;

            // Mínimo 1 teléfono
            if (rptTelefonos.Items.Count <= 1)
            {
                MostrarMensaje("Tenés que dejar al menos un teléfono. Agregá otro antes de quitar este.", false);
                return;
            }

            // int idTelefono = int.Parse((string)e.CommandArgument);
            // API: POST sp/EliminarTelefono con { idUsuarioActor = usuario.IdUsuario, tipoDueno, idDueno, idTelefono }
            // Después recargar la lista con CargarTelefonos()

            MostrarMensaje("Teléfono quitado correctamente.", true);
        }

        // ---------- CONTRASEÑA ----------

        protected void btnEditarClave_Click(object sender, EventArgs e)
        {
            pnlClaveVer.Visible = false;
            pnlClaveEditar.Visible = true;
            btnEditarClave.Visible = false;
        }

        protected void btnGuardarClave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtClaveActual.Text) ||
                string.IsNullOrWhiteSpace(txtClaveNueva.Text) ||
                string.IsNullOrWhiteSpace(txtClaveRepetida.Text))
            {
                MostrarMensaje("Completá la contraseña actual y la nueva.", false);
                return;
            }

            string clave = txtClaveNueva.Text;
            if (clave.Length < 8 || clave.Length > 20 ||
                !clave.Any(char.IsUpper) || !clave.Any(char.IsLower) ||
                !clave.Any(char.IsDigit) || !clave.Any(c => char.IsPunctuation(c) || char.IsSymbol(c)))
            {
                MostrarMensaje("La contraseña nueva debe tener entre 8 y 20 caracteres e incluir una mayúscula, una minúscula, un número y un símbolo.", false);
                return;
            }

            if (clave != txtClaveRepetida.Text)
            {
                MostrarMensaje("Las contraseñas nuevas no coinciden.", false);
                return;
            }

            if (clave == txtClaveActual.Text)
            {
                MostrarMensaje("La contraseña nueva tiene que ser distinta de la actual.", false);
                return;
            }

            // API: POST sp/CambiarClave con { idUsuarioActor = usuario.IdUsuario, claveActual, claveNueva }
            //   La API verifica que claveActual sea correcta (compara el hash), genera el hash de claveNueva
            //   y llama a sp_Usuario_CambiarClave. Si la actual es incorrecta, mostrar el mensaje con MostrarMensaje(mensaje, false)
            //   y no salir del modo edición

            SalirEdicionClave();
            MostrarMensaje("Contraseña actualizada correctamente.", true);
        }

        protected void btnCancelarClave_Click(object sender, EventArgs e)
        {
            SalirEdicionClave();
        }

        private void SalirEdicionClave()
        {
            txtClaveActual.Text = "";
            txtClaveNueva.Text = "";
            txtClaveRepetida.Text = "";
            pnlClaveEditar.Visible = false;
            pnlClaveVer.Visible = true;
            btnEditarClave.Visible = true;
        }

        // ---------- AUXILIARES ----------

        private static string ValorOGuion(string valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? "—" : valor;
        }

        private void MostrarMensaje(string texto, bool ok)
        {
            lblMensaje.Text = texto;
            lblMensaje.CssClass = ok ? "d-block mb-3 fg-success" : "d-block mb-3 fg-danger";
            lblMensaje.Visible = true;
        }
    }
}
