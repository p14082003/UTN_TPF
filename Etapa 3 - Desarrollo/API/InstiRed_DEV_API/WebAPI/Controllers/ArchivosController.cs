using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Negocio;
using Negocio.Helpers;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("archivos/PostulacionesAdminInstitucion")]
    public class ArchivosController : ControllerBase
    {
        private readonly IConfiguration configuracion;
        private readonly IWebHostEnvironment entorno;
        private readonly ILogger<ArchivosController> logger;

        public ArchivosController(IConfiguration configuracion, IWebHostEnvironment entorno, ILogger<ArchivosController> logger)
        {
            this.configuracion = configuracion;
            this.entorno = entorno;
            this.logger = logger;
        }

        [HttpGet("{tipo}/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Get(int id, string tipo)
        {
            ArchivosNegocio negocio = new ArchivosNegocio();
            return descargar(() => negocio.obtenerRutaAdministrador(id, tipo), (archivos, ruta) => archivos.abrir(ruta));
        }

        [HttpGet("/archivos/PostulacionesAlumnos/documento/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetDocumentoAlumno(int id)
        {
            ArchivosNegocio negocio = new ArchivosNegocio();
            return descargar(() => negocio.obtenerRutaDocumentoAlumno(id), (archivos, ruta) => archivos.abrirDocumentoAlumno(ruta));
        }

        [HttpGet("/archivos/Instituciones/imagen/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetImagenInstitucion(int id)
        {
            ArchivosNegocio negocio = new ArchivosNegocio();
            return descargar(() => negocio.obtenerRutaImagenInstitucion(id), (archivos, ruta) => archivos.abrirImagenInstitucion(ruta));
        }

        [AllowAnonymous]
        [HttpGet("/publico/Instituciones/imagen/{id:int:min(1)}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetImagenInstitucionPublica(int id)
        {
            ArchivosNegocio negocio = new ArchivosNegocio();
            return descargar(() => negocio.obtenerRutaImagenInstitucionPublica(id), (archivos, ruta) => archivos.abrirImagenInstitucion(ruta), comoAdjunto: false);
        }

        private IActionResult descargar(Func<string> obtenerRuta, Func<ArchivoHelper, string, FileStream> abrir, bool comoAdjunto = true)
        {
            try
            {
                string ruta = obtenerRuta();
                ArchivoHelper archivos = new ArchivoHelper(configuracion["Archivos:DirectorioRaiz"], entorno.ContentRootPath);
                string contentType = ArchivoHelper.obtenerContentType(ruta);
                FileStream contenido = abrir(archivos, ruta);
                Response.Headers["X-Content-Type-Options"] = "nosniff";
                Response.Headers["Cache-Control"] = "no-store";
                if (comoAdjunto)
                {
                    return File(contenido, contentType, Path.GetFileName(ruta));
                }

                return File(contenido, contentType);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (FileNotFoundException)
            {
                return NotFound(new { Message = "El documento no está disponible." });
            }
            catch (DirectoryNotFoundException)
            {
                return NotFound(new { Message = "El documento no está disponible." });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al descargar el archivo solicitado.");
                return StatusCode(500, new { Message = "No se pudo descargar el documento." });
            }
        }
    }
}
