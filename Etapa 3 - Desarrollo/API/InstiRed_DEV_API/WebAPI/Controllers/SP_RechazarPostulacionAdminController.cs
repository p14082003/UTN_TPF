using Dominio.DTOs;
using Microsoft.AspNetCore.Mvc;
using Negocio;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("sp/RechazarPostulacionAdmin")]
    public class SP_RechazarPostulacionAdminController : ControllerBase
    {
        private readonly ILogger<SP_RechazarPostulacionAdminController> logger;

        public SP_RechazarPostulacionAdminController(ILogger<SP_RechazarPostulacionAdminController> logger)
        {
            this.logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Post([FromBody] SP_RechazarPostulacionAdmin_in registro)
        {
            try
            {
                SP_RechazarPostulacionAdminNegocio negocio = new SP_RechazarPostulacionAdminNegocio();
                negocio.rechazar(registro);
                return Ok(new { Message = "Postulación de administrador rechazada." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al ejecutar RechazarPostulacionAdmin.");
                return StatusCode(500, new { Message = "No se pudo completar la operación. Intente nuevamente más tarde." });
            }
        }
    }
}
