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
    public partial class MiPerfil : Page
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
            if (usuario == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            Response.Cache.SetNoStore();
            if (!IsPostBack)
            {
                Ejecutar(CargarPerfilAsync);
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
                catch (Exception ex)
                {
                    string mensaje = "No se pudo completar la operación. Recargá la página antes de intentarlo nuevamente.";
                    if (ex is ArgumentException)
                    {
                        mensaje = ex.Message;
                    }
                    else
                    {
                        Trace.Warn("MiPerfil", "Error al consultar o guardar datos del perfil en la API.", ex);
                    }
                    if (cambioGuardado)
                    {
                        mensaje = "El cambio se guardó, pero no se pudo actualizar la pantalla. Recargá la página para verlo.";
                    }
                    MostrarMensaje(mensaje, false);
                }
            }));
        }

        private async Task CargarPerfilAsync()
        {
            var perfil = await InstiRedApi.GetAsync<PerfilApi>(
                "vw/PerfilUsuario/" + usuario.IdUsuario, permitirNoEncontrado: true);
            if (perfil == null || perfil.IdUsuario != usuario.IdUsuario)
            {
                throw new ArgumentException("No se encontró tu perfil. Volvé a iniciar sesión.");
            }

            usuario.Nombre = perfil.Nombre;
            usuario.Apellido = perfil.Apellido;
            usuario.Email = perfil.Email;
            usuario.IdTipoUsuario = perfil.IdTipoUsuario;
            usuario.IdResponsable = perfil.IdResponsable;
            usuario.IdAdministradorInstitucion = perfil.IdAdministrador;
            usuario.IdInstitucion = perfil.IdInstitucion;
            Session[UsuarioSesion.Clave] = usuario;

            bool esDevCo = usuario.IdTipoUsuario == UsuarioSesion.DevCo;
            bool esAdmin = usuario.IdTipoUsuario == UsuarioSesion.AdministradorInstitucion;
            pnlDatosPersonales.Visible = !esDevCo;
            pnlTelefonos.Visible = !esDevCo;
            pnlInstitucion.Visible = esAdmin;
            litNombreCompleto.Text = esDevCo ? "Equipo Dev&Co" : ValorOGuion((perfil.Nombre + " " + perfil.Apellido).Trim());
            litRol.Text = esDevCo ? "Equipo Dev&Co" : esAdmin ? "Administrador de institución" : "Responsable";
            litEmail.Text = ValorOGuion(perfil.Email);
            litInstitucion.Text = string.IsNullOrWhiteSpace(perfil.NombreInstitucion) ? "Sin institución asociada" : perfil.NombreInstitucion;
            litNombre.Text = ValorOGuion(perfil.Nombre);
            litApellido.Text = ValorOGuion(perfil.Apellido);
            litDocumento.Text = ValorOGuion(perfil.Documento);
            litFechaNacimiento.Text = perfil.FechaNacimiento.HasValue ? perfil.FechaNacimiento.Value.ToString("dd/MM/yyyy") : "—";
            litDomicilio.Text = ValorOGuion(perfil.Domicilio);
            btnEditarClave.Enabled = true;

            if (!esDevCo)
            {
                await CargarTelefonosAsync();
            }
        }

        private async Task CargarTelefonosAsync()
        {
            ValidarDuenoTelefonos();
            var telefonos = await InstiRedApi.GetAsync<List<TelefonoApi>>(
                "vw/TelefonosPorUsuario?idUsuario=" + usuario.IdUsuario);
            rptTelefonos.DataSource = telefonos.Select(t => new { Id = t.IdTelefono, Numero = t.Telefono }).ToList();
            rptTelefonos.DataBind();

            foreach (RepeaterItem item in rptTelefonos.Items)
            {
                item.FindControl("btnQuitarTelefono").Visible = pnlTelefonosEditar.Visible && telefonos.Count > 1;
            }
            pnlAgregarTelefono.Visible = telefonos.Count < 2;
            btnAgregarTelefono.Visible = telefonos.Count < 2;
            btnEditarTelefonos.Enabled = true;
        }

        private string TipoDueno
        {
            get { return usuario.IdTipoUsuario == UsuarioSesion.AdministradorInstitucion ? "Administrador" : "Responsable"; }
        }

        private int IdDueno
        {
            get
            {
                return usuario.IdTipoUsuario == UsuarioSesion.AdministradorInstitucion
                    ? usuario.IdAdministradorInstitucion.GetValueOrDefault()
                    : usuario.IdResponsable.GetValueOrDefault();
            }
        }

        private void ValidarDuenoTelefonos()
        {
            if ((usuario.IdTipoUsuario != UsuarioSesion.AdministradorInstitucion &&
                usuario.IdTipoUsuario != UsuarioSesion.Responsable) || IdDueno <= 0)
            {
                throw new ArgumentException("Tu cuenta no tiene un responsable o administrador asociado para gestionar teléfonos.");
            }
        }

        protected void btnEditarTelefonos_Click(object sender, EventArgs e)
        {
            pnlTelefonosEditar.Visible = true;
            btnEditarTelefonos.Visible = false;
            Ejecutar(CargarTelefonosAsync);
        }

        protected void btnListoTelefonos_Click(object sender, EventArgs e)
        {
            pnlTelefonosEditar.Visible = false;
            btnEditarTelefonos.Visible = true;
            txtTelefono.Text = "";
            Ejecutar(CargarTelefonosAsync);
        }

        protected void btnAgregarTelefono_Click(object sender, EventArgs e)
        {
            Ejecutar(AgregarTelefonoAsync);
        }

        private async Task AgregarTelefonoAsync()
        {
            ValidarDuenoTelefonos();
            string telefono = txtTelefono.Text.Trim();
            if (telefono == "" || telefono.Length > 30 || !telefono.Any(char.IsDigit))
            {
                throw new ArgumentException("Ingresá un teléfono de hasta 30 caracteres que incluya números.");
            }

            var resultado = await InstiRedApi.PostAsync("sp/AgregarTelefono", new
            {
                idUsuarioActor = usuario.IdUsuario,
                tipoDueno = TipoDueno,
                idDueno = IdDueno,
                telefono
            });
            ValidarRespuesta(resultado);
            cambioGuardado = true;
            txtTelefono.Text = "";
            await CargarTelefonosAsync();
            MostrarMensaje("Teléfono agregado correctamente.", true);
        }

        protected void rptTelefonos_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "Quitar")
            {
                return;
            }
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out int idTelefono) || idTelefono <= 0)
            {
                MostrarMensaje("Seleccioná un teléfono válido.", false);
                return;
            }
            Ejecutar(() => QuitarTelefonoAsync(idTelefono));
        }

        private async Task QuitarTelefonoAsync(int idTelefono)
        {
            ValidarDuenoTelefonos();
            var resultado = await InstiRedApi.PostAsync("sp/EliminarTelefono", new
            {
                idUsuarioActor = usuario.IdUsuario,
                tipoDueno = TipoDueno,
                idDueno = IdDueno,
                idTelefono
            });
            ValidarRespuesta(resultado);
            cambioGuardado = true;
            await CargarTelefonosAsync();
            MostrarMensaje("Teléfono quitado correctamente.", true);
        }

        protected void btnEditarClave_Click(object sender, EventArgs e)
        {
            lblMensaje.Visible = false;
            pnlClaveVer.Visible = false;
            pnlClaveEditar.Visible = true;
            btnEditarClave.Visible = false;
        }

        protected void btnGuardarClave_Click(object sender, EventArgs e)
        {
            Ejecutar(CambiarClaveAsync);
        }

        private async Task CambiarClaveAsync()
        {
            if (string.IsNullOrWhiteSpace(txtClaveActual.Text) ||
                string.IsNullOrWhiteSpace(txtClaveNueva.Text) ||
                string.IsNullOrWhiteSpace(txtClaveRepetida.Text))
            {
                throw new ArgumentException("Completá la contraseña actual y la nueva.");
            }

            string clave = txtClaveNueva.Text;
            if (clave.Length < 8 || clave.Length > 20 ||
                !clave.Any(char.IsUpper) || !clave.Any(char.IsLower) ||
                !clave.Any(char.IsDigit) || !clave.Any(c => char.IsPunctuation(c) || char.IsSymbol(c)))
            {
                throw new ArgumentException("La contraseña nueva debe tener entre 8 y 20 caracteres e incluir una mayúscula, una minúscula, un número y un símbolo.");
            }
            if (clave != txtClaveRepetida.Text)
            {
                throw new ArgumentException("Las contraseñas nuevas no coinciden.");
            }
            if (clave == txtClaveActual.Text)
            {
                throw new ArgumentException("La contraseña nueva tiene que ser distinta de la actual.");
            }

            var resultado = await InstiRedApi.PostAsync("sp/CambiarClave", new
            {
                idUsuarioActor = usuario.IdUsuario,
                claveActual = txtClaveActual.Text,
                claveNueva = clave
            });
            ValidarRespuesta(resultado);
            SalirEdicionClave();
            MostrarMensaje("Contraseña actualizada correctamente.", true);
        }

        protected void btnCancelarClave_Click(object sender, EventArgs e)
        {
            lblMensaje.Visible = false;
            SalirEdicionClave();
        }

        private void SalirEdicionClave()
        {
            txtClaveActual.Text = "";
            txtClaveNueva.Text = "";
            txtClaveRepetida.Text = "";
            pnlClaveEditar.Visible = false;
            pnlClaveVer.Visible = true;
            btnEditarClave.Visible = true;
        }

        private static void ValidarRespuesta(InstiRedApi.Respuesta respuesta)
        {
            if (respuesta.Estado != HttpStatusCode.OK)
            {
                throw new ArgumentException(respuesta.Mensaje ?? "No se pudo confirmar el cambio. Recargá la página antes de intentarlo nuevamente.");
            }
        }

        private static string ValorOGuion(string valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? "—" : valor;
        }

        private void MostrarMensaje(string texto, bool ok)
        {
            lblMensaje.Text = Server.HtmlEncode(texto);
            lblMensaje.CssClass = ok ? "d-block mb-3 fg-success" : "d-block mb-3 fg-danger";
            lblMensaje.Visible = true;
        }

        private class PerfilApi
        {
            public int IdUsuario { get; set; }
            public int IdTipoUsuario { get; set; }
            public string Nombre { get; set; }
            public string Apellido { get; set; }
            public string Email { get; set; }
            public string Documento { get; set; }
            public DateTime? FechaNacimiento { get; set; }
            public string Domicilio { get; set; }
            public int? IdResponsable { get; set; }
            public int? IdAdministrador { get; set; }
            public int? IdInstitucion { get; set; }
            public string NombreInstitucion { get; set; }
        }

        private class TelefonoApi
        {
            public int IdTelefono { get; set; }
            public string Telefono { get; set; }
        }
    }
}
