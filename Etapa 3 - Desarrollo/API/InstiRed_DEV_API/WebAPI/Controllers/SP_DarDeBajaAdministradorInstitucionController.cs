using Dominio.DTOs;
using Microsoft.AspNetCore.Mvc;
using Negocio;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("sp/DarDeBajaAdministradorInstitucion")]
    public class SP_DarDeBajaAdministradorInstitucionController : ControllerBase
    {
        private readonly ILogger<SP_DarDeBajaAdministradorInstitucionController> logger;

        public SP_DarDeBajaAdministradorInstitucionController(ILogger<SP_DarDeBajaAdministradorInstitucionController> logger)
        {
            this.logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Post([FromBody] SP_DarDeBajaAdministradorInstitucion_in registro)
        {
            try
            {
                SP_DarDeBajaAdministradorInstitucionNegocio negocio = new SP_DarDeBajaAdministradorInstitucionNegocio();
                negocio.darDeBaja(registro);
                return Ok(new { Message = "Administrador de institución dado de baja." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al ejecutar DarDeBajaAdministradorInstitucion.");
                return StatusCode(500, new { Message = "No se pudo completar la operación. Intente nuevamente más tarde." });
            }
        }
    }
}
