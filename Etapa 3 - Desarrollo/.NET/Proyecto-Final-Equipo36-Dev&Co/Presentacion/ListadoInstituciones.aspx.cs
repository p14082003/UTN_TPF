using Dominio;
using Negocio;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Presentacion
{
    public partial class ListadoInstituciones : Page
    {
        protected const string ImagenGenerica = "Imagenes/Institucion.png";
        public string InstitucionesJson { get; private set; } = "[]";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                foreach (string tipo in Discapacidad.ObtenerTiposDiscapacidad())
                {
                    ddlDiscapacidades.Items.Add(new ListItem(tipo, tipo));
                }
                int anioActual = DateTime.Today.Year;
                ddlCicloLectivo.Items.Add(new ListItem(anioActual.ToString(), anioActual.ToString()));
                ddlCicloLectivo.Items.Add(new ListItem((anioActual + 1).ToString(), (anioActual + 1).ToString()));
            }

            RegisterAsyncTask(new PageAsyncTask(CargarListadoAsync));
        }

        private async Task CargarListadoAsync()
        {
            if (Request.HttpMethod == "POST")
            {
                RedirigirBusqueda();
                return;
            }
            List<InstitucionListado> instituciones;
            List<IdiomaInstitucion> relaciones = new List<IdiomaInstitucion>();

            try
            {
                if (ddlOfertaEducativa.Items.Count == 1)
                {
                    await CargarCatalogosAsync();
                }
                RestaurarFiltros();

                instituciones = await InstiRedApi.GetAsync<List<InstitucionListado>>(CrearRutaInstituciones());

                if (ddlIdiomas.SelectedValue != "0")
                {
                    relaciones = await InstiRedApi.GetAsync<List<IdiomaInstitucion>>("ent/IdiomaInstitucion");
                }
            }
            catch (Exception ex)
            {
                Trace.Warn("ListadoInstituciones", "Error al consultar la API de InstiRed.", ex);
                lblListado.Text = "No pudimos cargar las instituciones. Intentá nuevamente.";
                lblListado.Visible = true;
                lblCantidad.Text = "No se pudo completar la búsqueda";
                Response.StatusCode = 502;
                Response.TrySkipIisCustomErrors = true;
                rptInstituciones.DataSource = null;
                rptInstituciones.DataBind();
                InstitucionesJson = "[]";
                return;
            }

            string texto = txtFiltro.Text.Trim();
            int idIdioma = int.Parse(ddlIdiomas.SelectedValue);
            string tipoDiscapacidad = ddlDiscapacidades.SelectedValue;
            var idsPorIdioma = new HashSet<int>(relaciones
                .Where(relacion => relacion.IdIdioma == idIdioma)
                .Select(relacion => relacion.IdInstitucion));

            var lista = new List<InstitucionListado>();
            foreach (InstitucionListado institucion in instituciones)
            {
                if (texto != "" && !CoincideTexto(institucion, texto))
                {
                    continue;
                }

                if (tipoDiscapacidad != "0" && !TieneAccesibilidad(institucion, tipoDiscapacidad))
                {
                    continue;
                }

                if (idIdioma != 0 && !idsPorIdioma.Contains(institucion.IdInstitucion))
                {
                    continue;
                }

                lista.Add(institucion);
            }

            if (lista.Count > 0)
            {
                await CargarFotosAsync(lista);
            }

            rptInstituciones.DataSource = lista;
            rptInstituciones.DataBind();
            lblListado.Text = "No encontramos instituciones con esos filtros.";
            lblListado.Visible = lista.Count == 0;
            lblCantidad.Text = lista.Count + (lista.Count == 1 ? " institución encontrada" : " instituciones encontradas");

            InstitucionesJson = new JavaScriptSerializer().Serialize(
                lista.Select(i => new { Id = i.IdInstitucion, i.Nombre, i.Direccion, i.Latitud, i.Longitud }));
        }

        private async Task CargarCatalogosAsync()
        {
            var niveles = InstiRedApi.GetAsync<List<CatalogoApi>>("ent/NivelesEscolares");
            var turnos = InstiRedApi.GetAsync<List<CatalogoApi>>("ent/Turnos");
            var idiomas = InstiRedApi.GetAsync<List<Idioma>>("ent/Idiomas");
            await Task.WhenAll(niveles, turnos, idiomas);

            foreach (var nivel in niveles.Result)
            {
                ddlOfertaEducativa.Items.Add(new ListItem(nivel.Descripcion, nivel.Id.ToString()));
            }
            ddlOfertaEducativa.Items.Add(new ListItem("Instituto de idiomas", "idiomas"));
            foreach (var turno in turnos.Result)
            {
                ddlTurno.Items.Add(new ListItem(turno.Descripcion, turno.Id.ToString()));
            }
            ddlIdiomas.DataSource = idiomas.Result;
            ddlIdiomas.DataValueField = "Id";
            ddlIdiomas.DataTextField = "NombreIdioma";
            ddlIdiomas.DataBind();
        }

        protected void OfertaEducativa_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarAnios();
        }

        private void CargarAnios()
        {
            ddlAnio.Items.Clear();
            string nivel = ddlOfertaEducativa.SelectedValue;
            ddlAnio.Enabled = nivel == "1" || nivel == "2" || nivel == "3";
            ddlAnio.Items.Add(new ListItem(ddlAnio.Enabled ? "Todos" : "Elegí un nivel escolar", "0"));
            if (!ddlAnio.Enabled)
            {
                return;
            }

            int minimo = nivel == "1" ? 3 : 1;
            int maximo = nivel == "1" ? 5 : nivel == "2" ? 7 : 6;
            for (int anio = minimo; anio <= maximo; anio++)
            {
                string texto = nivel == "1" ? "Sala de " + anio + " años"
                    : anio + (nivel == "2" ? "° grado" : "° año");
                ddlAnio.Items.Add(new ListItem(texto, anio.ToString()));
            }
        }

        private void RestaurarFiltros()
        {
            string texto = Request.QueryString["texto"] ?? "";
            txtFiltro.Text = texto.Substring(0, Math.Min(texto.Length, txtFiltro.MaxLength));
            SeleccionarFiltro(ddlOfertaEducativa, "oferta");
            CargarAnios();
            SeleccionarFiltro(ddlAnio, "anio");
            SeleccionarFiltro(ddlCicloLectivo, "cicloLectivo");
            SeleccionarFiltro(ddlTurno, "idTurno");
            SeleccionarFiltro(ddlIdiomas, "idIdioma");
            SeleccionarFiltro(ddlDiscapacidades, "accesibilidad");
            SeleccionarFiltro(ddlOrdenar, "orden");
            chkSoloConVacantes.Checked = Request.QueryString["soloConVacantes"] == "true";
            hfFiltrosAvanzados.Value = Request.QueryString["avanzados"] == "true" ? "true" : "false";
        }

        private void SeleccionarFiltro(DropDownList control, string parametro)
        {
            string valor = Request.QueryString[parametro];
            if (valor != null && control.Items.FindByValue(valor) != null)
            {
                control.SelectedValue = valor;
            }
        }

        private void RedirigirBusqueda()
        {
            string filtros = CrearParametrosBusqueda().ToString();
            string destino = ResolveUrl("~/ListadoInstituciones.aspx") + (filtros == "" ? "" : "?" + filtros);
            Response.Redirect(destino, false);
            Response.StatusCode = 303;
            Context.ApplicationInstance.CompleteRequest();
        }

        private NameValueCollection CrearParametrosBusqueda()
        {
            var filtros = HttpUtility.ParseQueryString("");
            if (!string.IsNullOrWhiteSpace(txtFiltro.Text))
            {
                filtros["texto"] = txtFiltro.Text.Trim();
            }
            AgregarParametroBusqueda(filtros, "oferta", ddlOfertaEducativa);
            AgregarParametroBusqueda(filtros, "anio", ddlAnio);
            AgregarParametroBusqueda(filtros, "cicloLectivo", ddlCicloLectivo);
            AgregarParametroBusqueda(filtros, "idTurno", ddlTurno);
            AgregarParametroBusqueda(filtros, "idIdioma", ddlIdiomas);
            AgregarParametroBusqueda(filtros, "accesibilidad", ddlDiscapacidades);
            if (ddlOrdenar.SelectedValue != "nombre") filtros["orden"] = ddlOrdenar.SelectedValue;
            if (chkSoloConVacantes.Checked) filtros["soloConVacantes"] = "true";
            if (hfFiltrosAvanzados.Value == "true") filtros["avanzados"] = "true";
            return filtros;
        }

        private static void AgregarParametroBusqueda(NameValueCollection filtros, string nombre, DropDownList control)
        {
            if (!string.IsNullOrEmpty(control.SelectedValue) && control.SelectedValue != "0")
            {
                filtros[nombre] = control.SelectedValue;
            }
        }

        private string CrearRutaInstituciones()
        {
            var ruta = new StringBuilder("vw/Instituciones?estado=true");
            if (ddlOfertaEducativa.SelectedValue == "idiomas")
            {
                ruta.Append("&idTipoInstitucion=4");
            }
            else
            {
                AgregarFiltro(ruta, "idNivel", ddlOfertaEducativa.SelectedValue);
            }
            AgregarFiltro(ruta, "anio", ddlAnio.SelectedValue);
            AgregarFiltro(ruta, "cicloLectivo", ddlCicloLectivo.SelectedValue);
            AgregarFiltro(ruta, "idTurno", ddlTurno.SelectedValue);
            if (chkSoloConVacantes.Checked)
            {
                ruta.Append("&soloConVacantes=true");
            }
            ruta.Append("&orden=" + Uri.EscapeDataString(ddlOrdenar.SelectedValue));
            return ruta.ToString();
        }

        private static void AgregarFiltro(StringBuilder ruta, string nombre, string valor)
        {
            if (!string.IsNullOrEmpty(valor) && valor != "0")
            {
                ruta.Append("&" + nombre + "=" + Uri.EscapeDataString(valor));
            }
        }

        private async Task CargarFotosAsync(List<InstitucionListado> instituciones)
        {
            try
            {
                var imagenes = await InstiRedApi.ListarImagenesAsync();
                var fotosPorInstitucion = imagenes.ToLookup(imagen => imagen.IdInstitucion);

                foreach (InstitucionListado institucion in instituciones)
                {
                    var foto = fotosPorInstitucion[institucion.IdInstitucion].FirstOrDefault();
                    if (foto != null)
                    {
                        institucion.UrlFoto = foto.UrlPublica;
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.Warn("ListadoInstituciones", "Error al consultar las fotos de las instituciones.", ex);
            }
        }

        private static bool CoincideTexto(InstitucionListado institucion, string texto)
        {
            return (institucion.Nombre ?? "").IndexOf(texto, StringComparison.CurrentCultureIgnoreCase) >= 0
                || (institucion.Direccion ?? "").IndexOf(texto, StringComparison.CurrentCultureIgnoreCase) >= 0;
        }

        private static bool TieneAccesibilidad(InstitucionListado institucion, string tipo)
        {
            var discapacidad = new Discapacidad
            {
                Movilidad = institucion.Movilidad == true,
                Visual = institucion.Visual == true,
                Auditiva = institucion.Auditiva == true,
                Verbal = institucion.Verbal == true,
                Intelectual = institucion.Intelectual == true,
                SindromeDown = institucion.SindromeDown == true,
                Autismo = institucion.Autismo == true
            };

            return discapacidad.ObtenerAccesibilidades().Contains(tipo);
        }

        private class InstitucionListado
        {
            public int IdInstitucion { get; set; }
            public string Nombre { get; set; }
            public string Direccion { get; set; }
            public string Descripcion { get; set; }
            public decimal? PromedioCalificacion { get; set; }
            public long CantidadResenias { get; set; }
            public string Valoracion => PromedioCalificacion.HasValue && CantidadResenias > 0
                ? "★ " + PromedioCalificacion.Value.ToString("0.0") + " / 5 · " +
                    CantidadResenias + (CantidadResenias == 1 ? " reseña" : " reseñas")
                : "Sin reseñas";
            public string UrlFoto { get; set; } = ImagenGenerica;
            public double Latitud { get; set; }
            public double Longitud { get; set; }
            public bool? Movilidad { get; set; }
            public bool? Visual { get; set; }
            public bool? Auditiva { get; set; }
            public bool? Verbal { get; set; }
            public bool? Intelectual { get; set; }
            public bool? SindromeDown { get; set; }
            public bool? Autismo { get; set; }
        }

        private class IdiomaInstitucion
        {
            public int IdInstitucion { get; set; }
            public int IdIdioma { get; set; }
        }

        private class CatalogoApi
        {
            public int Id { get; set; }
            public string Descripcion { get; set; }
        }
    }
}
