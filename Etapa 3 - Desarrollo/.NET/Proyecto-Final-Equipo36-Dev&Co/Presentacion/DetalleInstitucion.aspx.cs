using Dominio;
using Negocio;
using Presentacion.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Presentacion
{
    public partial class DetalleInstitucion : Page
    {
        protected string InstitucionJson { get; private set; } = "null";
        protected int ImagenesVisibles { get; private set; }
        protected string UrlFotoUnica { get; private set; }
        protected string DescripcionFotoUnica { get; private set; }
        private int idInstitucion;
        private UsuarioSesion usuario;
        private const int ReseniasPorPagina = 10;

        private int PaginaResenias
        {
            get { return (int?)ViewState["PaginaResenias"] ?? 0; }
            set { ViewState["PaginaResenias"] = value; }
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            ViewStateUserKey = Session.SessionID;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            pnlFormularioResenia.Visible = false;
            lblMensajeResenia.Visible = false;
            lblErrorResenia.Visible = false;
            txtComentarioResenia.Attributes["maxlength"] = "255";
            usuario = UsuarioSesion.Obtener(Session);
            btnReseña.Visible = usuario != null && usuario.IdTipoUsuario == UsuarioSesion.Responsable &&
                usuario.IdResponsable.GetValueOrDefault() > 0;
            lnkIniciarSesionResenia.Visible = usuario == null;
            if (!int.TryParse(Request.QueryString["id"], out idInstitucion) || idInstitucion <= 0)
            {
                MostrarError("Elegí una institución desde el listado.", 400);
                return;
            }

            RegisterAsyncTask(new PageAsyncTask(() => CargarDetalleAsync(idInstitucion)));
        }

        private async Task CargarDetalleAsync(int id)
        {
            pnlDetalle.Visible = false;
            pnlError.Visible = false;
            try
            {
                var institucion = await InstiRedApi.GetAsync<InstitucionApi>("vw/Instituciones/" + id, permitirNoEncontrado: true);
                if (institucion == null || !institucion.Estado)
                {
                    MostrarError("No encontramos la institución solicitada.", 404);
                    return;
                }

                pnlDetalle.Visible = true;
                CargarFicha(institucion);
                btnRegistrarAlumno.NavigateUrl = "~/FormularioPostulacionAlumno.aspx?id=" + id +
                    "&nombre=" + HttpUtility.UrlEncode(institucion.Nombre);
                await Task.WhenAll(CargarReseniasAsync(id), CargarImagenesAsync(id));
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
            lblValoracion.Text = institucion.PromedioCalificacion.HasValue && institucion.CantidadResenias > 0
                ? "★ " + institucion.PromedioCalificacion.Value.ToString("0.0") + " / 5 · " +
                    institucion.CantidadResenias + (institucion.CantidadResenias == 1 ? " reseña" : " reseñas")
                : "Sin reseñas";

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
            int totalPaginas = Math.Max(1, (resenias.Count + ReseniasPorPagina - 1) / ReseniasPorPagina);
            PaginaResenias = Math.Max(0, Math.Min(PaginaResenias, totalPaginas - 1));
            rptResenias.DataSource = resenias
                .OrderByDescending(resenia => PuedeEliminarResenia(resenia.IdResponsable))
                .ThenByDescending(resenia => resenia.Fecha)
                .ThenByDescending(resenia => resenia.IdResenia)
                .Skip(PaginaResenias * ReseniasPorPagina).Take(ReseniasPorPagina);
            rptResenias.DataBind();
            pnlPaginacionResenias.Visible = totalPaginas > 1;
            btnReseniasAnterior.Enabled = PaginaResenias > 0;
            btnReseniasSiguiente.Enabled = PaginaResenias < totalPaginas - 1;
            lblPaginaResenias.Text = "Página " + (PaginaResenias + 1) + " de " + totalPaginas;
            pnlSinResenias.Visible = resenias.Count == 0;
            bool yaPublico = usuario != null && usuario.IdResponsable.HasValue &&
                resenias.Any(r => r.IdResponsable == usuario.IdResponsable.Value);
            btnReseña.Visible = usuario != null && usuario.IdTipoUsuario == UsuarioSesion.Responsable &&
                usuario.IdResponsable.GetValueOrDefault() > 0 && !yaPublico;
        }

        protected void btnReseña_Click(object sender, EventArgs e)
        {
            RegisterAsyncTask(new PageAsyncTask(AbrirReseniaAsync));
        }

        protected void btnReseniasAnterior_Click(object sender, EventArgs e)
        {
            PaginaResenias = Math.Max(0, PaginaResenias - 1);
        }

        protected void btnReseniasSiguiente_Click(object sender, EventArgs e)
        {
            PaginaResenias++;
        }

        protected bool PuedeEliminarResenia(int idResponsable)
        {
            return usuario != null && usuario.IdTipoUsuario == UsuarioSesion.Responsable &&
                usuario.IdResponsable.GetValueOrDefault() > 0 && usuario.IdResponsable.Value == idResponsable;
        }

        protected void btnEliminarResenia_Click(object sender, EventArgs e)
        {
            int id;
            if (!int.TryParse(((LinkButton)sender).CommandArgument, out id) || id <= 0)
            {
                MostrarMensajeResenia("No se pudo identificar la reseña que querés eliminar.", false);
                return;
            }
            RegisterAsyncTask(new PageAsyncTask(() => EliminarReseniaAsync(id)));
        }

        private async Task EliminarReseniaAsync(int id)
        {
            try
            {
                if (usuario == null || usuario.IdTipoUsuario != UsuarioSesion.Responsable ||
                    usuario.IdResponsable.GetValueOrDefault() <= 0 || id <= 0 || idInstitucion <= 0)
                {
                    MostrarMensajeResenia("Iniciá sesión como responsable para eliminar una reseña propia.", false);
                    return;
                }
                var resenia = await InstiRedApi.GetAsync<ReseniaApi>("ent/Resenias/" + id, permitirNoEncontrado: true);
                if (resenia == null || resenia.IdInstitucion != idInstitucion || !PuedeEliminarResenia(resenia.IdResponsable))
                {
                    MostrarMensajeResenia("No encontramos una reseña tuya para eliminar en esta institución.", false);
                    return;
                }
                var respuesta = await InstiRedApi.DeleteAsync("ent/Resenias/" + id +
                    "?idResponsable=" + usuario.IdResponsable.Value);
                if (respuesta.Estado == HttpStatusCode.OK)
                {
                    PaginaResenias = 0;
                    await CargarDetalleAsync(idInstitucion);
                    if (pnlDetalle.Visible)
                    {
                        MostrarMensajeResenia("Tu reseña se eliminó correctamente.", true);
                    }
                    else
                    {
                        lblError.Text = "Tu reseña se eliminó, pero no pudimos actualizar la página. Volvé a abrir el detalle.";
                    }
                    return;
                }
                if (respuesta.Estado == HttpStatusCode.NotFound)
                {
                    await CargarDetalleAsync(idInstitucion);
                    MostrarMensajeResenia("La reseña ya no está disponible para eliminar.", false);
                    return;
                }
                MostrarMensajeResenia(respuesta.Estado == HttpStatusCode.BadRequest ? respuesta.Mensaje :
                    "No se pudo eliminar la reseña. Intentá nuevamente más tarde.", false);
            }
            catch (Exception ex)
            {
                Trace.Warn("DetalleInstitucion", "Error al eliminar la reseña.", ex);
                MostrarMensajeResenia("No pudimos confirmar si la reseña se eliminó. Actualizá el detalle para comprobarlo antes de reintentar.", false);
            }
        }

        private async Task<PostulacionReseniaApi> ObtenerPostulacionConfirmadaAsync()
        {
            if (usuario == null || usuario.IdTipoUsuario != UsuarioSesion.Responsable ||
                usuario.IdResponsable.GetValueOrDefault() <= 0 || idInstitucion <= 0)
            {
                throw new ArgumentException("Iniciá sesión como responsable para publicar una reseña.");
            }
            var postulaciones = await InstiRedApi.GetAsync<List<PostulacionReseniaApi>>(
                "vw/PostulacionesPorResponsable?idResponsable=" + usuario.IdResponsable.Value +
                "&idInstitucion=" + idInstitucion + "&idEstado=3");
            return postulaciones.FirstOrDefault(p => p.IdPostulacion > 0 && p.IdEstado == 3 &&
                p.IdResponsable == usuario.IdResponsable.Value && p.IdInstitucion == idInstitucion);
        }

        private async Task AbrirReseniaAsync()
        {
            try
            {
                var postulacion = await ObtenerPostulacionConfirmadaAsync();
                if (postulacion == null)
                {
                    MostrarMensajeResenia("Para reseñar esta institución necesitás una postulación confirmada.", false);
                    return;
                }
                lblErrorResenia.Visible = false;
                lblMensajeResenia.Visible = false;
                pnlFormularioResenia.Visible = true;
            }
            catch (ArgumentException ex)
            {
                MostrarMensajeResenia(ex.Message, false);
            }
            catch (Exception ex)
            {
                Trace.Warn("DetalleInstitucion", "Error al consultar las postulaciones para reseñar.", ex);
                MostrarMensajeResenia("No pudimos habilitar la reseña. Intentá nuevamente más tarde.", false);
            }
        }

        protected void btnPublicarResenia_Click(object sender, EventArgs e)
        {
            RegisterAsyncTask(new PageAsyncTask(PublicarReseniaAsync));
        }

        private async Task PublicarReseniaAsync()
        {
            try
            {
                int calificacion;
                if (!int.TryParse(ddlCalificacion.SelectedValue, out calificacion) || calificacion < 1 || calificacion > 5)
                {
                    throw new ArgumentException("Seleccioná una calificación de 1 a 5.");
                }
                string comentario = txtComentarioResenia.Text.Trim();
                if (comentario.Length == 0 || comentario.Length > 255)
                {
                    throw new ArgumentException("Ingresá un comentario de hasta 255 caracteres.");
                }
                var postulacion = await ObtenerPostulacionConfirmadaAsync();
                if (postulacion == null)
                {
                    throw new ArgumentException("Para reseñar esta institución necesitás una postulación confirmada.");
                }
                var respuesta = await InstiRedApi.PostAsync("sp/RegistrarResenia", new
                {
                    IdResponsable = usuario.IdResponsable.Value,
                    IdPostulacion = postulacion.IdPostulacion,
                    Calificacion = calificacion,
                    Comentario = comentario
                });
                if (respuesta.Estado == HttpStatusCode.OK)
                {
                    pnlFormularioResenia.Visible = false;
                    txtComentarioResenia.Text = "";
                    ddlCalificacion.SelectedIndex = 0;
                    PaginaResenias = 0;
                    await CargarDetalleAsync(idInstitucion);
                    if (pnlDetalle.Visible)
                    {
                        MostrarMensajeResenia("Tu reseña se publicó correctamente.", true);
                    }
                    else
                    {
                        lblError.Text = "Tu reseña se publicó, pero no pudimos actualizar la página. Volvé a abrir el detalle para verla.";
                    }
                    return;
                }
                if (respuesta.Estado == HttpStatusCode.BadRequest)
                {
                    MostrarErrorResenia(respuesta.Mensaje);
                    return;
                }
                MostrarErrorResenia("No se pudo publicar la reseña. Intentá nuevamente más tarde.");
            }
            catch (ArgumentException ex)
            {
                MostrarErrorResenia(ex.Message);
            }
            catch (Exception ex)
            {
                Trace.Warn("DetalleInstitucion", "Error al publicar la reseña.", ex);
                MostrarErrorResenia("No pudimos confirmar si la reseña se publicó. Volvé a abrir el detalle para comprobarlo antes de reintentar.");
            }
        }

        private void MostrarErrorResenia(string mensaje)
        {
            pnlFormularioResenia.Visible = true;
            lblErrorResenia.Visible = true;
            lblErrorResenia.Text = HttpUtility.HtmlEncode(mensaje);
        }

        private void MostrarMensajeResenia(string mensaje, bool exito)
        {
            lblMensajeResenia.Visible = true;
            lblMensajeResenia.Text = HttpUtility.HtmlEncode(mensaje);
            lblMensajeResenia.CssClass = exito ? "alert alert-success d-block my-3" : "alert alert-danger d-block my-3";
        }

        private async Task CargarImagenesAsync(int id)
        {
            var imagenes = await InstiRedApi.ListarImagenesAsync();
            var imagenesInstitucion = imagenes.Where(imagen => imagen.IdInstitucion == id).ToList();

            ImagenesVisibles = Math.Min(3, imagenesInstitucion.Count);
            pnlImagenes.Visible = imagenesInstitucion.Count > 0;
            UrlFotoUnica = null;
            DescripcionFotoUnica = null;

            if (imagenesInstitucion.Count == 0)
            {
                return;
            }

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
            public decimal? PromedioCalificacion { get; set; }
            public long CantidadResenias { get; set; }
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
            public int IdResenia { get; set; }
            public int IdInstitucion { get; set; }
            public int IdResponsable { get; set; }
            public string NombreResponsable { get; set; }
            public string ApellidoResponsable { get; set; }
            public int Calificacion { get; set; }
            public string Comentario { get; set; }
            public string RespuestaInstitucion { get; set; }
            public DateTime Fecha { get; set; }
        }

        private class PostulacionReseniaApi
        {
            public int IdPostulacion { get; set; }
            public int IdResponsable { get; set; }
            public int IdInstitucion { get; set; }
            public int IdEstado { get; set; }
        }

    }
}
