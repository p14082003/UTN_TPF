using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Presentacion
{
    public partial class AdminDevCo : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarDatosFalsos(); // Solo cargamos la tabla la primera vez
            }
        }

        private void CargarDatosFalsos()
        {
            // Creamos una lista temporal al vuelo con clases anónimas.
            var listaColegios = new List<object>
            {
                new { IdInstitucion = 1, Cuit = "30-12345678-9", Nombre = "Instituto San Martín", Email = "direccion@sanmartin.edu.ar" },
                new { IdInstitucion = 2, Cuit = "30-98765432-1", Nombre = "Colegio Nacional de Quilmes", Email = "contacto@cnq.edu.ar" },
                new { IdInstitucion = 3, Cuit = "33-44445555-9", Nombre = "Academia Oxford (Inglés)", Email = "hello@oxford.com.ar" }
            };

            // Le pasamos al GridView la fuente de datos hardcodeada y bindeamos para que dibuje
            dgvInstituciones.DataSource = listaColegios;

            dgvInstituciones.DataBind();
        }

        protected void btnVerDetalle_Click(object sender, EventArgs e)
        {
            // Ocultamos el panel de la tabla
            pnlListado.Visible = false;
            // Mostramos el panel del detalle
            pnlDetalle.Visible = true;
        }

        protected void btnCerrarDetalle_Click(object sender, EventArgs e)
        {
            pnlDetalle.Visible = false;
            pnlListado.Visible = true;
        }

        protected void btnAprobar_Click(object sender, EventArgs e)
        {
            // Por ahora solo volvemos a la pantalla principal
            pnlDetalle.Visible = false;
            pnlListado.Visible = true;
        }

        protected void btnConfirmarRechazo_Click(object sender, EventArgs e)
        {
            // Capturamos lo que el admin escribió en la caja de texto
            string motivo = txtMotivoRechazo.Text;

            // Limpiamos la caja para la próxima vez
            txtMotivoRechazo.Text = "";

            // Volvemos a la pantalla principal
            pnlDetalle.Visible = false;
            pnlListado.Visible = true;

            panelRechazo.Visible = false;
            btnDesplegarRechazo.Visible = true;
            btnAprobar.Visible = true;

        }

        protected void btnDesplegarRechazo_Click(object sender, EventArgs e)
        {
            // Mostramos la caja de texto para escribir el motivo
            panelRechazo.Visible = true;

            // Opcional: Ocultamos los botones principales para obligarlo a confirmar o cancelar
            btnDesplegarRechazo.Visible = false;
            btnAprobar.Visible = false;
        }
    }
}