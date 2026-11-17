using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Presentacion
{
    public partial class GestionarVacante : System.Web.UI.Page
    {
        // Id del curso que se está editando (se conserva entre postbacks).
        private int IdCursoSeleccionado
        {
            get { return ViewState["IdCursoSeleccionado"] == null ? 0 : (int)ViewState["IdCursoSeleccionado"]; }
            set { ViewState["IdCursoSeleccionado"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarFiltros();
                CargarListado();
            }
        }

        private void CargarFiltros()
        {
            int anioActual = DateTime.Today.Year;
            for (int anio = anioActual - 1; anio <= anioActual + 1; anio++)
                ddlCicloLectivo.Items.Add(new ListItem(anio.ToString(), anio.ToString()));
            ddlCicloLectivo.SelectedValue = anioActual.ToString();

            // TODO: cargar desde la tabla NivelesEscolares.
            ddlNivel.Items.Add(new ListItem("Todos", "0"));
            ddlNivel.Items.Add(new ListItem("Preescolar", "1"));
            ddlNivel.Items.Add(new ListItem("Primaria", "2"));
            ddlNivel.Items.Add(new ListItem("Secundaria", "3"));
        }

        private void CargarListado()
        {
            DataTable cursos = ObtenerCursos();

            string filtro = "CicloLectivo = " + ddlCicloLectivo.SelectedValue;
            if (ddlNivel.SelectedValue != "0")
                filtro += " AND IdNivel = " + ddlNivel.SelectedValue;

            DataView vista = new DataView(cursos, filtro, "NombreCurso", DataViewRowState.CurrentRows);
            dgvCursos.DataSource = vista;
            dgvCursos.DataBind();

            lblTotalCursos.Text = vista.Count.ToString();
            lblConVacantes.Text = cursos.Select(filtro + " AND Vacantes > 0").Length.ToString();
            lblSinVacantes.Text = cursos.Select(filtro + " AND Vacantes = 0").Length.ToString();
        }

        protected void Filtro_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarListado();
        }

        protected void btnEditar_Click(object sender, EventArgs e)
        {
            LinkButton boton = (LinkButton)sender;
            IdCursoSeleccionado = int.Parse(boton.CommandArgument);

            DataRow curso = ObtenerCursos().Rows.Find(IdCursoSeleccionado);

            lblCursoNombre.Text = Server.HtmlEncode(curso["NombreCurso"].ToString());
            lblCicloLectivo.Text = curso["CicloLectivo"].ToString();
            lblTurno.Text = Server.HtmlEncode(curso["Turno"].ToString());
            lblCapacidad.Text = curso["Capacidad"].ToString();
            lblInscriptos.Text = curso["Inscriptos"].ToString();

            txtVacantes.Text = curso["Vacantes"].ToString();
            txtVacantes.Attributes["min"] = "0";

            pnlMensaje.Visible = false;
            pnlDashboard.Visible = false;
            pnlListado.Visible = false;
            pnlEdicion.Visible = true;
        }

        protected void btnCerrarEdicion_Click(object sender, EventArgs e)
        {
            VolverAlListado();
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            DataRow curso = ObtenerCursos().Rows.Find(IdCursoSeleccionado);
            int inscriptos = (int)curso["Inscriptos"];

            // capacidad es SMALLINT en la base (máximo 32767).
            int maximoVacantes = short.MaxValue - inscriptos;

            int vacantes;
            if (!int.TryParse(txtVacantes.Text, out vacantes) || vacantes < 0 || vacantes > maximoVacantes)
            {
                MostrarMensaje("Ingresá un número de vacantes entre 0 y " + maximoVacantes + ".", false);
                return;
            }

            // Los inscriptos son alumnos reales y NO se tocan desde esta pantalla.
            // Lo que se modifica es la capacidad:  capacidad = inscriptos + vacantes.
            // Así vacantes (= capacidad - inscriptos) queda con el valor ingresado
            // y se respeta el CHECK (inscriptos <= capacidad) de la tabla.
            // TODO: UPDATE Cursos SET capacidad = inscriptos + @vacantes WHERE id = @id AND idInstitucion = @idInstitucion
            curso["Capacidad"] = inscriptos + vacantes;

            string nombre = curso["NombreCurso"].ToString();
            VolverAlListado();
            MostrarMensaje("Vacantes actualizadas: " + nombre + " ahora tiene " + vacantes + " disponibles.", true);
        }

        private void VolverAlListado()
        {
            pnlEdicion.Visible = false;
            pnlDashboard.Visible = true;
            pnlListado.Visible = true;
            CargarListado();
        }

        private void MostrarMensaje(string texto, bool exito)
        {
            pnlMensaje.Visible = true;
            pnlMensaje.CssClass = exito ? "alert alert-success mb-4" : "alert alert-danger mb-4";
            litMensaje.Text = Server.HtmlEncode(texto);
        }

        // ===== DATOS DE EJEMPLO (TEMPORAL: reemplazar por la consulta a la base) =====

        private DataTable ObtenerCursos()
        {
            DataTable cursos = Session["CursosDemo"] as DataTable;
            if (cursos != null)
                return cursos;

            cursos = new DataTable();
            cursos.Columns.Add("Id", typeof(int));
            cursos.Columns.Add("NombreCurso", typeof(string));
            cursos.Columns.Add("CicloLectivo", typeof(int));
            cursos.Columns.Add("Turno", typeof(string));
            cursos.Columns.Add("IdNivel", typeof(int));
            cursos.Columns.Add("Capacidad", typeof(int));
            cursos.Columns.Add("Inscriptos", typeof(int));
            cursos.Columns.Add("Vacantes", typeof(int), "Capacidad - Inscriptos");
            cursos.PrimaryKey = new[] { cursos.Columns["Id"] };

            int ciclo = DateTime.Today.Year;
            cursos.Rows.Add(1, ArmarNombre(1, 4, null, "Azul"), ciclo, "Mañana", 1, 25, 22);
            cursos.Rows.Add(2, ArmarNombre(2, 1, null, "A"), ciclo, "Mañana", 2, 30, 30);
            cursos.Rows.Add(3, ArmarNombre(2, 1, null, "B"), ciclo, "Tarde", 2, 30, 24);
            cursos.Rows.Add(4, ArmarNombre(3, 3, null, "2da división"), ciclo, "Mañana", 3, 35, 31);
            cursos.Rows.Add(5, ArmarNombre(3, null, "2nd Children", "Lunes y miércoles"), ciclo, "Tarde", 3, 20, 12);
            cursos.Rows.Add(6, ArmarNombre(2, 2, null, "A"), ciclo - 1, "Mañana", 2, 30, 28);

            Session["CursosDemo"] = cursos;
            return cursos;
        }

        // Preescolar -> "Salita de 4 - Azul" | Primaria -> "1° grado - A" | Secundaria -> "3° año - 2da división".
        // Si hay nivelPersonalizado se usa ese texto.
        private static string ArmarNombre(int idNivel, int? anio, string nivelPersonalizado, string division)
        {
            string nivel;

            if (!string.IsNullOrWhiteSpace(nivelPersonalizado))
                nivel = nivelPersonalizado;
            else if (idNivel == 1)
                nivel = "Salita de " + anio;
            else if (idNivel == 2)
                nivel = anio + "° grado";
            else if (idNivel == 3)
                nivel = anio + "° año";
            else
                nivel = "Curso";

            return nivel + " - " + division;
        }
    }
}