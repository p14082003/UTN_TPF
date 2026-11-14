using System.Data;
using Dominio;
using Microsoft.AspNetCore.Mvc;
using Negocio;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("ent/Discapacidades")]
    public class Ent_DiscapacidadesController : ControllerBase
    {
        private readonly ILogger<Ent_DiscapacidadesController> logger;

        public Ent_DiscapacidadesController(ILogger<Ent_DiscapacidadesController> logger)
        {
            this.logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<Ent_Discapacidad>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Get()
        {
            try
            {
                Ent_DiscapacidadNegocio negocio = new Ent_DiscapacidadNegocio();
                return Ok(negocio.listar());
            }
            catch (Exception ex)
            {
                return responderError(ex);
            }
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(Ent_Discapacidad), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetPorId(int id)
        {
            try
            {
                Ent_DiscapacidadNegocio negocio = new Ent_DiscapacidadNegocio();
                Ent_Discapacidad? registro = negocio.obtenerPorId(id);
                if (registro == null)
                {
                    return NotFound(new { Message = "No se encontró el registro de Discapacidades solicitado." });
                }
                return Ok(registro);
            }
            catch (Exception ex)
            {
                return responderError(ex);
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(Ent_Discapacidad), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Post([FromBody] Ent_Discapacidad nuevo)
        {
            try
            {
                Ent_DiscapacidadNegocio negocio = new Ent_DiscapacidadNegocio();
                nuevo.IdInstitucion = negocio.agregar(nuevo);
                return CreatedAtAction(nameof(GetPorId), new { id = nuevo.IdInstitucion }, nuevo);
            }
            catch (Exception ex)
            {
                return responderError(ex);
            }
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Put([FromBody] Ent_Discapacidad actualizado)
        {
            try
            {
                Ent_DiscapacidadNegocio negocio = new Ent_DiscapacidadNegocio();
                negocio.actualizar(actualizado);
                return Ok(new { Message = "Registro de Discapacidades modificado correctamente." });
            }
            catch (Exception ex)
            {
                return responderError(ex);
            }
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Delete(int id)
        {
            try
            {
                Ent_DiscapacidadNegocio negocio = new Ent_DiscapacidadNegocio();
                negocio.eliminar(id);
                return Ok(new { Message = "Registro de Discapacidades eliminado correctamente." });
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
            if (ex is KeyNotFoundException)
            {
                return NotFound(new { Message = ex.Message });
            }
            if (ex is ConstraintException)
            {
                return Conflict(new { Message = ex.Message });
            }

            logger.LogError(ex, "Error al procesar una operación de la entidad.");
            return StatusCode(500, new { Message = "No se pudo completar la operación. Intente nuevamente más tarde." });
        }
    }
}
