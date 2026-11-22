using Dominio.DTOs;
using Microsoft.AspNetCore.Mvc;
using Negocio;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("vw/Cursos")]

    public class VW_CursosController : ControllerBase
    {
        // El loger permite guardar información de la api, por ejemplo errores
        private readonly ILogger<VW_CursosController> logger;
        
        // Constructor
        public VW_CursosController(ILogger<VW_CursosController> logger)
        {
            this.logger = logger;
        }

        // ENDPOINT GET
        [HttpGet]
        [ProducesResponseType(typeof(List<VW_Cursos_out>), StatusCodes.Status200OK)] // Consulta correcta
        [ProducesResponseType(StatusCodes.Status400BadRequest)] // Los datos enviados no son válidos
        [ProducesResponseType(StatusCodes.Status500InternalServerError)] // Error interno
        public IActionResult Get([FromQuery] int idInstitucion)
        {
            try
            {
                VW_CursosNegocio negocio = new VW_CursosNegocio();

                return Ok(negocio.obtenerPorInstitucion(idInstitucion));
            }
            catch(Exception ex)
            {
                return responderError(ex);
            }
        }

        // Manejo de errores
        private IActionResult responderError(Exception ex)
        {
            // ArgumentException = El dato recibido es inválido
            if(ex is ArgumentException)
            {
                return BadRequest(new { Message = ex.Message });
            }

            // Cualquier otro error
            logger.LogError(ex, "Error al consultar cursos de una institucion");

            return StatusCode(500, new { Message = "No se pudo completar la consulta. Intente nuevamente más tarde." });
        }
    }
}
