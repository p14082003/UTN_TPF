using Dominio.DTOs;
using Microsoft.AspNetCore.Mvc;
using Negocio;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("sp/AgregarTelefono")]
    public class SP_AgregarTelefonoController : ControllerBase
    {
        private readonly ILogger<SP_AgregarTelefonoController> logger;

        public SP_AgregarTelefonoController(ILogger<SP_AgregarTelefonoController> logger)
        {
            this.logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Post([FromBody] SP_AgregarTelefono_in registro)
        {
            try
            {
                SP_AgregarTelefonoNegocio negocio = new SP_AgregarTelefonoNegocio();
                negocio.agregar(registro);
                return Ok(new { Message = "Teléfono agregado." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al ejecutar AgregarTelefono.");
                return StatusCode(500, new { Message = "No se pudo completar la operación. Intente nuevamente más tarde." });
            }
        }
    }
}
