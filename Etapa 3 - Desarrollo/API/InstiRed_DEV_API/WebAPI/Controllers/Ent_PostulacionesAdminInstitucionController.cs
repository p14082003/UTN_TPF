using System.Data;
using Dominio;
using Microsoft.AspNetCore.Mvc;
using Negocio;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("ent/PostulacionesAdminInstitucion")]
    public class Ent_PostulacionesAdminInstitucionController : ControllerBase
    {
        private readonly ILogger<Ent_PostulacionesAdminInstitucionController> logger;

        public Ent_PostulacionesAdminInstitucionController(ILogger<Ent_PostulacionesAdminInstitucionController> logger)
        {
            this.logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<Ent_PostulacionAdminInstitucion>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Get()
        {
            try
            {
                Ent_PostulacionAdminInstitucionNegocio negocio = new Ent_PostulacionAdminInstitucionNegocio();
                return Ok(negocio.listar());
            }
            catch (Exception ex)
            {
                return responderError(ex);
            }
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(Ent_PostulacionAdminInstitucion), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetPorId(int id)
        {
            try
            {
                Ent_PostulacionAdminInstitucionNegocio negocio = new Ent_PostulacionAdminInstitucionNegocio();
                Ent_PostulacionAdminInstitucion? registro = negocio.obtenerPorId(id);
                if (registro == null)
                {
                    return NotFound(new { Message = "No se encontró el registro de PostulacionesAdminInstitucion solicitado." });
                }
                return Ok(registro);
            }
            catch (Exception ex)
            {
                return responderError(ex);
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(Ent_PostulacionAdminInstitucion), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Post([FromBody] Ent_PostulacionAdminInstitucion nuevo)
        {
            try
            {
                Ent_PostulacionAdminInstitucionNegocio negocio = new Ent_PostulacionAdminInstitucionNegocio();
                nuevo.Id = negocio.agregar(nuevo);
                return CreatedAtAction(nameof(GetPorId), new { id = nuevo.Id }, nuevo);
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
        public IActionResult Put([FromBody] Ent_PostulacionAdminInstitucion actualizado)
        {
            try
            {
                Ent_PostulacionAdminInstitucionNegocio negocio = new Ent_PostulacionAdminInstitucionNegocio();
                negocio.actualizar(actualizado);
                return Ok(new { Message = "Registro de PostulacionesAdminInstitucion modificado correctamente." });
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
                Ent_PostulacionAdminInstitucionNegocio negocio = new Ent_PostulacionAdminInstitucionNegocio();
                negocio.eliminar(id);
                return Ok(new { Message = "Registro de PostulacionesAdminInstitucion eliminado correctamente." });
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
