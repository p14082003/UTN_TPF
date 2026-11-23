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
    public partial class GestionarResenia : Page
    {
        private UsuarioSesion usuario;
        private bool respuestaGuardada;
        private bool respuestaEliminada;

        private int IdReseniaSeleccionada
        {
            get { return (int?)ViewState["IdReseniaSeleccionada"] ?? 0; }
            set { ViewState["IdReseniaSeleccionada"] = value; }
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
                Ejecutar(CargarListadoAsync);
            }
        }

        private void Ejecutar(Func<Task> accion)
        {
            pnlMensaje.Visible = false;
            RegisterAsyncTask(new PageAsyncTask(async () =>
            {
                try
                {
                    await accion();
                }
                catch (ArgumentException ex)
                {
                    MostrarMensaje(ex.Message, false);
                }
                catch (Exception ex)
                {
                    Trace.Warn("GestionarResenia", "Error al consultar o responder reseñas en la API.", ex);
                    string mensaje = "No se pudo completar la operación. Intentá nuevamente más tarde.";
                    if (respuestaGuardada)
                    {
                        mensaje = "La respuesta se guardó, pero no se pudo actualizar el listado. Recargá la página para verla.";
                    }
                    else if (respuestaEliminada)
                    {
                        mensaje = "La respuesta se eliminó, pero no se pudo actualizar la pantalla. Volvé al listado para consultar el estado.";
                    }
                    MostrarMensaje(mensaje, false);
                }
            }));
        }

        private async Task CargarListadoAsync()
        {
            var resenias = await InstiRedApi.GetAsync<List<ReseniaApi>>(
                "vw/ValoracionesPorInstitucion?idInstitucion=" + usuario.IdInstitucion.Value);

            IEnumerable<ReseniaApi> filtradas = resenias;
            if (ddlFiltroEstado.SelectedValue == "SinResponder")
            {
                filtradas = resenias.Where(r => r.RespuestaInstitucion == null);
            }
            else if (ddlFiltroEstado.SelectedValue == "Respondidas")
            {
                filtradas = resenias.Where(r => r.RespuestaInstitucion != null);
            }

            var vista = filtradas.OrderByDescending(r => r.Fecha)
                .ThenByDescending(r => r.IdResenia).ToList();
            dgvResenias.PageIndex = Math.Min(dgvResenias.PageIndex, Math.Max(0, (vista.Count - 1) / dgvResenias.PageSize));
            dgvResenias.DataSource = vista;
            dgvResenias.DataBind();

            int total = resenias.Count;
            int sinResponder = resenias.Count(r => r.RespuestaInstitucion == null);
            lblTotal.Text = total.ToString();
            lblSinResponder.Text = sinResponder.ToString();
            lblRespondidas.Text = (total - sinResponder).ToString();
            lblPromedio.Text = total == 0 ? "-" : resenias.Average(r => r.Calificacion).ToString("0.0");
        }

        protected void Filtro_SelectedIndexChanged(object sender, EventArgs e)
        {
            dgvResenias.PageIndex = 0;
            Ejecutar(CargarListadoAsync);
        }

        protected void dgvResenias_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            dgvResenias.PageIndex = e.NewPageIndex;
            Ejecutar(CargarListadoAsync);
        }

        protected void btnVerDetalle_Click(object sender, EventArgs e)
        {
            var boton = (LinkButton)sender;
            if (!int.TryParse(boton.CommandArgument, out int id) || id <= 0)
            {
                MostrarMensaje("Seleccioná una reseña válida.", false);
                return;
            }
            Ejecutar(() => CargarDetalleAsync(id));
        }

        private async Task<ReseniaApi> ObtenerReseniaAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Seleccioná una reseña del listado.");
            }
            var resenia = await InstiRedApi.GetAsync<ReseniaApi>(
                "vw/ValoracionesPorInstitucion/" + id, permitirNoEncontrado: true);
            if (resenia == null || resenia.IdInstitucion != usuario.IdInstitucion.Value)
            {
                throw new ArgumentException("La reseña no existe o no pertenece a tu institución.");
            }
            return resenia;
        }

        private async Task CargarDetalleAsync(int id)
        {
            var resenia = await ObtenerReseniaAsync(id);
            IdReseniaSeleccionada = resenia.IdResenia;
            lblDetalleResponsable.Text = Server.HtmlEncode(resenia.NombreCompleto);
            lblDetalleFecha.Text = resenia.Fecha.ToString("dd/MM/yyyy");
            lblDetalleEstrellas.Text = new string('★', resenia.Calificacion) + new string('☆', 5 - resenia.Calificacion);
            lblDetalleComentario.Text = Server.HtmlEncode(resenia.Comentario);

            bool respondida = resenia.RespuestaInstitucion != null;
            btnCerrarDetalle.Visible = respondida;
            pnlResponder.Visible = !respondida;
            pnlRespuestaExistente.Visible = respondida;
            txtRespuesta.Text = "";
            lblRespuestaExistente.Text = Server.HtmlEncode(resenia.RespuestaInstitucion);
            pnlDashboard.Visible = false;
            pnlListado.Visible = false;
            pnlDetalle.Visible = true;
        }

        protected void btnCerrarDetalle_Click(object sender, EventArgs e)
        {
            Ejecutar(VolverAlListadoAsync);
        }

        protected void btnEnviarRespuesta_Click(object sender, EventArgs e)
        {
            Ejecutar(EnviarRespuestaAsync);
        }

        private async Task EnviarRespuestaAsync()
        {
            string respuesta = txtRespuesta.Text.Trim();
            if (respuesta == "")
            {
                throw new ArgumentException("Debe ingresar una respuesta en la reseña.");
            }
            if (respuesta.Length > 255)
            {
                throw new ArgumentException("La respuesta supera los 255 caracteres permitidos.");
            }

            var resenia = await ObtenerReseniaAsync(IdReseniaSeleccionada);
            if (resenia.RespuestaInstitucion != null)
            {
                await CargarDetalleAsync(resenia.IdResenia);
                MostrarMensaje("La reseña ya fue contestada.", false);
                return;
            }

            var resultado = await InstiRedApi.PostAsync("sp/ResponderResenia", new
            {
                idAdministradorInstitucion = usuario.IdAdministradorInstitucion.Value,
                idResenia = resenia.IdResenia,
                respuestaInstitucion = respuesta
            });
            if (resultado.Estado != HttpStatusCode.OK)
            {
                MostrarMensaje(resultado.Mensaje ?? "No se pudo confirmar el envío de la respuesta. Recargá la reseña antes de intentarlo nuevamente.", false);
                return;
            }

            respuestaGuardada = true;
            pnlResponder.Visible = false;
            pnlRespuestaExistente.Visible = true;
            lblRespuestaExistente.Text = Server.HtmlEncode(respuesta);
            await VolverAlListadoAsync();
            MostrarMensaje("Respuesta enviada correctamente.", true);
        }

        protected void btnEliminarRespuesta_Click(object sender, EventArgs e)
        {
            Ejecutar(EliminarRespuestaAsync);
        }

        private async Task EliminarRespuestaAsync()
        {
            if (usuario == null || usuario.IdTipoUsuario != UsuarioSesion.AdministradorInstitucion ||
                usuario.IdAdministradorInstitucion.GetValueOrDefault() <= 0 || usuario.IdInstitucion.GetValueOrDefault() <= 0)
            {
                throw new ArgumentException("Iniciá sesión como administrador de la institución para eliminar su respuesta.");
            }
            var resenia = await ObtenerReseniaAsync(IdReseniaSeleccionada);
            if (resenia.RespuestaInstitucion == null)
            {
                await CargarDetalleAsync(resenia.IdResenia);
                MostrarMensaje("La reseña ya no tiene una respuesta para eliminar.", false);
                return;
            }
            var resultado = await InstiRedApi.DeleteAsync("ent/Resenias/" + resenia.IdResenia +
                "/respuesta?idAdministradorInstitucion=" + usuario.IdAdministradorInstitucion.Value);
            if (resultado.Estado == HttpStatusCode.NotFound)
            {
                await VolverAlListadoAsync();
                MostrarMensaje("La respuesta ya no está disponible o tu cuenta no está habilitada para eliminarla.", false);
                return;
            }
            if (resultado.Estado != HttpStatusCode.OK)
            {
                MostrarMensaje(resultado.Mensaje ?? "No se pudo eliminar la respuesta. Intentá nuevamente más tarde.", false);
                return;
            }
            respuestaEliminada = true;
            await CargarDetalleAsync(resenia.IdResenia);
            MostrarMensaje("Respuesta eliminada. Podés volver a responder esta reseña.", true);
        }

        private async Task VolverAlListadoAsync()
        {
            await CargarListadoAsync();
            IdReseniaSeleccionada = 0;
            pnlDetalle.Visible = false;
            pnlDashboard.Visible = true;
            pnlListado.Visible = true;
        }

        private void MostrarMensaje(string texto, bool exito)
        {
            pnlMensaje.Visible = true;
            pnlMensaje.CssClass = exito ? "alert alert-success mb-4" : "alert alert-danger mb-4";
            litMensaje.Text = Server.HtmlEncode(texto);
        }

        private class ReseniaApi
        {
            public int IdResenia { get; set; }
            public int IdInstitucion { get; set; }
            public DateTime Fecha { get; set; }
            public string NombreResponsable { get; set; }
            public string ApellidoResponsable { get; set; }
            public int Calificacion { get; set; }
            public string Comentario { get; set; }
            public string RespuestaInstitucion { get; set; }
            public string NombreCompleto
            {
                get { return (NombreResponsable + " " + ApellidoResponsable).Trim(); }
            }
        }
    }
}
