using Dominio;
using Negocio;
using Presentacion.Auth;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

namespace Presentacion
{
    public partial class GestionarPostulacionAlumno : Page
    {
        private const int Pendiente = 1;
        private const int Aceptada = 2;
        private const int Confirmada = 3;
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
            if (usuario == null || usuario.IdTipoUsuario != UsuarioSesion.AdministradorInstitucion ||
                usuario.IdAdministradorInstitucion.GetValueOrDefault() <= 0)
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
            if (usuario.IdInstitucion.GetValueOrDefault() <= 0)
            {
                Response.Redirect("~/FormularioInstitucion.aspx");
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

        protected void ddlEstado_SelectedIndexChanged(object sender, EventArgs e)
        {
            dgvPostulaciones.PageIndex = 0;
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
                int estado;
                if (!int.TryParse(ddlEstado.SelectedValue, out estado) || estado < 0 || estado > 5)
                {
                    estado = Pendiente;
                    ddlEstado.SelectedValue = Pendiente.ToString();
                }
                var filtradas = propias.Where(p => estado == 0 || p.IdEstado == estado)
                    .OrderByDescending(p => p.Fecha)
                    .ThenByDescending(p => p.IdPostulacion)
                    .ToList();

                lblPendientes.Text = propias.Count(p => p.IdEstado == Pendiente).ToString();
                lblAceptadas.Text = propias.Count(p => p.IdEstado == Aceptada).ToString();
                lblConfirmadas.Text = propias.Count(p => p.IdEstado == Confirmada).ToString();
                lblRechazadas.Text = propias.Count(p => p.IdEstado == Rechazada).ToString();
                pnlDashboard.Visible = true;
                pnlListado.Visible = true;

                int totalPaginas = (int)Math.Ceiling(filtradas.Count / (double)dgvPostulaciones.PageSize);
                if(dgvPostulaciones.PageIndex >= totalPaginas)
                {
                    dgvPostulaciones.PageIndex = Math.Max(0, totalPaginas - 1);
                }

                dgvPostulaciones.DataSource = filtradas;
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
                var postulacion = await ObtenerPostulacionAsync(id);
                if (postulacion == null)
                {
                    MostrarMensaje("La postulación no existe o no pertenece a tu institución.", false);
                    return;
                }

                var curso = await InstiRedApi.GetAsync<CursoApi>("ent/Cursos/" + postulacion.IdCurso);
                var documentos = await InstiRedApi.GetAsync<List<InstiRedApi.DocumentoPostulacion>>(
                    "ent/DocumentosPostulacion?idPostulacionAlumnos=" + id);
                var adjuntos = documentos.Where(d => d.IdPostulacionAlumnos == id).ToList();

                int vacantes = Math.Max(0, curso.Capacidad - curso.Inscriptos);
                bool puedeResolver = usuario.IdAdministradorInstitucion.GetValueOrDefault() > 0;
                bool pendiente = postulacion.IdEstado == Pendiente;

                IdPostulacionSeleccionada = id;
                litCodigoSeguimiento.Text = postulacion.CodigoSeguimiento ?? "—";
                litEstadoPostulacion.Text = postulacion.Estado;
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
                pnlConVacantes.Visible = pendiente && vacantes > 0;
                pnlSinVacantes.Visible = pendiente && vacantes == 0;
                btnAceptar.Visible = pendiente;
                btnDesplegarRechazo.Visible = pendiente;
                btnConfirmarRechazo.Visible = pendiente;
                btnAceptar.Enabled = puedeResolver && vacantes > 0;
                btnDesplegarRechazo.Enabled = puedeResolver;
                btnConfirmarRechazo.Enabled = puedeResolver;
                txtMotivoRechazo.Text = "";
                panelRechazo.Visible = false;
                lblAvisoEstado.Visible = !pendiente;
                lblAvisoEstado.Text = "Esta postulación ya fue gestionada. Podés consultar sus datos y documentos.";
                if (postulacion.IdEstado == Aceptada)
                {
                    lblAvisoEstado.Text = "Aceptada por la institución. Falta la confirmación del responsable y todavía no ocupa un cupo.";
                }
                else if (postulacion.IdEstado == Confirmada)
                {
                    lblAvisoEstado.Text = "Confirmada por el responsable. Esta postulación ya ocupa un cupo del curso.";
                }
                pnlMotivoRechazo.Visible = !string.IsNullOrWhiteSpace(postulacion.MotivoRechazo);
                litMotivoRechazo.Text = postulacion.MotivoRechazo;

                pnlDashboard.Visible = false;
                pnlListado.Visible = false;

                if (pendiente && !puedeResolver)
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

        private async Task<PostulacionApi> ObtenerPostulacionAsync(int id)
        {
            if (id <= 0 || usuario.IdInstitucion.GetValueOrDefault() <= 0)
            {
                return null;
            }

            var postulacion = await InstiRedApi.GetAsync<PostulacionApi>(
                "vw/PostulacionesAlumnos/" + id, permitirNoEncontrado: true);
            if (postulacion == null || postulacion.IdInstitucion != usuario.IdInstitucion)
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
                var postulacion = await ObtenerPostulacionAsync(idPostulacion);
                if (postulacion == null || postulacion.IdEstado != Pendiente)
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
                    // Aprovecho el bool de aceptar para que el método evalue.
                    EnviarCorreo(postulacion, aceptar);

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
            pnlMensaje.CssClass = exito ? "alert theme-success mb-4" : "alert theme-danger mb-4";
            litMensaje.Text = Server.HtmlEncode(texto);
        }

        private void EnviarCorreo(PostulacionApi postulacion, bool aceptada)
        {
            if (string.IsNullOrWhiteSpace(postulacion.EmailResponsable))
            {
                // No uso la funcion "mostrar mensaje" ya que en tal caso en el que no tenga un correo (o por algun error no llegue el dato del corre),
                // el responsable no recibe un correo, pero el cambio de estado de la postulación se impacta igual
                Trace.Warn("GestionarPostulacionAlumno", "No se pudo enviar el correo: el responsable no tiene un email registrado.");
                return;
            }

            string asunto = aceptada ? "Postulación Aceptada - InstiRed" : "Postulación Rechazada - InstiRed";
            string cuerpo = aceptada ? CuerpoPostulacionAceptada(postulacion) : CuerpoPostulacionRechazada(postulacion);

            try
            {
                EmailService emailService = new EmailService();
                emailService.armarCorreo(postulacion.EmailResponsable, asunto, cuerpo);
                emailService.enviarEmail();
            }
            catch (Exception ex)
            {
                Trace.Warn("GestionarPostulacionAlumno", "Error al enviar el correo de resolución de la postulación.", ex);
            }
        }

        private string CuerpoPostulacionAceptada(PostulacionApi postulacion)
        {
            string responsable = postulacion.NombreTutor;
            string alumno = postulacion.Alumno;
            string curso = postulacion.Curso;

            string cuerpo = $@"
                <html>
                    <body style=""margin:0; padding:0; background-color:#F7F5FC; font-family:'Segoe UI', Roboto, Helvetica, Arial, sans-serif;"">
                        <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color:#F7F5FC;"">
                            <tr>
                                <td align=""center"" style=""padding:32px 16px;"">
                                    <table role=""presentation"" width=""600"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""width:100%; max-width:600px;"">
                                        <tr>
                                            <td align=""center"" bgcolor=""#373A72"" style=""background-color:#373A72; border-radius:15px 15px 0 0; padding:32px 24px;"">
                                                <span style=""font-size:36px; font-weight:700; color:#FFFFFF;"">InstiRed</span>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td bgcolor=""#FFFFFF"" style=""background-color:#FFFFFF; border:1px solid #E4E0F0; border-top:none; border-radius:0 0 15px 15px; padding:40px 36px;"">
                                                <h1 style=""margin:0 0 16px 0; font-size:22px; color:#373A72;"">¡Tu postulación fue aceptada!</h1>

                                                <p style=""font-size:15px; line-height:1.6; color:#373A72;"">
                                                    Hola, {responsable}. La institución aceptó la postulación de <strong>{alumno}</strong>.
                                                </p>

                                                <p style=""font-size:15px; line-height:1.6; color:#373A72;"">
                                                    <strong>Curso solicitado:</strong> {curso}
                                                </p>

                                                <p style=""font-size:15px; line-height:1.6; color:#373A72;"">
                                                    Para completar el proceso de inscripción, todavía falta que confirmes la vacante.
                                                </p>

                                                <hr style=""border:none; border-top:1px solid #E4E0F0; margin:24px 0 20px 0;"">

                                                <p style=""font-size:13px; line-height:1.5; color:#8D91D5;"">
                                                    Este es un correo automático. Por favor, no respondas a esta dirección.
                                                </p>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>
                        </table>
                    </body>
                </html>";

            return cuerpo;
        }

        private string CuerpoPostulacionRechazada(PostulacionApi postulacion)
        {
            string responsable = postulacion.NombreTutor;
            string alumno = postulacion.Alumno;
            string curso = postulacion.Curso;
            string motivoRechazo = postulacion.MotivoRechazo;

            string cuerpo = $@"
                <html>
                    <body style=""margin:0; padding:0; background-color:#F7F5FC; font-family:'Segoe UI', Roboto, Helvetica, Arial, sans-serif;"">
                        <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color:#F7F5FC;"">
                            <tr>
                                <td align=""center"" style=""padding:32px 16px;"">
                                    <table role=""presentation"" width=""600"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""width:100%; max-width:600px;"">
                                        <tr>
                                            <td align=""center"" bgcolor=""#373A72"" style=""background-color:#373A72; border-radius:15px 15px 0 0; padding:32px 24px;"">
                                                <span style=""font-size:36px; font-weight:700; color:#FFFFFF;"">InstiRed</span>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td bgcolor=""#FFFFFF"" style=""background-color:#FFFFFF; border:1px solid #E4E0F0; border-top:none; border-radius:0 0 15px 15px; padding:40px 36px;"">
                                                <h1 style=""margin:0 0 16px 0; font-size:22px; color:#373A72;"">Tu postulación fue rechazada</h1>

                                                <p style=""font-size:15px; line-height:1.6; color:#373A72;"">
                                                    Hola, {responsable}. La institución no pudo aceptar la postulación de <strong>{alumno}</strong>.
                                                </p>

                                                <p style=""font-size:15px; line-height:1.6; color:#373A72;"">
                                                    <strong>Curso solicitado:</strong> {curso}
                                                </p>

                                                <p style=""font-size:15px; line-height:1.6; color:#373A72;"">
                                                    <strong>Motivo del rechazo:</strong>
                                                </p>

                                                <div style=""background-color:#F7F5FC; border:1px solid #E4E0F0; border-radius:10px; padding:16px; font-size:14px; line-height:1.6; color:#373A72;"">
                                                    {motivoRechazo}
                                                </div>

                                                <hr style=""border:none; border-top:1px solid #E4E0F0; margin:24px 0 20px 0;"">

                                                <p style=""font-size:13px; line-height:1.5; color:#8D91D5;"">
                                                    Este es un correo automático. Por favor, no respondas a esta dirección.
                                                </p>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>
                        </table>
                    </body>
                </html>";

            return cuerpo;
        }

        private class PostulacionApi
        {
            public int IdPostulacion { get; set; }
            public int IdInstitucion { get; set; }
            public int IdEstado { get; set; }
            public string Estado { get; set; }
            public string MotivoRechazo { get; set; }
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

        protected void dgvPostulaciones_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            dgvPostulaciones.PageIndex = e.NewPageIndex;
            RegisterAsyncTask(new PageAsyncTask(() => CargarListadoAsync()));
        }
    }
}
