using Dominio.DTOs;
using Microsoft.AspNetCore.Mvc;
using Negocio;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("sp/RegistrarCurso")]
    public class SP_RegistrarCursoController : ControllerBase
    {
        private readonly ILogger<SP_RegistrarCursoController> logger;

        public SP_RegistrarCursoController(ILogger<SP_RegistrarCursoController> logger)
        {
            this.logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(typeof(SP_RegistrarCurso_out), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Post([FromBody] SP_RegistrarCurso_in registro)
        {
            try
            {
                SP_RegistrarCursoNegocio negocio = new SP_RegistrarCursoNegocio();
                SP_RegistrarCurso_out curso = negocio.registrar(registro);
                return CreatedAtAction(nameof(Ent_CursosController.GetPorId), "Ent_Cursos", new { id = curso.Id }, curso);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al ejecutar RegistrarCurso.");
                return StatusCode(500, new { Message = "No se pudo completar la operación. Intente nuevamente más tarde." });
            }
        }
    }
}
