using Dominio.DTOs;
using Microsoft.AspNetCore.Mvc;
using Negocio;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("sp/RegistrarResenia")]
    public class SP_RegistrarReseniaController : ControllerBase
    {
        private readonly ILogger<SP_RegistrarReseniaController> logger;

        public SP_RegistrarReseniaController(ILogger<SP_RegistrarReseniaController> logger)
        {
            this.logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Post([FromBody] SP_RegistrarResenia_in registro)
        {
            try
            {
                SP_RegistrarReseniaNegocio negocio = new SP_RegistrarReseniaNegocio();
                negocio.registrar(registro);
                return Ok(new { Message = "Reseña registrada correctamente." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al ejecutar RegistrarResenia.");
                return StatusCode(500, new { Message = "No se pudo completar la operación. Intente nuevamente más tarde." });
            }
        }
    }
}
