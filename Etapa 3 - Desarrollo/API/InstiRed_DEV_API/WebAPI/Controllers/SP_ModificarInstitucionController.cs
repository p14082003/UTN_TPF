using Dominio.DTOs;
using Microsoft.AspNetCore.Mvc;
using Negocio;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("sp/ModificarInstitucion")]
    public class SP_ModificarInstitucionController : ControllerBase
    {
        private readonly ILogger<SP_ModificarInstitucionController> logger;

        public SP_ModificarInstitucionController(ILogger<SP_ModificarInstitucionController> logger)
        {
            this.logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Post([FromBody] SP_ModificarInstitucion_in registro)
        {
            try
            {
                SP_ModificarInstitucionNegocio negocio = new SP_ModificarInstitucionNegocio();
                negocio.modificar(registro);
                return Ok(new { Message = "Institución modificada." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al ejecutar ModificarInstitucion.");
                return StatusCode(500, new { Message = "No se pudo completar la operación. Intente nuevamente más tarde." });
            }
        }
    }
}
