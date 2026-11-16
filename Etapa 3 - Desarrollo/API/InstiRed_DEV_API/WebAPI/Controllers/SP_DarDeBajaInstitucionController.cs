using Dominio.DTOs;
using Microsoft.AspNetCore.Mvc;
using Negocio;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("sp/DarDeBajaInstitucion")]
    public class SP_DarDeBajaInstitucionController : ControllerBase
    {
        private readonly ILogger<SP_DarDeBajaInstitucionController> logger;

        public SP_DarDeBajaInstitucionController(ILogger<SP_DarDeBajaInstitucionController> logger)
        {
            this.logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Post([FromBody] SP_DarDeBajaInstitucion_in registro)
        {
            try
            {
                SP_DarDeBajaInstitucionNegocio negocio = new SP_DarDeBajaInstitucionNegocio();
                negocio.darDeBaja(registro);
                return Ok(new { Message = "Institución dada de baja." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al ejecutar DarDeBajaInstitucion.");
                return StatusCode(500, new { Message = "No se pudo completar la operación. Intente nuevamente más tarde." });
            }
        }
    }
}
