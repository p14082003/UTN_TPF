using Dominio.DTOs;
using Microsoft.AspNetCore.Mvc;
using Negocio;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("vw/PostulacionesAlumnos")]
    public class VW_PostulacionesAlumnosController : ControllerBase
    {
        private readonly ILogger<VW_PostulacionesAlumnosController> logger;

        public VW_PostulacionesAlumnosController(ILogger<VW_PostulacionesAlumnosController> logger)
        {
            this.logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<VW_PostulacionesAlumnos_out>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Get([FromQuery] int? idAlumno = null, [FromQuery] int? idResponsable = null, [FromQuery] int? idInstitucion = null, [FromQuery] int? idCurso = null, [FromQuery] int? idEstado = null, [FromQuery] int? cicloLectivo = null, [FromQuery] string? codigoSeguimiento = null)
        {
            try
            {
                VW_PostulacionesAlumnosNegocio negocio = new VW_PostulacionesAlumnosNegocio();
                return Ok(negocio.listar(idAlumno, idResponsable, idInstitucion, idCurso, idEstado, cicloLectivo, codigoSeguimiento));
            }
            catch (Exception ex)
            {
                return responderError(ex);
            }
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(VW_PostulacionesAlumnos_out), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetPorId(int id)
        {
            try
            {
                VW_PostulacionesAlumnosNegocio negocio = new VW_PostulacionesAlumnosNegocio();
                VW_PostulacionesAlumnos_out? registro = negocio.obtenerPorId(id);
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
            logger.LogError(ex, "Error al consultar PostulacionesAlumnos.");
            return StatusCode(500, new { Message = "No se pudo completar la consulta. Intente nuevamente más tarde." });
        }
    }
}
