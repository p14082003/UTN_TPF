using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Presentacion
{
    public partial class GestionarPostulacionAlumno : System.Web.UI.Page
    {
        // Id de la postulación que se está revisando (se conserva entre postbacks).
        private int IdPostulacionSeleccionada
        {
            get { return ViewState["IdPostulacionSeleccionada"] == null ? 0 : (int)ViewState["IdPostulacionSeleccionada"]; }
            set { ViewState["IdPostulacionSeleccionada"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // TEMPORAL: datos de ejemplo para poder probar la pantalla.
                dgvPostulaciones.DataSource = new List<object>
                {
                    new { IdPostulacion = 1, NombreAlumno = "Martina Gómez",  Curso = "1° año - Turno mañana", FechaPostulacion = DateTime.Today.AddDays(-2), NombreTutor = "Laura Gómez" },
                    new { IdPostulacion = 2, NombreAlumno = "Tomás Pereyra",  Curso = "2° año - Turno tarde",  FechaPostulacion = DateTime.Today.AddDays(-1), NombreTutor = "Carlos Pereyra" }
                };
                dgvPostulaciones.DataBind();
            }
        }

        // ===== LISTADO =====

        protected void btnVerDetalle_Click(object sender, EventArgs e)
        {
            LinkButton boton = (LinkButton)sender;
            IdPostulacionSeleccionada = int.Parse(boton.CommandArgument);

            // TODO: cargar los datos reales de la postulación (alumno, tutor, documentos)
            // y consultar las vacantes disponibles del curso.
            // Para probar: la postulación 2 simula un curso sin vacantes.
            int vacantesDisponibles = (IdPostulacionSeleccionada == 2) ? 0 : 3;

            bool hayVacantes = vacantesDisponibles > 0;
            pnlConVacantes.Visible = hayVacantes;
            pnlSinVacantes.Visible = !hayVacantes;
            btnAceptar.Enabled = hayVacantes;   // sin vacantes solo se puede rechazar
            lblVacantes.Text = vacantesDisponibles.ToString();

            txtMotivoRechazo.Text = string.Empty;
            panelRechazo.Visible = false;
            pnlMensaje.Visible = false;

            pnlDashboard.Visible = false;
            pnlListado.Visible = false;
            pnlDetalle.Visible = true;
        }

        protected void btnCerrarDetalle_Click(object sender, EventArgs e)
        {
            VolverAlListado();
        }

        protected void btnDesplegarRechazo_Click(object sender, EventArgs e)
        {
            panelRechazo.Visible = true;
        }

        protected void btnAceptar_Click(object sender, EventArgs e)
        {
            // TODO: volver a validar vacantes en el servidor, descontar una vacante,
            // marcar la postulación como aceptada y notificar al tutor.

            VolverAlListado();
            MostrarMensaje("Postulación aceptada. Se descontó una vacante y se notificó al tutor.", true);
        }

        protected void btnConfirmarRechazo_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMotivoRechazo.Text))
            {
                MostrarMensaje("Ingresá el motivo del rechazo.", false);
                return;
            }

            // TODO: marcar la postulación como rechazada y notificar al tutor con el motivo.

            VolverAlListado();
            MostrarMensaje("Postulación rechazada. Se notificó al tutor.", true);
        }

        private void VolverAlListado()
        {
            pnlDetalle.Visible = false;
            panelRechazo.Visible = false;
            pnlDashboard.Visible = true;
            pnlListado.Visible = true;
        }

        private void MostrarMensaje(string texto, bool exito)
        {
            pnlMensaje.Visible = true;
            pnlMensaje.CssClass = exito ? "alert alert-success mb-4" : "alert alert-danger mb-4";
            litMensaje.Text = Server.HtmlEncode(texto);
        }
    }
}