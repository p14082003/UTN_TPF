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
    public partial class GestionarVacante : Page
    {
        private UsuarioSesion usuario;
        private InstitucionApi institucion;
        private List<CatalogoApi> niveles;
        private List<CatalogoApi> turnos;
        private bool operacionGuardada;
        private const int TipoInstitutoIdiomas = 4;

        private int IdCursoSeleccionado
        {
            get { return (int?)ViewState["IdCursoSeleccionado"] ?? 0; }
            set { ViewState["IdCursoSeleccionado"] = value; }
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
            if (!IsPostBack)
            {
                btnNuevoCurso.Enabled = false;
                Ejecutar(async () =>
                {
                    CargarFiltros();
                    await CargarListadoAsync();
                });
            }
        }

        private void Ejecutar(Func<Task> accion)
        {
            pnlMensaje.Visible = false;
            RegisterAsyncTask(new PageAsyncTask(async () =>
            {
                try
                {
                    await CargarContextoAsync();
                    await accion();
                }
                catch (Exception ex)
                {
                    Trace.Warn("GestionarVacante", "Error al consultar o guardar cursos en la API.", ex);
                    string mensaje = ex is ArgumentException ? ex.Message : "No se pudo completar la operación. Intentá nuevamente.";
                    if (operacionGuardada)
                    {
                        mensaje = "El cambio se guardó, pero no se pudo actualizar el listado. Recargá la página para verlo.";
                    }
                    MostrarMensaje(mensaje, false);
                }
            }));
        }

        private async Task CargarContextoAsync()
        {
            var admin = await InstiRedApi.GetAsync<AdministradorApi>(
                "ent/AdministradoresInstitucion/" + usuario.IdAdministradorInstitucion.Value);
            if (admin.IdUsuario != usuario.IdUsuario || admin.IdInstitucion.GetValueOrDefault() <= 0)
            {
                throw new ArgumentException("Tu cuenta no tiene una institución asociada para gestionar cursos.");
            }

            usuario.IdInstitucion = admin.IdInstitucion;
            Session[UsuarioSesion.Clave] = usuario;
            institucion = await InstiRedApi.GetAsync<InstitucionApi>("ent/Instituciones/" + usuario.IdInstitucion.Value);
            niveles = await InstiRedApi.GetAsync<List<CatalogoApi>>("ent/NivelesEscolares");
            turnos = await InstiRedApi.GetAsync<List<CatalogoApi>>("ent/Turnos");

            btnNuevoCurso.Enabled = institucion.Estado;
            btnGuardar.Enabled = institucion.Estado;
            btnGuardarCurso.Enabled = institucion.Estado;
            dgvCursos.Enabled = institucion.Estado;
            if (!institucion.Estado)
            {
                MostrarMensaje("La institución está dada de baja. Podés consultar sus cursos.", false);
            }
        }

        private void CargarFiltros()
        {
            ddlNivel.Items.Clear();
            ddlNivel.Items.Add(new ListItem("Todos", "0"));
            foreach (var nivel in niveles)
            {
                ddlNivel.Items.Add(new ListItem(nivel.Descripcion, nivel.Id.ToString()));
            }
        }

        private async Task CargarListadoAsync()
        {
            var cursos = await InstiRedApi.GetAsync<List<CursoApi>>(
                "ent/Cursos?idInstitucion=" + usuario.IdInstitucion.Value);
            cursos = cursos.Where(c => c.IdInstitucion == usuario.IdInstitucion).ToList();
            foreach (var curso in cursos)
            {
                curso.Turno = turnos.FirstOrDefault(t => t.Id == curso.IdTurno)?.Descripcion ?? "Sin especificar";
            }

            int ciclo = DateTime.Today.Year;
            if (int.TryParse(ddlCicloLectivo.SelectedValue, out int seleccionado))
            {
                ciclo = seleccionado;
            }
            var ciclos = cursos.Select(c => c.CicloLectivo)
                .Concat(new[] { DateTime.Today.Year - 1, DateTime.Today.Year, DateTime.Today.Year + 1, ciclo })
                .Distinct().OrderBy(c => c);
            ddlCicloLectivo.Items.Clear();
            foreach (int anio in ciclos)
            {
                ddlCicloLectivo.Items.Add(new ListItem(anio.ToString(), anio.ToString()));
            }
            ddlCicloLectivo.SelectedValue = ciclo.ToString();

            int idNivel = int.Parse(ddlNivel.SelectedValue);
            var vista = cursos.Where(c => c.CicloLectivo == ciclo && (idNivel == 0 || c.IdNivel == idNivel))
                .OrderBy(c => c.NombreCurso).ToList();
            dgvCursos.DataSource = vista;
            dgvCursos.DataBind();
            lblTotalCursos.Text = vista.Count.ToString();
            lblConVacantes.Text = vista.Count(c => c.Vacantes > 0).ToString();
            lblSinVacantes.Text = vista.Count(c => c.Vacantes == 0).ToString();
        }

        protected void Filtro_SelectedIndexChanged(object sender, EventArgs e)
        {
            Ejecutar(CargarListadoAsync);
        }

        protected void btnEditar_Click(object sender, EventArgs e)
        {
            IdCursoSeleccionado = int.Parse(((LinkButton)sender).CommandArgument);
            Ejecutar(async () =>
            {
                var curso = await ObtenerCursoAsync();
                lblCursoNombre.Text = Server.HtmlEncode(curso.NombreCurso);
                lblCicloLectivo.Text = curso.CicloLectivo.ToString();
                lblTurno.Text = Server.HtmlEncode(turnos.FirstOrDefault(t => t.Id == curso.IdTurno)?.Descripcion);
                lblCapacidad.Text = curso.Capacidad.ToString();
                lblInscriptos.Text = curso.Inscriptos.ToString();
                txtVacantes.Text = curso.Vacantes.ToString();
                txtVacantes.Attributes["min"] = "0";
                txtVacantes.Attributes["max"] = (short.MaxValue - curso.Inscriptos).ToString();
                MostrarVista("edicion");
            });
        }

        private async Task<CursoApi> ObtenerCursoAsync()
        {
            var curso = await InstiRedApi.GetAsync<CursoApi>(
                "ent/Cursos/" + IdCursoSeleccionado, permitirNoEncontrado: true);
            if (curso == null || curso.IdInstitucion != usuario.IdInstitucion)
            {
                throw new ArgumentException("El curso no está disponible en tu institución.");
            }
            return curso;
        }

        protected void btnCerrarEdicion_Click(object sender, EventArgs e)
        {
            Ejecutar(VolverAlListadoAsync);
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtVacantes.Text, out int vacantes) || vacantes < 0 || vacantes > short.MaxValue)
            {
                MostrarMensaje("Ingresá un número de vacantes entre 0 y 32767.", false);
                return;
            }

            Ejecutar(async () =>
            {
                var curso = await ObtenerCursoAsync();
                var respuesta = await InstiRedApi.PatchAsync("ent/Cursos/" + curso.Id + "/vacantes", new
                {
                    idAdministrador = usuario.IdAdministradorInstitucion.Value,
                    vacantes
                });
                ValidarRespuesta(respuesta, HttpStatusCode.OK);
                operacionGuardada = true;
                await VolverAlListadoAsync();
                MostrarMensaje("Vacantes actualizadas. " + curso.NombreCurso + " tiene " + vacantes + " disponibles.", true);
            });
        }

        private void CargarCombosAlta()
        {
            int anioActual = DateTime.Today.Year;
            ddlAltaCiclo.Items.Clear();
            ddlAltaCiclo.Items.Add(new ListItem(anioActual.ToString(), anioActual.ToString()));
            ddlAltaCiclo.Items.Add(new ListItem((anioActual + 1).ToString(), (anioActual + 1).ToString()));
            if (institucion.IdTipoInstitucion != TipoInstitutoIdiomas &&
                DateTime.Today > new DateTime(anioActual, 2, 1))
            {
                ddlAltaCiclo.SelectedIndex = 1;
            }

            ddlAltaTurno.Items.Clear();
            ddlAltaTurno.Items.Add(new ListItem("Seleccioná un turno", "0"));
            foreach (var turno in turnos)
            {
                ddlAltaTurno.Items.Add(new ListItem(turno.Descripcion, turno.Id.ToString()));
            }
            ddlAltaNivel.Items.Clear();
            ddlAltaNivel.Items.Add(new ListItem("Seleccioná un nivel", "0"));
            foreach (var nivel in niveles.Where(n => NivelesPermitidos(institucion.IdTipoInstitucion).Contains(n.Id)))
            {
                ddlAltaNivel.Items.Add(new ListItem(nivel.Descripcion, nivel.Id.ToString()));
            }
        }

        protected void btnNuevoCurso_Click(object sender, EventArgs e)
        {
            Ejecutar(() =>
            {
                CargarCombosAlta();
                bool esIdiomas = institucion.IdTipoInstitucion == TipoInstitutoIdiomas;
                pnlAltaEstandar.Visible = !esIdiomas;
                pnlAltaPersonalizado.Visible = esIdiomas;
                txtAltaAnio.Text = "";
                txtAltaPersonalizado.Text = "";
                txtAltaDivision.Text = "";
                txtAltaCapacidad.Text = "";
                txtAltaInscriptos.Text = "0";
                MostrarVista("alta");
                return Task.CompletedTask;
            });
        }

        protected void btnCancelarAlta_Click(object sender, EventArgs e)
        {
            Ejecutar(VolverAlListadoAsync);
        }

        protected void btnGuardarCurso_Click(object sender, EventArgs e)
        {
            Ejecutar(RegistrarCursoAsync);
        }

        private async Task RegistrarCursoAsync()
        {
            if (!institucion.Estado)
            {
                throw new ArgumentException("La institución debe estar activa para crear cursos.");
            }

            int ciclo = int.Parse(ddlAltaCiclo.SelectedValue);
            int idTurno = int.Parse(ddlAltaTurno.SelectedValue);
            if (idTurno <= 0 || !turnos.Any(t => t.Id == idTurno))
            {
                throw new ArgumentException("Seleccioná un turno válido.");
            }

            bool esIdiomas = institucion.IdTipoInstitucion == TipoInstitutoIdiomas;
            int? idNivel = null;
            int? anio = null;
            string personalizado = null;
            if (esIdiomas)
            {
                personalizado = txtAltaPersonalizado.Text.Trim();
                if (personalizado == "" || personalizado.Length > 30)
                {
                    throw new ArgumentException("Ingresá un nombre de curso de hasta 30 caracteres.");
                }
            }
            else
            {
                idNivel = int.Parse(ddlAltaNivel.SelectedValue);
                if (!NivelesPermitidos(institucion.IdTipoInstitucion).Contains(idNivel.Value))
                {
                    throw new ArgumentException("Seleccioná un nivel que ofrezca tu institución.");
                }

                int minimo, maximo;
                RangoAnios(idNivel.Value, out minimo, out maximo);
                if (!int.TryParse(txtAltaAnio.Text, out int anioIngresado) || anioIngresado < minimo || anioIngresado > maximo)
                {
                    throw new ArgumentException("El año para ese nivel debe estar entre " + minimo + " y " + maximo + ".");
                }
                anio = anioIngresado;
            }

            string division = txtAltaDivision.Text.Trim();
            if (division == "" || division.Length > 50)
            {
                throw new ArgumentException("Ingresá una división de hasta 50 caracteres.");
            }
            if (!int.TryParse(txtAltaCapacidad.Text, out int capacidad) || capacidad < 1 || capacidad > short.MaxValue)
            {
                throw new ArgumentException("Ingresá una capacidad entre 1 y 32767.");
            }
            if (!int.TryParse(txtAltaInscriptos.Text, out int inscriptos) || inscriptos < 0 || inscriptos > capacidad)
            {
                throw new ArgumentException("Los inscriptos deben estar entre 0 y la capacidad del curso.");
            }

            var respuesta = await InstiRedApi.PostAsync("sp/RegistrarCurso", new
            {
                idAdministradorInstitucion = usuario.IdAdministradorInstitucion.Value,
                idTurno,
                cicloLectivo = ciclo,
                idNivel,
                anio,
                nivelPersonalizado = personalizado,
                division,
                capacidad,
                inscriptos
            });
            ValidarRespuesta(respuesta, HttpStatusCode.Created);
            operacionGuardada = true;
            btnGuardarCurso.Enabled = false;
            ddlCicloLectivo.SelectedValue = ciclo.ToString();
            ddlNivel.SelectedValue = "0";
            await VolverAlListadoAsync();
            MostrarMensaje("Curso creado correctamente.", true);
        }

        private static void ValidarRespuesta(InstiRedApi.Respuesta respuesta, HttpStatusCode esperado)
        {
            if (respuesta.Estado != esperado)
            {
                throw new ArgumentException(respuesta.Mensaje ?? "La API no pudo guardar el cambio. Intentá nuevamente.");
            }
        }

        private async Task VolverAlListadoAsync()
        {
            await CargarListadoAsync();
            MostrarVista("listado");
        }

        private void MostrarVista(string vista)
        {
            pnlEdicion.Visible = vista == "edicion";
            pnlAlta.Visible = vista == "alta";
            pnlDashboard.Visible = vista == "listado";
            pnlListado.Visible = vista == "listado";
        }

        private void MostrarMensaje(string texto, bool exito)
        {
            pnlMensaje.Visible = true;
            pnlMensaje.CssClass = exito ? "alert alert-success mb-4" : "alert alert-danger mb-4";
            litMensaje.Text = Server.HtmlEncode(texto);
        }

        private static int[] NivelesPermitidos(int tipo)
        {
            if (tipo >= 1 && tipo <= 3)
            {
                return new[] { tipo };
            }
            if (tipo == 5)
            {
                return new[] { 1, 2 };
            }
            if (tipo == 6)
            {
                return new[] { 2, 3 };
            }
            if (tipo == 7)
            {
                return new[] { 1, 2, 3 };
            }
            if (tipo == 8)
            {
                return new[] { 1, 3 };
            }
            return new int[0];
        }

        private static void RangoAnios(int nivel, out int minimo, out int maximo)
        {
            minimo = nivel == 1 ? 3 : 1;
            if (nivel == 1)
            {
                maximo = 5;
            }
            else if (nivel == 2)
            {
                maximo = 7;
            }
            else
            {
                maximo = 6;
            }
        }

        private static string ArmarNombre(int? idNivel, int? anio, string personalizado, string division)
        {
            string nombre;
            if (!string.IsNullOrWhiteSpace(personalizado))
            {
                nombre = personalizado;
            }
            else if (idNivel == 1)
            {
                nombre = "Salita de " + anio;
            }
            else if (idNivel == 2)
            {
                nombre = anio + "° grado";
            }
            else if (idNivel == 3)
            {
                nombre = anio + "° año";
            }
            else
            {
                nombre = "Curso";
            }
            return nombre + " - " + division;
        }

        private class AdministradorApi
        {
            public int IdUsuario { get; set; }
            public int? IdInstitucion { get; set; }
        }

        private class InstitucionApi
        {
            public int IdTipoInstitucion { get; set; }
            public bool Estado { get; set; }
        }

        private class CatalogoApi
        {
            public int Id { get; set; }
            public string Descripcion { get; set; }
        }

        private class CursoApi
        {
            public int Id { get; set; }
            public int IdInstitucion { get; set; }
            public int IdTurno { get; set; }
            public int CicloLectivo { get; set; }
            public int? IdNivel { get; set; }
            public int? Anio { get; set; }
            public string NivelPersonalizado { get; set; }
            public string Division { get; set; }
            public int Capacidad { get; set; }
            public int Inscriptos { get; set; }
            public string Turno { get; set; }
            public int Vacantes => Capacidad - Inscriptos;
            public string NombreCurso => ArmarNombre(IdNivel, Anio, NivelPersonalizado, Division);
        }
    }
}
