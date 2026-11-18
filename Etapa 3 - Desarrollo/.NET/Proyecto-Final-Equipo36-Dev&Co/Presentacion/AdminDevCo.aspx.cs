using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Presentacion
{
    public partial class AdminDevCo : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarSolicitudesFalsas(); // Solo cargamos la tabla la primera vez
                MostrarVista("solicitudes");
            }
        }

        // ---------------------------------------------------------------------
        // DATOS DE PRUEBA (hardcodeados). reemplazar por la llamada a la API que se indica en cada uno.
        // ---------------------------------------------------------------------
        private void CargarSolicitudesFalsas()
        {
            // API: traer las solicitudes de vw_Postulaciones_Admin_Institucion filtradas por el estado elegido
            // en ddlFiltroEstado (Pendiente / Confirmado / Rechazado / (FALTA Cancelado - ver con Juli) / Todas). Entra mostrando Pendiente por defecto.
            var lista = new List<object>
            {
                new { IdPostulacion = 12, Fecha = "29/10/2026", NombreInstitucion = "Instituto Nuevo Horizonte", Cuit = "30-71000012-0", Administrador = "Ramiro Villalba", Email = "rvillalba@mail.test", Estado = "Pendiente" },
                new { IdPostulacion = 13, Fecha = "03/11/2026", NombreInstitucion = "Colegio San Martín", Cuit = "30-12345678-9", Administrador = "Julia Peralta", Email = "jperalta@mail.test", Estado = "Pendiente" },
                new { IdPostulacion = 14, Fecha = "09/11/2026", NombreInstitucion = "Academia Oxford (Inglés)", Cuit = "33-44445555-9", Administrador = "Mario Quiroga", Email = "mquiroga@mail.test", Estado = "Pendiente" }
            };

            dgvSolicitudes.DataSource = lista;
            dgvSolicitudes.DataBind();
        }

        private void CargarInstitucionesFalsas()
        {
            // API: traer las instituciones de vw_Instituciones (agregue el cuit a la vista por si tienen que volver a crearlo)
            var lista = new List<object>
            {
                new { IdInstitucion = 1, Nombre = "Jardín de Infantes N° 902 Arco Iris", Cuit = "30-71000001-4", Tipo = "Jardín", Email = "jardin902@instired.test", Estado = "Activa" },
                new { IdInstitucion = 4, Nombre = "Instituto de Idiomas Lingua Norte", Cuit = "30-71000004-9", Tipo = "Instituto de idiomas", Email = "info@linguanorte.instired.test", Estado = "Activa" },
                new { IdInstitucion = 8, Nombre = "Escuela Primaria N° 31 Juana Manso", Cuit = "30-71000008-1", Tipo = "Primaria", Email = "primaria31@instired.test", Estado = "Dada de baja" }
            };

            dgvInstituciones.DataSource = lista;
            dgvInstituciones.DataBind();
        }

        private void CargarAdministradoresFalsos()
        {
            // API: traer los administradores de la institución elegida con su estado (activo / dado de baja).
            // nueva vista creada para esto vw_Administradores_Por_Institucion.
            var lista = new List<object>
            {
                new { IdAdministrador = 4, NombreCompleto = "Gabriela Torres", Documento = "34087532", Email = "gtorres.adm@mail.test", Estado = "Activo", Activo = true },
                new { IdAdministrador = 9, NombreCompleto = "Natalia Ocampo", Documento = "30441275", Email = "nocampo@mail.test", Estado = "Dado de baja", Activo = false }
            };

            dgvAdministradores.DataSource = lista;
            dgvAdministradores.DataBind();
        }

        // ---------------------------------------------------------------------
        // MENÚ Y PANELES
        // ---------------------------------------------------------------------
        private void MostrarVista(string vista)
        {
            pnlSolicitudes.Visible = vista == "solicitudes";
            pnlDetalleSolicitud.Visible = vista == "detalleSolicitud";
            pnlInstituciones.Visible = vista == "instituciones";
            pnlDetalleInstitucion.Visible = vista == "detalleInstitucion";
        }

        private void MostrarMensaje(string texto, bool exito)
        {
            lblMensaje.Text = texto;
            lblMensaje.CssClass = exito ? "fw-bold fg-success" : "fw-bold fg-danger";
            pnlMensaje.Visible = true;
        }

        // Deja la zona de rechazo como nueva. Los controles recuerdan su estado entre clicks,
        // por eso hay que llamarlo al abrir y al cerrar el detalle.
        private void ReiniciarRechazo()
        {
            txtMotivoRechazo.Text = "";
            panelRechazo.Visible = false;
            btnDesplegarRechazo.Visible = true;
            btnAprobar.Visible = true;
        }

        protected void btnMenuSolicitudes_Click(object sender, EventArgs e)
        {
            pnlMensaje.Visible = false;
            CargarSolicitudesFalsas();
            MostrarVista("solicitudes");
        }

        protected void btnMenuInstituciones_Click(object sender, EventArgs e)
        {
            pnlMensaje.Visible = false;
            CargarInstitucionesFalsas();
            MostrarVista("instituciones");
        }

        // ---------------------------------------------------------------------
        // SOLICITUDES
        // ---------------------------------------------------------------------
        protected void ddlFiltroEstado_SelectedIndexChanged(object sender, EventArgs e)
        {
            // API: volver a pedir las solicitudes con el estado elegido (ddlFiltroEstado.SelectedValue)
            CargarSolicitudesFalsas();
        }

        protected void btnVerSolicitud_Click(object sender, EventArgs e)
        {
            pnlMensaje.Visible = false;

            // El botón apretado trae en CommandArgument el id de la solicitud de su fila:
            // int idPostulacion = int.Parse(((LinkButton)sender).CommandArgument);
            // API: con ese id traer el detalle de la solicitud y completar el panel (hoy está escrito a mano en el .aspx).
            // Guardar el id (por ejemplo en ViewState) para usarlo al aprobar o rechazar.

            // API: si la solicitud ya fue resuelta, mostrar su estado y el motivo del rechazo, y ocultar los botones.
            // Con el estado y el motivoRechazo de la solicitud elegida:
            //   bool pendiente = estado == "Pendiente";
            //   pnlAcciones.Visible = pendiente;                 // solo se aprueba o rechaza si está Pendiente
            //   pnlResuelta.Visible = !pendiente;
            //   lblSolEstado.Text = estado == "Confirmado" ? "Aprobada" : "Rechazada";
            //   lblSolEstado.CssClass = estado == "Confirmado" ? "fw-bold fg-success" : "fw-bold fg-danger";
            //   pnlMotivoRechazo.Visible = estado == "Rechazado";
            //   lblMotivoRechazo.Text = motivoRechazo;
            ReiniciarRechazo();
            MostrarVista("detalleSolicitud");
        }

        protected void btnVolverSolicitud_Click(object sender, EventArgs e)
        {
            ReiniciarRechazo();
            CargarSolicitudesFalsas();
            MostrarVista("solicitudes");
        }

        protected void btnDesplegarRechazo_Click(object sender, EventArgs e)
        {
            // Mostramos la caja de texto para escribir el motivo y ocultamos los botones principales
            panelRechazo.Visible = true;
            btnDesplegarRechazo.Visible = false;
            btnAprobar.Visible = false;
        }

        protected void btnCancelarRechazo_Click(object sender, EventArgs e)
        {
            ReiniciarRechazo();
        }

        protected void btnAprobar_Click(object sender, EventArgs e)
        {
            // API: aprobar la solicitud con sp_PostulacionAdmin_Aprobar(idPostulacion, idUsuarioDevCo)
            // idUsuarioDevCo deberia estar en la sesión del login. Si la API devuelve error, mostrar su mensaje con MostrarMensaje(mensaje, false).
            // Después de aprobar: ¿enviar mail de aviso al administrador?

            MostrarMensaje("Solicitud aprobada.", true);

            ReiniciarRechazo();
            CargarSolicitudesFalsas();
            MostrarVista("solicitudes");
        }

        protected void btnConfirmarRechazo_Click(object sender, EventArgs e)
        {
            // El motivo es obligatorio: si está vacío no se llama a la API y se queda en pantalla
            string motivo = txtMotivoRechazo.Text.Trim();

            if (motivo == "")
            {
                MostrarMensaje("El motivo de rechazo es obligatorio.", false);
                return;
            }

            // API: rechazar la solicitud con sp_PostulacionAdmin_Rechazar(idPostulacion, idUsuarioDevCo, motivo)
            // Si la API devuelve error, mostrar su mensaje con MostrarMensaje(mensaje, false).
            // Después de rechazar: ¿enviar mail al administrador con el motivo??

            MostrarMensaje("Solicitud rechazada.", true);

            ReiniciarRechazo();
            CargarSolicitudesFalsas();
            MostrarVista("solicitudes");
        }

        // ---------------------------------------------------------------------
        // INSTITUCIONES
        // ---------------------------------------------------------------------
        protected void btnVerInstitucion_Click(object sender, EventArgs e)
        {
            pnlMensaje.Visible = false;

            // int idInstitucion = int.Parse(((LinkButton)sender).CommandArgument);
            // API: con ese id traer el detalle de la institución (hoy esta hardcodeado en el .aspx)
            // y mostrar pnlBajaInstitucion si está activa o pnlReactivarInstitucion si está dada de baja.
            // Guardar el id para usarlo en la baja, la reactivación y la lista de administradores.

            CargarAdministradoresFalsos();
            MostrarVista("detalleInstitucion");
        }

        protected void btnVolverInstitucion_Click(object sender, EventArgs e)
        {
            CargarInstitucionesFalsas();
            MostrarVista("instituciones");
        }

        protected void btnBajaInstitucion_Click(object sender, EventArgs e)
        {
            // API: dar de baja la institución con sp_Institucion_DarDeBaja(idInstitucion, idUsuarioDevCo)
            // Si la API devuelve error, mostrar su mensaje con MostrarMensaje(mensaje, false).
            // Despues recargar el detalle para que pase a mostrar el botón Reactivar.

            MostrarMensaje("Institución dada de baja.", true);
        }

        protected void btnReactivarInstitucion_Click(object sender, EventArgs e)
        {
            // API: reactivar la institución con sp_Institucion_Reactivar(idInstitucion, idUsuarioDevCo)
            // Si la API devuelve error, mostrar su mensaje con MostrarMensaje(mensaje, false).
            // Despues recargar el detalle para que pase a mostrar el boton para dar de baja.

            MostrarMensaje("Institución reactivada.", true);
        }

        protected void btnBajaAdministrador_Click(object sender, EventArgs e)
        {
            // int idAdministrador = int.Parse(((LinkButton)sender).CommandArgument);
            // API: dar de baja al administrador con sp_AdministradorInstitucion_DarDeBaja(idUsuarioDevCo, idAdministrador)
            // Si la API devuelve error, mostrar su mensaje con MostrarMensaje(mensaje, false).
            // Despues recargar la lista de administradores.

            MostrarMensaje("Administrador dado de baja.", true);
        }

        protected void btnReactivarAdministrador_Click(object sender, EventArgs e)
        {
            // int idAdministrador = int.Parse(((LinkButton)sender).CommandArgument);
            // API: reactivar al administrador con sp_AdministradorInstitucion_Reactivar(idUsuarioDevCo, idAdministrador)
            // Si la API devuelve error, mostrar su mensaje con MostrarMensaje(mensaje, false).
            // Despues recargar la lista de administradores.

            MostrarMensaje("Administrador reactivado.", true);
        }
    }
}
