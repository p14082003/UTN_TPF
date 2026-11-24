using Dominio.DTOs;
using Microsoft.AspNetCore.Mvc;
using Negocio;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("vw/Instituciones")]
    public class VW_InstitucionesController : ControllerBase
    {
        private readonly ILogger<VW_InstitucionesController> logger;

        public VW_InstitucionesController(ILogger<VW_InstitucionesController> logger)
        {
            this.logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<VW_Instituciones_out>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Get([FromQuery] int? idTipoInstitucion = null, [FromQuery] bool? estado = null,
            [FromQuery] string? nombre = null, [FromQuery] int? idNivel = null, [FromQuery] int? anio = null,
            [FromQuery] int? cicloLectivo = null, [FromQuery] int? idTurno = null,
            [FromQuery] bool soloConVacantes = false, [FromQuery] string orden = "nombre")
        {
            try
            {
                VW_InstitucionesNegocio negocio = new VW_InstitucionesNegocio();
                return Ok(negocio.listar(idTipoInstitucion, estado, nombre, idNivel, anio,
                    cicloLectivo, idTurno, soloConVacantes, orden));
            }
            catch (Exception ex)
            {
                return responderError(ex);
            }
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(VW_Instituciones_out), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetPorId(int id)
        {
            try
            {
                VW_InstitucionesNegocio negocio = new VW_InstitucionesNegocio();
                VW_Instituciones_out? registro = negocio.obtenerPorId(id);
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
            logger.LogError(ex, "Error al consultar Instituciones.");
            return StatusCode(500, new { Message = "No se pudo completar la consulta. Intente nuevamente más tarde." });
        }
    }
}
