using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Web.UI;
using Negocio;
using Presentacion.Auth;

namespace Presentacion
{
    public partial class Login : System.Web.UI.Page
    {
        protected void btnLogin_Click(object sender, EventArgs e)
        {
            lblError.Visible = false;
            RegisterAsyncTask(new PageAsyncTask(IniciarSesionAsync));
        }

        protected void btnRegistrarse_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/RegistroIntermedia.aspx");
        }

        private async Task IniciarSesionAsync()
        {
            string email = txtEmail.Text.Trim();
            string clave = txtPassword.Text;
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(clave))
            {
                MostrarError("Ingresá tu correo electrónico y tu contraseña.");
                return;
            }

            UsuarioSesion usuario;
            try
            {
                var respuesta = await InstiRedApi.PostAsync("sp/Login", new { email, clave }, leerCuerpo: true);
                if (respuesta.Estado == HttpStatusCode.Forbidden)
                {
                    Session.Remove(UsuarioSesion.Clave);
                    MostrarError("Tu institución está dada de baja. Contactate con DevCo para consultar su reactivación.");
                    return;
                }
                if (respuesta.Estado == HttpStatusCode.BadRequest)
                {
                    MostrarError("Correo o contraseña incorrectos.");
                    return;
                }

                if (respuesta.Estado != HttpStatusCode.OK)
                {
                    MostrarError("No se pudo iniciar sesión. Intentá nuevamente más tarde.");
                    return;
                }

                usuario = new JavaScriptSerializer().Deserialize<UsuarioSesion>(respuesta.Cuerpo);
            }
            catch (Exception ex)
            {
                Trace.Warn("Login", "Error al consultar la API de InstiRed.", ex);
                MostrarError("No se pudo iniciar sesión. Intentá nuevamente más tarde.");
                return;
            }

            if (usuario == null || !usuario.Ok || usuario.IdUsuario <= 0)
            {
                MostrarError("No se pudo iniciar sesión. Intentá nuevamente más tarde.");
                return;
            }

            string destino;
            switch (usuario.IdTipoUsuario)
            {
                case UsuarioSesion.Responsable:
                    destino = "~/ListadoInstituciones.aspx";
                    break;
                case UsuarioSesion.AdministradorInstitucion:
                    destino = usuario.IdInstitucion.GetValueOrDefault() > 0
                        ? "~/GestionarPostulacionAlumno.aspx"
                        : "~/FormularioInstitucion.aspx";
                    break;
                case UsuarioSesion.DevCo:
                    destino = "~/AdminDevCo.aspx";
                    break;
                default:
                    MostrarError("Tu usuario no tiene un rol habilitado para ingresar.");
                    return;
            }

            Session.Clear();
            Session[UsuarioSesion.Clave] = usuario;
            Response.Redirect(destino, false);
            Context.ApplicationInstance.CompleteRequest();
        }

        private void MostrarError(string mensaje)
        {
            lblError.Text = mensaje;
            lblError.Visible = true;
        }
    }
}
