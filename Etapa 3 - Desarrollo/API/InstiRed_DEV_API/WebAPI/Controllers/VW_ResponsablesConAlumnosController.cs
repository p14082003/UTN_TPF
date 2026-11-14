using Dominio.DTOs;
using Microsoft.AspNetCore.Mvc;
using Negocio;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("vw/ResponsablesConAlumnos")]
    public class VW_ResponsablesConAlumnosController : ControllerBase
    {
        private readonly ILogger<VW_ResponsablesConAlumnosController> logger;

        public VW_ResponsablesConAlumnosController(ILogger<VW_ResponsablesConAlumnosController> logger)
        {
            this.logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<VW_ResponsablesConAlumnos_out>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Get([FromQuery] int? idResponsable = null, [FromQuery] int? idAlumno = null)
        {
            try
            {
                VW_ResponsablesConAlumnosNegocio negocio = new VW_ResponsablesConAlumnosNegocio();
                return Ok(negocio.listar(idResponsable, idAlumno));
            }
            catch (Exception ex)
            {
                return responderError(ex);
            }
        }

        private IActionResult responderError(Exception ex)
        {
            if (ex is ArgumentException)
            {
                return BadRequest(new { Message = ex.Message });
            }
            logger.LogError(ex, "Error al consultar ResponsablesConAlumnos.");
            return StatusCode(500, new { Message = "No se pudo completar la consulta. Intente nuevamente más tarde." });
        }
    }
}
