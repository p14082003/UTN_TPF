using Dominio.DTOs;
using Microsoft.AspNetCore.Mvc;
using Negocio;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("vw/TelefonosPorUsuario")]
    public class VW_TelefonosPorUsuarioController : ControllerBase
    {
        private readonly ILogger<VW_TelefonosPorUsuarioController> logger;

        public VW_TelefonosPorUsuarioController(ILogger<VW_TelefonosPorUsuarioController> logger)
        {
            this.logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<VW_TelefonosPorUsuario_out>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Get([FromQuery] int idUsuario)
        {
            try
            {
                return Ok(new VW_TelefonosPorUsuarioNegocio().listar(idUsuario));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al consultar TelefonosPorUsuario.");
                return StatusCode(500, new { Message = "No se pudo completar la consulta. Intente nuevamente más tarde." });
            }
        }
    }
}
