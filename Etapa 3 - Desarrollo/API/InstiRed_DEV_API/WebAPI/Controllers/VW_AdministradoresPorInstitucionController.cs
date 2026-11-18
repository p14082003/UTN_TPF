using Dominio.DTOs;
using Microsoft.AspNetCore.Mvc;
using Negocio;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("vw/AdministradoresPorInstitucion")]
    public class VW_AdministradoresPorInstitucionController : ControllerBase
    {
        private readonly ILogger<VW_AdministradoresPorInstitucionController> logger;

        public VW_AdministradoresPorInstitucionController(ILogger<VW_AdministradoresPorInstitucionController> logger)
        {
            this.logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<VW_AdministradoresPorInstitucion_out>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Get([FromQuery] int? idInstitucion = null)
        {
            try
            {
                VW_AdministradoresPorInstitucionNegocio negocio = new VW_AdministradoresPorInstitucionNegocio();
                return Ok(negocio.listar(idInstitucion));
            }
            catch (Exception ex)
            {
                return responderError(ex);
            }
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(VW_AdministradoresPorInstitucion_out), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetPorId(int id)
        {
            try
            {
                VW_AdministradoresPorInstitucionNegocio negocio = new VW_AdministradoresPorInstitucionNegocio();
                var administrador = negocio.obtenerPorId(id);
                if (administrador == null)
                {
                    return NotFound(new { Message = "No se encontró un administrador vinculado a una institución con ese ID." });
                }
                return Ok(administrador);
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

            logger.LogError(ex, "Error al consultar AdministradoresPorInstitucion.");
            return StatusCode(500, new { Message = "No se pudo completar la consulta. Intente nuevamente más tarde." });
        }
    }
}
