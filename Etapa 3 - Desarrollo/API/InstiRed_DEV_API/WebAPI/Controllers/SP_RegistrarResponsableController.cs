using Microsoft.AspNetCore.Mvc;
using Dominio.DTOs;
using Negocio;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("sp/RegistrarResponsable")]
    public class SP_RegistrarResponsableController : ControllerBase
    {
        private readonly ILogger<SP_RegistrarResponsableController> logger;

        public SP_RegistrarResponsableController(ILogger<SP_RegistrarResponsableController> logger)
        {
            this.logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(typeof(SP_RegistrarResponsable_out), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Post([FromBody] SP_RegistrarResponsable_in registro)
        {
            try
            {
                SP_RegistrarResponsableNegocio negocio = new SP_RegistrarResponsableNegocio();
                SP_RegistrarResponsable_out respuesta = negocio.registrar(registro);
                return CreatedAtAction("GetPorId", "Ent_Responsables", new { id = respuesta.IdResponsable }, respuesta);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al registrar un responsable.");
                return StatusCode(500, new { Message = "No se pudo completar el registro. Intente nuevamente más tarde." });
            }
        }
    }
}
