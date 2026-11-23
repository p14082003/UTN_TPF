using Dominio.DTOs;
using Microsoft.AspNetCore.Mvc;
using Negocio;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("sp/PostulacionesPorResponsable")]
    public class SP_PostulacionesPorResponsableController : ControllerBase
    {
        private readonly ILogger<SP_PostulacionesPorResponsableController> logger;

        public SP_PostulacionesPorResponsableController(ILogger<SP_PostulacionesPorResponsableController> logger)
        {
            this.logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<SP_PostulacionesPorResponsable_out>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Get([FromQuery] int idResponsable)
        {
            try
            {
                return Ok(new SP_PostulacionesPorResponsableNegocio().listar(idResponsable));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al consultar PostulacionesPorResponsable.");
                return StatusCode(500, new { Message = "No se pudo completar la consulta. Intente nuevamente más tarde." });
            }
        }
    }
}
