using Dominio;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Presentacion
{
    public partial class FormularioInstitucion : System.Web.UI.Page
    {
        private Institucion institucion;
        public string institucionJson { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            institucion = CargarInstitucion();

            // Marcador del mapa
            institucionJson = new JavaScriptSerializer().Serialize(new { institucion.Id, institucion.Nombre, institucion.Direccion, institucion.Latitud, institucion.Longitud });

            if (!IsPostBack)
            {
                CargarCampos();
            }
        }

        private Institucion CargarInstitucion()
        {
            Institucion actual = (Institucion)Session["institucion"];

            // REGISTRAR
            if(actual == null)
            {
                actual = NuevaInstitucion();
                Session["institucion"] = actual;
            }
            
            // MODIFICAR
            if(actual.Telefonos == null)
            {
                actual.Telefonos = new List<Telefono>();
            }

            if(actual.Idiomas == null)
            {
                actual.Idiomas = new List<Idioma>();
            }

            if(actual.Discapacidad == null)
            {
                actual.Discapacidad = new Discapacidad();
            }

            if(actual.Imagenes == null)
            {
                actual.Imagenes = new List<Imagen>();
            }

            return actual;

        }

        private Institucion NuevaInstitucion()
        {
            Institucion nueva = new Institucion();

            nueva.Telefonos = new List<Telefono>();
            nueva.Idiomas = new List<Idioma>();
            nueva.Discapacidad = new Discapacidad();
            nueva.Imagenes = new List<Imagen>();

            // REGISTROS DE EJEMPLO (eliminar cuando se carguen imágenes reales)
            nueva.Imagenes.Add(new Imagen { Id = 1, Descripcion = "Ejemplo 1", Url = "https://placehold.co/400x250" });
            nueva.Imagenes.Add(new Imagen { Id = 2, Descripcion = "Ejemplo 2", Url = "https://placehold.co/400x250" });
            nueva.Imagenes.Add(new Imagen { Id = 3, Descripcion = "Ejemplo 3", Url = "https://placehold.co/400x250" });
            nueva.Imagenes.Add(new Imagen { Id = 4, Descripcion = "Ejemplo 4", Url = "https://placehold.co/400x250" });
            nueva.Imagenes.Add(new Imagen { Id = 5, Descripcion = "Ejemplo 5", Url = "https://placehold.co/400x250" });
            nueva.Imagenes.Add(new Imagen { Id = 6, Descripcion = "Ejemplo 6", Url = "https://placehold.co/400x250" });

            return nueva;
        }

        public void CargarCampos()
        {
            CargarDatosBasicos();
            CargarIdiomas();
            CargarAccesibilidades();
            CargarImagenes();
        }

        private void CargarDatosBasicos()
        {
            txtNombre.Text = institucion.Nombre;
            txtCuit.Text = institucion.Cuit;
            txtEmail.Text = institucion.Email;
            txtDescripcion.Text = institucion.Descripcion;
            txtTelefono.Text = institucion.Telefonos.Count > 0 ? institucion.Telefonos[0].NumeroTelefono : ""; // En caso de que este vacio, ya que los telefonos son null
            txtTelefonoAlternativo.Text = institucion.Telefonos.Count > 1 ? institucion.Telefonos[1].NumeroTelefono : "";
            txtDireccion.Text = institucion.Direccion;
        }

        private void CargarIdiomas()
        {
            // REGISTRO DE EJEMPLO (TRAER DE LA DB)
            List<Idioma> idiomas = new List<Idioma>
            {
                new Idioma { Id = 1, NombreIdioma = "Español" },
                new Idioma { Id = 2, NombreIdioma = "Inglés" },
                new Idioma { Id = 3, NombreIdioma = "Portugués" },
                new Idioma { Id = 4, NombreIdioma = "Francés" },
                new Idioma { Id = 5, NombreIdioma = "Italiano" }
            };

            // IDIOMAS ELEGIDOS
            rptIdiomas.DataSource = institucion.Idiomas;
            rptIdiomas.DataBind();

            // IDIOMAS SIN ELEGIR
            ddlIdiomas.DataSource = idiomas.Where(aux => !institucion.Idiomas.Any(i => i.Id == aux.Id)).ToList();
            ddlIdiomas.DataTextField = "NombreIdioma";
            ddlIdiomas.DataValueField = "Id";
            ddlIdiomas.DataBind();
            ddlIdiomas.Items.Insert(0, new ListItem("Seleccionar...", ""));
        }

        private void CargarAccesibilidades()
        {
            List<string> elegidas = institucion.Discapacidad.ObtenerAccesibilidades();

            // ACCESIBILIDADES ELEGIDAS
            rptAccesibilidades.DataSource = elegidas;
            rptAccesibilidades.DataBind();

            // ACCESIBILIDADES SIN ELEGIR
            ddlAccesibilidades.DataSource = Discapacidad.ObtenerTiposDiscapacidad().Where(d => !elegidas.Contains(d)).ToList();
            ddlAccesibilidades.DataBind();
            ddlAccesibilidades.Items.Insert(0, new ListItem("Seleccionar...", ""));
        }

        private void CargarImagenes()
        {
            rptImagenes.DataSource = institucion.Imagenes;
            rptImagenes.DataBind();
        }

        protected void btn_AgregarIdioma_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(ddlIdiomas.SelectedValue))
            {
                int id = int.Parse(ddlIdiomas.SelectedValue);

                if (!institucion.Idiomas.Any(i => i.Id == id))
                {
                    institucion.Idiomas.Add(new Idioma
                    {
                        Id = id,
                        NombreIdioma = ddlIdiomas.SelectedItem.Text
                    });
                }
            }

            CargarIdiomas();
        }

        protected void rptIdiomas_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "EliminarIdioma")
            {
                int id = int.Parse(e.CommandArgument.ToString());
                institucion.Idiomas.RemoveAll(i => i.Id == id);

                CargarIdiomas();
            }
        }

        protected void btn_AgregarAccesibilidad_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(ddlAccesibilidades.SelectedValue))
            {
                AsignarAccesibilidad(ddlAccesibilidades.SelectedValue, true);
            }

            CargarAccesibilidades();
        }

        protected void rptAccesibilidades_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "EliminarAccesibilidad")
            {
                AsignarAccesibilidad(e.CommandArgument.ToString(), false);

                CargarAccesibilidades();
            }
        }

        private void AsignarAccesibilidad(string nombre, bool valor)
        {
            Discapacidad discapacidad = institucion.Discapacidad;

            switch (nombre)
            {
                case "Visual":
                    discapacidad.Visual = valor;
                    break;

                case "Verbal":
                    discapacidad.Verbal = valor;
                    break;

                case "Auditiva":
                    discapacidad.Auditiva = valor;
                    break;

                case "Intelectual":
                    discapacidad.Intelectual = valor;
                    break;

                case "Movilidad":
                    discapacidad.Movilidad = valor;
                    break;

                case "Síndrome de Down":
                    discapacidad.SindromeDown = valor;
                    break;

                case "Autismo":
                    discapacidad.Autismo = valor;
                    break;
            }
        }

        protected void rptImagenes_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "EliminarImagen")
            {
                // Se elimina por posición, porque dos imágenes podrían repetir la URL
                institucion.Imagenes.RemoveAt(int.Parse(e.CommandArgument.ToString()));

                CargarImagenes();
            }
        }
    }
}