using Negocio;
using Presentacion.Auth;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;

namespace Presentacion
{
    public partial class ResponsableGestionarPostulacionAlumno : Page
    {
        private UsuarioSesion usuario;
        private int idPostulacion;

        protected void Page_Init(object sender, EventArgs e)
        {
            ViewStateUserKey = Session.SessionID;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            usuario = UsuarioSesion.Obtener(Session);
            if (usuario == null || usuario.IdTipoUsuario != UsuarioSesion.Responsable || usuario.IdResponsable.GetValueOrDefault() <= 0)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }
            Response.Cache.SetNoStore();
            if (!int.TryParse(Request.QueryString["id"], out idPostulacion) || idPostulacion <= 0)
            {
                pnlDatos.Visible = false;
                OcultarBotones();
                MostrarMensaje("Elegí una postulación desde el listado.", false);
                return;
            }
            if (!IsPostBack)
            {
                RegisterAsyncTask(new PageAsyncTask(() => CargarDetalleAsync()));
            }
        }

        protected void btnConfirmar_Click(object sender, EventArgs e)
        {
            RegisterAsyncTask(new PageAsyncTask(() => ResolverPostulacionAsync(true)));
        }

        protected void btnCancelarPostulacion_Click(object sender, EventArgs e)
        {
            RegisterAsyncTask(new PageAsyncTask(() => ResolverPostulacionAsync(false)));
        }

        protected void btnVolver_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/ListadoPostulacionAlumnos.aspx");
        }

        private async Task<PostulacionApi> ObtenerPostulacionAsync()
        {
            if (idPostulacion <= 0)
            {
                return null;
            }
            var postulacion = await InstiRedApi.GetAsync<PostulacionApi>(
                "vw/PostulacionesAlumnos/" + idPostulacion, permitirNoEncontrado: true);
            if (postulacion == null || postulacion.IdResponsable != usuario.IdResponsable)
            {
                return null;
            }
            return postulacion;
        }

        private async Task CargarDetalleAsync(string mensajeExito = null)
        {
            pnlDatos.Visible = false;
            OcultarBotones();
            try
            {
                var postulacion = await ObtenerPostulacionAsync();
                if (postulacion == null)
                {
                    MostrarMensaje("No encontramos esa postulación en tu cuenta.", false);
                    return;
                }
                var curso = await InstiRedApi.GetAsync<CursoApi>("ent/Cursos/" + postulacion.IdCurso);
                lblNombreCompleto.Text = HttpUtility.HtmlEncode(postulacion.NombreAlumno + " " + postulacion.ApellidoAlumno);
                lblDocumento.Text = HttpUtility.HtmlEncode("DNI: " + postulacion.DocAlumno);
                lblEstado.Text = HttpUtility.HtmlEncode(postulacion.Estado);
                litCodigoSeguimiento.Text = postulacion.CodigoSeguimiento;
                lblAnio.Text = postulacion.Anio.HasValue ? postulacion.Anio.Value.ToString() : "—";
                lblNivel.Text = HttpUtility.HtmlEncode(postulacion.Nivel);
                lblNivelPersonalizado.Text = HttpUtility.HtmlEncode(curso.NivelPersonalizado);
                lblDivision.Text = HttpUtility.HtmlEncode(postulacion.Division);
                lblFechaPostulacion.Text = postulacion.Fecha.ToString("dd/MM/yyyy");
                lblCicloLectivo.Text = postulacion.CicloLectivo.ToString();
                lblInstitucion.Text = HttpUtility.HtmlEncode(postulacion.Institucion);
                lblTurno.Text = HttpUtility.HtmlEncode(postulacion.Turno);
                lblMotivoRechazo.Text = HttpUtility.HtmlEncode(postulacion.MotivoRechazo);
                lblVacantesAviso.Text = "Vacantes disponibles en este momento: " + Math.Max(0, curso.Capacidad - curso.Inscriptos) +
                    ". La disponibilidad se vuelve a comprobar al confirmar.";
                lblVacantesAviso.Visible = postulacion.IdEstado == 2;
                ConfigurarTipoDeCurso(curso.IdNivel, curso.Anio, curso.NivelPersonalizado, postulacion.MotivoRechazo);
                ConfigurarColorEtiquetaEstado(postulacion.IdEstado);
                ConfigurarBotonesPorEstado(postulacion.IdEstado, postulacion.CicloLectivo);
                pnlDatos.Visible = true;
                if (mensajeExito != null)
                {
                    MostrarMensaje(mensajeExito, true);
                }
            }
            catch (Exception ex)
            {
                Trace.Warn("ResponsableGestionarPostulacionAlumno", "Error al cargar la postulación.", ex);
                MostrarMensaje(mensajeExito == null ? "No pudimos cargar la postulación. Intentá nuevamente." :
                    mensajeExito + " No pudimos actualizar el detalle. Volvé al listado para consultar el estado.", false);
            }
        }

        private async Task ResolverPostulacionAsync(bool confirmar)
        {
            try
            {
                var postulacion = await ObtenerPostulacionAsync();
                if (postulacion == null)
                {
                    pnlDatos.Visible = false;
                    OcultarBotones();
                    MostrarMensaje("No encontramos esa postulación en tu cuenta.", false);
                    return;
                }
                bool cicloValido = postulacion.CicloLectivo >= DateTime.Now.Year && postulacion.CicloLectivo <= DateTime.Now.Year + 1;
                bool estadoValido = confirmar ? postulacion.IdEstado == 2 :
                    postulacion.IdEstado == 1 || postulacion.IdEstado == 2 || postulacion.IdEstado == 3;
                if (!cicloValido || !estadoValido)
                {
                    await CargarDetalleAsync();
                    MostrarMensaje("La acción no está disponible para el estado o ciclo lectivo actual de esta postulación.", false);
                    return;
                }
                string ruta = confirmar ? "sp/ConfirmarPostulacionAlumno" : "sp/CancelarPostulacionAlumno";
                var respuesta = await InstiRedApi.PostAsync(ruta, new
                {
                    IdPostulacion = idPostulacion,
                    IdResponsable = usuario.IdResponsable.Value
                });
                if (respuesta.Estado == HttpStatusCode.OK)
                {
                    Session.Remove("PostulacionesResponsable:" + usuario.IdResponsable.Value);
                    await CargarDetalleAsync(confirmar ? "La postulación fue confirmada con éxito." : "La postulación fue cancelada correctamente.");
                    return;
                }
                if (respuesta.Estado == HttpStatusCode.BadRequest)
                {
                    await CargarDetalleAsync();
                    MostrarMensaje(respuesta.Mensaje, false);
                    return;
                }
                MostrarMensaje("No se pudo completar la operación. Intentá nuevamente más tarde.", false);
            }
            catch (Exception ex)
            {
                Trace.Warn("ResponsableGestionarPostulacionAlumno", "Error al gestionar la postulación.", ex);
                OcultarBotones();
                MostrarMensaje("No pudimos confirmar el resultado de la operación. Volvé al listado para consultar el estado antes de reintentarlo.", false);
            }
        }

        private void ConfigurarBotonesPorEstado(int idEstado, int cicloLectivo)
        {
            int anioActual = DateTime.Now.Year;
            btnCancelarPostulacion.OnClientClick = "";

            btnConfirmar.Enabled = true;
            btnConfirmar.Visible = false;
            btnCancelarPostulacion.Visible = false;
            btnVolver.Visible = true;


            if (cicloLectivo < anioActual || cicloLectivo >anioActual + 1)
                return;

            switch (idEstado)
            {
                case 1:
                    // Pendiente
                    btnCancelarPostulacion.Visible = true;
                    break;

                case 2:
                    //Aceptado
                    btnConfirmar.Visible = true;
                    btnCancelarPostulacion.Visible = true;

                    break;
               
                case 3:
                    //Confirmado
              
                        btnCancelarPostulacion.Visible = true;
                        // aca se podría poner un modal pero no se como probar si funciona con este boostrap. Hay que probar cuando este funcionando la pantalla
                        btnCancelarPostulacion.OnClientClick = "return confirm('Esta postulación se encuentra confirmada. Si se cancela se perderá el cupo reservado en el curso.¿Desea continuar?');";
                    
                    break;  
            }

        }

        private void ConfigurarTipoDeCurso(int? idNivel, int? anio, String nivelPersonalizado, String motivoRechazo)
        {
            pnlAnio.Visible = false;
            pnlMotivoRechazo.Visible = false;
            pnlNivel.Visible = false;
            pnlNivelPersonalizado.Visible = false;
            pnlDivision.Visible = true;

            if (!string.IsNullOrWhiteSpace(nivelPersonalizado) && !idNivel.HasValue && !anio.HasValue){
               
                pnlNivelPersonalizado.Visible = true;
            }
            else if (idNivel.HasValue && anio.HasValue)
            {
                pnlAnio.Visible = true;
                pnlNivel.Visible = true;

            }
            if(!string.IsNullOrWhiteSpace(motivoRechazo))
            {
                pnlMotivoRechazo.Visible = true;
            }
          
        }

        private void ConfigurarColorEtiquetaEstado(int idEstado)
        {
            switch(idEstado)
            {
                case 1: 
                    // pendiente
                    lblEstado.CssClass = "badge rounded-pill estado-pendiente";
                    break;

                case 2: 
                    //aceptado
                    lblEstado.CssClass = "badge rounded-pill bg-success";
                    break;
                case 3:
                    //confirmado
                    lblEstado.CssClass = "badge rounded-pill bg-primary";
                    break;
                case 4: 
                    //rechazada
                    lblEstado.CssClass = "badge rounded-pill bg-danger";
                    break;
                case 5:
                    //cancelada
                    lblEstado.CssClass = "badge rounded-pill bg-secondary";
                    break;

            }
        }
        private void OcultarBotones()
        {
            btnConfirmar.Enabled = true;
            btnConfirmar.Visible = false;
            btnCancelarPostulacion.Visible = false;
        }

        private void MostrarMensaje(string texto, bool ok)
        {
            lblMensaje.Text = HttpUtility.HtmlEncode(texto);
            lblMensaje.CssClass = ok ? "alert alert-success d-block mb-4" : "alert alert-danger d-block mb-4";
            lblMensaje.Visible = true;
        }

        private class PostulacionApi
        {
            public int IdPostulacion { get; set; }
            public int? IdResponsable { get; set; }
            public int IdCurso { get; set; }
            public DateTime Fecha { get; set; }
            public string CodigoSeguimiento { get; set; }
            public int IdEstado { get; set; }
            public string Estado { get; set; }
            public string NombreAlumno { get; set; }
            public string ApellidoAlumno { get; set; }
            public string DocAlumno { get; set; }
            public string Nivel { get; set; }
            public int? Anio { get; set; }
            public string Division { get; set; }
            public int CicloLectivo { get; set; }
            public string Institucion { get; set; }
            public string Turno { get; set; }
            public string MotivoRechazo { get; set; }
        }

        private class CursoApi
        {
            public int? IdNivel { get; set; }
            public int? Anio { get; set; }
            public string NivelPersonalizado { get; set; }
            public int Capacidad { get; set; }
            public int Inscriptos { get; set; }
        }
    }
}
