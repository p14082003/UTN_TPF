using Dominio.DTOs;
using Microsoft.AspNetCore.Mvc;
using Negocio;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("sp/ReactivarInstitucion")]
    public class SP_ReactivarInstitucionController : ControllerBase
    {
        private readonly ILogger<SP_ReactivarInstitucionController> logger;

        public SP_ReactivarInstitucionController(ILogger<SP_ReactivarInstitucionController> logger)
        {
            this.logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Post([FromBody] SP_ReactivarInstitucion_in registro)
        {
            try
            {
                SP_ReactivarInstitucionNegocio negocio = new SP_ReactivarInstitucionNegocio();
                negocio.reactivar(registro);
                return Ok(new { Message = "Institución reactivada." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al ejecutar ReactivarInstitucion.");
                return StatusCode(500, new { Message = "No se pudo completar la operación. Intente nuevamente más tarde." });
            }
        }
    }
}
