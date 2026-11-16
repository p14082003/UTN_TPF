using Dominio.DTOs;
using Microsoft.AspNetCore.Mvc;
using Negocio;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("sp/RechazarPostulacionAlumno")]
    public class SP_RechazarPostulacionAlumnoController : ControllerBase
    {
        private readonly ILogger<SP_RechazarPostulacionAlumnoController> logger;

        public SP_RechazarPostulacionAlumnoController(ILogger<SP_RechazarPostulacionAlumnoController> logger)
        {
            this.logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Post([FromBody] SP_RechazarPostulacionAlumno_in registro)
        {
            try
            {
                SP_RechazarPostulacionAlumnoNegocio negocio = new SP_RechazarPostulacionAlumnoNegocio();
                negocio.rechazar(registro);
                return Ok(new { Message = "Postulación de alumno rechazada." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al ejecutar RechazarPostulacionAlumno.");
                return StatusCode(500, new { Message = "No se pudo completar la operación. Intente nuevamente más tarde." });
            }
        }
    }
}
