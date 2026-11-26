using Negocio;
using Presentacion.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Presentacion
{
    public partial class AdminDevCo : Page
    {
        private const string RutaSolicitudes = "vw/PostulacionesAdminInstitucion";
        private const string RutaInstituciones = "vw/Instituciones";
        private const int Pendiente = 1;
        private const int Confirmado = 3;
        private const int Rechazado = 4;

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            ViewStateUserKey = Session.SessionID;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!UsuarioSesion.TieneRol(Session, UsuarioSesion.DevCo))
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            Response.Cache.SetNoStore();
            if (!IsPostBack)
            {
                MostrarVista("solicitudes");
                Ejecutar(async () =>
                {
                    await CargarSolicitudesAsync();
                    await CargarInstitucionesAsync();
                });
            }
        }

        private async Task CargarSolicitudesAsync()
        {
            string estado = ddlFiltroEstado.SelectedValue;
            string ruta = RutaSolicitudes;
            if (estado != "")
            {
                ruta += "?idEstado=" + estado;
            }

            var solicitudes = await InstiRedApi.GetAsync<List<SolicitudApi>>(ruta);
            dgvSolicitudes.DataSource = solicitudes;
            dgvSolicitudes.DataBind();
            if (estado == "" || estado == Pendiente.ToString())
            {
                litPendientes.Text = solicitudes.Count(s => s.IdEstado == Pendiente).ToString();
            }
        }

        private async Task CargarInstitucionesAsync()
        {
            var instituciones = await InstiRedApi.GetAsync<List<InstitucionApi>>(RutaInstituciones);
            int activas = instituciones.Count(i => i.Estado);
            litActivas.Text = activas.ToString();
            litDadasDeBaja.Text = (instituciones.Count - activas).ToString();
            dgvInstituciones.DataSource = instituciones;
            dgvInstituciones.DataBind();
        }

        private void MostrarVista(string vista)
        {
            pnlSolicitudes.Visible = vista == "solicitudes";
            pnlDetalleSolicitud.Visible = vista == "detalleSolicitud";
            pnlInstituciones.Visible = vista == "instituciones";
            pnlDetalleInstitucion.Visible = vista == "detalleInstitucion";
        }

        private void MostrarMensaje(string texto, bool exito)
        {
            lblMensaje.Text = Server.HtmlEncode(texto);
            lblMensaje.CssClass = exito ? "alert theme-success" : "alert theme-danger";
            pnlMensaje.Visible = true;
        }

        private void Ejecutar(Func<Task> accion)
        {
            pnlMensaje.Visible = false;
            RegisterAsyncTask(new PageAsyncTask(async () =>
            {
                try
                {
                    await accion();
                }
                catch (Exception ex)
                {
                    Trace.Warn("AdminDevCo", "Error al comunicarse con InstiRed.", ex);
                    MostrarMensaje("No se pudo actualizar el panel. Intentá nuevamente.", false);
                }
            }));
        }

        private void ReiniciarRechazo()
        {
            txtMotivoRechazo.Text = "";
            panelRechazo.Visible = false;
            btnDesplegarRechazo.Visible = true;
            btnAprobar.Visible = true;
        }

        protected void btnMenuSolicitudes_Click(object sender, EventArgs e)
        {
            MostrarVista("solicitudes");
            Ejecutar(CargarSolicitudesAsync);
        }

        protected void btnMenuInstituciones_Click(object sender, EventArgs e)
        {
            MostrarVista("instituciones");
            Ejecutar(CargarInstitucionesAsync);
        }

        protected void ddlFiltroEstado_SelectedIndexChanged(object sender, EventArgs e)
        {
            Ejecutar(CargarSolicitudesAsync);
        }

        protected void btnVerSolicitud_Click(object sender, EventArgs e)
        {
            int id = int.Parse(((LinkButton)sender).CommandArgument);
            Ejecutar(() => CargarDetalleSolicitudAsync(id));
        }

        private async Task CargarDetalleSolicitudAsync(int id)
        {
            var solicitud = await InstiRedApi.GetAsync<SolicitudApi>(RutaSolicitudes + "/" + id, permitirNoEncontrado: true);
            if (solicitud == null)
            {
                MostrarMensaje("La solicitud ya no está disponible.", false);
                return;
            }

            ViewState["IdPostulacion"] = solicitud.IdPostulacion;
            litSolInstitucion.Text = solicitud.NombreInstitucion;
            litSolCuit.Text = solicitud.CuitInstitucion;
            litSolFecha.Text = solicitud.Fecha.ToString("dd/MM/yyyy");
            litSolCodigo.Text = solicitud.CodigoSeguimiento;
            litSolAdministrador.Text = solicitud.Administrador;
            litSolDocumento.Text = solicitud.DocAdministrador;
            litSolEmail.Text = solicitud.EmailAdministrador;

            PrepararAdjunto(lnkConstanciaFiscal, id, "constancia-fiscal", solicitud.RutaConstanciaFiscal);
            PrepararAdjunto(lnkHabilitacion, id, "habilitacion", solicitud.RutaHabilitacion);
            PrepararAdjunto(lnkConstanciaLaboral, id, "constancia-laboral", solicitud.RutaConstanciaLaboral);

            ReiniciarRechazo();
            bool pendiente = solicitud.IdEstado == Pendiente;
            pnlAcciones.Visible = pendiente;
            pnlResuelta.Visible = !pendiente;
            lblSolEstado.Text = Server.HtmlEncode(solicitud.EstadoVisible);
            lblSolEstado.CssClass = solicitud.IdEstado == Confirmado ? "fw-bold fg-success" : "fw-bold fg-danger";
            pnlMotivoRechazo.Visible = solicitud.IdEstado == Rechazado;
            lblMotivoRechazo.Text = Server.HtmlEncode(solicitud.MotivoRechazo);
            MostrarVista("detalleSolicitud");
        }

        private void PrepararAdjunto(HyperLink enlace, int id, string tipo, string ruta)
        {
            enlace.Visible = !string.IsNullOrWhiteSpace(ruta);
            enlace.NavigateUrl = "~/DescargarDocumentoAdmin.ashx?id=" + id + "&tipo=" + tipo;
        }

        protected void btnVolverSolicitud_Click(object sender, EventArgs e)
        {
            btnMenuSolicitudes_Click(sender, e);
        }

        protected void btnDesplegarRechazo_Click(object sender, EventArgs e)
        {
            pnlMensaje.Visible = false;
            panelRechazo.Visible = true;
            btnDesplegarRechazo.Visible = false;
            btnAprobar.Visible = false;
        }

        protected void btnCancelarRechazo_Click(object sender, EventArgs e)
        {
            pnlMensaje.Visible = false;
            ReiniciarRechazo();
        }

        protected void btnAprobar_Click(object sender, EventArgs e)
        {
            ResolverSolicitud("sp/AprobarPostulacionAdmin", "", "Solicitud aprobada.");
        }

        protected void btnConfirmarRechazo_Click(object sender, EventArgs e)
        {
            string motivo = txtMotivoRechazo.Text.Trim();
            if (motivo == "" || motivo.Length > 255)
            {
                MostrarMensaje("Ingresá un motivo de rechazo de hasta 255 caracteres.", false);
                return;
            }

            ResolverSolicitud("sp/RechazarPostulacionAdmin", motivo, "Solicitud rechazada.");
        }

        private void ResolverSolicitud(string ruta, string motivo, string mensaje)
        {
            int idPostulacion = (int)ViewState["IdPostulacion"];
            int idUsuarioDevCo = UsuarioSesion.Obtener(Session).IdUsuario;
            object datos = new { idPostulacion, idUsuarioDevCo };
            if (motivo != "")
            {
                datos = new { idPostulacion, idUsuarioDevCo, motivoRechazo = motivo };
            }

            Ejecutar(async () =>
            {
                if (await GuardarAsync(ruta, datos))
                {
                    MostrarVista("solicitudes");
                    await CargarSolicitudesAsync();
                    string estado = ddlFiltroEstado.SelectedValue;
                    if (estado != "" && estado != Pendiente.ToString())
                    {
                        var pendientes = await InstiRedApi.GetAsync<List<SolicitudApi>>(RutaSolicitudes + "?idEstado=" + Pendiente);
                        litPendientes.Text = pendientes.Count.ToString();
                    }
                    MostrarMensaje(mensaje, true);
                }
            });
        }

        protected void btnVerInstitucion_Click(object sender, EventArgs e)
        {
            int id = int.Parse(((LinkButton)sender).CommandArgument);
            Ejecutar(() => CargarDetalleInstitucionAsync(id));
        }

        private async Task CargarDetalleInstitucionAsync(int id)
        {
            var institucion = await InstiRedApi.GetAsync<InstitucionApi>(RutaInstituciones + "/" + id, permitirNoEncontrado: true);
            if (institucion == null)
            {
                MostrarMensaje("La institución ya no está disponible.", false);
                return;
            }

            var administradores = await ListarAdministradoresAsync(id);
            ViewState["IdInstitucion"] = institucion.IdInstitucion;
            litInstNombre.Text = institucion.Nombre;
            litInstCuit.Text = institucion.Cuit;
            litInstTipo.Text = institucion.TipoInstitucion;
            litInstEmail.Text = institucion.Email;
            lblInstEstado.Text = institucion.EstadoVisible;
            lblInstEstado.CssClass = institucion.Estado ? "fw-bold fg-success" : "fw-bold fg-danger";
            pnlBajaInstitucion.Visible = institucion.Estado;
            pnlReactivarInstitucion.Visible = !institucion.Estado;

            dgvAdministradores.DataSource = administradores;
            dgvAdministradores.DataBind();
            MostrarVista("detalleInstitucion");
        }

        private Task<List<AdministradorApi>> ListarAdministradoresAsync(int idInstitucion)
        {
            return InstiRedApi.GetAsync<List<AdministradorApi>>("vw/AdministradoresPorInstitucion?idInstitucion=" + idInstitucion);
        }

        protected void btnVolverInstitucion_Click(object sender, EventArgs e)
        {
            btnMenuInstituciones_Click(sender, e);
        }

        protected void btnBajaInstitucion_Click(object sender, EventArgs e)
        {
            CambiarEstadoInstitucion("sp/DarDeBajaInstitucion", "Institución dada de baja.");
        }

        protected void btnReactivarInstitucion_Click(object sender, EventArgs e)
        {
            CambiarEstadoInstitucion("sp/ReactivarInstitucion", "Institución reactivada.");
        }

        private void CambiarEstadoInstitucion(string ruta, string mensaje)
        {
            int idInstitucion = (int)ViewState["IdInstitucion"];
            int idUsuarioDevCo = UsuarioSesion.Obtener(Session).IdUsuario;
            Ejecutar(async () =>
            {
                if (await GuardarAsync(ruta, new { idInstitucion, idUsuarioDevCo }))
                {
                    await CargarDetalleInstitucionAsync(idInstitucion);
                    await CargarInstitucionesAsync();
                    MostrarMensaje(mensaje, true);
                }
            });
        }

        protected void btnBajaAdministrador_Click(object sender, EventArgs e)
        {
            CambiarEstadoAdministrador(sender, darDeBaja: true);
        }

        protected void btnReactivarAdministrador_Click(object sender, EventArgs e)
        {
            CambiarEstadoAdministrador(sender, darDeBaja: false);
        }

        private void CambiarEstadoAdministrador(object sender, bool darDeBaja)
        {
            int idAdministrador = int.Parse(((LinkButton)sender).CommandArgument);
            int idInstitucion = (int)ViewState["IdInstitucion"];
            int idUsuarioActor = UsuarioSesion.Obtener(Session).IdUsuario;
            Ejecutar(async () =>
            {
                if (darDeBaja)
                {
                    var administradores = await ListarAdministradoresAsync(idInstitucion);
                    var activos = administradores.Where(a => a.UsuarioActivo).ToList();
                    if (activos.Count == 1 && activos[0].IdAdministrador == idAdministrador)
                    {
                        MostrarMensaje("No se puede dar de baja al último administrador activo de la institución.", false);
                        return;
                    }
                }

                string ruta = darDeBaja ? "sp/DarDeBajaAdministradorInstitucion" : "sp/ReactivarAdministradorInstitucion";
                if (await GuardarAsync(ruta, new { idAdministrador, idUsuarioActor }))
                {
                    await CargarDetalleInstitucionAsync(idInstitucion);
                    MostrarMensaje(darDeBaja ? "Administrador dado de baja." : "Administrador reactivado.", true);
                }
            });
        }

        private async Task<bool> GuardarAsync(string ruta, object datos)
        {
            var respuesta = await InstiRedApi.PostAsync(ruta, datos);
            if (respuesta.Estado == HttpStatusCode.OK)
            {
                return true;
            }

            MostrarMensaje(respuesta.Mensaje ?? "No se pudo completar la operación.", false);
            return false;
        }

        private class SolicitudApi
        {
            public int IdPostulacion { get; set; }
            public DateTime Fecha { get; set; }
            public int IdEstado { get; set; }
            public string Estado { get; set; }
            public string NombreAdministrador { get; set; }
            public string ApellidoAdministrador { get; set; }
            public string DocAdministrador { get; set; }
            public string EmailAdministrador { get; set; }
            public string NombreInstitucion { get; set; }
            public string CuitInstitucion { get; set; }
            public string CodigoSeguimiento { get; set; }
            public string RutaConstanciaFiscal { get; set; }
            public string RutaHabilitacion { get; set; }
            public string RutaConstanciaLaboral { get; set; }
            public string MotivoRechazo { get; set; }
            public string Administrador => (NombreAdministrador + " " + ApellidoAdministrador).Trim();
            public string EstadoVisible => IdEstado == Confirmado ? "Aprobada" : Estado;
        }

        private class InstitucionApi
        {
            public int IdInstitucion { get; set; }
            public string Nombre { get; set; }
            public string Cuit { get; set; }
            public string TipoInstitucion { get; set; }
            public string Email { get; set; }
            public bool Estado { get; set; }
            public string EstadoVisible => Estado ? "Activa" : "Dada de baja";
        }

        private class AdministradorApi
        {
            public int IdAdministrador { get; set; }
            public string NombreAdministrador { get; set; }
            public string ApellidoAdministrador { get; set; }
            public string DocAdministrador { get; set; }
            public string EmailAdministrador { get; set; }
            public bool UsuarioActivo { get; set; }
            public string NombreCompleto => (NombreAdministrador + " " + ApellidoAdministrador).Trim();
            public string EstadoVisible => UsuarioActivo ? "Activo" : "Dado de baja";
        }
    }
}
