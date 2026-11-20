using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Presentacion
{
    public partial class ResponsableGestionarPostulacionAlumno : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            btnVolver.Visible = true;

            int id;
            if (!int.TryParse(Request.QueryString["id"], out id) || id <= 0)
            {
                MostrarMensaje("Elegí una postulación desde el listado.", false);
                OcultarBotones();
                return;
            }
         
            
            if (!IsPostBack)
            {
                CargarPostulacionAlumno(id);
                
            }

        }

        protected void btnCancelarPostulacion_Click(object sender, EventArgs e)
        {
            // aca llamamos el sp_Cancelar_Postulacion_Alumno

            try
            {
                MostrarMensaje("La postulación fue cancelada correctamente!", true);
                RedirigirListadoPostulaciones();
            }catch(Exception ex) {
                MostrarMensaje(ex.Message, false);
    
            }
        }

        protected void btnConfirmar_Click(object sender, EventArgs e)
        {
            //aca llamamos el sp_Confirmar_Postulacion_Alumno
            try
            {
                MostrarMensaje("La postulación fue confirmada con éxito!",true);
                RedirigirListadoPostulaciones();
            }catch(Exception ex)
            {
                MostrarMensaje(ex.Message,false);
          
            }
           
            
        }

        protected void btnVolver_Click(object sender, EventArgs e)
        {
            Response.Redirect("ListadoPostulacionesAlumnos.aspx");
        }

        private void CargarPostulacionAlumno(int id)
        {
            //obtenemos el ID de la postulación seleccionada en el listado de postulaciones de los alumnos 
            //Consultar a la API para obtener los datos de la postulacion
            //Completamos los labels con los datos de la postulación
            //si no se carga: Mostramos mensaje ("No se pudo cargar la postulación", false); y oculatamos botones!!  OcultarBotones();

            //ConfigurarColorEtiquetaEstado(idEstado);
            //ConfigurarBotonesPorEstado(idEstado, cicloLectivo);
            //ConfigurarTipoDeCurso(idNivel,anio, nivelPersonalizado, motivoRechazo);

        }

        private void ConfigurarBotonesPorEstado(int idEstado, int cicloLectivo)
        {
            int anioActual = DateTime.Now.Year;

            btnConfirmar.Visible = false;
            btnCancelarPostulacion.Visible = false;
            btnVolver.Visible = true;


            if (cicloLectivo < anioActual || cicloLectivo >anioActual + 1)
                return;

            switch (idEstado)
            {
                case 1:
                    // Pendiente
                    btnCancelarPostulacion.Visible = true;
                    break;

                case 2:
                    //Aceptado
                    btnConfirmar.Visible = true;
                    btnCancelarPostulacion.Visible = true;

                    break;
               
                case 3:
                    //Confirmado
              
                        btnCancelarPostulacion.Visible = true;
                        // aca se podría poner un modal pero no se como probar si funciona con este boostrap. Hay que probar cuando este funcionando la pantalla
                        btnCancelarPostulacion.OnClientClick = "return confirm('Esta postulación se encuentra confirmada. Si se cancela se perderá el cupo reservado en el curso.¿Desea continuar?');";
                    
                    break;  
            }

        }

        private void ConfigurarTipoDeCurso(int? idNivel, int? anio, String nivelPersonalizado, String motivoRechazo)
        {
            pnlAnio.Visible = false;
            pnlMotivoRechazo.Visible = false;
            pnlNivel.Visible = false;
            pnlNivelPersonalizado.Visible = false;
            pnlDivision.Visible = true;

            if (!string.IsNullOrWhiteSpace(nivelPersonalizado) && !idNivel.HasValue && !anio.HasValue){
               
                pnlNivelPersonalizado.Visible = true;
            }
            else if (idNivel.HasValue && anio.HasValue)
            {
                pnlAnio.Visible = true;
                pnlNivel.Visible = true;

            }
            if(!string.IsNullOrWhiteSpace(motivoRechazo))
            {
                pnlMotivoRechazo.Visible = true;
            }
          
        }

        private void ConfigurarColorEtiquetaEstado(int idEstado)
        {
            switch(idEstado)
            {
                case 1: 
                    // pendiente
                    lblEstado.CssClass = "badge rounded-pill estado-pendiente";
                    break;

                case 2: 
                    //aceptado
                    lblEstado.CssClass = "badge rounded-pill bg-success";
                    break;
                case 3:
                    //confirmado
                    lblEstado.CssClass = "badge rounded-pill bg-primary";
                    break;
                case 4: 
                    //rechazada
                    lblEstado.CssClass = "badge rounded-pill bg-danger";
                    break;
                case 5:
                    //cancelada
                    lblEstado.CssClass = "badge rounded-pill bg-secondary";
                    break;

            }
        }
        private void OcultarBotones()
        {
            btnConfirmar.Visible = false;
            btnCancelarPostulacion.Visible = false;
        }

        private void RedirigirListadoPostulaciones()
        {
            ClientScript.RegisterStartupScript(this.GetType(), "Redirección", "setTimeout(() => window.location.href='ListadoPostulacionesAlumnos.aspx',3000);", true);
        }

        private void MostrarMensaje(string texto, bool ok)
        {
            lblMensaje.Text = texto;
            lblMensaje.CssClass = ok ? "d-block mb-3 fg-success" : "d-block mb-3 fg-danger";
            lblMensaje.Visible = true;
        }


    }
}