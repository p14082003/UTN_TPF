using Dominio.DTOs;
using Microsoft.AspNetCore.Mvc;
using Negocio;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("sp/ConfirmarPostulacionAlumno")]
    public class SP_ConfirmarPostulacionAlumnoController : ControllerBase
    {
        private readonly ILogger<SP_ConfirmarPostulacionAlumnoController> logger;

        public SP_ConfirmarPostulacionAlumnoController(ILogger<SP_ConfirmarPostulacionAlumnoController> logger)
        {
            this.logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Post([FromBody] SP_ConfirmarPostulacionAlumno_in registro)
        {
            try
            {
                SP_ConfirmarPostulacionAlumnoNegocio negocio = new SP_ConfirmarPostulacionAlumnoNegocio();
                negocio.confirmar(registro);
                return Ok(new { Message = "Postulación confirmada." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al ejecutar ConfirmarPostulacionAlumno.");
                return StatusCode(500, new { Message = "No se pudo completar la operación. Intente nuevamente más tarde." });
            }
        }
    }
}
