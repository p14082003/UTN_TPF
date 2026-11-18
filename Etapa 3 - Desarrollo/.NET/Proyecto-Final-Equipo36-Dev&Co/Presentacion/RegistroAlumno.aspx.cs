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
            pnlRol.Visible = !chkAutogestor.Checked;
        }

        protected void btnRegistrar_Click(object sender, EventArgs e)
        {
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
            //          Si chkAutogestor está tildado se manda null (el SP creo que lo ignora)
            //          Si chkAutogestor está tildado no se debería pedir el DNI pero hoy no tenemos un SP que lo devuelva, ver para agregarlo en login o buscar otra alternativa.           
            //   Respuesta: { idAlumno, idPersona }
            //   Si da error, mostrar el mensaje que devuelve con MostrarMensaje(mensaje, false)

            MostrarMensaje("Alumno registrado correctamente.", true);
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

        private void MostrarMensaje(string texto, bool ok)
        {
            lblMensaje.Text = texto;
            lblMensaje.CssClass = ok ? "d-block mb-3 fg-success" : "d-block mb-3 fg-danger";
            lblMensaje.Visible = true;
        }
    }
}