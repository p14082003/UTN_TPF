using Dominio;
using Negocio;
using Presentacion.Auth;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.UI;

namespace Presentacion
{
    public partial class RegistroAlumno : Page
    {
        private UsuarioSesion usuario;

        protected void Page_Init(object sender, EventArgs e)
        {
            ViewStateUserKey = Session.SessionID;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            usuario = UsuarioSesion.Obtener(Session);
            if (usuario == null || usuario.IdTipoUsuario != UsuarioSesion.Responsable ||
                usuario.IdResponsable.GetValueOrDefault() <= 0)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }
            Response.Cache.SetNoStore();
        }

        protected void chkAutogestor_CheckedChanged(object sender, EventArgs e)
        {
            lblMensaje.Visible = false;
            pnlRol.Visible = !chkAutogestor.Checked;
            if (!chkAutogestor.Checked)
            {
                LimpiarCampos();
                return;
            }

            BloquearDatos(true);
            btnRegistrar.Enabled = false;
            Ejecutar(async () =>
            {
                var persona = await ObtenerPersonaResponsableAsync();
                txtNombre.Text = persona.Nombre;
                txtApellido.Text = persona.Apellido;
                txtDocumento.Text = persona.Documento;
                txtDomicilio.Text = persona.Domicilio;
                txtFechaNacimiento.Text = persona.FechaNacimiento.ToString("yyyy-MM-dd");
                btnRegistrar.Enabled = true;
            });
        }

        private async Task<Persona> ObtenerPersonaResponsableAsync()
        {
            var responsable = await InstiRedApi.GetAsync<ResponsableApi>(
                "ent/Responsables/" + usuario.IdResponsable.Value);
            if (responsable.IdUsuario != usuario.IdUsuario || responsable.IdPersona <= 0)
            {
                throw new ArgumentException("La cuenta no corresponde al responsable de la sesión.");
            }

            return await InstiRedApi.GetAsync<Persona>("ent/Personas/" + responsable.IdPersona);
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
                    Trace.Warn("RegistroAlumno", "Error al comunicarse con la API de InstiRed.", ex);
                    MostrarMensaje("No se pudo completar la operación. Intentá nuevamente más tarde.", false);
                }
            }));
        }

        protected void btnRegistrar_Click(object sender, EventArgs e)
        {
            Ejecutar(RegistrarAsync);
        }

        private async Task RegistrarAsync()
        {
            object registro;
            if (chkAutogestor.Checked)
            {
                // El SP reutiliza a la persona del responsable cuando coincide su documento.
                var persona = await ObtenerPersonaResponsableAsync();
                registro = new
                {
                    idResponsable = usuario.IdResponsable.Value,
                    idRol = (int?)null,
                    documento = persona.Documento
                };
            }
            else
            {
                ValidarDatos();
                registro = new
                {
                    idResponsable = usuario.IdResponsable.Value,
                    idRol = ObtenerRol(),
                    nombre = txtNombre.Text.Trim(),
                    apellido = txtApellido.Text.Trim(),
                    documento = txtDocumento.Text.Trim(),
                    domicilio = txtDomicilio.Text.Trim(),
                    fechaNacimiento = txtFechaNacimiento.Text
                };
            }

            var respuesta = await InstiRedApi.PostAsync("sp/RegistrarAlumno", registro);
            if (respuesta.Estado == HttpStatusCode.Created)
            {
                LimpiarCampos();
                MostrarMensaje("Alumno registrado correctamente.", true);
                return;
            }

            MostrarMensaje(respuesta.Mensaje ?? "No se pudo completar el registro. Intentá nuevamente más tarde.", false);
        }

        private void ValidarDatos()
        {
            if (string.IsNullOrWhiteSpace(txtDocumento.Text) ||
                string.IsNullOrWhiteSpace(txtNombre.Text) ||
                string.IsNullOrWhiteSpace(txtApellido.Text) ||
                string.IsNullOrWhiteSpace(txtDomicilio.Text) ||
                string.IsNullOrWhiteSpace(txtFechaNacimiento.Text))
            {
                throw new ArgumentException("Completá todos los datos del alumno.");
            }

            string documento = txtDocumento.Text.Trim();
            if (documento.Length > 8 || documento.StartsWith("0") ||
                documento.Any(c => c < '0' || c > '9'))
            {
                throw new ArgumentException("Ingresá un DNI de hasta 8 dígitos, sin puntos y sin empezar con cero.");
            }
            if (txtNombre.Text.Trim().Length > 50 || txtApellido.Text.Trim().Length > 50 ||
                txtDomicilio.Text.Trim().Length > 100)
            {
                throw new ArgumentException("Revisá la longitud del nombre, apellido y domicilio.");
            }
            if (!DateTime.TryParse(txtFechaNacimiento.Text, out DateTime fecha) ||
                fecha.Year < 1000 || fecha.Date > DateTime.Today)
            {
                throw new ArgumentException("Ingresá una fecha de nacimiento válida.");
            }
            if (ObtenerRol() == 0)
            {
                throw new ArgumentException("Elegí tu vínculo con el alumno.");
            }
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/ListadoInstituciones.aspx");
        }

        private int ObtenerRol()
        {
            if (rbPadre.Checked)
            {
                return 1;
            }
            if (rbMadre.Checked)
            {
                return 2;
            }
            if (rbTutor.Checked)
            {
                return 3;
            }
            if (rbFamiliar.Checked)
            {
                return 4;
            }
            return 0;
        }

        private void LimpiarCampos()
        {
            chkAutogestor.Checked = false;
            BloquearDatos(false);
            btnRegistrar.Enabled = true;
            pnlRol.Visible = true;
            rbPadre.Checked = false;
            rbMadre.Checked = false;
            rbTutor.Checked = false;
            rbFamiliar.Checked = false;
            txtNombre.Text = "";
            txtApellido.Text = "";
            txtDocumento.Text = "";
            txtFechaNacimiento.Text = "";
            txtDomicilio.Text = "";
        }

        private void BloquearDatos(bool bloquear)
        {
            txtNombre.ReadOnly = bloquear;
            txtApellido.ReadOnly = bloquear;
            txtDocumento.ReadOnly = bloquear;
            txtFechaNacimiento.ReadOnly = bloquear;
            txtDomicilio.ReadOnly = bloquear;
        }

        private void MostrarMensaje(string texto, bool ok)
        {
            lblMensaje.Text = Server.HtmlEncode(texto);
            lblMensaje.CssClass = ok ? "d-block mb-3 fg-success" : "d-block mb-3 fg-danger";
            lblMensaje.Visible = true;
        }

        private class ResponsableApi
        {
            public int IdPersona { get; set; }
            public int IdUsuario { get; set; }
        }
    }
}
