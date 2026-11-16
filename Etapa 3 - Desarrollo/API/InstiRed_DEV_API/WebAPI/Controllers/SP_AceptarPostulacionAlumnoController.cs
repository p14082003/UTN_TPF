using Dominio.DTOs;
using Microsoft.AspNetCore.Mvc;
using Negocio;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("sp/AceptarPostulacionAlumno")]
    public class SP_AceptarPostulacionAlumnoController : ControllerBase
    {
        private readonly ILogger<SP_AceptarPostulacionAlumnoController> logger;

        public SP_AceptarPostulacionAlumnoController(ILogger<SP_AceptarPostulacionAlumnoController> logger)
        {
            this.logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(typeof(SP_AceptarPostulacionAlumno_out), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Post([FromBody] SP_AceptarPostulacionAlumno_in registro)
        {
            try
            {
                SP_AceptarPostulacionAlumnoNegocio negocio = new SP_AceptarPostulacionAlumnoNegocio();
                SP_AceptarPostulacionAlumno_out respuesta = negocio.aceptar(registro);
                return Ok(respuesta);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al ejecutar AceptarPostulacionAlumno.");
                return StatusCode(500, new { Message = "No se pudo completar la operación. Intente nuevamente más tarde." });
            }
        }
    }
}
