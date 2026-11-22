using Dominio;
using Negocio;
using Presentacion.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Presentacion
{
    public partial class FormularioPostulacionAlumno : System.Web.UI.Page
    {
        private UsuarioSesion usuario;
        private List<CursoApi> cursos;

        protected void Page_Init(object sender, EventArgs e)
        {
            ViewStateUserKey = Session.SessionID;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            usuario = UsuarioSesion.Obtener(Session);

            // Validar que el usuario sea un responsable
            if (usuario == null || usuario.IdTipoUsuario != UsuarioSesion.Responsable || usuario.IdResponsable.GetValueOrDefault() <= 0)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            Response.Cache.SetNoStore(); // Evita que la pagina guarde cache

            // Valido que el formulario reciba el idInstitucion de la seleccionada en el detalle de la institucion
            if (Request.QueryString["id"] == null || Request.QueryString["id"] == "")
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            int idInstitucion = int.Parse(Request.QueryString["id"]);
            string nombreInstitucion = Request.QueryString["nombre"];

            if(idInstitucion <= 0 || string.IsNullOrWhiteSpace(nombreInstitucion))
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            txtInstitucion.Text = nombreInstitucion;

            if (!IsPostBack)
            {
                // Traer los cursos al cargar por primera vez el formulario
                RegisterAsyncTask(new PageAsyncTask(() => CargarCursosAsync(idInstitucion)));
            }
        }

        private void CargarAlumnos(List<AlumnoApi> alumnos)
        {
            ddlAlumnos.Items.Clear();
            ddlAlumnos.Items.Add(new ListItem("Seleccionar alumno", ""));

            if (alumnos == null)
            {
                return;
            }

            foreach (AlumnoApi alumno in alumnos)
            {
                string nombreCompleto = alumno.Nombre + " " + alumno.Apellido;

                ddlAlumnos.Items.Add(new ListItem(nombreCompleto, alumno.IdAlumno.ToString()));
            }
        }

        private async Task CargarCursosAsync(int idInstitucion)
        {
            try
            {
                // "await" permite esperar hasta que la API (El endpoint) devuelva un resultado
                List<CursoApi> cursos = await InstiRedApi.GetAsync<List<CursoApi>>("vw/Cursos?idInstitucion=" + idInstitucion);

                Session["cursos"] = cursos;

                CargarCiclosLectivos(cursos);
            }
            catch(Exception ex)
            {
                // Trace.Warn es una forma de .NET de almacenar errores
                Trace.Warn("FormularioPostulacionAlumno", "Error al consultar los cursos de la institucion", ex);
                Response.StatusCode = 502;
            }
        }

        private void CargarCiclosLectivos(List<CursoApi> cursos)
        {
            ddlCiclo.Items.Clear();

            ddlCiclo.Items.Add(new ListItem("Seleccionar ciclo", ""));

            // Ciclos lectivos de la institucion ordenados y sin repetir
            List<int> ciclos = cursos.Select(c => c.CicloLectivo).Distinct().OrderByDescending(c => c).ToList();

            // Agregar cada ciclo lectivo disponible al ddl
            foreach(int ciclo in ciclos)
            {
                ddlCiclo.Items.Add(new ListItem(ciclo.ToString(), ciclo.ToString()));
            }
        }

        private void CargarNivelesEducativos(List<CursoApi> cursos, int cicloSeleccionado)
        {
            ddlNivel.Items.Clear();

            ddlNivel.Items.Add(new ListItem("Seleccionar nivel", ""));

            // Filtro solo los niveles correspondientes al ciclo lectivo seleccionado
            List<CursoApi> niveles = cursos.Where(c => c.CicloLectivo == cicloSeleccionado).GroupBy(c => new { c.IdNivel, c.NivelPersonalizado }).Select(g => g.First()).ToList();

            foreach (CursoApi curso in niveles)
            {
                string descripcion = curso.Nivel;

                if (!string.IsNullOrWhiteSpace(curso.NivelPersonalizado) && curso.NivelPersonalizado != descripcion)
                {
                    descripcion += " - " + curso.NivelPersonalizado;
                }

                ddlNivel.Items.Add(new ListItem(descripcion, curso.IdCurso.ToString()));
            }

            // NOTA: Por como esta armada la DB, y los registros insertados, los cursos pueden tener nivel normal y
            //  el nivel personalizado al mismo tiempo. Por lo tanto hay que contemplar el caso en el que esten ambos
        }

        private void CargarAnios(List<CursoApi> cursos, int cicloSeleccionado, CursoApi nivelSeleccionado)
        {
            ddlAnio.Items.Clear();

            ddlAnio.Items.Add(new ListItem("Seleccionar año", ""));

            // Filtra los cursos correspondientes al ciclo y nivel seleccionados
            List<CursoApi> anios = cursos.Where(c => c.CicloLectivo == cicloSeleccionado && c.IdNivel == nivelSeleccionado.IdNivel &&c.NivelPersonalizado == nivelSeleccionado.NivelPersonalizado).GroupBy(c => c.Anio).Select(g => g.First()).OrderBy(c => c.Anio).ToList();

            foreach (CursoApi curso in anios)
            {
                string descripcion = curso.Anio.HasValue ? curso.Anio.Value.ToString() : "Sin año";

                string valor = curso.Anio.HasValue? curso.Anio.Value.ToString() : "null";

                ddlAnio.Items.Add(new ListItem(descripcion, valor));
            }
        }

        private void CargarDivisiones(List<CursoApi> cursos, int cicloSeleccionado, CursoApi nivelSeleccionado, int? anioSeleccionado)
        {
            ddlDivision.Items.Clear();

            ddlDivision.Items.Add(new ListItem("Seleccionar división", ""));

            List<CursoApi> divisiones = cursos.Where(c => c.CicloLectivo == cicloSeleccionado && c.IdNivel == nivelSeleccionado.IdNivel && c.NivelPersonalizado == nivelSeleccionado.NivelPersonalizado && c.Anio == anioSeleccionado).GroupBy(c => c.Division).Select(g => g.First()).OrderBy(c => c.Division).ToList();

            foreach (CursoApi curso in divisiones)
            {
                ddlDivision.Items.Add(new ListItem(curso.Division, curso.Division));
            }
        }

        private void CargarTurnos(List<CursoApi> cursos, int cicloSeleccionado, CursoApi nivelSeleccionado, int? anioSeleccionado, string divisionSeleccionada)
        {
            ddlTurno.Items.Clear();

            ddlTurno.Items.Add(new ListItem("Seleccionar turno", ""));

            List<CursoApi> turnos = cursos.Where(c => c.CicloLectivo == cicloSeleccionado && c.IdNivel == nivelSeleccionado.IdNivel && c.NivelPersonalizado == nivelSeleccionado.NivelPersonalizado && c.Anio == anioSeleccionado && c.Division == divisionSeleccionada).GroupBy(c => new { c.IdTurno, c.Turno }).Select(g => g.First()).OrderBy(c => c.Turno).ToList();

            foreach (CursoApi curso in turnos)
            {
                ddlTurno.Items.Add(new ListItem(curso.Turno, curso.IdTurno.ToString()));
            }
        }

        protected void ddlCiclo_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Validar cursos
            List<CursoApi> cursos = (List<CursoApi>)Session["cursos"];

            if (cursos == null || ddlCiclo.SelectedValue == "")
            {
                return;
            }

            int cicloSeleccionado = int.Parse(ddlCiclo.SelectedValue);

            CargarNivelesEducativos(cursos, cicloSeleccionado);

            ddlAnio.Items.Clear();
            ddlDivision.Items.Clear();
            ddlTurno.Items.Clear();
        }

        protected void ddlNivel_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Validar cursos
            List<CursoApi> cursos = (List<CursoApi>)Session["cursos"];

            if (cursos == null || ddlCiclo.SelectedValue == "" || ddlNivel.SelectedValue == "")
            {
                return;
            }

            int cicloSeleccionado = int.Parse(ddlCiclo.SelectedValue);
            int idCurso = int.Parse(ddlNivel.SelectedValue);

            CursoApi nivelSeleccionado = cursos.FirstOrDefault(c => c.IdCurso == idCurso);

            if (nivelSeleccionado == null)
            {
                return;
            }

            CargarAnios(cursos, cicloSeleccionado, nivelSeleccionado);

            ddlDivision.Items.Clear();
            ddlTurno.Items.Clear();
        }

        protected void ddlAnio_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Validar cursos
            List<CursoApi> cursos = (List<CursoApi>)Session["cursos"];

            if (cursos == null || ddlCiclo.SelectedValue == "" || ddlNivel.SelectedValue == "" ||
                ddlAnio.SelectedValue == "")
            {
                return;
            }

            int cicloSeleccionado = int.Parse(ddlCiclo.SelectedValue);
            int idCurso = int.Parse(ddlNivel.SelectedValue);

            CursoApi nivelSeleccionado = cursos.FirstOrDefault(c => c.IdCurso == idCurso);

            if (nivelSeleccionado == null)
            {
                return;
            }

            // "int?" permite guardar tanto un int como null (Ya que la DB permite que el anio sea null)
            int? anioSeleccionado = null;

            if (ddlAnio.SelectedValue != "null")
            {
                anioSeleccionado = int.Parse(ddlAnio.SelectedValue);
            }

            CargarDivisiones(cursos, cicloSeleccionado, nivelSeleccionado, anioSeleccionado);

            ddlTurno.Items.Clear();
        }

        protected void ddlDivision_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Validar cursos
            List<CursoApi> cursos = (List<CursoApi>)Session["cursos"];

            if (cursos == null || ddlCiclo.SelectedValue == "" || ddlNivel.SelectedValue == "" ||
                ddlAnio.SelectedValue == "" || ddlDivision.SelectedValue == "")
            {
                return;
            }

            int cicloSeleccionado = int.Parse(ddlCiclo.SelectedValue);
            int idCurso = int.Parse(ddlNivel.SelectedValue);
            string divisionSeleccionada = ddlDivision.SelectedValue;

            CursoApi nivelSeleccionado = cursos.FirstOrDefault(c => c.IdCurso == idCurso);

            if (nivelSeleccionado == null)
            {
                return;
            }

            // "int?" permite guardar tanto un int como null (Ya que la DB permite que el anio sea null)
            int? anioSeleccionado = null;

            if (ddlAnio.SelectedValue != "null")
            {
                anioSeleccionado = int.Parse(ddlAnio.SelectedValue);
            }

            CargarTurnos(cursos, cicloSeleccionado, nivelSeleccionado, anioSeleccionado, divisionSeleccionada);
        }

        protected void ddlTurno_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Validar cursos
            List<CursoApi> cursos = (List<CursoApi>)Session["cursos"];

            if (cursos == null || ddlCiclo.SelectedValue == "" || ddlNivel.SelectedValue == "" ||
                ddlAnio.SelectedValue == "" || ddlDivision.SelectedValue == "" || ddlTurno.SelectedValue == "")
            {
                txtVacantes.Text = "";
                return;
            }

            int cicloSeleccionado = int.Parse(ddlCiclo.SelectedValue);
            int idCurso = int.Parse(ddlNivel.SelectedValue);
            int idTurnoSeleccionado = int.Parse(ddlTurno.SelectedValue);
            string divisionSeleccionada = ddlDivision.SelectedValue;

            CursoApi nivelSeleccionado = cursos.FirstOrDefault(c => c.IdCurso == idCurso);

            if (nivelSeleccionado == null)
            {
                txtVacantes.Text = "";
                return;
            }

            // "int?" permite guardar tanto un int como null (Ya que la DB permite que el anio sea null)
            int? anioSeleccionado = null;

            if (ddlAnio.SelectedValue != "null")
            {
                anioSeleccionado = int.Parse(ddlAnio.SelectedValue);
            }

            CursoApi cursoSeleccionado = cursos.FirstOrDefault(c => c.CicloLectivo == cicloSeleccionado && c.IdNivel == nivelSeleccionado.IdNivel && c.NivelPersonalizado == nivelSeleccionado.NivelPersonalizado && c.Anio == anioSeleccionado && c.Division == divisionSeleccionada && c.IdTurno == idTurnoSeleccionado);

            if (cursoSeleccionado == null)
            {
                txtVacantes.Text = "";
                return;
            }

            txtVacantes.Text = cursoSeleccionado.VacantesDisponibles.ToString();
        }


        private class CursoApi
        {
            public int IdCurso { get; set; }
            public int IdInstitucion { get; set; }
            public int CicloLectivo { get; set; }
            public int? IdNivel { get; set; }
            public string Nivel { get; set; }
            public int? Anio { get; set; }
            public string NivelPersonalizado { get; set; }
            public string Division { get; set; }
            public int IdTurno { get; set; }
            public string Turno { get; set; }
            public int Capacidad { get; set; }
            public int Inscriptos { get; set; }
            public int VacantesDisponibles { get; set; }
        }

        private class AlumnoApi
        {
            public int IdAlumno { get; set; }
            public string Nombre { get; set; }
            public string Apellido { get; set; }
        }

        // NOTAS

        // "async" --> Permite utilizar "await"
        // "Task" --> Operacion que se ejecuta de manera asincronica
        // "await" --> Permite esperar el resultado de una operacion asyncronica (Task) sin bloquear el
        //             hilo mientras espera. Si la respuesta vuelve, el metodo que lo llamo sigue.

    }
}