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
        [ProducesResponseType(typeof(SP_Login_out), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Post([FromBody] SP_Login_in ingreso)
        {
            try
            {
                SP_LoginNegocio negocio = new SP_LoginNegocio();
                SP_Login_out respuesta = negocio.login(ingreso);
                if (!respuesta.Ok)
                {
                    return BadRequest(respuesta);
                }
                return Ok(respuesta);
            }
            catch (ArgumentException)
            {
                return BadRequest(new SP_Login_out { Ok = false });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al procesar el login.");
                return StatusCode(500, new { Ok = false, Message = "No se pudo completar el ingreso. Intente nuevamente más tarde." });
            }
        }
    }
}
