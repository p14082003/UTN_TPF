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
        private const string BotonActivo = "btn btn-indigo fw-bold rounded-pill px-4";
        private const string BotonInactivo = "btn btn-outline-lavanda fw-bold rounded-pill px-4";
        private string mensajeOperacionGuardada;

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

        // Carga todas las solicitudes una sola vez: las cuenta para las tarjetas y filtra la grilla según el desplegable.
        private async Task CargarSolicitudesAsync()
        {
            var solicitudes = await InstiRedApi.GetAsync<List<SolicitudApi>>(RutaSolicitudes);
            litSolicitudesPendientes.Text = solicitudes.Count(s => s.IdEstado == Pendiente).ToString();
            litSolicitudesAprobadas.Text = solicitudes.Count(s => s.IdEstado == Confirmado).ToString();
            litSolicitudesRechazadas.Text = solicitudes.Count(s => s.IdEstado == Rechazado).ToString();

            string estado = ddlFiltroEstado.SelectedValue;
            dgvSolicitudes.DataSource = estado == "" ? solicitudes : solicitudes.Where(s => s.IdEstado == int.Parse(estado)).ToList();
            dgvSolicitudes.DataBind();
        }

        // Igual que las solicitudes: trae todas las instituciones (activas y dadas de baja) y filtra.
        private async Task CargarInstitucionesAsync()
        {
            var instituciones = await InstiRedApi.GetAsync<List<InstitucionApi>>(RutaInstituciones);
            int activas = instituciones.Count(i => i.Estado);
            litInstitucionesActivas.Text = activas.ToString();
            litInstitucionesBaja.Text = (instituciones.Count - activas).ToString();
            litInstitucionesTotal.Text = instituciones.Count.ToString();

            string estado = ddlFiltroInstituciones.SelectedValue;
            dgvInstituciones.DataSource = estado == "" ? instituciones : instituciones.Where(i => i.Estado == (estado == "1")).ToList();
            dgvInstituciones.DataBind();
        }

        // La pestaña activa también incluye sus paneles de detalle.
        private void MostrarVista(string vista)
        {
            pnlSolicitudes.Visible = vista == "solicitudes"; 
            pnlDetalleSolicitud.Visible = vista == "detalleSolicitud";
            pnlInstituciones.Visible = vista == "instituciones";
            pnlDetalleInstitucion.Visible = vista == "detalleInstitucion";

            bool enSolicitudes = vista == "solicitudes" || vista == "detalleSolicitud";
            //enSolicitudes Guarda un true o false. Vale true si estás en el listado de solicitudes o en el detalle de una solicitud o false en cualquier otro caso (Instituciones o en su detalle)
            btnMenuSolicitudes.CssClass = enSolicitudes ? BotonActivo : BotonInactivo;
            btnMenuInstituciones.CssClass = enSolicitudes ? BotonInactivo : BotonActivo;
            pnlTarjetasSolicitudes.Visible = enSolicitudes;
            pnlTarjetasInstituciones.Visible = !enSolicitudes;
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
                    MostrarMensaje(mensajeOperacionGuardada == null
                        ? "No se pudo actualizar el panel. Intentá nuevamente."
                        : mensajeOperacionGuardada + " No pudimos actualizar el panel. Volvé a abrirlo para consultar el estado; no hace falta repetir la operación.", false);
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
            dgvSolicitudes.PageIndex = 0;
            Ejecutar(CargarSolicitudesAsync);
        }

        protected void ddlFiltroInstituciones_SelectedIndexChanged(object sender, EventArgs e)
        {
            dgvInstituciones.PageIndex = 0;
            Ejecutar(CargarInstitucionesAsync);
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
                    mensajeOperacionGuardada = mensaje;
                    bool correoEnviado = await NotificarSolicitudAsync(idPostulacion, motivo);
                    mensajeOperacionGuardada = MensajeConNotificacion(mensaje, correoEnviado);
                    dgvSolicitudes.PageIndex = 0;
                    MostrarVista("solicitudes");
                    await CargarSolicitudesAsync();
                    MostrarMensaje(mensajeOperacionGuardada, correoEnviado);
                }
            });
        }

        protected void btnVerInstitucion_Click(object sender, EventArgs e)
        {
            int id = int.Parse(((LinkButton)sender).CommandArgument);
            dgvAdministradores.PageIndex = 0;
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
            lnkVerPerfil.Visible = institucion.Estado;
            lnkVerPerfil.NavigateUrl = "~/DetalleInstitucion.aspx?id=" + institucion.IdInstitucion;

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
                    mensajeOperacionGuardada = mensaje;
                    bool correoEnviado = await NotificarEstadoInstitucionAsync(idInstitucion, ruta == "sp/ReactivarInstitucion");
                    mensajeOperacionGuardada = MensajeConNotificacion(mensaje, correoEnviado);
                    await CargarDetalleInstitucionAsync(idInstitucion);
                    dgvInstituciones.PageIndex = 0;
                    await CargarInstitucionesAsync();
                    MostrarMensaje(mensajeOperacionGuardada, correoEnviado);
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
                var administradores = await ListarAdministradoresAsync(idInstitucion);
                var administrador = administradores.FirstOrDefault(a => a.IdAdministrador == idAdministrador);
                if (administrador == null)
                {
                    MostrarMensaje("El administrador ya no está disponible en esta institución.", false);
                    return;
                }
                if (darDeBaja)
                {
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
                    string mensaje = darDeBaja ? "Administrador dado de baja." : "Administrador reactivado.";
                    mensajeOperacionGuardada = mensaje;
                    bool correoEnviado = NotificarEstadoAdministrador(administrador, !darDeBaja);
                    mensajeOperacionGuardada = MensajeConNotificacion(mensaje, correoEnviado);
                    await CargarDetalleInstitucionAsync(idInstitucion);
                    MostrarMensaje(mensajeOperacionGuardada, correoEnviado);
                }
            });
        }

        private static string MensajeConNotificacion(string mensaje, bool correoEnviado)
        {
            return correoEnviado ? mensaje : mensaje + " El cambio quedó guardado, pero no pudimos enviar todos los correos de aviso. No repitas la operación.";
        }

        private bool EnviarNotificacion(NotificacionEmail correo)
        {
            return correo.IntentarEnviar(ex => Trace.Warn("AdminDevCo", "Error al enviar un correo de notificación.", ex));
        }

        private async Task<bool> NotificarSolicitudAsync(int idPostulacion, string motivo)
        {
            try
            {
                var solicitud = await InstiRedApi.GetAsync<SolicitudApi>(RutaSolicitudes + "/" + idPostulacion);
                bool aprobada = string.IsNullOrWhiteSpace(motivo);
                return EnviarNotificacion(new NotificacionEmail
                {
                    Destinatario = solicitud.EmailAdministrador,
                    NombreDestinatario = solicitud.Administrador,
                    Asunto = aprobada ? "Solicitud de administrador aprobada - InstiRed" : "Solicitud de administrador rechazada - InstiRed",
                    Titulo = aprobada ? "Tu solicitud fue aprobada" : "Tu solicitud fue rechazada",
                    Mensaje = aprobada ? "DevCo aprobó tu solicitud de administrador de institución. Tu cuenta quedó habilitada."
                        : "DevCo revisó tu solicitud de administrador de institución y la rechazó por el motivo indicado a continuación.",
                    Datos = new Dictionary<string, string>
                    {
                        { "Institución", solicitud.NombreInstitucion },
                        { "Código de seguimiento", solicitud.CodigoSeguimiento },
                        { "Motivo del rechazo", motivo }
                    },
                    Indicaciones = aprobada ? "Ingresá a InstiRed con el correo y la contraseña de tu registro para completar el alta de tu institución."
                        : "Si querés volver a postularte, enviá una solicitud nueva con la documentación correspondiente."
                });
            }
            catch (Exception ex)
            {
                Trace.Warn("AdminDevCo", "No se pudieron obtener los datos para notificar la resolución de la solicitud.", ex);
                return false;
            }
        }

        private bool NotificarEstadoAdministrador(AdministradorApi administrador, bool activo)
        {
            return EnviarNotificacion(new NotificacionEmail
            {
                Destinatario = administrador.EmailAdministrador,
                NombreDestinatario = administrador.NombreCompleto,
                Asunto = activo ? "Administrador reactivado - InstiRed" : "Baja de administrador - InstiRed",
                Titulo = activo ? "Tu cuenta fue reactivada" : "Tu cuenta fue dada de baja",
                Mensaje = activo ? "DevCo reactivó tu cuenta de administrador de institución."
                    : "DevCo dio de baja tu cuenta de administrador de institución. Ya no podrás iniciar sesión con esta cuenta.",
                Indicaciones = activo ? "Podés ingresar con tus credenciales habituales siempre que tu institución esté activa."
                    : "Para consultar sobre la baja de tu cuenta, contactate con DevCo."
            });
        }

        private async Task<bool> NotificarEstadoInstitucionAsync(int idInstitucion, bool activa)
        {
            try
            {
                var institucion = await InstiRedApi.GetAsync<InstitucionApi>(RutaInstituciones + "/" + idInstitucion);
                var administradores = await ListarAdministradoresAsync(idInstitucion);
                var destinatarios = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                if (!string.IsNullOrWhiteSpace(institucion.Email))
                    destinatarios[institucion.Email.Trim()] = institucion.Nombre;
                foreach (var administrador in administradores.Where(a => a.UsuarioActivo && !string.IsNullOrWhiteSpace(a.EmailAdministrador)))
                    destinatarios[administrador.EmailAdministrador.Trim()] = administrador.NombreCompleto;

                if (destinatarios.Count == 0)
                    throw new InvalidOperationException("La institución no tiene correos de contacto ni administradores activos con email.");

                bool enviados = true;
                foreach (var destinatario in destinatarios)
                {
                    if (!EnviarNotificacion(new NotificacionEmail
                    {
                        Destinatario = destinatario.Key,
                        NombreDestinatario = destinatario.Value,
                        Asunto = activa ? "Institución reactivada - InstiRed" : "Baja de institución - InstiRed",
                        Titulo = activa ? "La institución fue reactivada" : "La institución fue dada de baja",
                        Mensaje = activa ? "DevCo reactivó la institución. Vuelve a estar disponible en la búsqueda y puede gestionar su oferta educativa."
                            : "DevCo dio de baja la institución. Dejó de aparecer en la búsqueda y sus administradores no podrán iniciar sesión mientras permanezca inactiva.",
                        Datos = new Dictionary<string, string> { { "Institución", institucion.Nombre }, { "CUIT", institucion.Cuit } },
                        Indicaciones = activa ? "Los administradores con cuenta activa pueden volver a ingresar. Las postulaciones rechazadas por la baja no se restauran; las familias deben postularse nuevamente."
                            : "Las postulaciones pendientes y aceptadas quedaron rechazadas. Las confirmadas se conservan. Para consultar sobre la baja, contactate con DevCo."
                    }))
                        enviados = false;
                }
                return enviados;
            }
            catch (Exception ex)
            {
                Trace.Warn("AdminDevCo", "No se pudieron completar los avisos del cambio de estado de la institución.", ex);
                return false;
            }
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

        protected void dgvSolicitudes_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            dgvSolicitudes.PageIndex = e.NewPageIndex;
            Ejecutar(CargarSolicitudesAsync);
        }

        protected void dgvInstituciones_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            dgvInstituciones.PageIndex = e.NewPageIndex;
            Ejecutar(CargarInstitucionesAsync);
        }

        protected void dgvAdministradores_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            dgvAdministradores.PageIndex = e.NewPageIndex;
            int idInstitucion = (int)ViewState["IdInstitucion"];
            Ejecutar(() => CargarDetalleInstitucionAsync(idInstitucion));
        }
    }
}
