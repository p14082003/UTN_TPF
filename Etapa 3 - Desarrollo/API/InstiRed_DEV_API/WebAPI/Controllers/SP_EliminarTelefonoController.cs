using Dominio.DTOs;
using Microsoft.AspNetCore.Mvc;
using Negocio;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("sp/EliminarTelefono")]
    public class SP_EliminarTelefonoController : ControllerBase
    {
        private readonly ILogger<SP_EliminarTelefonoController> logger;

        public SP_EliminarTelefonoController(ILogger<SP_EliminarTelefonoController> logger)
        {
            this.logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Post([FromBody] SP_EliminarTelefono_in registro)
        {
            try
            {
                SP_EliminarTelefonoNegocio negocio = new SP_EliminarTelefonoNegocio();
                negocio.eliminar(registro);
                return Ok(new { Message = "Teléfono desvinculado." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al ejecutar EliminarTelefono.");
                return StatusCode(500, new { Message = "No se pudo completar la operación. Intente nuevamente más tarde." });
            }
        }
    }
}
