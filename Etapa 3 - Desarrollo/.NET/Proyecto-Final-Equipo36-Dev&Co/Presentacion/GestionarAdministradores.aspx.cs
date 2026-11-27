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

        protected void dgvAdministradores_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            dgvAdministradores.PageIndex = e.NewPageIndex;
            Ejecutar(CargarAdministradoresAsync);
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
            EnviarCorreo(administrador, reactivar); // Enviar correo para notificar al administrador

            await CargarAdministradoresAsync();
            MostrarMensaje(reactivar ? "Administrador reactivado." : "Administrador dado de baja.", true);
        }

        private void MostrarMensaje(string texto, bool ok)
        {
            lblMensaje.Text = Server.HtmlEncode(texto);
            lblMensaje.CssClass = ok ? "d-block mb-3 fg-success" : "d-block mb-3 fg-danger";
            lblMensaje.Visible = true;
        }

        private void EnviarCorreo(AdministradorApi administrador, bool reactivar)
        {
            if (string.IsNullOrWhiteSpace(administrador.EmailAdministrador))
            {
                // No uso la funcion "mostrar mensaje" ya que en tal caso en el que no tenga un correo (o por algun error no llegue el dato del correo),
                // el administrador no recibe un correo, pero el cambio de estado impacta igual
                Trace.Warn("GestionarAdministradores", "No se pudo enviar el correo: el administrador no tiene un email registrado.");
                return;
            }

            string asunto = reactivar ? "Administrador Reactivado - InstiRed" : "Baja de Administrador - InstiRed";
            string cuerpo = reactivar ? CuerpoAdministradorReactivado(administrador) : CuerpoAdministradorDadoDeBaja(administrador);

            try
            {
                EmailService emailService = new EmailService();
                emailService.armarCorreo(administrador.EmailAdministrador, asunto, cuerpo);
                emailService.enviarEmail();
            }
            catch (Exception ex)
            {
                Trace.Warn("GestionarAdministradores", "Error al enviar el correo de alta o baja de usuario.", ex);
            }
        }

        private string CuerpoAdministradorReactivado(AdministradorApi administrador)
        {
            string nombre = administrador.NombreAdministrador + " " + administrador.ApellidoAdministrador;

            string cuerpo = $@"
                <html>
                    <body style=""margin:0; padding:0; background-color:#F7F5FC; font-family:'Segoe UI', Roboto, Helvetica, Arial, sans-serif;"">
                        <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color:#F7F5FC;"">
                            <tr>
                                <td align=""center"" style=""padding:32px 16px;"">
                                    <table role=""presentation"" width=""600"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""width:100%; max-width:600px;"">
                                        <tr>
                                            <td align=""center"" bgcolor=""#373A72"" style=""background-color:#373A72; border-radius:15px 15px 0 0; padding:32px 24px;"">
                                                <span style=""font-size:36px; font-weight:700; color:#FFFFFF;"">InstiRed</span>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td bgcolor=""#FFFFFF"" style=""background-color:#FFFFFF; border:1px solid #E4E0F0; border-top:none; border-radius:0 0 15px 15px; padding:40px 36px;"">
                                                <h1 style=""margin:0 0 16px 0; font-size:22px; color:#373A72;"">Tu cuenta fue reactivada</h1>

                                                <p style=""font-size:15px; line-height:1.6; color:#373A72;"">
                                                    Hola, <strong>{nombre}</strong>. Tu cuenta de administrador fue reactivada correctamente.
                                                </p>

                                                <p style=""font-size:15px; line-height:1.6; color:#373A72;"">
                                                    Ya podés volver a ingresar a InstiRed y utilizar las funcionalidades correspondientes a tu cuenta.
                                                </p>

                                                <hr style=""border:none; border-top:1px solid #E4E0F0; margin:24px 0 20px 0;"">

                                                <p style=""font-size:13px; line-height:1.5; color:#8D91D5;"">
                                                    Este es un correo automático. Por favor, no respondas a esta dirección.
                                                </p>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>
                        </table>
                    </body>
                </html>";

            return cuerpo;
        }

        private string CuerpoAdministradorDadoDeBaja(AdministradorApi administrador)
        {
            string nombre = administrador.NombreAdministrador + " " + administrador.ApellidoAdministrador;

            string cuerpo = $@"
                <html>
                    <body style=""margin:0; padding:0; background-color:#F7F5FC; font-family:'Segoe UI', Roboto, Helvetica, Arial, sans-serif;"">
                        <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color:#F7F5FC;"">
                            <tr>
                                <td align=""center"" style=""padding:32px 16px;"">
                                    <table role=""presentation"" width=""600"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""width:100%; max-width:600px;"">
                                        <tr>
                                            <td align=""center"" bgcolor=""#373A72"" style=""background-color:#373A72; border-radius:15px 15px 0 0; padding:32px 24px;"">
                                                <span style=""font-size:36px; font-weight:700; color:#FFFFFF;"">InstiRed</span>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td bgcolor=""#FFFFFF"" style=""background-color:#FFFFFF; border:1px solid #E4E0F0; border-top:none; border-radius:0 0 15px 15px; padding:40px 36px;"">
                                                <h1 style=""margin:0 0 16px 0; font-size:22px; color:#373A72;"">Tu cuenta fue dada de baja</h1>

                                                <p style=""font-size:15px; line-height:1.6; color:#373A72;"">
                                                    Hola, <strong>{nombre}</strong>. Tu cuenta de administrador fue dada de baja.
                                                </p>

                                                <p style=""font-size:15px; line-height:1.6; color:#373A72;"">
                                                    Ya no podrás ingresar a InstiRed ni utilizar las funcionalidades correspondientes a esta cuenta.
                                                </p>

                                                <hr style=""border:none; border-top:1px solid #E4E0F0; margin:24px 0 20px 0;"">

                                                <p style=""font-size:13px; line-height:1.5; color:#8D91D5;"">
                                                    Este es un correo automático. Por favor, no respondas a esta dirección.
                                                </p>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>
                        </table>
                    </body>
                </html>";

            return cuerpo;
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
