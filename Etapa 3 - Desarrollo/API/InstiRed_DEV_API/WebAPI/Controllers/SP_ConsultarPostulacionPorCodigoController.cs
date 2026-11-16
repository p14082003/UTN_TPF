using Dominio.DTOs;
using Microsoft.AspNetCore.Mvc;
using Negocio;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("sp/ConsultarPostulacionPorCodigo")]
    public class SP_ConsultarPostulacionPorCodigoController : ControllerBase
    {
        private readonly ILogger<SP_ConsultarPostulacionPorCodigoController> logger;

        public SP_ConsultarPostulacionPorCodigoController(ILogger<SP_ConsultarPostulacionPorCodigoController> logger)
        {
            this.logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(typeof(SP_ConsultarPostulacionPorCodigo_out), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Post([FromBody] SP_ConsultarPostulacionPorCodigo_in registro)
        {
            try
            {
                SP_ConsultarPostulacionPorCodigoNegocio negocio = new SP_ConsultarPostulacionPorCodigoNegocio();
                SP_ConsultarPostulacionPorCodigo_out respuesta = negocio.consultar(registro);
                return Ok(respuesta);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al ejecutar ConsultarPostulacionPorCodigo.");
                return StatusCode(500, new { Message = "No se pudo completar la operación. Intente nuevamente más tarde." });
            }
        }
    }
}
