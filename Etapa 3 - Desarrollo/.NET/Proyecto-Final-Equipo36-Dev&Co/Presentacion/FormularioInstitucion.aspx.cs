using Dominio;
using Negocio;
using Presentacion.Auth;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Presentacion
{
    public partial class FormularioInstitucion : Page
    {
        private Institucion institucion;
        private UsuarioSesion usuario;
        private bool guardando;
        private static readonly string[] extensiones = { ".jpg", ".jpeg", ".png" };
        private const int tamañoMaximo = 5 * 1024 * 1024;
        private const string carpetaImagenes = "~/Imagenes/Instituciones/";

        public string institucionJson
        {
            get
            {
                return new JavaScriptSerializer().Serialize(new
                {
                    Latitud = institucion?.Latitud ?? 0,
                    Longitud = institucion?.Longitud ?? 0
                });
            }
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            ViewStateUserKey = Session.SessionID;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            usuario = UsuarioSesion.Obtener(Session);
            if (usuario == null || usuario.IdTipoUsuario != UsuarioSesion.AdministradorInstitucion ||
                usuario.IdAdministradorInstitucion.GetValueOrDefault() <= 0)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            Response.Cache.SetNoStore();
            Form.Enctype = "multipart/form-data";
            institucion = Session["institucion"] as Institucion;
            if (!IsPostBack)
            {
                btnGuardar.Enabled = false;
                Ejecutar(CargarFormularioAsync);
            }
            else if (institucion == null || Session["idiomasDisponibles"] == null)
            {
                Response.Redirect("~/FormularioInstitucion.aspx");
            }
        }

        private void Ejecutar(Func<Task> accion)
        {
            RegisterAsyncTask(new PageAsyncTask(async () =>
            {
                try
                {
                    await accion();
                }
                catch (Exception ex)
                {
                    Trace.Warn("FormularioInstitucion", "Error al comunicarse con la API de InstiRed.", ex);
                    string mensaje = ex is ArgumentException ? ex.Message : "No se pudo completar la operación. Intentá nuevamente más tarde.";
                    if (guardando && institucion.Id > 0)
                    {
                        await RecargarImagenesAsync();
                        mensaje = "La institución está registrada, pero quedaron cambios pendientes. " + mensaje +
                            " Volvé a guardar para completar la carga.";
                    }
                    MostrarMensaje(mensaje);
                }
            }));
        }

        private async Task<AdministradorApi> ObtenerAdministradorAsync()
        {
            var admin = await InstiRedApi.GetAsync<AdministradorApi>(
                "ent/AdministradoresInstitucion/" + usuario.IdAdministradorInstitucion.Value);
            if (admin.IdUsuario != usuario.IdUsuario)
            {
                throw new ArgumentException("La cuenta no corresponde al administrador de la sesión.");
            }
            usuario.IdInstitucion = admin.IdInstitucion;
            Session[UsuarioSesion.Clave] = usuario;
            return admin;
        }

        private async Task CargarFormularioAsync()
        {
            var admin = await ObtenerAdministradorAsync();
            var idiomas = await InstiRedApi.GetAsync<List<Idioma>>("ent/Idiomas");
            var tipos = await InstiRedApi.GetAsync<List<TipoInstitucion>>("ent/TipoInstitucion");
            Session["idiomasDisponibles"] = idiomas;

            if (institucion == null || institucion.Id != admin.IdInstitucion.GetValueOrDefault() ||
                string.IsNullOrWhiteSpace(institucion.Nombre) || string.IsNullOrWhiteSpace(institucion.Cuit))
            {
                if (admin.IdInstitucion.GetValueOrDefault() > 0)
                {
                    institucion = await CargarInstitucionAsync(admin.IdInstitucion.Value, idiomas);
                }
                else
                {
                    var solicitudes = await InstiRedApi.GetAsync<List<SolicitudAprobadaApi>>(
                        "vw/PostulacionesAdminInstitucion?idAdministradorInstitucion=" +
                        usuario.IdAdministradorInstitucion.Value + "&idEstado=3");
                    var aprobada = solicitudes.OrderByDescending(s => s.IdPostulacion).FirstOrDefault();
                    if (aprobada == null)
                    {
                        throw new ArgumentException("Necesitás una postulación aprobada para registrar tu institución.");
                    }

                    institucion = new Institucion
                    {
                        Nombre = aprobada.NombreInstitucion,
                        Cuit = aprobada.CuitInstitucion,
                        Telefonos = new List<Telefono>(),
                        Idiomas = new List<Idioma>(),
                        Discapacidad = new Discapacidad(),
                        Imagenes = new List<Imagen>()
                    };
                }
                Session["institucion"] = institucion;
                Session.Remove("eliminadas");
            }

            ddlTipoInstitucion.DataSource = tipos;
            ddlTipoInstitucion.DataTextField = "TipoDeInstitucion";
            ddlTipoInstitucion.DataValueField = "Id";
            ddlTipoInstitucion.DataBind();
            ddlTipoInstitucion.Items.Insert(0, new ListItem("Seleccionar...", ""));
            if (institucion.TipoInstitucion != null)
            {
                ddlTipoInstitucion.SelectedValue = institucion.TipoInstitucion.Id.ToString();
            }

            CargarCampos();
            btnGuardar.Text = institucion.Id > 0 ? "Guardar cambios" : "Registrar institución";
            btnGuardar.Enabled = true;
        }

        private async Task<Institucion> CargarInstitucionAsync(int id, List<Idioma> idiomas)
        {
            var datos = await InstiRedApi.GetAsync<InstitucionVista>("vw/Instituciones/" + id);
            var telefonos = await InstiRedApi.GetAsync<List<TelefonoApi>>("ent/Telefonos");
            var vinculos = await InstiRedApi.GetAsync<List<IdiomaInstitucionApi>>("ent/IdiomaInstitucion");
            var imagenes = await InstiRedApi.ListarImagenesAsync();

            return new Institucion
            {
                Id = datos.IdInstitucion,
                Nombre = datos.Nombre,
                Cuit = datos.Cuit,
                Direccion = datos.Direccion,
                Email = datos.Email,
                Descripcion = datos.Descripcion,
                Latitud = datos.Latitud,
                Longitud = datos.Longitud,
                Estado = datos.Estado,
                TipoInstitucion = new TipoInstitucion { Id = datos.IdTipoInstitucion },
                Discapacidad = new Discapacidad
                {
                    Movilidad = datos.Movilidad.GetValueOrDefault(),
                    Visual = datos.Visual.GetValueOrDefault(),
                    Auditiva = datos.Auditiva.GetValueOrDefault(),
                    Verbal = datos.Verbal.GetValueOrDefault(),
                    Intelectual = datos.Intelectual.GetValueOrDefault(),
                    SindromeDown = datos.SindromeDown.GetValueOrDefault(),
                    Autismo = datos.Autismo.GetValueOrDefault()
                },
                Telefonos = telefonos.Where(t => t.IdInstitucion == id)
                    .Select(t => new Telefono { Id = t.Id, NumeroTelefono = t.NumeroTelefono }).ToList(),
                Idiomas = idiomas.Where(i => vinculos.Any(v => v.IdInstitucion == id && v.IdIdioma == i.Id)).ToList(),
                Imagenes = imagenes.Where(i => i.IdInstitucion == id)
                    .Select(i => new Imagen { Id = i.Id, Descripcion = i.Descripcion, Url = i.UrlPublica }).ToList()
            };
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
            bool tieneUbicacion = institucion.Id > 0 || institucion.Latitud != 0 || institucion.Longitud != 0;
            hfLatitud.Value = tieneUbicacion ? institucion.Latitud.ToString(CultureInfo.InvariantCulture) : "";
            hfLongitud.Value = tieneUbicacion ? institucion.Longitud.ToString(CultureInfo.InvariantCulture) : "";
        }

        private void CargarIdiomas()
        {
            var idiomas = (List<Idioma>)Session["idiomasDisponibles"];

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
            AgregarImagenesSeleccionadas();
        }

        private bool AgregarImagenesSeleccionadas()
        {
            if (!fuImagenes.HasFiles)
            {
                MostrarMensaje("Seleccioná las imágenes que querés agregar.");
                return false;
            }
            foreach (HttpPostedFile archivo in fuImagenes.PostedFiles)
            {
                if (!EsImagenValida(archivo))
                {
                    MostrarMensaje("Cada imagen debe ser JPG o PNG, de hasta 5 MB, con un nombre de hasta 255 caracteres.");
                    return false;
                }
            }
            try
            {
                foreach (HttpPostedFile archivo in fuImagenes.PostedFiles)
                {
                    institucion.Imagenes.Add(new Imagen
                    {
                        Descripcion = Path.GetFileNameWithoutExtension(archivo.FileName),
                        Url = GuardarArchivo(archivo)
                    });
                }
            }
            catch (Exception ex)
            {
                Trace.Warn("FormularioInstitucion", "Error al guardar una imagen temporal.", ex);
                MostrarMensaje("No se pudieron preparar todas las imágenes. Revisá las que se agregaron.");
                CargarImagenes();
                return false;
            }
            CargarImagenes();
            return true;
        }

        private bool EsImagenValida(HttpPostedFile archivo)
        {
            if (archivo.ContentLength == 0 || archivo.ContentLength > tamañoMaximo)
            {
                return false;
            }

            string extension = Path.GetExtension(archivo.FileName).ToLower();
            string nombre = Path.GetFileNameWithoutExtension(archivo.FileName);
            return extensiones.Contains(extension) && !string.IsNullOrWhiteSpace(nombre) && nombre.Length <= 255;
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
            institucion.Email = txtEmail.Text.Trim();
            institucion.Descripcion = txtDescripcion.Text.Trim();
            institucion.Direccion = txtDireccion.Text.Trim();
            institucion.TipoInstitucion = new TipoInstitucion { Id = int.Parse(ddlTipoInstitucion.SelectedValue) };
            institucion.Telefonos = new[] { txtTelefono.Text.Trim(), txtTelefonoAlternativo.Text.Trim() }
                .Where(numero => numero != "")
                .Select(numero => new Telefono { NumeroTelefono = numero }).ToList();
            institucion.Latitud = double.Parse(hfLatitud.Value, CultureInfo.InvariantCulture);
            institucion.Longitud = double.Parse(hfLongitud.Value, CultureInfo.InvariantCulture);
        }

        private bool ValidarCampos()
        {
            if (string.IsNullOrWhiteSpace(txtEmail.Text) || string.IsNullOrWhiteSpace(txtDireccion.Text) ||
                string.IsNullOrWhiteSpace(txtDescripcion.Text))
            {
                MostrarMensaje("Completá el correo, la dirección y la descripción de la institución.");
                return false;
            }
            if (txtEmail.Text.Trim().Length > 100 || txtDireccion.Text.Trim().Length > 100 ||
                txtDescripcion.Text.Trim().Length > 3000 ||
                txtTelefono.Text.Trim().Length > 30 || txtTelefonoAlternativo.Text.Trim().Length > 30)
            {
                MostrarMensaje("Revisá la longitud de los datos ingresados.");
                return false;
            }
            try
            {
                string email = txtEmail.Text.Trim();
                if (new MailAddress(email).Address != email)
                {
                    MostrarMensaje("Ingresá un correo electrónico válido.");
                    return false;
                }
            }
            catch (FormatException)
            {
                MostrarMensaje("Ingresá un correo electrónico válido.");
                return false;
            }
            if (!int.TryParse(ddlTipoInstitucion.SelectedValue, out int tipo) || tipo <= 0)
            {
                MostrarMensaje("Seleccioná el tipo de institución.");
                return false;
            }
            if (txtTelefono.Text.Trim() != "" && txtTelefono.Text.Trim() == txtTelefonoAlternativo.Text.Trim())
            {
                MostrarMensaje("Los teléfonos principal y alternativo deben ser distintos.");
                return false;
            }
            double latitud, longitud;
            if (!double.TryParse(hfLatitud.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out latitud) ||
                !double.TryParse(hfLongitud.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out longitud) ||
                double.IsNaN(latitud) || double.IsNaN(longitud) ||
                latitud < -90 || latitud > 90 || longitud < -180 || longitud > 180)
            {
                MostrarMensaje("Confirmá una ubicación válida en el mapa antes de guardar.");
                return false;
            }
            return true;
        }

        private void MostrarMensaje(string mensaje)
        {
            string script = "alert('" + HttpUtility.JavaScriptStringEncode(mensaje) + "');";
            ScriptManager.RegisterStartupScript(this, GetType(), "mensaje", script, true);
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            if (ValidarCampos())
            {
                if (fuImagenes.HasFiles && !AgregarImagenesSeleccionadas())
                {
                    return;
                }
                ActualizarInstitucionDesdeCampos();
                Ejecutar(GuardarAsync);
            }
        }

        private async Task GuardarAsync()
        {
            guardando = true;
            // Se consulta el vínculo actual por si un envío anterior llegó a guardarse.
            var admin = await ObtenerAdministradorAsync();
            institucion.Id = admin.IdInstitucion.GetValueOrDefault();
            var discapacidad = institucion.Discapacidad;
            var datos = new
            {
                idAdministrador = usuario.IdAdministradorInstitucion.Value,
                idInstitucion = institucion.Id,
                direccion = institucion.Direccion,
                email = institucion.Email,
                latitud = institucion.Latitud,
                longitud = institucion.Longitud,
                descripcion = institucion.Descripcion,
                idTipoInstitucion = institucion.TipoInstitucion.Id,
                movilidad = discapacidad.Movilidad,
                visual = discapacidad.Visual,
                auditiva = discapacidad.Auditiva,
                verbal = discapacidad.Verbal,
                intelectual = discapacidad.Intelectual,
                sindromeDown = discapacidad.SindromeDown,
                autismo = discapacidad.Autismo
            };

            if (institucion.Id == 0)
            {
                var respuesta = await InstiRedApi.PostAsync("sp/CrearInstitucion", datos, leerCuerpo: true);
                ValidarRespuesta(respuesta);
                var creada = new JavaScriptSerializer().Deserialize<InstitucionCreadaApi>(respuesta.Cuerpo);
                if (creada == null || creada.IdInstitucion <= 0)
                {
                    throw new InvalidOperationException("La API no devolvió el ID de la institución.");
                }
                institucion.Id = creada.IdInstitucion;
                institucion.Estado = true;
                usuario.IdInstitucion = institucion.Id;
                Session[UsuarioSesion.Clave] = usuario;
            }
            else
            {
                ValidarRespuesta(await InstiRedApi.PostAsync("sp/ModificarInstitucion", datos));
            }
            btnGuardar.Text = "Guardar cambios";

            await GuardarTelefonosAsync();
            await GuardarIdiomasAsync();
            await EliminarImagenesAsync();
            await EnviarImagenesAsync();

            Session.Remove("institucion");
            Session.Remove("eliminadas");
            Response.Redirect("~/DetalleInstitucion.aspx?id=" + institucion.Id, false);
            Context.ApplicationInstance.CompleteRequest();
        }

        private static void ValidarRespuesta(InstiRedApi.Respuesta respuesta)
        {
            if (respuesta.Estado != HttpStatusCode.OK && respuesta.Estado != HttpStatusCode.Created)
            {
                throw new ArgumentException(respuesta.Mensaje ?? "No se pudo guardar el cambio en la API. Intentá nuevamente.");
            }
        }

        private async Task GuardarTelefonosAsync()
        {
            var todos = await InstiRedApi.GetAsync<List<TelefonoApi>>("ent/Telefonos");
            var actuales = todos.Where(t => t.IdInstitucion == institucion.Id).ToList();
            foreach (var telefono in actuales)
            {
                if (!institucion.Telefonos.Any(t => t.NumeroTelefono == telefono.NumeroTelefono))
                {
                    ValidarRespuesta(await InstiRedApi.PostAsync("sp/EliminarTelefono", new
                    {
                        idUsuarioActor = usuario.IdUsuario,
                        tipoDueno = "Institucion",
                        idDueno = institucion.Id,
                        idTelefono = telefono.Id
                    }));
                }
            }
            foreach (var telefono in institucion.Telefonos)
            {
                if (!actuales.Any(t => t.NumeroTelefono == telefono.NumeroTelefono))
                {
                    ValidarRespuesta(await InstiRedApi.PostAsync("sp/AgregarTelefono", new
                    {
                        idUsuarioActor = usuario.IdUsuario,
                        tipoDueno = "Institucion",
                        idDueno = institucion.Id,
                        telefono = telefono.NumeroTelefono
                    }));
                }
            }
        }

        private async Task GuardarIdiomasAsync()
        {
            var todos = await InstiRedApi.GetAsync<List<IdiomaInstitucionApi>>("ent/IdiomaInstitucion");
            var actuales = todos.Where(i => i.IdInstitucion == institucion.Id).ToList();
            foreach (var idioma in actuales)
            {
                if (!institucion.Idiomas.Any(i => i.Id == idioma.IdIdioma))
                {
                    ValidarRespuesta(await InstiRedApi.PostAsync("sp/EliminarIdiomaInstitucion", new
                    {
                        idAdministrador = usuario.IdAdministradorInstitucion.Value,
                        idInstitucion = institucion.Id,
                        idIdioma = idioma.IdIdioma
                    }));
                }
            }
            foreach (var idioma in institucion.Idiomas)
            {
                if (!actuales.Any(i => i.IdIdioma == idioma.Id))
                {
                    ValidarRespuesta(await InstiRedApi.PostAsync("sp/AgregarIdiomaInstitucion", new
                    {
                        idAdministrador = usuario.IdAdministradorInstitucion.Value,
                        idInstitucion = institucion.Id,
                        idIdioma = idioma.Id
                    }));
                }
            }
        }

        private async Task EliminarImagenesAsync()
        {
            var eliminadas = Session["eliminadas"] as List<Imagen>;
            if (eliminadas == null)
            {
                return;
            }
            var actuales = await InstiRedApi.ListarImagenesAsync();
            foreach (var imagen in eliminadas.ToList())
            {
                if (actuales.Any(i => i.IdInstitucion == institucion.Id && i.Id == imagen.Id))
                {
                    ValidarRespuesta(await InstiRedApi.PostAsync("sp/EliminarImagenInstitucion", new
                    {
                        idAdministrador = usuario.IdAdministradorInstitucion.Value,
                        idInstitucion = institucion.Id,
                        idImagen = imagen.Id
                    }));
                }
                eliminadas.Remove(imagen);
            }
        }

        private async Task EnviarImagenesAsync()
        {
            foreach (var imagen in institucion.Imagenes.Where(i => i.Id == 0).ToList())
            {
                using (var formulario = new MultipartFormDataContent())
                {
                    formulario.Add(new StringContent(usuario.IdAdministradorInstitucion.Value.ToString()), "IdAdministrador");
                    formulario.Add(new StringContent(institucion.Id.ToString()), "IdInstitucion");
                    formulario.Add(new StringContent(imagen.Descripcion), "Descripcion");
                    formulario.Add(new StreamContent(File.OpenRead(Server.MapPath(imagen.Url))), "Imagen", Path.GetFileName(imagen.Url));
                    ValidarRespuesta(await InstiRedApi.PostAsync("sp/AgregarImagenInstitucion", formulario));
                }

                // La API confirmó la carga, por eso deja de ser una imagen pendiente.
                institucion.Imagenes.Remove(imagen);
                try
                {
                    EliminarArchivo(imagen.Url);
                }
                catch (Exception ex)
                {
                    Trace.Warn("FormularioInstitucion", "No se pudo borrar la imagen temporal ya enviada.", ex);
                }
            }
        }

        private async Task RecargarImagenesAsync()
        {
            try
            {
                var imagenes = await InstiRedApi.ListarImagenesAsync();
                var pendientes = institucion.Imagenes.Where(i => i.Id == 0).ToList();
                var eliminadas = Session["eliminadas"] as List<Imagen> ?? new List<Imagen>();
                institucion.Imagenes = imagenes
                    .Where(i => i.IdInstitucion == institucion.Id && !eliminadas.Any(e => e.Id == i.Id))
                    .Select(i => new Imagen { Id = i.Id, Descripcion = i.Descripcion, Url = i.UrlPublica }).ToList();
                institucion.Imagenes.AddRange(pendientes);
                CargarImagenes();
            }
            catch (Exception ex)
            {
                Trace.Warn("FormularioInstitucion", "No se pudieron actualizar las imágenes guardadas.", ex);
            }
        }

        private class AdministradorApi
        {
            public int IdUsuario { get; set; }
            public int? IdInstitucion { get; set; }
        }

        private class SolicitudAprobadaApi
        {
            public int IdPostulacion { get; set; }
            public string NombreInstitucion { get; set; }
            public string CuitInstitucion { get; set; }
        }

        private class InstitucionCreadaApi
        {
            public int IdInstitucion { get; set; }
        }

        private class TelefonoApi
        {
            public int Id { get; set; }
            public int? IdInstitucion { get; set; }
            public string NumeroTelefono { get; set; }
        }

        private class IdiomaInstitucionApi
        {
            public int IdInstitucion { get; set; }
            public int IdIdioma { get; set; }
        }

        private class InstitucionVista
        {
            public int IdInstitucion { get; set; }
            public string Nombre { get; set; }
            public string Cuit { get; set; }
            public string Direccion { get; set; }
            public string Email { get; set; }
            public string Descripcion { get; set; }
            public double Latitud { get; set; }
            public double Longitud { get; set; }
            public bool Estado { get; set; }
            public int IdTipoInstitucion { get; set; }
            public bool? Movilidad { get; set; }
            public bool? Visual { get; set; }
            public bool? Auditiva { get; set; }
            public bool? Verbal { get; set; }
            public bool? Intelectual { get; set; }
            public bool? SindromeDown { get; set; }
            public bool? Autismo { get; set; }
        }
    }
}
