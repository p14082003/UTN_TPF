using Negocio;
using Presentacion.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Presentacion
{
    public partial class ListadoPostulacionAlumnos : Page
    {
        private UsuarioSesion usuario;

        private List<PostulacionAlumnoListado> ListaPostulaciones
        {
            get { return Session["PostulacionesResponsable:" + usuario.IdResponsable.Value] as List<PostulacionAlumnoListado>; }
            set { Session["PostulacionesResponsable:" + usuario.IdResponsable.Value] = value; }
        }

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
            if (!IsPostBack)
            {
                RegisterAsyncTask(new PageAsyncTask(CargarListadoAsync));
            }
        }

        private async Task CargarListadoAsync()
        {
            try
            {
                int idResponsable = usuario.IdResponsable.Value;
                var postulaciones = InstiRedApi.GetAsync<List<PostulacionAlumnoListado>>(
                    "sp/PostulacionesPorResponsable?idResponsable=" + idResponsable);
                var alumnos = InstiRedApi.GetAsync<List<AlumnoResponsable>>(
                    "sp/AlumnosPorResponsable?idResponsable=" + idResponsable);
                var estados = InstiRedApi.GetAsync<List<EstadoPostulacionApi>>("ent/EstadosPostulacion");
                await Task.WhenAll(postulaciones, alumnos, estados);

                ListaPostulaciones = postulaciones.Result;
                ddlAlumnos.Items.Clear();
                ddlAlumnos.Items.Add(new ListItem("Todos los alumnos", "0"));
                foreach (var alumno in alumnos.Result)
                {
                    ddlAlumnos.Items.Add(new ListItem(alumno.NombreCompleto, alumno.IdAlumno.ToString()));
                }
                ddlEstadosPostulacion.Items.Clear();
                ddlEstadosPostulacion.Items.Add(new ListItem("Todos los estados", "0"));
                foreach (var estado in estados.Result)
                {
                    ddlEstadosPostulacion.Items.Add(new ListItem(estado.Descripcion, estado.Id.ToString()));
                }
                AplicarFiltros();
            }
            catch (Exception ex)
            {
                Trace.Warn("ListadoPostulacionAlumnos", "Error al cargar las postulaciones del responsable.", ex);
                ListaPostulaciones = null;
                dgvPostulaciones.DataSource = null;
                dgvPostulaciones.DataBind();
                MostrarMensaje("No pudimos cargar tus postulaciones. Volvé a abrir el listado para intentar nuevamente.", false);
            }
        }

        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            dgvPostulaciones.PageIndex = 0;
            AplicarFiltros();
        }

        private void AplicarFiltros()
        {
            if (ListaPostulaciones == null)
            {
                RegisterAsyncTask(new PageAsyncTask(CargarListadoAsync));
                return;
            }
            int idAlumno;
            int idEstado;
            int.TryParse(ddlAlumnos.SelectedValue, out idAlumno);
            int.TryParse(ddlEstadosPostulacion.SelectedValue, out idEstado);
            var resultado = new List<PostulacionAlumnoListado>();
            foreach (var postulacion in ListaPostulaciones)
            {
                if ((idAlumno == 0 || postulacion.IdAlumno == idAlumno) &&
                    (idEstado == 0 || postulacion.IdEstado == idEstado))
                {
                    resultado.Add(postulacion);
                }
            }
            dgvPostulaciones.DataSource = resultado;
            dgvPostulaciones.DataBind();
        }

        protected void btnVerPostulacion_Click1(object sender, EventArgs e)
        {
            int idPostulacion;
            if (!int.TryParse(((LinkButton)sender).CommandArgument, out idPostulacion) ||
                ListaPostulaciones == null || !ListaPostulaciones.Any(p => p.IdPostulacion == idPostulacion))
            {
                MostrarMensaje("Volvé a cargar el listado y elegí una postulación de tu cuenta.", false);
                return;
            }
            Response.Redirect("~/ResponsableGestionarPostulacionAlumno.aspx?id=" + idPostulacion);
        }

        protected void dgvPostulaciones_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            dgvPostulaciones.PageIndex = e.NewPageIndex;
            AplicarFiltros();
        }

        private void MostrarMensaje(string texto, bool ok)
        {
            lblMensaje.Text = HttpUtility.HtmlEncode(texto);
            lblMensaje.CssClass = ok ? "alert alert-success d-block mb-3" : "alert alert-danger d-block mb-3";
            lblMensaje.Visible = true;
        }

        [Serializable]
        public class PostulacionAlumnoListado
        {
            public int IdPostulacion { get; set; }
            public int IdAlumno { get; set; }
            public string Alumno { get; set; }
            public string Documento { get; set; }
            public string Institucion { get; set; }
            public int CicloLectivo { get; set; }
            public int IdEstado { get; set; }
            public string Estado { get; set; }
        }

        public class AlumnoResponsable
        {
            public int IdAlumno { get; set; }
            public string NombreCompleto { get; set; }
        }

        private class EstadoPostulacionApi
        {
            public int Id { get; set; }
            public string Descripcion { get; set; }
        }
    }
}
