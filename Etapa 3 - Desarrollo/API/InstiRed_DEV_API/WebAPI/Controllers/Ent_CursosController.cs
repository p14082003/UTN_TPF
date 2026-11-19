using System.Data;
using Dominio;
using Dominio.DTOs;
using Microsoft.AspNetCore.Mvc;
using Negocio;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("ent/Cursos")]
    public class Ent_CursosController : ControllerBase
    {
        private readonly ILogger<Ent_CursosController> logger;

        public Ent_CursosController(ILogger<Ent_CursosController> logger)
        {
            this.logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<Ent_Curso>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Get([FromQuery] int? idInstitucion = null)
        {
            try
            {
                Ent_CursoNegocio negocio = new Ent_CursoNegocio();
                return Ok(negocio.listar(idInstitucion));
            }
            catch (Exception ex)
            {
                return responderError(ex);
            }
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(Ent_Curso), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetPorId(int id)
        {
            try
            {
                Ent_CursoNegocio negocio = new Ent_CursoNegocio();
                Ent_Curso? curso = negocio.obtenerPorId(id);
                if (curso == null)
                {
                    return NotFound(new { Message = "No se encontró el curso solicitado." });
                }
                return Ok(curso);
            }
            catch (Exception ex)
            {
                return responderError(ex);
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(Ent_Curso), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Post([FromBody] Ent_Curso nuevo)
        {
            try
            {
                Ent_CursoNegocio negocio = new Ent_CursoNegocio();
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
        public IActionResult Put([FromBody] Ent_Curso actualizado)
        {
            try
            {
                Ent_CursoNegocio negocio = new Ent_CursoNegocio();
                negocio.actualizar(actualizado);
                return Ok(new { Message = "Curso modificado correctamente." });
            }
            catch (Exception ex)
            {
                return responderError(ex);
            }
        }

        [HttpPatch("{id:int}/vacantes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult PatchVacantes(int id, [FromBody] Ent_CursoVacantes_in registro)
        {
            try
            {
                Ent_CursoNegocio negocio = new Ent_CursoNegocio();
                negocio.actualizarVacantes(id, registro);
                return Ok(new { Message = "Vacantes actualizadas correctamente." });
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
                Ent_CursoNegocio negocio = new Ent_CursoNegocio();
                negocio.eliminar(id);
                return Ok(new { Message = "Curso eliminado correctamente." });
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

            logger.LogError(ex, "Error al procesar una operación de cursos.");
            return StatusCode(500, new { Message = "No se pudo completar la operación. Intente nuevamente más tarde." });
        }
    }
}
