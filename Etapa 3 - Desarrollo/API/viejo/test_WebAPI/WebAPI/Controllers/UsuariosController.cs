using Microsoft.AspNetCore.Mvc;
using Dominio;
using Negocio;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            var negocio = new UsuarioNegocio();
            return Ok(negocio.listar());
        }

        [HttpPost]
        public IActionResult Post([FromBody] Usuario nuevo)
        {
            var negocio = new UsuarioNegocio();
            negocio.agregar(nuevo);
            return Ok(new { Message = "Usuario agregado correctamente." });
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var negocio = new UsuarioNegocio();
            negocio.eliminar(id);
            return Ok(new { Message = "Usuario eliminado correctamente." });
        }

        [HttpPut]
        public IActionResult Put([FromBody] Usuario actualizado)
        {
            var negocio = new UsuarioNegocio();
            negocio.actualizar(actualizado);
            return Ok(new { Message = "Usuario modificado correctamente." });
        }
    }
}
