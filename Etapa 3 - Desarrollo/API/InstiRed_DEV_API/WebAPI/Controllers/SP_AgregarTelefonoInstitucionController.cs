using Dominio.DTOs;
using Microsoft.AspNetCore.Mvc;
using Negocio;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("sp/AgregarTelefonoInstitucion")]
    public class SP_AgregarTelefonoInstitucionController : ControllerBase
    {
        private readonly ILogger<SP_AgregarTelefonoInstitucionController> logger;

        public SP_AgregarTelefonoInstitucionController(ILogger<SP_AgregarTelefonoInstitucionController> logger)
        {
            this.logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Post([FromBody] SP_AgregarTelefonoInstitucion_in registro)
        {
            try
            {
                SP_AgregarTelefonoInstitucionNegocio negocio = new SP_AgregarTelefonoInstitucionNegocio();
                negocio.agregar(registro);
                return Ok(new { Message = "Teléfono agregado." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al ejecutar AgregarTelefonoInstitucion.");
                return StatusCode(500, new { Message = "No se pudo completar la operación. Intente nuevamente más tarde." });
            }
        }
    }
}
