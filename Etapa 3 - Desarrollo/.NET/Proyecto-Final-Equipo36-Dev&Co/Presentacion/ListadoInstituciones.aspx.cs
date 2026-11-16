using Dominio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Presentacion
{
    public partial class ListadoInstituciones : System.Web.UI.Page
    {
        public List<Institucion> listaInstituciones;
        public string InstitucionesJson { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {       
                CargarDiscapacidades();
                CargarIdiomas();
                CargarListado();
            }

        }

        private void CargarListado()
        {
            List<Institucion> lista = CargarInstituciones(txtFiltro.Text.Trim(), ddlDiscapacidades.SelectedValue, int.Parse(ddlIdiomas.SelectedValue));

            // Repeater de cards
            rptInstituciones.DataSource = lista;
            rptInstituciones.DataBind();

            // Json para los marcadores del mapa
            InstitucionesJson = new JavaScriptSerializer().Serialize(
                lista.Select(i => new { i.Id, i.Nombre, i.Direccion, i.Latitud, i.Longitud }));
        }

        private List<Institucion> CargarInstituciones(string texto, string tipoDiscapacidad, int idIdioma)
        {
            listaInstituciones = ObtenerHardcodeadas(); // SE TIENE QUE TRAER LA LISTA CON LA CLASE DE NEGOCIO
            List<Institucion> listaFiltrada = new List<Institucion>();

            foreach (Institucion institucion in listaInstituciones)
            {
                bool bandera = true;

                // Texto: nombre o dirección
                if (texto != "")
                {
                    string aux = texto.ToLower();

                    if (!institucion.Nombre.ToLower().Contains(aux) && !institucion.Direccion.ToLower().Contains(aux))
                    {
                        bandera = false;
                    }
                }

                // Discapacidad
                if (tipoDiscapacidad != "" && tipoDiscapacidad != "0")
                {
                    if (institucion.Discapacidad == null || !institucion.Discapacidad.ObtenerAccesibilidades().Contains(tipoDiscapacidad))
                    {
                        bandera = false;
                    }
                }

                // Idioma
                if (idIdioma != 0)
                {
                    bool tieneIdioma = false;

                    if (institucion.Idiomas != null)
                    {
                        foreach (Idioma idioma in institucion.Idiomas)
                        {
                            if (idioma.Id == idIdioma)
                            {
                                tieneIdioma = true;
                            }
                        }
                    }

                    if (!tieneIdioma)
                    {
                        bandera = false;
                    }
                }

                if (bandera)
                {
                    listaFiltrada.Add(institucion);
                }
            }

            return listaFiltrada;
        }

        private void CargarDiscapacidades()
        {
            foreach(string tipo in Discapacidad.ObtenerTiposDiscapacidad())
            {
                ddlDiscapacidades.Items.Add(new ListItem(tipo, tipo));
            }
        }

        private void CargarIdiomas()
        {
            // ACA DEBERIA IR EL MÉTODO DE NEGOCIO QUE TRAIGA LA LISTA DE IDIOMAS DE LA BD
            List<Idioma> lista = new List<Idioma>();

            lista.Add(new Idioma { Id = 1, NombreIdioma = "Español" });
            lista.Add(new Idioma { Id = 2, NombreIdioma = "Inglés" });

            ddlIdiomas.DataSource = lista;
            ddlIdiomas.DataValueField = "Id";
            ddlIdiomas.DataTextField = "NombreIdioma";
            ddlIdiomas.DataBind();  
        }

        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            CargarListado();
        }

        protected void btnDetalle_Click(object sender, EventArgs e)
        {
            Button boton = (Button)sender;
            int id = int.Parse(boton.CommandArgument);

            Response.Redirect("DetalleInstitucion.aspx?id=" + id, false);
        }

        private List<Institucion> ObtenerHardcodeadas()
        {
            Idioma espanol = new Idioma { Id = 1, NombreIdioma = "Español" };
            Idioma ingles = new Idioma { Id = 2, NombreIdioma = "Inglés" };

            List<Institucion> instituciones = new List<Institucion>
            {
                new Institucion
                {
                    Id = 1,
                    Nombre = "Instituto Educativo San Martín",
                    Cuit = "30-12345678-9",
                    Direccion = "Av. Corrientes 1000, Buenos Aires",
                    Email = "contacto@sanmartin.com",
                    Descripcion = "Institución educativa de nivel secundario.",
                    Latitud = -34.6037,
                    Longitud = -58.3816,
                    Discapacidad = new Discapacidad { Movilidad = true },
                    Idiomas = new List<Idioma> { espanol },
                    Estado = true
                },
                new Institucion
                {
                    Id = 2,
                    Nombre = "Escuela Primaria Belgrano",
                    Cuit = "30-98765432-1",
                    Direccion = "Av. Rivadavia 2000, Buenos Aires",
                    Email = "info@belgrano.com",
                    Descripcion = "Institución educativa de nivel primario.",
                    Latitud = -34.6090,
                    Longitud = -58.3920,
                    Discapacidad = new Discapacidad { Movilidad = true, Visual = true, Auditiva = true, Verbal = true, Intelectual = true, SindromeDown = true, Autismo = true },
                    Idiomas = new List<Idioma> { espanol, ingles },
                    Imagenes = new List<Imagen>(),
                    Estado = true,
                },
                new Institucion
                {
                    Id = 3,
                    Nombre = "Colegio Nuestra Señora",
                    Cuit = "30-45678912-3",
                    Direccion = "Av. Santa Fe 1500, Buenos Aires",
                    Email = "contacto@nuestrasenora.com",
                    Descripcion = "Institución educativa de nivel inicial y secundario.",
                    Latitud = -34.5950,
                    Longitud = -58.3980,
                    Discapacidad = new Discapacidad { Movilidad = true, Auditiva = true, Verbal = true },
                    Idiomas = new List<Idioma> { espanol, ingles },
                    Estado = true
                },
                new Institucion
                {
                    Id = 4,
                    Nombre = "Instituto Técnico Nacional",
                    Cuit = "30-78912345-6",
                    Direccion = "Av. Independencia 800, Buenos Aires",
                    Email = "info@tecnico.com",
                    Descripcion = "Institución de formación técnica.",
                    Latitud = -34.6170,
                    Longitud = -58.3810,
                    Discapacidad = new Discapacidad(),
                    Idiomas = new List<Idioma> { espanol },
                     Imagenes = new List<Imagen> { new Imagen { Url = "https://placehold.co/400x250?text=prueba" } },
                    Estado = true
                },
                new Institucion
                {
                    Id = 5,
                    Nombre = "Escuela Integral del Sur",
                    Cuit = "30-32165498-7",
                    Direccion = "Av. San Juan 1200, Buenos Aires",
                    Email = "contacto@integraldelsur.com",
                    Descripcion = "Institución educativa integral.",
                    Latitud = -34.6210,
                    Longitud = -58.3890,
                    Discapacidad = new Discapacidad { Movilidad = true },
                    Idiomas = new List<Idioma> { espanol },
                    Estado = true
                },
                new Institucion
                {
                    Id = 6,
                    Nombre = "Colegio Nuestra Señora del Carmen",
                    Cuit = "30-45678912-3",
                    Direccion = "Av. Santa Fe 100, Buenos Aires",
                    Email = "contacto@nuestrasenoradelcarmen.com",
                    Descripcion = "Institución educativa de nivel inicial y secundario.",
                    Latitud = -34.5950,
                    Longitud = -59.3980,
                    Discapacidad = new Discapacidad { Movilidad = true, Verbal = true, Intelectual = true, Autismo = true },
                    Idiomas = new List<Idioma> { espanol, ingles },
                    Estado = true
                },
                new Institucion
                {
                    Id = 7,
                    Nombre = "Instituto Técnico Estatal",
                    Cuit = "30-78912345-6",
                    Direccion = "Av. Independencia 1200, Buenos Aires",
                    Email = "info@tecnicoestatal.com",
                    Descripcion = "Institución de formación técnica.",
                    Latitud = -34.6170,
                    Longitud = -59.3810,
                    Discapacidad = new Discapacidad { Movilidad = true },
                    Idiomas = new List<Idioma> { ingles },
                    Estado = true
                },
                new Institucion
                {
                    Id = 8,
                    Nombre = "Escuela Integral del Norte",
                    Cuit = "30-32165498-7",
                    Direccion = "Av. San Juan 600, Buenos Aires",
                    Email = "contacto@integraldelnorte.com",
                    Descripcion = "Institución educativa integral.",
                    Latitud = -34.6210,
                    Longitud = -59.3890,
                    Discapacidad = new Discapacidad(),
                    Idiomas = new List<Idioma> { espanol, ingles },
                    Estado = true
                }
            };

            return instituciones;
        }
    }
}