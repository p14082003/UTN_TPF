using Dominio.DTOs;
using Microsoft.AspNetCore.Mvc;
using Negocio;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("sp/EliminarAlumno")]
    public class SP_EliminarAlumnoController : ControllerBase
    {
        private readonly ILogger<SP_EliminarAlumnoController> logger;

        public SP_EliminarAlumnoController(ILogger<SP_EliminarAlumnoController> logger)
        {
            this.logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Post([FromBody] SP_EliminarAlumno_in registro)
        {
            try
            {
                new SP_EliminarAlumnoNegocio().eliminar(registro);
                return Ok(new { Message = "Alumno eliminado." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al ejecutar EliminarAlumno.");
                return StatusCode(500, new { Message = "No se pudo completar la operación. Intente nuevamente más tarde." });
            }
        }
    }
}
