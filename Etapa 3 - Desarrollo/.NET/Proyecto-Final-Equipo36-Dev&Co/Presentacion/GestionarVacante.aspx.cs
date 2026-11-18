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
                CargarCombosAlta();
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

        // ===== ALTA DE CURSO =====

        private const int TipoInstitutoIdiomas = 4;

        // Pasar a true cuando la base de datos esté conectada: el alta llama a sp_Registrar_Curso.
        // Con false se usan los datos de ejemplo y las mismas validaciones del procedimiento.
        private static readonly bool UsarBaseDeDatos = false;

        private int ObtenerTipoInstitucion()
        {
            // TODO: leer Instituciones.idTipoInstitucion de la institución del administrador logueado.
            return 7; // demo: ofrece Preescolar, Primaria y Secundaria
        }

        private int ObtenerIdAdministradorInstitucion()
        {
            // TODO: tomar el id del administrador logueado (por ejemplo, de la sesión).
            return Session["IdAdministradorInstitucion"] == null ? 0 : (int)Session["IdAdministradorInstitucion"];
        }

        // Niveles que ofrece cada tipo de institución (mismas reglas que sp_Registrar_Curso).
        private static int[] NivelesPermitidos(int tipoInstitucion)
        {
            switch (tipoInstitucion)
            {
                case 1: return new[] { 1 };
                case 2: return new[] { 2 };
                case 3: return new[] { 3 };
                case 5: return new[] { 1, 2 };
                case 6: return new[] { 2, 3 };
                case 7: return new[] { 1, 2, 3 };
                case 8: return new[] { 1, 3 };
                default: return new int[0];   // 4 = idiomas: solo nivel personalizado
            }
        }

        private static string NombreNivel(int idNivel)
        {
            switch (idNivel)
            {
                case 1: return "Preescolar";
                case 2: return "Primaria";
                default: return "Secundaria";
            }
        }

        // Rango de años válido por nivel (mismo que el procedimiento).
        private static void RangoAnios(int idNivel, out int minimo, out int maximo)
        {
            if (idNivel == 1) { minimo = 3; maximo = 5; }
            else if (idNivel == 2) { minimo = 1; maximo = 7; }
            else { minimo = 1; maximo = 6; }
        }

        private void CargarCombosAlta()
        {
            int anioActual = DateTime.Today.Year;
            ddlAltaCiclo.Items.Add(new ListItem(anioActual.ToString(), anioActual.ToString()));
            ddlAltaCiclo.Items.Add(new ListItem((anioActual + 1).ToString(), (anioActual + 1).ToString()));

            // TODO: cargar desde la tabla Turnos.
            ddlAltaTurno.Items.Add(new ListItem("Seleccioná un turno", "0"));
            ddlAltaTurno.Items.Add(new ListItem("Mañana", "1"));
            ddlAltaTurno.Items.Add(new ListItem("Tarde", "2"));
            ddlAltaTurno.Items.Add(new ListItem("Noche", "3"));
        }

        protected void btnNuevoCurso_Click(object sender, EventArgs e)
        {
            int tipo = ObtenerTipoInstitucion();
            bool esIdiomas = tipo == TipoInstitutoIdiomas;

            // Idiomas: solo nombre personalizado. Resto: nivel + año (solo los niveles que ofrece la institución).
            pnlAltaEstandar.Visible = !esIdiomas;
            pnlAltaPersonalizado.Visible = esIdiomas;

            ddlAltaNivel.Items.Clear();
            ddlAltaNivel.Items.Add(new ListItem("Seleccioná un nivel", "0"));
            foreach (int nivel in NivelesPermitidos(tipo))
                ddlAltaNivel.Items.Add(new ListItem(NombreNivel(nivel), nivel.ToString()));

            ddlAltaCiclo.SelectedIndex = 0;
            ddlAltaTurno.SelectedIndex = 0;
            txtAltaAnio.Text = string.Empty;
            txtAltaPersonalizado.Text = string.Empty;
            txtAltaDivision.Text = string.Empty;
            txtAltaCapacidad.Text = string.Empty;
            txtAltaInscriptos.Text = "0";

            pnlMensaje.Visible = false;
            pnlDashboard.Visible = false;
            pnlListado.Visible = false;
            pnlAlta.Visible = true;
        }

        protected void btnCancelarAlta_Click(object sender, EventArgs e)
        {
            VolverAlListado();
        }

        protected void btnGuardarCurso_Click(object sender, EventArgs e)
        {
            int tipo = ObtenerTipoInstitucion();
            bool esIdiomas = tipo == TipoInstitutoIdiomas;

            int ciclo = int.Parse(ddlAltaCiclo.SelectedValue);
            int idTurno = int.Parse(ddlAltaTurno.SelectedValue);
            string division = txtAltaDivision.Text.Trim();

            if (idTurno == 0)
            {
                MostrarMensaje("Seleccioná un turno.", false);
                return;
            }

            // Curso estándar (nivel + año) o personalizado (solo idiomas): nunca los dos a la vez.
            int idNivel = 0, anio = 0;
            string personalizado = null;

            if (esIdiomas)
            {
                personalizado = txtAltaPersonalizado.Text.Trim();
                if (personalizado == string.Empty)
                {
                    MostrarMensaje("Ingresá el nombre del curso.", false);
                    return;
                }
                if (personalizado.Length > 30)
                {
                    MostrarMensaje("El nombre del curso no puede superar los 30 caracteres.", false);
                    return;
                }
            }
            else
            {
                idNivel = int.Parse(ddlAltaNivel.SelectedValue);
                if (idNivel == 0 || Array.IndexOf(NivelesPermitidos(tipo), idNivel) < 0)
                {
                    MostrarMensaje("Seleccioná un nivel válido para tu institución.", false);
                    return;
                }

                int minimo, maximo;
                RangoAnios(idNivel, out minimo, out maximo);
                if (!int.TryParse(txtAltaAnio.Text, out anio) || anio < minimo || anio > maximo)
                {
                    MostrarMensaje("El año para " + NombreNivel(idNivel) + " debe estar entre " + minimo + " y " + maximo + ".", false);
                    return;
                }
            }

            if (division == string.Empty)
            {
                MostrarMensaje("Ingresá la división (si hay una sola, completá con 1).", false);
                return;
            }
            if (division.Length > 50)
            {
                MostrarMensaje("La división no puede superar los 50 caracteres.", false);
                return;
            }

            int capacidad, inscriptos;
            if (!int.TryParse(txtAltaCapacidad.Text, out capacidad) || capacidad < 1 || capacidad > short.MaxValue)
            {
                MostrarMensaje("Ingresá una capacidad válida (mayor a 0).", false);
                return;
            }
            if (!int.TryParse(txtAltaInscriptos.Text, out inscriptos) || inscriptos < 0 || inscriptos > capacidad)
            {
                MostrarMensaje("Los inscriptos deben estar entre 0 y la capacidad (" + capacidad + ").", false);
                return;
            }
            /*
            int? nivelParam = esIdiomas ? (int?)null : idNivel;
            int? anioParam = esIdiomas ? (int?)null : anio;
            string nombre = ArmarNombre(nivelParam, anioParam, personalizado, division);
            string turno = ddlAltaTurno.SelectedItem.Text;
            DataTable cursos = ObtenerCursos();
            int nuevoId;

            if (UsarBaseDeDatos)
            {
                try
                {
                    // El procedimiento vuelve a validar todo y rechaza duplicados.
                    DataTable creado = new CursoNegocio().Registrar(ObtenerIdAdministradorInstitucion(),
                        idTurno, ciclo, nivelParam, anioParam, personalizado, division, capacidad, inscriptos);
                    nuevoId = Convert.ToInt32(creado.Rows[0]["Id"]);
                }
                catch (ReglaDeNegocioException ex)
                {
                    MostrarMensaje(ex.Message, false);
                    return;
                }
                catch (Exception)
                {
                    // TODO: registrar el error en tu sistema de logs.
                    MostrarMensaje("No se pudo crear el curso. Intentá nuevamente.", false);
                    return;
                }
            }
            else
            {
                // Modo demo: misma regla de duplicados que el procedimiento (año calendario, turno y nombre).
                foreach (DataRow existente in cursos.Rows)
                {
                    if ((int)existente["CicloLectivo"] == ciclo
                        && existente["Turno"].ToString() == turno
                        && string.Equals(existente["NombreCurso"].ToString(), nombre, StringComparison.OrdinalIgnoreCase))
                    {
                        MostrarMensaje("El curso ya se encuentra registrado.", false);
                        return;
                    }
                }
                nuevoId = cursos.Rows.Count == 0 ? 1 : ((int)cursos.Compute("MAX(Id)", "") + 1);
            }

            // El listado todavía es de ejemplo: se agrega la fila para que se vea. Al leer de la base, borrar esta línea.
            cursos.Rows.Add(nuevoId, nombre, ciclo, turno,
                esIdiomas ? (object)DBNull.Value : idNivel, capacidad, inscriptos);

            // Deja el filtro en el año del curso nuevo para que se vea en el listado.
            ddlCicloLectivo.SelectedValue = ciclo.ToString();
            ddlNivel.SelectedIndex = 0;

            VolverAlListado();
            MostrarMensaje("Curso creado: " + nombre + " (" + turno + ", " + ciclo + ") con " + (capacidad - inscriptos) + " vacantes.", true);
        */
        }

        private void VolverAlListado()
        {
            pnlEdicion.Visible = false;
            pnlAlta.Visible = false;
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
        private static string ArmarNombre(int? idNivel, int? anio, string nivelPersonalizado, string division)
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