using Dominio.DTOs;
using Microsoft.AspNetCore.Mvc;
using Negocio;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("sp/RecuperarClave")]
    public class SP_RecuperarClaveController : ControllerBase
    {
        private readonly ILogger<SP_RecuperarClaveController> logger;

        public SP_RecuperarClaveController(ILogger<SP_RecuperarClaveController> logger)
        {
            this.logger = logger;
        }

        [HttpPost]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        [ProducesResponseType(typeof(SP_RecuperarClave_out), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Post([FromBody] SP_RecuperarClave_in registro)
        {
            try
            {
                return Ok(new SP_RecuperarClaveNegocio().recuperar(registro));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al recuperar la contraseña.");
                return StatusCode(500, new { Message = "No se pudo confirmar el cambio de contraseña. Volvé a solicitar la recuperación." });
            }
        }
    }
}
