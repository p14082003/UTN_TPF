using Dominio;
using Negocio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;

namespace Presentacion
{
    public partial class DetalleInstitucion : Page
    {
        protected string InstitucionJson { get; private set; } = "null";
        protected int ImagenesVisibles { get; private set; }
        protected string UrlFotoUnica { get; private set; }
        protected string DescripcionFotoUnica { get; private set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            int id;
            if (!int.TryParse(Request.QueryString["id"], out id) || id <= 0)
            {
                MostrarError("Elegí una institución desde el listado.", 400);
                return;
            }

            RegisterAsyncTask(new PageAsyncTask(() => CargarDetalleAsync(id)));
        }

        private async Task CargarDetalleAsync(int id)
        {
            try
            {
                var institucion = await InstiRedApi.GetAsync<InstitucionApi>("vw/Instituciones/" + id, permitirNoEncontrado: true);
                if (institucion == null || !institucion.Estado)
                {
                    MostrarError("No encontramos la institución solicitada.", 404);
                    return;
                }

                CargarFicha(institucion);
                await Task.WhenAll(CargarReseniasAsync(id), CargarImagenesAsync(id));
                pnlDetalle.Visible = true;
            }
            catch (Exception ex)
            {
                Trace.Warn("DetalleInstitucion", "Error al consultar la API de InstiRed.", ex);
                MostrarError("No pudimos cargar el detalle de la institución. Intentá nuevamente.", 502);
            }
        }

        private void CargarFicha(InstitucionApi institucion)
        {
            lblNombreInstitucion.Text = HttpUtility.HtmlEncode(institucion.Nombre);
            lblDescripcion.Text = HttpUtility.HtmlEncode(institucion.Descripcion);
            lblTipoInstitucion.Text = HttpUtility.HtmlEncode(institucion.TipoInstitucion);
            lblDireccion.Text = HttpUtility.HtmlEncode(institucion.Direccion);
            lblEmail.Text = HttpUtility.HtmlEncode(institucion.Email);

            rptTelefonos.DataSource = SepararValores(institucion.Telefonos);
            rptTelefonos.DataBind();
            rptIdiomas.DataSource = SepararValores(institucion.Idiomas);
            rptIdiomas.DataBind();

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
            rptAccesibilidades.DataSource = discapacidad.ObtenerAccesibilidades();
            rptAccesibilidades.DataBind();

            InstitucionJson = new JavaScriptSerializer().Serialize(
                new { institucion.Nombre, institucion.Direccion, institucion.Latitud, institucion.Longitud });
        }

        private async Task CargarReseniasAsync(int id)
        {
            var resenias = await InstiRedApi.GetAsync<List<ReseniaApi>>("vw/ValoracionesPorInstitucion?idInstitucion=" + id);
            rptResenias.DataSource = resenias.OrderByDescending(resenia => resenia.Fecha);
            rptResenias.DataBind();
            pnlSinResenias.Visible = resenias.Count == 0;
        }

        private async Task CargarImagenesAsync(int id)
        {
            var imagenes = await InstiRedApi.ListarImagenesAsync();
            var imagenesInstitucion = imagenes.Where(imagen => imagen.IdInstitucion == id).ToList();

            if (imagenesInstitucion.Count == 0)
            {
                return;
            }

            ImagenesVisibles = Math.Min(3, imagenesInstitucion.Count);
            pnlImagenes.Visible = true;
            if (ImagenesVisibles == 1)
            {
                UrlFotoUnica = imagenesInstitucion[0].UrlPublica;
                DescripcionFotoUnica = imagenesInstitucion[0].Descripcion;
                return;
            }

            rptImagenes.DataSource = imagenesInstitucion.Concat(imagenesInstitucion.Take(ImagenesVisibles - 1));
            rptImagenes.DataBind();
            rptIndicadores.DataSource = imagenesInstitucion;
            rptIndicadores.DataBind();
        }

        private static string[] SepararValores(string valores)
        {
            return (valores ?? "").Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
        }

        private void MostrarError(string mensaje, int estado)
        {
            InstitucionJson = "null";
            pnlDetalle.Visible = false;
            pnlError.Visible = true;
            lblError.Text = mensaje;
            Response.StatusCode = estado;
            Response.TrySkipIisCustomErrors = true;
        }

        private class InstitucionApi
        {
            public string Nombre { get; set; }
            public string Descripcion { get; set; }
            public string TipoInstitucion { get; set; }
            public string Direccion { get; set; }
            public string Email { get; set; }
            public string Telefonos { get; set; }
            public string Idiomas { get; set; }
            public double Latitud { get; set; }
            public double Longitud { get; set; }
            public bool Estado { get; set; }
            public bool? Movilidad { get; set; }
            public bool? Visual { get; set; }
            public bool? Auditiva { get; set; }
            public bool? Verbal { get; set; }
            public bool? Intelectual { get; set; }
            public bool? SindromeDown { get; set; }
            public bool? Autismo { get; set; }
        }

        private class ReseniaApi
        {
            public string NombreResponsable { get; set; }
            public string ApellidoResponsable { get; set; }
            public int Calificacion { get; set; }
            public string Comentario { get; set; }
            public string RespuestaInstitucion { get; set; }
            public DateTime Fecha { get; set; }
        }

    }
}
