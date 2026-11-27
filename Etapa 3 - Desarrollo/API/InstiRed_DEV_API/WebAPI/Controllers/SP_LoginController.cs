using Microsoft.AspNetCore.Mvc;
using Dominio.DTOs;
using Negocio;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("sp/Login")]
    public class SP_LoginController : ControllerBase
    {
        private readonly ILogger<SP_LoginController> logger;

        public SP_LoginController(ILogger<SP_LoginController> logger)
        {
            this.logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(typeof(SP_Login_out), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Post([FromBody] SP_Login_in ingreso)
        {
            try
            {
                SP_LoginNegocio negocio = new SP_LoginNegocio();
                SP_Login_out respuesta = negocio.login(ingreso);
                if (!respuesta.Ok)
                {
                    return BadRequest(new { Ok = false });
                }
                return Ok(respuesta);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { Ok = false, Message = ex.Message });
            }
            catch (ArgumentException)
            {
                return BadRequest(new { Ok = false });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al procesar el login.");
                return StatusCode(500, new { Ok = false });
            }
        }
    }
}
