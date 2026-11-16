using Dominio.DTOs;
using Microsoft.AspNetCore.Mvc;
using Negocio;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("sp/CrearInstitucion")]
    public class SP_CrearInstitucionController : ControllerBase
    {
        private readonly ILogger<SP_CrearInstitucionController> logger;

        public SP_CrearInstitucionController(ILogger<SP_CrearInstitucionController> logger)
        {
            this.logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(typeof(SP_CrearInstitucion_out), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Post([FromBody] SP_CrearInstitucion_in registro)
        {
            try
            {
                SP_CrearInstitucionNegocio negocio = new SP_CrearInstitucionNegocio();
                SP_CrearInstitucion_out respuesta = negocio.crear(registro);
                return CreatedAtAction("GetPorId", "Ent_Instituciones", new { id = respuesta.IdInstitucion }, respuesta);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al ejecutar CrearInstitucion.");
                return StatusCode(500, new { Message = "No se pudo completar la operación. Intente nuevamente más tarde." });
            }
        }
    }
}
