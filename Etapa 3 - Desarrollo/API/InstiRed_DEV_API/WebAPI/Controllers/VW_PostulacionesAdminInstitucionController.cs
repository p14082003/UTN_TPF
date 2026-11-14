using Dominio.DTOs;
using Microsoft.AspNetCore.Mvc;
using Negocio;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("vw/PostulacionesAdminInstitucion")]
    public class VW_PostulacionesAdminInstitucionController : ControllerBase
    {
        private readonly ILogger<VW_PostulacionesAdminInstitucionController> logger;

        public VW_PostulacionesAdminInstitucionController(ILogger<VW_PostulacionesAdminInstitucionController> logger)
        {
            this.logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<VW_PostulacionesAdminInstitucion_out>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Get([FromQuery] int? idAdministradorInstitucion = null, [FromQuery] int? idInstitucion = null, [FromQuery] int? idEstado = null, [FromQuery] string? codigoSeguimiento = null)
        {
            try
            {
                VW_PostulacionesAdminInstitucionNegocio negocio = new VW_PostulacionesAdminInstitucionNegocio();
                return Ok(negocio.listar(idAdministradorInstitucion, idInstitucion, idEstado, codigoSeguimiento));
            }
            catch (Exception ex)
            {
                return responderError(ex);
            }
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(VW_PostulacionesAdminInstitucion_out), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetPorId(int id)
        {
            try
            {
                VW_PostulacionesAdminInstitucionNegocio negocio = new VW_PostulacionesAdminInstitucionNegocio();
                VW_PostulacionesAdminInstitucion_out? registro = negocio.obtenerPorId(id);
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
            logger.LogError(ex, "Error al consultar PostulacionesAdminInstitucion.");
            return StatusCode(500, new { Message = "No se pudo completar la consulta. Intente nuevamente más tarde." });
        }
    }
}
