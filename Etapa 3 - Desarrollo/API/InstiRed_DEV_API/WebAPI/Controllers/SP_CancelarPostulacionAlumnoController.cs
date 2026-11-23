using Dominio.DTOs;
using Microsoft.AspNetCore.Mvc;
using Negocio;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("sp/CancelarPostulacionAlumno")]
    public class SP_CancelarPostulacionAlumnoController : ControllerBase
    {
        private readonly ILogger<SP_CancelarPostulacionAlumnoController> logger;

        public SP_CancelarPostulacionAlumnoController(ILogger<SP_CancelarPostulacionAlumnoController> logger)
        {
            this.logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Post([FromBody] SP_CancelarPostulacionAlumno_in registro)
        {
            try
            {
                new SP_CancelarPostulacionAlumnoNegocio().cancelar(registro);
                return Ok(new { Message = "Postulación cancelada." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al ejecutar CancelarPostulacionAlumno.");
                return StatusCode(500, new { Message = "No se pudo completar la operación. Intente nuevamente más tarde." });
            }
        }
    }
}
