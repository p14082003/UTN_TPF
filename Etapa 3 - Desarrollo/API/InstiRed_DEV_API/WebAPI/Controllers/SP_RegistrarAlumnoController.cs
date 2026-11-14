using Dominio.DTOs;
using Microsoft.AspNetCore.Mvc;
using Negocio;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("sp/RegistrarAlumno")]
    public class SP_RegistrarAlumnoController : ControllerBase
    {
        private readonly ILogger<SP_RegistrarAlumnoController> logger;

        public SP_RegistrarAlumnoController(ILogger<SP_RegistrarAlumnoController> logger)
        {
            this.logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(typeof(SP_RegistrarAlumno_out), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Post([FromBody] SP_RegistrarAlumno_in registro)
        {
            try
            {
                SP_RegistrarAlumnoNegocio negocio = new SP_RegistrarAlumnoNegocio();
                SP_RegistrarAlumno_out respuesta = negocio.registrar(registro);
                return CreatedAtAction("GetPorId", "Ent_Alumnos", new { id = respuesta.IdAlumno }, respuesta);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al ejecutar RegistrarAlumno.");
                return StatusCode(500, new { Message = "No se pudo completar la operación. Intente nuevamente más tarde." });
            }
        }
    }
}
