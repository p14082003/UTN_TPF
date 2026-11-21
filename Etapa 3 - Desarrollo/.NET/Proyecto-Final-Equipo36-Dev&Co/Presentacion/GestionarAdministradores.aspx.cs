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
    public partial class GestionarAdministradores : Page
    {
        private UsuarioSesion usuario;
        private bool cambioGuardado;

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
                Ejecutar(CargarAdministradoresAsync);
            }
        }

        private void Ejecutar(Func<Task> accion)
        {
            lblMensaje.Visible = false;
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
                    Trace.Warn("GestionarAdministradores", "Error al consultar o gestionar administradores en la API.", ex);
                    string mensaje = "No se pudo completar la operación. Recargá el listado antes de intentarlo nuevamente.";
                    if (cambioGuardado)
                    {
                        mensaje = "El cambio se guardó, pero no se pudo actualizar el listado. Recargá la página para verlo.";
                    }
                    MostrarMensaje(mensaje, false);
                }
            }));
        }

        private async Task CargarAdministradoresAsync()
        {
            var administradores = await InstiRedApi.GetAsync<List<AdministradorApi>>(
                "vw/AdministradoresPorInstitucion?idInstitucion=" + usuario.IdInstitucion.Value);

            dgvAdministradores.DataSource = administradores.Select(a => new
            {
                a.IdAdministrador,
                NombreCompleto = (a.NombreAdministrador + " " + a.ApellidoAdministrador).Trim(),
                Documento = a.DocAdministrador,
                Email = a.EmailAdministrador,
                Estado = a.UsuarioActivo ? "Activo" : "Dado de baja",
                Activo = a.UsuarioActivo,
                EsPropio = a.IdAdministrador == usuario.IdAdministradorInstitucion.Value
            }).ToList();
            dgvAdministradores.DataBind();
        }

        protected void btnAgregarAdministrador_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/FormularioAdministradorAdicional.aspx");
        }

        protected void btnBajaAdministrador_Click(object sender, EventArgs e)
        {
            CambiarEstado(sender, false);
        }

        protected void btnReactivarAdministrador_Click(object sender, EventArgs e)
        {
            CambiarEstado(sender, true);
        }

        private void CambiarEstado(object sender, bool reactivar)
        {
            var boton = (LinkButton)sender;
            if (!int.TryParse(boton.CommandArgument, out int id) || id <= 0)
            {
                MostrarMensaje("Seleccioná un administrador válido.", false);
                return;
            }
            Ejecutar(() => CambiarEstadoAsync(id, reactivar));
        }

        private async Task CambiarEstadoAsync(int idAdministrador, bool reactivar)
        {
            var administrador = await InstiRedApi.GetAsync<AdministradorApi>(
                "vw/AdministradoresPorInstitucion/" + idAdministrador, permitirNoEncontrado: true);
            if (administrador == null || administrador.IdInstitucion != usuario.IdInstitucion.Value)
            {
                throw new ArgumentException("El administrador no existe o no pertenece a tu institución.");
            }
            if (!reactivar && idAdministrador == usuario.IdAdministradorInstitucion.Value)
            {
                throw new ArgumentException("No podés darte de baja a vos mismo.");
            }

            string ruta = reactivar ? "sp/ReactivarAdministradorInstitucion" : "sp/DarDeBajaAdministradorInstitucion";
            var resultado = await InstiRedApi.PostAsync(ruta, new
            {
                idUsuarioActor = usuario.IdUsuario,
                idAdministrador
            });
            if (resultado.Estado != HttpStatusCode.OK)
            {
                MostrarMensaje(resultado.Mensaje ?? "No se pudo confirmar el cambio. Recargá el listado antes de intentarlo nuevamente.", false);
                return;
            }

            cambioGuardado = true;
            await CargarAdministradoresAsync();
            MostrarMensaje(reactivar ? "Administrador reactivado." : "Administrador dado de baja.", true);
        }

        private void MostrarMensaje(string texto, bool ok)
        {
            lblMensaje.Text = Server.HtmlEncode(texto);
            lblMensaje.CssClass = ok ? "d-block mb-3 fg-success" : "d-block mb-3 fg-danger";
            lblMensaje.Visible = true;
        }

        private class AdministradorApi
        {
            public int IdAdministrador { get; set; }
            public int IdInstitucion { get; set; }
            public string NombreAdministrador { get; set; }
            public string ApellidoAdministrador { get; set; }
            public string DocAdministrador { get; set; }
            public string EmailAdministrador { get; set; }
            public bool UsuarioActivo { get; set; }
        }
    }
}
