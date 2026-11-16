using Dominio.DTOs;
using Microsoft.AspNetCore.Mvc;
using Negocio;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("sp/AprobarPostulacionAdmin")]
    public class SP_AprobarPostulacionAdminController : ControllerBase
    {
        private readonly ILogger<SP_AprobarPostulacionAdminController> logger;

        public SP_AprobarPostulacionAdminController(ILogger<SP_AprobarPostulacionAdminController> logger)
        {
            this.logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Post([FromBody] SP_AprobarPostulacionAdmin_in registro)
        {
            try
            {
                SP_AprobarPostulacionAdminNegocio negocio = new SP_AprobarPostulacionAdminNegocio();
                negocio.aprobar(registro);
                return Ok(new { Message = "Postulación de administrador aprobada." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al ejecutar AprobarPostulacionAdmin.");
                return StatusCode(500, new { Message = "No se pudo completar la operación. Intente nuevamente más tarde." });
            }
        }
    }
}
