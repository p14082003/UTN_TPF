using Dominio.DTOs;
using Microsoft.AspNetCore.Mvc;
using Negocio;
using Negocio.Helpers;
using WebAPI.Helpers;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("sp/AgregarImagenInstitucion")]
    public class SP_AgregarImagenInstitucionController : ControllerBase
    {
        private readonly IConfiguration configuracion;
        private readonly IWebHostEnvironment entorno;
        private readonly ILogger<SP_AgregarImagenInstitucionController> logger;

        public SP_AgregarImagenInstitucionController(IConfiguration configuracion, IWebHostEnvironment entorno,
            ILogger<SP_AgregarImagenInstitucionController> logger)
        {
            this.configuracion = configuracion;
            this.entorno = entorno;
            this.logger = logger;
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(6 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 6 * 1024 * 1024)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Post([FromForm] SP_AgregarImagenInstitucion_form formulario)
        {
            ArchivoHelper? archivos = null;
            string carga = Guid.NewGuid().ToString("N");
            bool seIntentoRegistrar = false;

            try
            {
                if (string.IsNullOrWhiteSpace(formulario.Descripcion) || formulario.Descripcion.Trim().Length > 255)
                {
                    throw new ArgumentException("La imagen requiere una descripción de hasta 255 caracteres.");
                }
                byte[] contenido = ArchivoFormularioHelper.leer(formulario.Imagen);
                archivos = new ArchivoHelper(configuracion["Archivos:DirectorioRaiz"], entorno.ContentRootPath);
                SP_AgregarImagenInstitucion_in registro = new SP_AgregarImagenInstitucion_in
                {
                    IdAdministrador = formulario.IdAdministrador,
                    IdInstitucion = formulario.IdInstitucion,
                    Descripcion = formulario.Descripcion,
                    UrlImagen = archivos.guardarImagenInstitucion(carga, formulario.Imagen?.FileName ?? "", contenido)
                };

                SP_AgregarImagenInstitucionNegocio negocio = new SP_AgregarImagenInstitucionNegocio();
                seIntentoRegistrar = true;
                negocio.agregar(registro);
                return Ok(new { Message = "Imagen agregada." });
            }
            catch (ArgumentException ex)
            {
                if (archivos != null)
                {
                    ArchivoFormularioHelper.limpiarCarga(carga, archivos.eliminarCargaImagenInstitucion, logger);
                }
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                if (archivos != null && !seIntentoRegistrar)
                {
                    ArchivoFormularioHelper.limpiarCarga(carga, archivos.eliminarCargaImagenInstitucion, logger);
                }
                if (seIntentoRegistrar)
                {
                    logger.LogWarning("Se conservan los archivos de la carga {Carga}; comprobar el resultado antes de limpiarlos.", carga);
                }
                logger.LogError(ex, "Error al ejecutar AgregarImagenInstitucion.");
                return StatusCode(500, new { Message = "No se pudo completar la operación. Intente nuevamente más tarde." });
            }
        }
    }
}
