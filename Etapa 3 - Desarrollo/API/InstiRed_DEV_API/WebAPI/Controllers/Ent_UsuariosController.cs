using System.Data;
using Dominio;
using Microsoft.AspNetCore.Mvc;
using Negocio;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("ent/Usuarios")]
    public class Ent_UsuariosController : ControllerBase
    {
        private readonly ILogger<Ent_UsuariosController> logger;

        public Ent_UsuariosController(ILogger<Ent_UsuariosController> logger)
        {
            this.logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Get()
        {
            try
            {
                Ent_UsuarioNegocio negocio = new Ent_UsuarioNegocio();
                List<Ent_Usuario> usuarios = negocio.listar();
                List<object> respuesta = new List<object>();
                foreach (Ent_Usuario usuario in usuarios)
                {
                    respuesta.Add(respuestaUsuario(usuario));
                }
                return Ok(respuesta);
            }
            catch (Exception ex)
            {
                return responderError(ex);
            }
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetPorId(int id)
        {
            try
            {
                Ent_UsuarioNegocio negocio = new Ent_UsuarioNegocio();
                Ent_Usuario? registro = negocio.obtenerPorId(id);
                if (registro == null)
                {
                    return NotFound(new { Message = "No se encontró el registro de Usuarios solicitado." });
                }
                return Ok(respuestaUsuario(registro));
            }
            catch (Exception ex)
            {
                return responderError(ex);
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Post([FromBody] Ent_Usuario nuevo)
        {
            try
            {
                Ent_UsuarioNegocio negocio = new Ent_UsuarioNegocio();
                nuevo.Id = negocio.agregar(nuevo);
                return CreatedAtAction(nameof(GetPorId), new { id = nuevo.Id }, respuestaUsuario(nuevo));
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
        public IActionResult Put([FromBody] Ent_Usuario actualizado)
        {
            try
            {
                Ent_UsuarioNegocio negocio = new Ent_UsuarioNegocio();
                negocio.actualizar(actualizado);
                return Ok(new { Message = "Registro de Usuarios modificado correctamente." });
            }
            catch (Exception ex)
            {
                return responderError(ex);
            }
        }

        [HttpPatch("{id:int}/estado")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult CambiarEstado(int id)
        {
            try
            {
                Ent_UsuarioNegocio negocio = new Ent_UsuarioNegocio();
                bool nuevoEstado = negocio.cambiarEstado(id);
                return Ok(new { Message = "Estado del registro de Usuarios modificado correctamente.", Id = id, Activo = nuevoEstado });
            }
            catch (Exception ex)
            {
                return responderError(ex);
            }
        }

        private object respuestaUsuario(Ent_Usuario usuario)
        {
            return new
            {
                usuario.Id,
                usuario.Email,
                usuario.Activo,
                usuario.IdTipoUsuario
            };
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
