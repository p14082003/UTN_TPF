using Dominio.DTOs;
using Microsoft.AspNetCore.Mvc;
using Negocio;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("sp/AgregarIdiomaInstitucion")]
    public class SP_AgregarIdiomaInstitucionController : ControllerBase
    {
        private readonly ILogger<SP_AgregarIdiomaInstitucionController> logger;

        public SP_AgregarIdiomaInstitucionController(ILogger<SP_AgregarIdiomaInstitucionController> logger)
        {
            this.logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Post([FromBody] SP_AgregarIdiomaInstitucion_in registro)
        {
            try
            {
                SP_AgregarIdiomaInstitucionNegocio negocio = new SP_AgregarIdiomaInstitucionNegocio();
                negocio.agregar(registro);
                return Ok(new { Message = "Idioma agregado." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al ejecutar AgregarIdiomaInstitucion.");
                return StatusCode(500, new { Message = "No se pudo completar la operación. Intente nuevamente más tarde." });
            }
        }
    }
}
