using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Presentacion
{
    public partial class RegistroAlumno : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        // Si el alumno es el propio responsable (autogestor) no hace falta el vínculo
        protected void chkAutogestor_CheckedChanged(object sender, EventArgs e)
        {
            lblMensaje.Visible = false;
            pnlRol.Visible = !chkAutogestor.Checked;
            // PRUEBA provisoria: borrar cuando se conecte la API (GET ent/Personas/{idPersona})
            if (chkAutogestor.Checked)
            {
                txtNombre.Text = "prueba bloqueo";
                txtApellido.Text = "Apellido bloqueado";
                txtDocumento.Text = "40111222";
                txtDomicilio.Text = "Calle bloqueada 123";
                txtFechaNacimiento.Text = "1992-03-15";
                BloquearDatos(true);
            }

            // API: si es autogestor, completar los datos con los de la persona logueada.
            //   GET ent/Personas/{idPersona}  (idPersona sale de la sesión del login, Session["IdPersona"])
            //   Respuesta: { id, nombre, apellido, documento, domicilio, fechaNacimiento }
            //   Completar txtNombre, txtApellido, txtDocumento, txtDomicilio y txtFechaNacimiento
            //   (la fecha en formato yyyy-MM-dd) y luego llamar a BloquearDatos(true) para dejarlos en solo lectura.
            //   Al destildar, LimpiarCampos() los vuelve a dejar editables y vacíos.
            if (!chkAutogestor.Checked)
            {
                LimpiarCampos();
            }
        }

        protected void btnRegistrar_Click(object sender, EventArgs e)
        {
            lblMensaje.Visible = false;
            // Validaciones básicas 
            if (string.IsNullOrWhiteSpace(txtDocumento.Text))
            {
                MostrarMensaje("El documento es obligatorio.", false);
                return;
            }
            if (txtDocumento.Text.Trim().StartsWith("0"))
            {
                MostrarMensaje("El documento no puede empezar con 0.", false);
                return;
            }
            if (string.IsNullOrWhiteSpace(txtFechaNacimiento.Text))
            {
                MostrarMensaje("La fecha de nacimiento es obligatoria.", false);
                return;
            }
            if (!chkAutogestor.Checked && ObtenerRol() == 0)
            {
                MostrarMensaje("Elegí tu vínculo con el alumno.", false);
                return;
            }
            if (string.IsNullOrWhiteSpace(txtNombre.Text) || string.IsNullOrWhiteSpace(txtApellido.Text) || string.IsNullOrWhiteSpace(txtDomicilio.Text))
            {
                MostrarMensaje("Nombre, apellido y domicilio son obligatorios.", false);
                return;
            }

            // API: POST al sp_RegistrarAlumno (idResponsable, idRol, nombre, apellido documento, domicilio, fechaNacimiento)
            //   idResponsable: deberia salir de la sesión del login (Session["IdResponsable"])
            //   idRol: ObtenerRol() -> 1 Padre, 2 Madre, 3 Tutor legal, 4 Familiar a cargo (igual que en archivo de Inserts)
            //          Si chkAutogestor está tildado se deben autocompletar los datos, idRol se debería mandar null (el SP creo que lo ignora).           
            //   Respuesta: { idAlumno, idPersona }
            //   Si da error, mostrar el mensaje que devuelve con MostrarMensaje(mensaje, false)
            //   Si da ok, MostrarMensaje("Alumno registrado correctamente.", true) y LimpiarCampos()

            MostrarMensaje("Alumno registrado correctamente.", true);
            LimpiarCampos();
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            // Ajustar el destino cuando exista la pantalla principal del responsable
            Response.Redirect("~/Login.aspx");
        }

        // Devuelve el id del rol elegido (0 si no eligió ninguno)
        private int ObtenerRol()
        {
            if (rbPadre.Checked) return 1;
            if (rbMadre.Checked) return 2;
            if (rbTutor.Checked) return 3;
            if (rbFamiliar.Checked) return 4;
            return 0;
        }

        // Deja el formulario como recién abierto (se mantiene el mensaje de resultado)
        private void LimpiarCampos()
        {
            chkAutogestor.Checked = false;
            BloquearDatos(false);
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

        // Deja los campos de datos en solo lectura (true) o editables (false).
        // Se usa con el autogestor: sus datos vienen de la API y no se pueden cambiar.
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
            lblMensaje.Text = texto;
            lblMensaje.CssClass = ok ? "d-block mb-3 fg-success" : "d-block mb-3 fg-danger";
            lblMensaje.Visible = true;
        }
    }
}
