using Dominio;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Presentacion
{
    public partial class ListadoPostulacionAlumnos : System.Web.UI.Page
    {
        private string SessionPostulaciones = "PostulacionesResponsable";

        //Lista completa de alumnos del responsable, se guarda en Session para filtrar sin volver a la API 
        private List<PostulacionAlumnoListado> ListaPostulaciones
        {
            get {  return Session[SessionPostulaciones] as List<PostulacionAlumnoListado>; }
            set { Session[SessionPostulaciones] = value; }
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            if(!IsPostBack)
            {
                //se recarga lista siempre al cargar página para que no quede desactualiada pro si el estado de alguna cambio en la otra pantalla
                
                 CargarPostulaciones();
                 CargarDatosPruebaDesplegables();
                //CargarDesplegableEstados();
                //CargarDeplegableAlumnos();
                AplicarFiltros();

                //CargarDatosPrueba(); //Esto fue para probar pantalla hay que volarlo, mismo elmetodo de mas abajo.

            }
        }


        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            // aca llamamos el metodo aplicar filtros
             dgvPostulaciones.PageIndex = 0;
             AplicarFiltros();
        }

        protected void btnVerPostulacion_Click1(object sender, EventArgs e)
        {
            // Obtenemos IdPostulacion
            int idPostulacion = int.Parse(((LinkButton)sender).CommandArgument);
            //Redirigimos y enviamos ID
            Response.Redirect("ResponsableGestionarPostulacionAlumno.aspx?id=" + idPostulacion);
            
        }

        private void CargarPostulaciones() // FALTA ESTO POR API
        {
            //aca con la API y el sp_postulaciones_Por_Responsable llenamos la lista cuando se carga la pantalla por primera vez
            //listaPostulaciones = completamos con API
            //guardamos lista en la session para poder usarla filtrandola sin tener que volver a la db 
            ListaPostulaciones = CargarDatosPrueba(); //esto se vuela cuando este conectado
        
        }
        private void CargarDesplegableEstados() //FALTA COMPLETAR ESTO POR API - LOS METODOS ESTAN MAS ABAJO
        {
            //Obtener TODOS Estados Id+ descripcion mediante API
            //ddlEstadosPostulacion.DataSource = ObtenerEstados(); // DESCOMENTAR CUANDO ObtenerEstados(); ESTE VINCULADO CON API 
            ddlEstadosPostulacion.DataValueField = "Id";
            ddlEstadosPostulacion.DataTextField = "Descripcion";
            ddlEstadosPostulacion.DataBind();
           
        }
        private void CargarDeplegableAlumnos() //FALTA COMPLETAR ESTO POR API - LOS METODOS ESTAN MAS ABAJO
        {
            //Obtener ALumnos SOLO vinculados al idResponsable logueado mediante API (Alumno = concat nombrbe + apellido) y id 
            //ddlAlumnos.DataSource = ObtenerAlumnos(); //traerlos con API mediante el  sp_alumnos_Por_Responsable // DESCOMENTAR CUANDO ESTEN VINCULADAS CON API 
            ddlAlumnos.DataValueField = "IdAlumno";
            ddlAlumnos.DataTextField = "NombreCompleto";
            ddlAlumnos.DataBind();
          
        }
        private void AplicarFiltros()
        {
            //Traemos la lista de la session que guardamos luego de acceder al sp mediante API
            //con un foreach la recorremos y buscamos coicidencias de estado o alumno 
            ////CargarPostulaciones();
            ///limpiamos filtro
            List<PostulacionAlumnoListado> postulaciones = ListaPostulaciones;
            if (postulaciones == null)
            {
                return;
            }
            // obtenemos valores seleccionadis de los desplegables
            int idAlumno = int.Parse(ddlAlumnos.SelectedValue);
            int idEstado = int.Parse(ddlEstadosPostulacion.SelectedValue);
            //creamos lista para guardar el resultado 
            List<PostulacionAlumnoListado> resultado = new List<PostulacionAlumnoListado>();
            // recorremos la lista buscando coicidencias 
            foreach(PostulacionAlumnoListado postulacion in postulaciones) 
            { 
                if((idAlumno ==0 || postulacion.IdAlumno ==idAlumno) && (idEstado == 0 || postulacion.IdEstado == idEstado)){
                    resultado.Add(postulacion);
                }
            }
            //completamos el gridView con los datos nuevos filtrados 
            dgvPostulaciones.DataSource = resultado;
            dgvPostulaciones.DataBind();    

        }

        private List<PostulacionAlumnoListado> CargarDatosPrueba()
        {
            //provisorio lo volamos cuando este todo conectado
            List<PostulacionAlumnoListado> lista = new List<PostulacionAlumnoListado>();
            
            PostulacionAlumnoListado p1 = new PostulacionAlumnoListado();
            p1.IdPostulacion = 1;
            p1.IdAlumno = 1;
            p1.Alumno = "Sofía Martinez";
            p1.Documento = "40123456";
            p1.Institucion = "Colegio San Martín";
            p1.CicloLectivo = 2027;
            p1.IdEstado = 1;
            p1.Estado = "Pendiente";

            lista.Add(p1);

            PostulacionAlumnoListado p2 = new PostulacionAlumnoListado();
            p2.IdPostulacion = 2;
            p2.IdAlumno = 2;
            p2.Alumno = "Mateo Gonzalez";
            p2.Documento = "40123476";
            p2.Institucion = "Instituto Cervantes";
            p2.CicloLectivo = 2027;
            p2.IdEstado = 3;
            p2.Estado = "Confirmada";

            lista.Add(p2);

            PostulacionAlumnoListado p3 = new PostulacionAlumnoListado();
            p3.IdPostulacion = 3;
            p3.IdAlumno = 3;
            p3.Alumno = "Valentina Lopez";
            p3.Documento = "42123456";
            p3.Institucion = "Escuela Belgrano";
            p3.CicloLectivo = 2027;
            p3.IdEstado = 4;
            p3.Estado = "Rechazada";

            lista.Add(p3);
            PostulacionAlumnoListado p4 = new PostulacionAlumnoListado();
            p4.IdPostulacion = 4;
            p4.IdAlumno = 4;
            p4.Alumno = "Tomás Fernandez";
            p4.Documento = "40567890";
            p4.Institucion = "Coelgio del Sur";
            p4.CicloLectivo = 2027;
            p4.IdEstado = 1;
            p4.Estado = "Pendiente";

            lista.Add(p4);

            return lista;
        }
        //OJO ESTOS METODOS HAY QUE DESCOMENTARLOS Y VINCULARLOS CON LA API
        //private List <EstadoPostulacion> ObtenerEstados()
        //{
            //De API traer TODOS los estados
            //return new List<EstadoPostulacion> { };
        //}

        //private List<AlumnoResponsable> ObtenerAlumnos()
       // {
            // Mediante API y SP CREADO obtener TODOS alumnos vinculados al responsable
           // return new List<AlumnoResponsable> { };
       // }

        private void MostrarMensaje(string texto, bool ok)
        {
            lblMensaje.Text = texto;
            lblMensaje.CssClass = ok ? "d-block mb-3 fg-success" : "d-block mb-3 fg-danger";
            lblMensaje.Visible = true;
        }
        private void CargarDatosPruebaDesplegables()
        { 
            //esto es para probar, hay que volvarlo
            ddlAlumnos.Items.Clear();
            ddlAlumnos.Items.Add(new ListItem("Todos los alumnos", "0"));
            ddlAlumnos.Items.Add(new ListItem("Sofía Martínez", "1"));
            ddlAlumnos.Items.Add(new ListItem("Mateo González", "2"));
            ddlAlumnos.Items.Add(new ListItem("Valentina López", "3"));

            ddlEstadosPostulacion.Items.Clear();
            ddlEstadosPostulacion.Items.Add(new ListItem("Todos los estados", "0"));
            ddlEstadosPostulacion.Items.Add(new ListItem("Pendiente", "1"));
            ddlEstadosPostulacion.Items.Add(new ListItem("Confirmada", "3"));
            ddlEstadosPostulacion.Items.Add(new ListItem("Rechazada", "4"));
        }


        public class PostulacionAlumnoListado  //clase para representar postulacion para guardar en memoria la lista del sp y pooder filtrar desde aca
        {
            public int IdPostulacion {  get; set; }
            public int IdAlumno { get; set; }
            public string Alumno { get; set; }
            public string Documento { get; set; }
            public string Institucion  { get; set; }
            public int CicloLectivo {  get; set; }
            public int IdEstado  { get; set; }
            public string Estado  { get; set; }

        }
        public class AlumnoResponsable
        {
            public int IdAlumno { get;set; }
            public string NombreCompleto { get; set; }
        }

        protected void dgvPostulaciones_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            dgvPostulaciones.PageIndex = e.NewPageIndex;
            AplicarFiltros();
        }
    }
}