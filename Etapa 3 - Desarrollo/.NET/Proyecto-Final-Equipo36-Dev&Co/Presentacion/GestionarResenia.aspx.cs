using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Presentacion
{
    public partial class GestionarResenia : System.Web.UI.Page
    {
        // Id de la reseña que se está viendo (se conserva entre postbacks).
        private int IdReseniaSeleccionada
        {
            get { return ViewState["IdReseniaSeleccionada"] == null ? 0 : (int)ViewState["IdReseniaSeleccionada"]; }
            set { ViewState["IdReseniaSeleccionada"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
                CargarListado();
        }

        private void CargarListado()
        {
            DataTable resenias = ObtenerResenias();

            string filtro = string.Empty;
            if (ddlFiltroEstado.SelectedValue == "SinResponder")
                filtro = "RespuestaInstitucion IS NULL";
            else if (ddlFiltroEstado.SelectedValue == "Respondidas")
                filtro = "RespuestaInstitucion IS NOT NULL";

            dgvResenias.DataSource = new DataView(resenias, filtro, "Fecha DESC", DataViewRowState.CurrentRows);
            dgvResenias.DataBind();

            int total = resenias.Rows.Count;
            int sinResponder = resenias.Select("RespuestaInstitucion IS NULL").Length;
            lblTotal.Text = total.ToString();
            lblSinResponder.Text = sinResponder.ToString();
            lblRespondidas.Text = (total - sinResponder).ToString();
            lblPromedio.Text = total == 0 ? "-" : Convert.ToDouble(resenias.Compute("AVG(Calificacion)", "")).ToString("0.0");
        }

        protected void Filtro_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarListado();
        }

        protected void btnVerDetalle_Click(object sender, EventArgs e)
        {
            LinkButton boton = (LinkButton)sender;
            IdReseniaSeleccionada = int.Parse(boton.CommandArgument);

            DataRow resenia = ObtenerResenias().Rows.Find(IdReseniaSeleccionada);
            int calificacion = (int)resenia["Calificacion"];

            lblDetalleResponsable.Text = Server.HtmlEncode(resenia["NombreResponsable"].ToString());
            lblDetalleFecha.Text = ((DateTime)resenia["Fecha"]).ToString("dd/MM/yyyy");
            lblDetalleEstrellas.Text = new string('★', calificacion) + new string('☆', 5 - calificacion);
            lblDetalleComentario.Text = Server.HtmlEncode(resenia["Comentario"].ToString());

            bool respondida = resenia["RespuestaInstitucion"] != DBNull.Value;
            pnlResponder.Visible = !respondida;
            pnlRespuestaExistente.Visible = respondida;
            txtRespuesta.Text = string.Empty;
            if (respondida)
                lblRespuestaExistente.Text = Server.HtmlEncode(resenia["RespuestaInstitucion"].ToString());

            pnlMensaje.Visible = false;
            pnlDashboard.Visible = false;
            pnlListado.Visible = false;
            pnlDetalle.Visible = true;
        }

        protected void btnCerrarDetalle_Click(object sender, EventArgs e)
        {
            VolverAlListado();
        }

        protected void btnEnviarRespuesta_Click(object sender, EventArgs e)
        {
            string respuesta = txtRespuesta.Text.Trim();

            if (respuesta == string.Empty)
            {
                MostrarMensaje("Debe ingresar una respuesta en la reseña.", false);
                return;
            }
            if (respuesta.Length > 255)
            {
                MostrarMensaje("La respuesta supera los 255 caracteres permitidos.", false);
                return;
            }

            DataRow resenia = ObtenerResenias().Rows.Find(IdReseniaSeleccionada);
            if (resenia["RespuestaInstitucion"] != DBNull.Value)
            {
                MostrarMensaje("La reseña ya fue contestada.", false);
                return;
            }

            // TODO: reemplazar por la llamada a sp_Responder_Resenia (id del administrador, id de la reseña, respuesta).
            resenia["RespuestaInstitucion"] = respuesta;

            VolverAlListado();
            MostrarMensaje("Respuesta enviada. La reseña ya no admite más respuestas.", true);
        }

        private void VolverAlListado()
        {
            pnlDetalle.Visible = false;
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

        private DataTable ObtenerResenias()
        {
            DataTable resenias = Session["ReseniasDemo"] as DataTable;
            if (resenias != null)
                return resenias;

            resenias = new DataTable();
            resenias.Columns.Add("Id", typeof(int));
            resenias.Columns.Add("Fecha", typeof(DateTime));
            resenias.Columns.Add("NombreResponsable", typeof(string));
            resenias.Columns.Add("Calificacion", typeof(int));
            resenias.Columns.Add("Comentario", typeof(string));
            resenias.Columns.Add("RespuestaInstitucion", typeof(string));
            resenias.PrimaryKey = new[] { resenias.Columns["Id"] };

            resenias.Rows.Add(1, DateTime.Today.AddDays(-3), "Laura Gómez", 5,
                "Excelente atención del equipo directivo y muy buena comunicación con las familias.", DBNull.Value);
            resenias.Rows.Add(2, DateTime.Today.AddDays(-7), "Carlos Pereyra", 3,
                "Buena propuesta académica, pero tardaron en responder nuestras consultas de inscripción.", DBNull.Value);
            resenias.Rows.Add(3, DateTime.Today.AddDays(-20), "Marta Ruiz", 4,
                "Instalaciones muy cuidadas y docentes comprometidos.",
                "¡Gracias por tu comentario, Marta! Nos alegra que te guste la institución.");

            Session["ReseniasDemo"] = resenias;
            return resenias;
        }
    }
}