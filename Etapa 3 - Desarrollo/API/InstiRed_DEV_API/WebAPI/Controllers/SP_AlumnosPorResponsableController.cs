using Dominio.DTOs;
using Microsoft.AspNetCore.Mvc;
using Negocio;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("sp/AlumnosPorResponsable")]
    public class SP_AlumnosPorResponsableController : ControllerBase
    {
        private readonly ILogger<SP_AlumnosPorResponsableController> logger;

        public SP_AlumnosPorResponsableController(ILogger<SP_AlumnosPorResponsableController> logger)
        {
            this.logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<SP_AlumnosPorResponsable_out>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Get([FromQuery] int idResponsable)
        {
            try
            {
                return Ok(new SP_AlumnosPorResponsableNegocio().listar(idResponsable));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al consultar AlumnosPorResponsable.");
                return StatusCode(500, new { Message = "No se pudo completar la consulta. Intente nuevamente más tarde." });
            }
        }
    }
}
