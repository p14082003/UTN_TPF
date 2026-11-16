using Dominio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Presentacion
{
    public partial class DetalleInstitucion : System.Web.UI.Page
    {
        Institucion institucion;
        public string institucionJson { get; set; }
        List<Imagen> imagenes;

        protected void Page_Load(object sender, EventArgs e)
        {
            CargarInstitucion();

            // Marcador del mapa
            institucionJson = new JavaScriptSerializer().Serialize(new { institucion.Id, institucion.Nombre, institucion.Direccion, institucion.Latitud, institucion.Longitud });

            if (!IsPostBack)
            {
                CargarCampos();
                CargarRepeaters();
            }
        }

        public void CargarInstitucion()
        {
            institucion = new Institucion
            {
                Id = 1,
                Nombre = "Jardín de Infantes N° 902 Arco Iris",
                Cuit = "30710000014",
                Direccion = "Av. Hipólito Yrigoyen 1250, Gral. Pacheco",
                Email = "jardin902@instired.test",
                Descripcion = "Jardín de infantes con salas de 4 y 5 años, patio de juegos y acompañamiento cercano a las familias.",
                Latitud = -34.4586,
                Longitud = -58.6431,

                Discapacidad = new Discapacidad { Id = 1, Movilidad = true },

                TipoInstitucion = new TipoInstitucion { Id = 1, TipoDeInstitucion = "Jardín" },

                Idiomas = new List<Idioma>
                {
                    new Idioma { Id = 1, NombreIdioma = "Español" },
                    new Idioma { Id = 2, NombreIdioma = "Inglés" },
                    new Idioma { Id = 3, NombreIdioma = "Portuges" }
                },

                Cursos = new List<Curso>
                {
                    new Curso { Id = 1, NivelPersonalizado = "Sala de 4", Division = "Azul", CicloLectivo = 2026, Capacidad = 20, Inscriptos = 1 },
                    new Curso { Id = 2, NivelPersonalizado = "Sala de 5", Division = "Roja", CicloLectivo = 2026, Capacidad = 22, Inscriptos = 1 }
                },

                Telefonos = new List<Telefono>
                {
                    new Telefono { Id = 1, NumeroTelefono = "+54 11 5555-0101" },
                    new Telefono { Id = 2, NumeroTelefono = "+54 11 3434-9090"}
                },

                Resenias = new List<Resenia>
                {
                    new Resenia
                    {
                        Id = 1,
                        Calificacion = 5,
                        Comentario = "Las seños son un amor y mis hijos van felices todos los días. Muy buena comunicación con las familias.",
                        RespuestaInstitucion = "¡Gracias por su comentario! Nos alegra que haya tenido una buena experiencia.",
                        Fecha = new DateTime(2026, 4, 10),
                        Responsable = new Responsable { Id = 1, Nombre = "Laura Benítez" }
                    },
                    new Resenia
                    {
                        Id = 2,
                        Calificacion = 4,
                        Comentario = "Excelente propuesta pedagógica. Solo mejoraría el horario de ingreso, que es un poco temprano.",
                        RespuestaInstitucion = null,
                        Fecha = new DateTime(2026, 5, 2),
                        Responsable = new Responsable { Id = 2, Nombre = "Carlos Benítez" }
                    }
                },

                Imagenes = new List<Imagen>
                {
                    new Imagen { Id = 1, Descripcion = "Fachada del jardín", Url = "https://placehold.co/1200x400?text=Fachada" },
                    new Imagen { Id = 2, Descripcion = "Patio de juegos", Url = "https://placehold.co/1200x400?text=Patio" },
                    new Imagen { Id = 2, Descripcion = "Aula", Url = "https://placehold.co/1200x400?text=Aula" },
                    new Imagen { Id = 2, Descripcion = "Comedor", Url = "https://placehold.co/1200x400?text=Comedor" }
                }
            };
        }

        public void CargarCampos()
        {
            lblNombreInstitucion.Text = institucion.Nombre;
            lblDescripcion.Text = institucion.Descripcion;

            lblTipoInstitucion.Text = institucion.TipoInstitucion.TipoDeInstitucion;
            lblDireccion.Text = institucion.Direccion;
            lblEmail.Text = institucion.Email;
            lblTelefonoPrincipal.Text = institucion.Telefonos[0].NumeroTelefono;
            lblTelefonoAlternativo.Text = institucion.Telefonos[1].NumeroTelefono;
        }

        public void CargarRepeaters()
        {
            CargarRepeaterImagenes();
            CargarRepeaterIdiomas();
            CargarRepeaterAccesibilidad();
            CargarRepeaterResenias();
        }

        public void CargarRepeaterImagenes()
        {
            // Imagenes
            if (institucion.Imagenes != null && institucion.Imagenes.Count > 0)
            {
                int visibles = 3;

                imagenes = new List<Imagen>(institucion.Imagenes);

                for (int i = 0; i < visibles - 1 && i < institucion.Imagenes.Count; i++)
                {
                    imagenes.Add(institucion.Imagenes[i]);
                }

                rptImagenes.DataSource = imagenes;
                rptImagenes.DataBind();

                // Indicador de slide (Imagen seleccionada)
                rptIndicadores.DataSource = institucion.Imagenes;
                rptIndicadores.DataBind();

                pnlImagenes.Visible = true;
            }
            else
            {
                pnlImagenes.Visible = false;
            }
        }

        public void CargarRepeaterIdiomas()
        {
            rptIdiomas.DataSource = institucion.Idiomas;
            rptIdiomas.DataBind();
        }

        public void CargarRepeaterAccesibilidad()
        {
            List<string> caracteristicas = null;

            if (institucion.Discapacidad != null)
            {
                caracteristicas = institucion.Discapacidad.ObtenerAccesibilidades();
            }

            if (caracteristicas != null && caracteristicas.Count > 0)
            {
                rptAccesibilidades.DataSource = caracteristicas;
                rptAccesibilidades.DataBind();
            }
        }

        public void CargarRepeaterResenias()
        {
            rptResenias.DataSource = institucion.Resenias;
            rptResenias.DataBind();

            pnlSinResenias.Visible = institucion.Resenias == null || !institucion.Resenias.Any();
        }
    }
}