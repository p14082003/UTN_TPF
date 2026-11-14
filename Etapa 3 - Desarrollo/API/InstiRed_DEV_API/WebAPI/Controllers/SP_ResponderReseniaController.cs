using Dominio.DTOs;
using Microsoft.AspNetCore.Mvc;
using Negocio;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("sp/ResponderResenia")]
    public class SP_ResponderReseniaController : ControllerBase
    {
        private readonly ILogger<SP_ResponderReseniaController> logger;

        public SP_ResponderReseniaController(ILogger<SP_ResponderReseniaController> logger)
        {
            this.logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Post([FromBody] SP_ResponderResenia_in registro)
        {
            try
            {
                SP_ResponderReseniaNegocio negocio = new SP_ResponderReseniaNegocio();
                negocio.responder(registro);
                return Ok(new { Message = "Respuesta registrada correctamente." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al ejecutar ResponderResenia.");
                return StatusCode(500, new { Message = "No se pudo completar la operación. Intente nuevamente más tarde." });
            }
        }
    }
}
