using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
//using Dominio;

namespace Presentacion
{
    public partial class FormularioAdministradorInstitucion : System.Web.UI.Page
    {

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                //ddlInstitucion.DataSource =;
                ddlInstitucion.DataTextField = "Nombre";
                ddlInstitucion.DataValueField = "Id";
                ddlInstitucion.DataBind();
                CargarTiposUsuario();
                CargarTiposInstitucion();
            }
        }


        private void CargarTiposUsuario()
        {

        }

        private void CargarTiposInstitucion()
        {

        }


        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(ddlInstitucion.SelectedValue))
            {
                return;
            }

            if (ddlInstitucion.SelectedValue == "NUEVA")
            {
            }
            else
            {
                int idInstitucion = int.Parse(ddlInstitucion.SelectedValue);
            }
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {

        }


        /*private AdministradorInstitucion ArmarAdministrador(string claveGenerada)
        {
            return null;
        }*/

        private List<string> GuardarDocumentos()
        {
            return null;
        }

        private static string GenerarClave(int largo)
        {
            return null;
        }


        /*private bool EnviarClavePorMail(Usuario usuario, string claveGenerada)
        {
            return false;
        }*/

    }
}