using System.Data;
using Dominio;
using Microsoft.AspNetCore.Mvc;
using Negocio;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("ent/IdiomaInstitucion")]
    public class Ent_IdiomaInstitucionController : ControllerBase
    {
        private readonly ILogger<Ent_IdiomaInstitucionController> logger;

        public Ent_IdiomaInstitucionController(ILogger<Ent_IdiomaInstitucionController> logger)
        {
            this.logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<Ent_IdiomaInstitucion>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Get()
        {
            try
            {
                Ent_IdiomaInstitucionNegocio negocio = new Ent_IdiomaInstitucionNegocio();
                return Ok(negocio.listar());
            }
            catch (Exception ex)
            {
                return responderError(ex);
            }
        }

        [HttpGet("{idInstitucion:int}/{idIdioma:int}")]
        [ProducesResponseType(typeof(Ent_IdiomaInstitucion), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetPorId(int idInstitucion, int idIdioma)
        {
            try
            {
                Ent_IdiomaInstitucionNegocio negocio = new Ent_IdiomaInstitucionNegocio();
                Ent_IdiomaInstitucion? registro = negocio.obtenerPorId(idInstitucion, idIdioma);
                if (registro == null)
                {
                    return NotFound(new { Message = "No se encontró el registro de IdiomaInstitucion solicitado." });
                }
                return Ok(registro);
            }
            catch (Exception ex)
            {
                return responderError(ex);
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(Ent_IdiomaInstitucion), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Post([FromBody] Ent_IdiomaInstitucion nuevo)
        {
            try
            {
                Ent_IdiomaInstitucionNegocio negocio = new Ent_IdiomaInstitucionNegocio();
                negocio.agregar(nuevo);
                return CreatedAtAction(nameof(GetPorId), new { idInstitucion = nuevo.IdInstitucion, idIdioma = nuevo.IdIdioma }, nuevo);
            }
            catch (Exception ex)
            {
                return responderError(ex);
            }
        }

        [HttpPut("{idInstitucion:int}/{idIdioma:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Put(int idInstitucion, int idIdioma, [FromBody] Ent_IdiomaInstitucion actualizado)
        {
            try
            {
                Ent_IdiomaInstitucionNegocio negocio = new Ent_IdiomaInstitucionNegocio();
                negocio.actualizar(idInstitucion, idIdioma, actualizado);
                return Ok(new { Message = "Registro de IdiomaInstitucion modificado correctamente." });
            }
            catch (Exception ex)
            {
                return responderError(ex);
            }
        }

        [HttpDelete("{idInstitucion:int}/{idIdioma:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Delete(int idInstitucion, int idIdioma)
        {
            try
            {
                Ent_IdiomaInstitucionNegocio negocio = new Ent_IdiomaInstitucionNegocio();
                negocio.eliminar(idInstitucion, idIdioma);
                return Ok(new { Message = "Registro de IdiomaInstitucion eliminado correctamente." });
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
