using Dominio.DTOs;
using Microsoft.AspNetCore.Mvc;
using Negocio;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("sp/CambiarClave")]
    public class SP_CambiarClaveController : ControllerBase
    {
        private readonly ILogger<SP_CambiarClaveController> logger;

        public SP_CambiarClaveController(ILogger<SP_CambiarClaveController> logger)
        {
            this.logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Post([FromBody] SP_CambiarClave_in registro)
        {
            try
            {
                new SP_CambiarClaveNegocio().cambiar(registro);
                return Ok(new { Message = "Contraseña actualizada correctamente." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al cambiar la contraseña.");
                return StatusCode(500, new { Message = "No se pudo completar la operación. Intente nuevamente más tarde." });
            }
        }
    }
}
