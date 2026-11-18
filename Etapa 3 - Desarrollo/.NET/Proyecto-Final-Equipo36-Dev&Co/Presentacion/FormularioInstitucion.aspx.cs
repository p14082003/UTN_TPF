using Dominio;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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

        private static string[] extensiones = { ".jpg", ".jpeg", ".png" };
        private const int tamañoMaximo = 6 * 1024 * 1024; // 6 MB por imagen
        private const string carpetaImagenes = "~/Imagenes/Instituciones/";

        protected void Page_Load(object sender, EventArgs e)
        {
            // Validar que el usuario que ingrese exista y sea administrador (ID = 2)
            // Se valida en cada request (no solo en el primer load), porque los postbacks tambien pueden venir con la sesion vencida
            //Usuario usuario = Session["usuario"] as Usuario;

            //if (usuario == null || usuario.TipoUsuario == null || usuario.TipoUsuario.Id != 2)
            //{
            //    Response.Redirect("Login.aspx");
            //    return;
            //}

            // Al entrar por primera vez se descarta lo que haya quedado en sesion de una visita anterior
            if (!IsPostBack)
            {
                Session.Remove("institucion");
                Session.Remove("eliminadas");
                Session.Remove("administrador");
            }

            institucion = CargarInstitucion();

            // Marcador del mapa (se arma despues de cargar la institucion, para que nunca sea null)
            institucionJson = new JavaScriptSerializer().Serialize(new { institucion.Id, institucion.Nombre, institucion.Direccion, institucion.Latitud, institucion.Longitud });

            if (!IsPostBack)
            {
                CargarCampos();
            }
        }

        private Institucion CargarInstitucion()
        {
            // La institucion se guarda en sesion para conservar los cambios (idiomas, accesibilidades, imagenes) entre postbacks
            Institucion actual = (Institucion)Session["institucion"];

            if (actual == null)
            {
                // Consultar la institucion del admin logeado
                // Usuario usuario = (Usuario)Session["usuario"];
                //
                // API - GET USUARIO
                // AdministradorInstitucion admin = API
                // Session["administrador"] = admin;
                //
                // MODIFICAR: si el admin ya tiene una institucion asociada, se traen los datos completos
                // if (admin != null && admin.Institucion != null && admin.Institucion.Id != 0)
                // {
                //     // API - GET INSTITUCION
                //     // actual = API
                // }

                // REGISTRAR: si no tiene institucion (o la API no devolvio nada) se arranca con una vacia
                if (actual == null)
                {
                    actual = NuevaInstitucion();
                }

                Session["institucion"] = actual;
            }

            // La API puede devolver estas colecciones en null
            if (actual.Telefonos == null)
            {
                actual.Telefonos = new List<Telefono>();
            }

            if (actual.Idiomas == null)
            {
                actual.Idiomas = new List<Idioma>();
            }

            if (actual.Discapacidad == null)
            {
                actual.Discapacidad = new Discapacidad();
            }

            if (actual.Imagenes == null)
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

            // REGISTROS DE EJEMPLO (descomentar para probar el diseño) 
            // IMPORTANTE: tienen Id != 0, asi que el sistema los toma como imagenes ya guardadas en la BD
            nueva.Imagenes.Add(new Imagen { Id = 1, Descripcion = "ejemplo 1", Url = "https://placehold.co/400x250" });
            nueva.Imagenes.Add(new Imagen { Id = 2, Descripcion = "ejemplo 2", Url = "https://placehold.co/400x250" });
            nueva.Imagenes.Add(new Imagen { Id = 3, Descripcion = "ejemplo 3", Url = "https://placehold.co/400x250" });
            nueva.Imagenes.Add(new Imagen { Id = 4, Descripcion = "ejemplo 4", Url = "https://placehold.co/400x250" });
            nueva.Imagenes.Add(new Imagen { Id = 5, Descripcion = "ejemplo 5", Url = "https://placehold.co/400x250" });
            nueva.Imagenes.Add(new Imagen { Id = 6, Descripcion = "ejemplo 6", Url = "https://placehold.co/400x250" });

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
            hfLatitud.Value = institucion.Latitud != 0 ? institucion.Latitud.ToString(CultureInfo.InvariantCulture) : "";
            hfLongitud.Value = institucion.Longitud != 0 ? institucion.Longitud.ToString(CultureInfo.InvariantCulture) : "";
        }

        private void CargarIdiomas()
        {
            // REGISTRO DE EJEMPLO (TRAER DE LA DB)
            // GET api/idiomas
            // List<Idioma> idiomas = await ApiIdiomas.ObtenerTodos();

            // IDIOMAS DE EJEMPLO (BORRAR AL AGREGAR LA API)
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
                    Idioma aux = new Idioma();

                    aux.Id = id;
                    aux.NombreIdioma = ddlIdiomas.SelectedItem.Text;

                    institucion.Idiomas.Add(aux);
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
                int posicion = int.Parse(e.CommandArgument.ToString());

                if (posicion < 0 || posicion >= institucion.Imagenes.Count)
                {
                    return;
                }

                Imagen imagen = institucion.Imagenes[posicion];

                if (imagen.Id != 0)
                {
                    // Como la imagen ya esta guardada en la DB (Porque tiene id asiganado) la API la tiene que eliminar al ejecutar el btnGuardar
                    // Traigo la lista de session. Si no existe creo una lista nueva
                    List<Imagen> eliminadas = (List<Imagen>)Session["eliminadas"] ?? new List<Imagen>();
                    eliminadas.Add(imagen);

                    // Sobrescribo la lista de imagenes eliminadas
                    Session["eliminadas"] = eliminadas;
                }
                else
                {
                    // La imagen no esta cargada en la DB (No tiene id)
                    // Solo hay que borrar el archivo que se subio al servidor
                    EliminarArchivo(imagen.Url);
                }

                institucion.Imagenes.RemoveAt(posicion);

                CargarImagenes();
            }
        }

        protected void btnSubirImagenes_Click(object sender, EventArgs e)
        {
            if (fuImagenes.HasFiles)
            {
                foreach (HttpPostedFile archivo in fuImagenes.PostedFiles)
                {
                    if (EsImagenValida(archivo))
                    {
                        // Id = 0: imagen nueva, todavía no existe en la base
                        Imagen aux = new Imagen();

                        aux.Id = 0;
                        aux.Descripcion = Path.GetFileNameWithoutExtension(archivo.FileName);
                        aux.Url = GuardarArchivo(archivo);

                        institucion.Imagenes.Add(aux);
                    }
                }
            }

            CargarImagenes();
        }

        private bool EsImagenValida(HttpPostedFile archivo)
        {
            if (archivo.ContentLength == 0 || archivo.ContentLength > tamañoMaximo)
            {
                return false;
            }

            string extension = Path.GetExtension(archivo.FileName).ToLower();
            return extensiones.Contains(extension);
        }

        private string GuardarArchivo(HttpPostedFile archivo)
        {
            string carpeta = Server.MapPath(carpetaImagenes);
            Directory.CreateDirectory(carpeta);
            
            // Nombre de la carpeta temporal
            string nombre = Guid.NewGuid() + Path.GetExtension(archivo.FileName).ToLower();
            archivo.SaveAs(Path.Combine(carpeta, nombre));

            return carpetaImagenes + nombre;
        }

        private void EliminarArchivo(string url)
        {
            // Solo se borran los archivos propios / locales (los de ejemplo o los que vienen de la API son URLs externas)
            if (string.IsNullOrEmpty(url) || !url.StartsWith(carpetaImagenes))
            {
                return;
            }

            string ruta = Server.MapPath(url);

            if (File.Exists(ruta))
            {
                File.Delete(ruta);
            }
        }

        private void ActualizarInstitucionDesdeCampos()
        {
            // En los postbacks solo se conserva en memoria lo de los repeaters, lo escrito en los campos hay que pasarlo a la institucion
            institucion.Nombre = txtNombre.Text.Trim();
            institucion.Cuit = txtCuit.Text.Trim();
            institucion.Email = txtEmail.Text.Trim();
            institucion.Descripcion = txtDescripcion.Text.Trim();
            institucion.Direccion = txtDireccion.Text.Trim();

            // Telefonos: se reutilizan los objetos existentes para no perder su Id
            List<Telefono> telefonos = new List<Telefono>();
            string[] numeros = { txtTelefono.Text.Trim(), txtTelefonoAlternativo.Text.Trim() };

            for (int i = 0; i < numeros.Length; i++)
            {
                if (!string.IsNullOrEmpty(numeros[i]))
                {
                    Telefono telefono = i < institucion.Telefonos.Count ? institucion.Telefonos[i] : new Telefono();
                    telefono.NumeroTelefono = numeros[i];
                    telefonos.Add(telefono);
                }
            }

            institucion.Telefonos = telefonos;

            double latitud, longitud;

            if (double.TryParse(hfLatitud.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out latitud))
            {
                institucion.Latitud = latitud;
            }

            if (double.TryParse(hfLongitud.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out longitud))
            {
                institucion.Longitud = longitud;
            }
        }

        private bool ValidarCampos()
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MostrarMensaje("Ingrese el nombre de la institución.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtCuit.Text))
            {
                MostrarMensaje("Ingrese el CUIT de la institución.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MostrarMensaje("Ingrese el email de la institución.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtDireccion.Text))
            {
                MostrarMensaje("Ingrese la dirección de la institución.");
                return false;
            }

            return true;
        }

        private void MostrarMensaje(string mensaje)
        {
            string script = "alert('" + HttpUtility.JavaScriptStringEncode(mensaje) + "');";
            ScriptManager.RegisterStartupScript(this, GetType(), "mensaje", script, true);
        }

        private void EnviarImagenes(int idAdministrador, int idInstitucion)
        {
            // Solo se envian las nuevas (Id = 0), las que ya estaban en la BD no se vuelven a subir
            List<Imagen> nuevas = institucion.Imagenes.Where(i => i.Id == 0).ToList();

            foreach (Imagen imagen in nuevas)
            {
                // API - POST IMAGEN

                // Una vez que la API guardo la imagen, se puede borrar el archivo temporal del servidor
                EliminarArchivo(imagen.Url);
            }
        }

        private void EliminarImagenes(int idAdministrador, int idInstitucion)
        {
            List<Imagen> eliminadas = (List<Imagen>)Session["eliminadas"];

            if (eliminadas == null)
                return;

            foreach (Imagen imagen in eliminadas)
            {
                // API - DELETE IMAGEN
            }
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                if (!ValidarCampos())
                {
                    return;
                }

                ActualizarInstitucionDesdeCampos();

                // El administrador se trajo de la API al cargar la institucion (queda en sesion)
                AdministradorInstitucion admin = Session["administrador"] as AdministradorInstitucion;
                int idAdministrador = admin != null ? admin.IdAdministradorInstitucion : 0;

                if (institucion.Id == 0)
                {
                    // REGISTRAR
                    // API - POST INSTITUCION
                }
                else
                {
                    // MODIFICAR
                    // API - PUT INSTITUCION
                }

                // IMAGENES
                // EliminarImagenes(idAdministrador, institucion.Id);
                // EnviarImagenes(idAdministrador, institucion.Id);

                // Limpiar la sesion y recargar la pagina con los datos ya guardados (O redirigir al Dashboard del admin)
                // Session.Remove("institucion");
                // Session.Remove("eliminadas");
                // Response.Redirect("FormularioInstitucion.aspx", false);
            }
            catch (Exception ex)
            {
                MostrarMensaje("Ocurrió un error al guardar la institución. Intente nuevamente.");
            }
        }
    }
}