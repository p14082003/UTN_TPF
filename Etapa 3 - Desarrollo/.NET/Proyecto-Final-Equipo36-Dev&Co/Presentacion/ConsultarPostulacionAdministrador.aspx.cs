using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Presentacion
{
    public partial class ConsultarPostulacionAdministrador : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnConsultar_Click(object sender, EventArgs e)
        {
            // TODO: llamar a sp_Postulacion_ConsultarPorCodigo con txtCodigo.Text y txtEmail.Text.
            // Mostrar el resultado solo si Tipo == "Administrador".
        }

        protected void btnNuevaConsulta_Click(object sender, EventArgs e)
        {
            pnlResultado.Visible = false;
            pnlConsulta.Visible = true;
            pnlMensaje.Visible = false;
            txtCodigo.Text = string.Empty;
            txtEmail.Text = string.Empty;
        }
    }
}