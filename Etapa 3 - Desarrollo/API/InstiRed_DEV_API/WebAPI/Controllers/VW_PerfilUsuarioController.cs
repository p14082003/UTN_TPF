using Dominio.DTOs;
using Microsoft.AspNetCore.Mvc;
using Negocio;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("vw/PerfilUsuario")]
    public class VW_PerfilUsuarioController : ControllerBase
    {
        private readonly ILogger<VW_PerfilUsuarioController> logger;

        public VW_PerfilUsuarioController(ILogger<VW_PerfilUsuarioController> logger)
        {
            this.logger = logger;
        }

        [HttpGet("{idUsuario:int}")]
        [ProducesResponseType(typeof(VW_PerfilUsuario_out), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetPorId(int idUsuario)
        {
            try
            {
                var perfil = new VW_PerfilUsuarioNegocio().obtenerPorId(idUsuario);
                if (perfil == null)
                {
                    return NotFound(new { Message = "No se encontró el perfil del usuario solicitado." });
                }
                return Ok(perfil);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al consultar PerfilUsuario.");
                return StatusCode(500, new { Message = "No se pudo completar la consulta. Intente nuevamente más tarde." });
            }
        }
    }
}
