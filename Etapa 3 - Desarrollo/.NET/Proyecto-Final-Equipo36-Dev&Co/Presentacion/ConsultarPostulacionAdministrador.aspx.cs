using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Web.UI;
using Negocio;

namespace Presentacion
{
    public partial class ConsultarPostulacionAdministrador : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Cache.SetNoStore();
        }

        protected void btnConsultar_Click(object sender, EventArgs e)
        {
            pnlMensaje.Visible = false;
            pnlResultado.Visible = false;
            pnlMotivoRechazo.Visible = false;
            pnlConsulta.Visible = true;
            RegisterAsyncTask(new PageAsyncTask(ConsultarAsync));
        }

        private async Task ConsultarAsync()
        {
            string codigo = txtCodigo.Text.Trim();
            string email = txtEmail.Text.Trim();
            if (codigo == "" || email == "")
            {
                MostrarError("Ingresá el código de seguimiento y el correo electrónico.");
                return;
            }
            if (codigo.Length > 30 || email.Length > 100)
            {
                MostrarError("El código admite hasta 30 caracteres y el correo hasta 100.");
                return;
            }
            if (!EmailValido(email))
            {
                MostrarError("Ingresá un correo electrónico válido.");
                return;
            }

            try
            {
                var respuesta = await InstiRedApi.PostAsync("sp/ConsultarPostulacionPorCodigo",
                    new { codigo, email }, leerCuerpo: true);
                if (respuesta.Estado == HttpStatusCode.BadRequest)
                {
                    MostrarError(respuesta.Mensaje ?? "No se encontró una postulación con esos datos.");
                    return;
                }
                if (respuesta.Estado != HttpStatusCode.OK)
                {
                    MostrarError("No se pudo consultar la postulación. Intentá nuevamente más tarde.");
                    return;
                }

                var postulacion = new JavaScriptSerializer().Deserialize<PostulacionApi>(respuesta.Cuerpo);
                if (postulacion == null || postulacion.Tipo != "Administrador")
                {
                    MostrarError("No se encontró una postulación de administrador con esos datos.");
                    return;
                }

                lblInstitucion.Text = Server.HtmlEncode(postulacion.NombreInstitucion);
                lblCuit.Text = Server.HtmlEncode(postulacion.Cuit);
                lblFecha.Text = postulacion.Fecha.ToString("dd/MM/yyyy");
                lblEstado.Text = Server.HtmlEncode(postulacion.Estado);

                string colorEstado = "bg-lavanda";
                if (postulacion.Estado == "Confirmado")
                {
                    colorEstado = "bg-indigo";
                }
                else if (postulacion.Estado == "Rechazado")
                {
                    colorEstado = "bg-secondary";
                }
                lblEstado.CssClass = "badge rounded-pill px-3 py-2 " + colorEstado;
                pnlMotivoRechazo.Visible = postulacion.Estado == "Rechazado";
                lblMotivoRechazo.Text = Server.HtmlEncode(postulacion.MotivoRechazo);
                pnlConsulta.Visible = false;
                pnlResultado.Visible = true;
            }
            catch (Exception ex)
            {
                Trace.Warn("ConsultarPostulacionAdministrador", "Error al consultar la API de InstiRed.", ex);
                MostrarError("No se pudo consultar la postulación. Intentá nuevamente más tarde.");
            }
        }

        protected void btnNuevaConsulta_Click(object sender, EventArgs e)
        {
            pnlResultado.Visible = false;
            pnlConsulta.Visible = true;
            pnlMensaje.Visible = false;
            pnlMotivoRechazo.Visible = false;
            txtCodigo.Text = "";
            txtEmail.Text = "";
        }

        private void MostrarError(string mensaje)
        {
            litMensaje.Text = Server.HtmlEncode(mensaje);
            pnlMensaje.Visible = true;
        }

        private static bool EmailValido(string email)
        {
            try
            {
                return new MailAddress(email).Address == email;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private class PostulacionApi
        {
            public string Tipo { get; set; }
            public DateTime Fecha { get; set; }
            public string Estado { get; set; }
            public string NombreInstitucion { get; set; }
            public string Cuit { get; set; }
            public string MotivoRechazo { get; set; }
        }
    }
}
