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
        public string InstitucionesJson { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                List<Institucion> listaInstituciones = CargarInstituciones();

                rptInstituciones.DataSource = listaInstituciones;
                rptInstituciones.DataBind();

                InstitucionesJson = new JavaScriptSerializer().Serialize(
                    listaInstituciones.Select(i => new { i.Id, i.Nombre, i.Direccion, i.Latitud, i.Longitud })
                );
            }

        }

        private List<Institucion> CargarInstituciones()
        {
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
                    Estado = true
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
                    Estado = true
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
                    Longitud = -59.3980,
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
                    Longitud = -59.3810,
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
                    Longitud = -59.3890,
                    Estado = true
                }
            };

            return instituciones;
        }

    }
}