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
    public partial class GestionarPostulacionAlumno : Page
    {
        private const int Pendiente = 1;
        private const int Aceptada = 2;
        private const int Rechazada = 4;
        private UsuarioSesion usuario;

        private int IdPostulacionSeleccionada
        {
            get { return (int?)ViewState["IdPostulacionSeleccionada"] ?? 0; }
            set { ViewState["IdPostulacionSeleccionada"] = value; }
        }

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

            Response.Cache.SetNoStore();
            if (usuario.IdInstitucion.GetValueOrDefault() <= 0)
            {
                pnlDashboard.Visible = false;
                pnlListado.Visible = false;
                pnlDetalle.Visible = false;
                MostrarMensaje("Tu sesión no tiene una institución asociada. Volvé a iniciar sesión.", false);
                return;
            }

            if (!IsPostBack)
            {
                RegisterAsyncTask(new PageAsyncTask(() => CargarListadoAsync()));
            }
        }

        protected void btnVerDetalle_Click(object sender, EventArgs e)
        {
            var boton = (LinkButton)sender;
            if (int.TryParse(boton.CommandArgument, out int id) && id > 0)
            {
                RegisterAsyncTask(new PageAsyncTask(() => CargarDetalleAsync(id)));
            }
        }

        protected void btnCerrarDetalle_Click(object sender, EventArgs e)
        {
            RegisterAsyncTask(new PageAsyncTask(() => CargarListadoAsync()));
        }

        protected void btnDesplegarRechazo_Click(object sender, EventArgs e)
        {
            panelRechazo.Visible = true;
        }

        protected void btnAceptar_Click(object sender, EventArgs e)
        {
            RegisterAsyncTask(new PageAsyncTask(() => ResolverPostulacionAsync(true)));
        }

        protected void btnConfirmarRechazo_Click(object sender, EventArgs e)
        {
            string motivo = txtMotivoRechazo.Text.Trim();
            if (motivo == "" || motivo.Length > 255)
            {
                MostrarMensaje("Ingresá un motivo de rechazo de hasta 255 caracteres.", false);
                return;
            }

            RegisterAsyncTask(new PageAsyncTask(() => ResolverPostulacionAsync(false, motivo)));
        }

        private async Task CargarListadoAsync(string mensajeExito = null)
        {
            IdPostulacionSeleccionada = 0;
            pnlDetalle.Visible = false;
            panelRechazo.Visible = false;
            pnlMensaje.Visible = false;

            if (usuario.IdInstitucion.GetValueOrDefault() <= 0)
            {
                pnlDashboard.Visible = false;
                pnlListado.Visible = false;
                MostrarMensaje("Tu sesión no tiene una institución asociada.", false);
                return;
            }

            try
            {
                var postulaciones = await InstiRedApi.GetAsync<List<PostulacionApi>>(
                    "vw/PostulacionesAlumnos?idInstitucion=" + usuario.IdInstitucion.Value);
                var propias = postulaciones.Where(p => p.IdInstitucion == usuario.IdInstitucion).ToList();
                var pendientes = propias.Where(p => p.IdEstado == Pendiente)
                    .OrderByDescending(p => p.Fecha)
                    .ThenByDescending(p => p.IdPostulacion)
                    .ToList();

                lblPendientes.Text = pendientes.Count.ToString();
                lblAceptadas.Text = propias.Count(p => p.IdEstado == Aceptada).ToString();
                lblRechazadas.Text = propias.Count(p => p.IdEstado == Rechazada).ToString();
                pnlDashboard.Visible = true;
                pnlListado.Visible = true;
                dgvPostulaciones.DataSource = pendientes;
                dgvPostulaciones.DataBind();

                if (mensajeExito != null)
                {
                    MostrarMensaje(mensajeExito, true);
                }
            }
            catch (Exception ex)
            {
                Trace.Warn("GestionarPostulacionAlumno", "Error al cargar el listado.", ex);
                pnlDashboard.Visible = false;
                pnlListado.Visible = false;
                string mensaje = "No se pudo cargar el listado de postulaciones.";
                if (mensajeExito != null)
                {
                    mensaje = mensajeExito + " No se pudo actualizar el listado.";
                }
                MostrarMensaje(mensaje, false);
            }
        }

        private async Task CargarDetalleAsync(int id)
        {
            pnlMensaje.Visible = false;
            try
            {
                var postulacion = await ObtenerPostulacionPendienteAsync(id);
                if (postulacion == null)
                {
                    MostrarMensaje("La postulación ya no está disponible para revisar.", false);
                    return;
                }

                var curso = await InstiRedApi.GetAsync<CursoApi>("ent/Cursos/" + postulacion.IdCurso);
                var documentos = await InstiRedApi.GetAsync<List<InstiRedApi.DocumentoPostulacion>>(
                    "ent/DocumentosPostulacion?idPostulacionAlumnos=" + id);
                var adjuntos = documentos.Where(d => d.IdPostulacionAlumnos == id).ToList();

                int vacantes = Math.Max(0, curso.Capacidad - curso.Inscriptos);
                bool puedeResolver = usuario.IdAdministradorInstitucion.GetValueOrDefault() > 0;

                IdPostulacionSeleccionada = id;
                litCodigoSeguimiento.Text = postulacion.CodigoSeguimiento ?? "—";
                litAlumno.Text = postulacion.Alumno;
                litCurso.Text = postulacion.Curso;
                litDniAlumno.Text = postulacion.DocAlumno ?? "—";
                litFechaNacimientoAlumno.Text = postulacion.FechaNacimientoAlumno?.ToString("dd/MM/yyyy") ?? "—";
                litTutor.Text = postulacion.NombreTutor;
                litRolResponsable.Text = postulacion.RolResponsable ?? "—";
                litCorreoTutor.Text = postulacion.EmailResponsable ?? "—";
                litTelefonosResponsable.Text = postulacion.TelefonosResponsable ?? "—";

                pnlDetalle.Visible = true;
                pnlSinDocumentos.Visible = adjuntos.Count == 0;
                rptDocumentos.DataSource = adjuntos;
                rptDocumentos.DataBind();

                lblVacantes.Text = vacantes.ToString();
                pnlConVacantes.Visible = vacantes > 0;
                pnlSinVacantes.Visible = vacantes == 0;
                btnAceptar.Enabled = puedeResolver && vacantes > 0;
                btnDesplegarRechazo.Enabled = puedeResolver;
                btnConfirmarRechazo.Enabled = puedeResolver;
                txtMotivoRechazo.Text = "";
                panelRechazo.Visible = false;

                pnlDashboard.Visible = false;
                pnlListado.Visible = false;

                if (!puedeResolver)
                {
                    MostrarMensaje("Volvé a iniciar sesión para aceptar o rechazar postulaciones.", false);
                }
            }
            catch (Exception ex)
            {
                Trace.Warn("GestionarPostulacionAlumno", "Error al cargar el detalle.", ex);
                pnlDetalle.Visible = false;
                MostrarMensaje("No se pudo cargar el detalle de la postulación.", false);
            }
        }

        private async Task<PostulacionApi> ObtenerPostulacionPendienteAsync(int id)
        {
            if (id <= 0 || usuario.IdInstitucion.GetValueOrDefault() <= 0)
            {
                return null;
            }

            var postulacion = await InstiRedApi.GetAsync<PostulacionApi>(
                "vw/PostulacionesAlumnos/" + id, permitirNoEncontrado: true);
            if (postulacion == null || postulacion.IdInstitucion != usuario.IdInstitucion || postulacion.IdEstado != Pendiente)
            {
                return null;
            }

            return postulacion;
        }

        private async Task ResolverPostulacionAsync(bool aceptar, string motivo = null)
        {
            int idAdministradorInstitucion = usuario.IdAdministradorInstitucion.GetValueOrDefault();
            if (idAdministradorInstitucion <= 0)
            {
                MostrarMensaje("Volvé a iniciar sesión para aceptar o rechazar postulaciones.", false);
                return;
            }

            try
            {
                int idPostulacion = IdPostulacionSeleccionada;
                if (await ObtenerPostulacionPendienteAsync(idPostulacion) == null)
                {
                    await CargarListadoAsync();
                    MostrarMensaje("La postulación ya no está disponible para revisar.", false);
                    return;
                }

                InstiRedApi.Respuesta respuesta;
                if (aceptar)
                {
                    respuesta = await InstiRedApi.PostAsync("sp/AceptarPostulacionAlumno",
                        new { idPostulacion, idAdministradorInstitucion });
                }
                else
                {
                    respuesta = await InstiRedApi.PostAsync("sp/RechazarPostulacionAlumno",
                        new { idPostulacion, idAdministradorInstitucion, motivoRechazo = motivo });
                }

                if (respuesta.Estado == HttpStatusCode.OK)
                {
                    await CargarListadoAsync(aceptar ? "Postulación aceptada." : "Postulación rechazada.");
                    return;
                }

                string mensaje = "No se pudo completar la operación. Intentá nuevamente.";
                if (respuesta.Estado == HttpStatusCode.BadRequest)
                {
                    mensaje = respuesta.Mensaje;
                }
                MostrarMensaje(mensaje, false);
            }
            catch (Exception ex)
            {
                Trace.Warn("GestionarPostulacionAlumno", "Error al resolver la postulación.", ex);
                MostrarMensaje("No se pudo completar la operación. Revisá el estado antes de reintentar.", false);
            }
        }

        private void MostrarMensaje(string texto, bool exito)
        {
            pnlMensaje.Visible = true;
            pnlMensaje.CssClass = exito ? "alert alert-success mb-4" : "alert alert-danger mb-4";
            litMensaje.Text = Server.HtmlEncode(texto);
        }

        private class PostulacionApi
        {
            public int IdPostulacion { get; set; }
            public int IdInstitucion { get; set; }
            public int IdEstado { get; set; }
            public int IdCurso { get; set; }
            public DateTime Fecha { get; set; }
            public string CodigoSeguimiento { get; set; }
            public string NombreAlumno { get; set; }
            public string ApellidoAlumno { get; set; }
            public string DocAlumno { get; set; }
            public DateTime? FechaNacimientoAlumno { get; set; }
            public string NombreResponsable { get; set; }
            public string ApellidoResponsable { get; set; }
            public string EmailResponsable { get; set; }
            public string TelefonosResponsable { get; set; }
            public string RolResponsable { get; set; }
            public string Nivel { get; set; }
            public int? Anio { get; set; }
            public string Division { get; set; }
            public string Turno { get; set; }

            public string Alumno => (NombreAlumno + " " + ApellidoAlumno).Trim();

            public string NombreTutor
            {
                get
                {
                    string nombre = (NombreResponsable + " " + ApellidoResponsable).Trim();
                    return nombre == "" ? "—" : nombre;
                }
            }

            public string Curso
            {
                get
                {
                    string anio = Anio.HasValue ? Anio.Value + "° año" : "";
                    var partes = new[] { Nivel, anio, Division, Turno };
                    return string.Join(" · ", partes.Where(p => !string.IsNullOrWhiteSpace(p)));
                }
            }
        }

        private class CursoApi
        {
            public int Capacidad { get; set; }
            public int Inscriptos { get; set; }
        }
    }
}
