using Dominio.DTOs;
using Microsoft.AspNetCore.Mvc;
using Negocio;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("sp/EliminarIdiomaInstitucion")]
    public class SP_EliminarIdiomaInstitucionController : ControllerBase
    {
        private readonly ILogger<SP_EliminarIdiomaInstitucionController> logger;

        public SP_EliminarIdiomaInstitucionController(ILogger<SP_EliminarIdiomaInstitucionController> logger)
        {
            this.logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Post([FromBody] SP_EliminarIdiomaInstitucion_in registro)
        {
            try
            {
                SP_EliminarIdiomaInstitucionNegocio negocio = new SP_EliminarIdiomaInstitucionNegocio();
                negocio.eliminar(registro);
                return Ok(new { Message = "Idioma eliminado." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al ejecutar EliminarIdiomaInstitucion.");
                return StatusCode(500, new { Message = "No se pudo completar la operación. Intente nuevamente más tarde." });
            }
        }
    }
}
