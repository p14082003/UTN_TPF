using Presentacion.Auth;
using System;
using System.Linq;
using System.Web.UI.WebControls;

namespace Presentacion
{
    public partial class GestionarAdministradores : System.Web.UI.Page
    {
        private UsuarioSesion usuario;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Acceso: permitir solo administrador de institución
            if (!UsuarioSesion.TieneRol(Session, UsuarioSesion.AdministradorInstitucion))
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            Response.Cache.SetNoStore();

            lblMensaje.Visible = false;

            if (!IsPostBack)
            {
                CargarAdministradores();
            }
        }

        private void CargarAdministradores()
        {
            // API: GET a vw/AdministradoresPorInstitucion y quedarse con los de la institución del usuario logueado
            //   Campos de la vista: idAdministrador, idInstitucion, nombreAdministrador, apellidoAdministrador, docAdministrador, emailAdministrador, usuarioActivo
            //   Estado = usuarioActivo ? "Activo" : "Dado de baja"
            //   EsPropio = true si es el administrador logueado (para no mostrarle el botón "Dar de baja" en su propia fila)

            // Datos de prueba mientras no esté conectado a la API:
            var lista = new[]
            {
                new { IdAdministrador = 4, NombreCompleto = "Gabriela Torres", Documento = "34087532", Email = "gtorres.adm@mail.test", Estado = "Activo", Activo = true, EsPropio = false },
                new { IdAdministrador = 9, NombreCompleto = "Natalia Ocampo", Documento = "30441275", Email = "nocampo@mail.test", Estado = "Dado de baja", Activo = false, EsPropio = false }
            };

            dgvAdministradores.DataSource = lista;
            dgvAdministradores.DataBind();
        }
        protected void btnAgregarAdministrador_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/FormularioAdministradorAdicional.aspx");
        }

        protected void btnBajaAdministrador_Click(object sender, EventArgs e)
        {
            // int idAdministrador = int.Parse(((LinkButton)sender).CommandArgument);
            // API: POST sp/DarDeBajaAdministradorInstitucion con { idUsuarioActor = usuario.IdUsuario, idAdministrador }

            // Después recargar la lista con CargarAdministradores()

            MostrarMensaje("Administrador dado de baja.", true);
        }

        protected void btnReactivarAdministrador_Click(object sender, EventArgs e)
        {
            // int idAdministrador = int.Parse(((LinkButton)sender).CommandArgument);
            // API: POST sp/ReactivarAdministradorInstitucion con { idUsuarioActor = usuario.IdUsuario, idAdministrador }

            // Después recargar la lista con CargarAdministradores()

            MostrarMensaje("Administrador reactivado.", true);
        }

        private void MostrarMensaje(string texto, bool ok)
        {
            lblMensaje.Text = texto;
            lblMensaje.CssClass = ok ? "d-block mb-3 fg-success" : "d-block mb-3 fg-danger";
            lblMensaje.Visible = true;
        }
    }
}
