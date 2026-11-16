using Dominio.DTOs;
using Microsoft.AspNetCore.Mvc;
using Negocio;
using Negocio.Helpers;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("sp/EliminarImagenInstitucion")]
    public class SP_EliminarImagenInstitucionController : ControllerBase
    {
        private readonly IConfiguration configuracion;
        private readonly IWebHostEnvironment entorno;
        private readonly ILogger<SP_EliminarImagenInstitucionController> logger;

        public SP_EliminarImagenInstitucionController(IConfiguration configuracion, IWebHostEnvironment entorno,
            ILogger<SP_EliminarImagenInstitucionController> logger)
        {
            this.configuracion = configuracion;
            this.entorno = entorno;
            this.logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Post([FromBody] SP_EliminarImagenInstitucion_in registro)
        {
            try
            {
                SP_EliminarImagenInstitucionNegocio negocio = new SP_EliminarImagenInstitucionNegocio();
                string? ruta = negocio.eliminar(registro);
                eliminarArchivoLocal(ruta, registro.IdImagen);
                return Ok(new { Message = "Imagen eliminada de la institución." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al ejecutar EliminarImagenInstitucion.");
                return StatusCode(500, new { Message = "No se pudo completar la operación. Intente nuevamente más tarde." });
            }
        }

        private void eliminarArchivoLocal(string? ruta, int idImagen)
        {
            if (ruta == null || !ArchivoHelper.esRutaImagenInstitucion(ruta))
            {
                return;
            }
            try
            {
                ArchivoHelper archivos = new ArchivoHelper(configuracion["Archivos:DirectorioRaiz"], entorno.ContentRootPath);
                archivos.eliminarImagenInstitucion(ruta);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "No se pudo borrar el archivo de la imagen {IdImagen}.", idImagen);
            }
        }
    }
}
