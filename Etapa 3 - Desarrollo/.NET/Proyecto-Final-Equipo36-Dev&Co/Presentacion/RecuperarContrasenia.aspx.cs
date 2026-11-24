using Negocio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Presentacion
{
    public partial class RecuperarContrasenia : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            

           
        }

        protected void btnEnviarCredenciales_Click(object sender, EventArgs e)
        {
            string email = txtEmail.Text.Trim();

            if(string.IsNullOrWhiteSpace(email) )
            {
                MostrarMensaje("Ingresá tu correo electrónico",false);
                return;
            }
            //ACA NECESITAMOS VINCULAR CON API
            // sp_Olvide_Clave : Necesita email ingresado por usuario en el form por parametro. RECUPERAR nombre, apellido, contraseña nueva (texto plano) y idUsuairo. 

            // HASHEAMOS CLAVE 

            //sp_actualizar enviar por parametro IdUsuario y Conttaseña  HASHEADA 

            //llamamos el metodo de envio de mail. Por parametro nueva clave SIN HASHEAR .
            // bool enviado = envioEmailRecuperacionClave(nombre,apellido,email,claveNueva);  dejo comentado porque hay que recuperar con API datos para enviar por parametro 
            
            // mostramos mensaje solo cuando se haya actualizado la contraseña y enviado por mail con exito y redirigimos
            //MostrarMensaje("Se envió un correo con la nueva credencial de acceso", true);

            //ClientScript.RegisterStartupScript(this.GetType(), "redirigir", "setTimeout(function(){ window.location='Login.aspx'; }, 3000);", true);

        }

        private bool envioEmailRecuperacionClave(String nombre, string apellido, string emailUsuario, string claveNueva)
        {
            try
            {
                EmailService email = new EmailService();
                string cuerpo = $@"
                    <html>
                      <body style='font-family: Arial, sans-serif; background-color:#f5f5f5; padding:20px; color:#000;'>
                        <div style='max-width:600px; margin:auto; background:#fff; border:1px solid #ddd; border-radius:8px; padding:20px;'>
                          <h2 style='text-align:center;'>¡Hola {nombre} {apellido}! Aquí tienes tus datos de acceso para acceder a InstiRed</h2>
                          <p>Tu usuario para ingresar es: 
                            <b>{emailUsuario}</b>
                          </p>
                          <p>Tu nueva contraseña generada es:</p>
                          <p style='font-size:20px; font-weight:bold; text-align:center; margin:15px 0;'>{claveNueva} </p>
                          <p style='text-align:center;'> Por seguridad, te sugerimos que al ingresar a tu cuenta la actualices por una nueva contraseña.</p>
                          <hr style='border:none; border-top:1px solid #eee; margin:20px 0;' />
                          <p style='font-size:12px; text-align:center;'>
                            Este mensaje fue generado automáticamente por <b>InstiRed</b>.<br/>
                            Por favor, no respondas a este correo.
                          </p>
                        </div>
                      </body>
                    </html>";

                email.armarCorreo(emailUsuario,"Recupero de datos de acceso - InstiRed",cuerpo);
                email.enviarEmail();
                return true;


            }catch (Exception ex)
            {
                
                Trace.Warn("RecuperarContrasenia", "Error al enviar email.", ex);
                return false;
            }  

        }

        private void MostrarMensaje(string texto, bool ok)
        {
            lblMensaje.Text = HttpUtility.HtmlEncode(texto);
            lblMensaje.CssClass = ok ? "alert alert-success d-block mb-4" : "alert alert-danger d-block mb-4";
            lblMensaje.Visible = true;
        }
    }
}