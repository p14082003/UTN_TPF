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
            try
            {
                ArchivosNegocio negocio = new ArchivosNegocio();
                string ruta = negocio.obtenerRutaAdministrador(id, tipo);
                ArchivoHelper archivos = new ArchivoHelper(configuracion["Archivos:DirectorioRaiz"], entorno.ContentRootPath);
                string contentType = ArchivoHelper.obtenerContentType(ruta);
                FileStream contenido = archivos.abrir(ruta);
                Response.Headers["X-Content-Type-Options"] = "nosniff";
                Response.Headers["Cache-Control"] = "no-store";
                return File(contenido, contentType, Path.GetFileName(ruta));
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
                logger.LogError(ex, "Error al descargar un documento de una postulación.");
                return StatusCode(500, new { Message = "No se pudo descargar el documento." });
            }
        }
    }
}
