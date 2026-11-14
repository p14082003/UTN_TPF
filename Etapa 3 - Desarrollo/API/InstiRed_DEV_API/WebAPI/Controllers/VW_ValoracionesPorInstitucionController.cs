using Dominio.DTOs;
using Microsoft.AspNetCore.Mvc;
using Negocio;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("vw/ValoracionesPorInstitucion")]
    public class VW_ValoracionesPorInstitucionController : ControllerBase
    {
        private readonly ILogger<VW_ValoracionesPorInstitucionController> logger;

        public VW_ValoracionesPorInstitucionController(ILogger<VW_ValoracionesPorInstitucionController> logger)
        {
            this.logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<VW_ValoracionesPorInstitucion_out>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Get([FromQuery] int? idInstitucion = null, [FromQuery] int? idResponsable = null)
        {
            try
            {
                VW_ValoracionesPorInstitucionNegocio negocio = new VW_ValoracionesPorInstitucionNegocio();
                return Ok(negocio.listar(idInstitucion, idResponsable));
            }
            catch (Exception ex)
            {
                return responderError(ex);
            }
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(VW_ValoracionesPorInstitucion_out), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetPorId(int id)
        {
            try
            {
                VW_ValoracionesPorInstitucionNegocio negocio = new VW_ValoracionesPorInstitucionNegocio();
                VW_ValoracionesPorInstitucion_out? registro = negocio.obtenerPorId(id);
                if (registro == null)
                {
                    return NotFound(new { Message = "No se encontró el registro solicitado." });
                }
                return Ok(registro);
            }
            catch (Exception ex)
            {
                return responderError(ex);
            }
        }

        private IActionResult responderError(Exception ex)
        {
            if (ex is ArgumentException)
            {
                return BadRequest(new { Message = ex.Message });
            }
            logger.LogError(ex, "Error al consultar ValoracionesPorInstitucion.");
            return StatusCode(500, new { Message = "No se pudo completar la consulta. Intente nuevamente más tarde." });
        }
    }
}
