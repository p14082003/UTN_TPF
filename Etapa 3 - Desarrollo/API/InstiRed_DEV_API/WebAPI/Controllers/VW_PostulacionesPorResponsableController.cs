using Dominio.DTOs;
using Microsoft.AspNetCore.Mvc;
using Negocio;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("vw/PostulacionesPorResponsable")]
    public class VW_PostulacionesPorResponsableController : ControllerBase
    {
        private readonly ILogger<VW_PostulacionesPorResponsableController> logger;

        public VW_PostulacionesPorResponsableController(ILogger<VW_PostulacionesPorResponsableController> logger)
        {
            this.logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<VW_PostulacionesPorResponsable_out>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Get([FromQuery] int? idResponsable = null, [FromQuery] int? idAlumno = null, [FromQuery] int? idInstitucion = null, [FromQuery] int? idCurso = null, [FromQuery] int? idEstado = null, [FromQuery] int? cicloLectivo = null)
        {
            try
            {
                VW_PostulacionesPorResponsableNegocio negocio = new VW_PostulacionesPorResponsableNegocio();
                return Ok(negocio.listar(idResponsable, idAlumno, idInstitucion, idCurso, idEstado, cicloLectivo));
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
            logger.LogError(ex, "Error al consultar PostulacionesPorResponsable.");
            return StatusCode(500, new { Message = "No se pudo completar la consulta. Intente nuevamente más tarde." });
        }
    }
}
