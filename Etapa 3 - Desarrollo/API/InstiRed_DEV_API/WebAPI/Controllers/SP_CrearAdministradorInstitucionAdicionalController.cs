using Dominio.DTOs;
using Microsoft.AspNetCore.Mvc;
using Negocio;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("sp/CrearAdministradorInstitucionAdicional")]
    public class SP_CrearAdministradorInstitucionAdicionalController : ControllerBase
    {
        private readonly ILogger<SP_CrearAdministradorInstitucionAdicionalController> logger;

        public SP_CrearAdministradorInstitucionAdicionalController(ILogger<SP_CrearAdministradorInstitucionAdicionalController> logger)
        {
            this.logger = logger;
        }

        [HttpPost]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        [ProducesResponseType(typeof(SP_CrearAdministradorInstitucionAdicional_out), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Post([FromBody] SP_CrearAdministradorInstitucionAdicional_in registro)
        {
            try
            {
                SP_CrearAdministradorInstitucionAdicionalNegocio negocio = new SP_CrearAdministradorInstitucionAdicionalNegocio();
                SP_CrearAdministradorInstitucionAdicional_out respuesta = negocio.crear(registro);
                return CreatedAtAction("GetPorId", "Ent_AdministradoresInstitucion", new { id = respuesta.IdAdministrador }, respuesta);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al ejecutar CrearAdministradorInstitucionAdicional.");
                return StatusCode(500, new { Message = "No se pudo completar la operación. Intente nuevamente más tarde." });
            }
        }
    }
}
