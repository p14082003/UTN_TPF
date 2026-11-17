using Dominio;
using Negocio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Presentacion
{
    public partial class ListadoInstituciones : Page
    {
        protected const string ImagenGenerica = "https://placehold.co/400x250";
        public string InstitucionesJson { get; private set; } = "[]";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                foreach (string tipo in Discapacidad.ObtenerTiposDiscapacidad())
                {
                    ddlDiscapacidades.Items.Add(new ListItem(tipo, tipo));
                }
            }

            RegisterAsyncTask(new PageAsyncTask(CargarListadoAsync));
        }

        private async Task CargarListadoAsync()
        {
            int idIdioma = int.Parse(ddlIdiomas.SelectedValue);
            List<InstitucionListado> instituciones;
            List<IdiomaInstitucion> relaciones = new List<IdiomaInstitucion>();

            try
            {
                if (ddlIdiomas.Items.Count == 1)
                {
                    List<Idioma> idiomas = await InstiRedApi.GetAsync<List<Idioma>>("ent/Idiomas");
                    ddlIdiomas.DataSource = idiomas;
                    ddlIdiomas.DataValueField = "Id";
                    ddlIdiomas.DataTextField = "NombreIdioma";
                    ddlIdiomas.DataBind();
                }

                instituciones = await InstiRedApi.GetAsync<List<InstitucionListado>>("vw/Instituciones?estado=true");

                if (idIdioma != 0)
                {
                    relaciones = await InstiRedApi.GetAsync<List<IdiomaInstitucion>>("ent/IdiomaInstitucion");
                }
            }
            catch (Exception ex)
            {
                Trace.Warn("ListadoInstituciones", "Error al consultar la API de InstiRed.", ex);
                lblListado.Text = "No pudimos cargar las instituciones. Intentá nuevamente.";
                lblListado.Visible = true;
                return;
            }

            string texto = txtFiltro.Text.Trim();
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

            InstitucionesJson = new JavaScriptSerializer().Serialize(
                lista.Select(i => new { Id = i.IdInstitucion, i.Nombre, i.Direccion, i.Latitud, i.Longitud }));
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
    }
}
