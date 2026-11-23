using System;
using Presentacion.Auth;

namespace Presentacion
{
    public partial class Site1 : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var usuario = UsuarioSesion.Obtener(Session);
            lnkIniciarSesion.Visible = usuario == null;
            pnlSesion.Visible = usuario != null;
            pnlMenuResponsable.Visible = usuario != null &&
                usuario.IdTipoUsuario == UsuarioSesion.Responsable &&
                usuario.IdResponsable.GetValueOrDefault() > 0;
            lnkRegistrarAlumno.Visible = pnlMenuResponsable.Visible;
            pnlMenuAdmin.Visible = usuario != null &&
                usuario.IdTipoUsuario == UsuarioSesion.AdministradorInstitucion &&
                usuario.IdInstitucion.GetValueOrDefault() > 0;
            lnkVacantes.Visible = pnlMenuAdmin.Visible;
            lnkAgregarAdministrador.Visible = pnlMenuAdmin.Visible;
            lnkMiInstitucion.Visible = pnlMenuAdmin.Visible && usuario.IdAdministradorInstitucion.GetValueOrDefault() > 0;

            if (usuario != null)
            {
                Response.Cache.SetNoStore();
                if (usuario.IdTipoUsuario == UsuarioSesion.DevCo)
                {
                    litSaludo.Text = "¡Hola, DevCo!";
                }
                else
                {
                    litSaludo.Text = string.IsNullOrWhiteSpace(usuario.Nombre)
                        ? "¡Hola!"
                        : "¡Hola, " + usuario.Nombre + "!";
                }
            }
        }

        protected void btnCerrarSesion_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Response.Redirect("~/ListadoInstituciones.aspx");
        }
    }
}
