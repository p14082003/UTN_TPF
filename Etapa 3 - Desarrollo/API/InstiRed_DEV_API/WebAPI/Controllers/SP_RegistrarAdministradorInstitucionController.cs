using Dominio.DTOs;
using Microsoft.AspNetCore.Mvc;
using Negocio;
using Negocio.Helpers;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("sp/RegistrarAdministradorInstitucion")]
    public class SP_RegistrarAdministradorInstitucionController : ControllerBase
    {
        private readonly ILogger<SP_RegistrarAdministradorInstitucionController> logger;
        private readonly IConfiguration configuracion;
        private readonly IWebHostEnvironment entorno;

        public SP_RegistrarAdministradorInstitucionController(ILogger<SP_RegistrarAdministradorInstitucionController> logger,
            IConfiguration configuracion, IWebHostEnvironment entorno)
        {
            this.logger = logger;
            this.configuracion = configuracion;
            this.entorno = entorno;
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(20 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 20 * 1024 * 1024)]
        [ProducesResponseType(typeof(SP_RegistrarAdministradorInstitucion_out), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Post([FromForm] SP_RegistrarAdministradorInstitucion_form formulario)
        {
            ArchivoHelper? archivos = null;
            string? carga = null;
            bool seIntentoRegistrar = false;
            try
            {
                byte[] fiscal = leerArchivo(formulario.ConstanciaFiscal);
                byte[] habilitacion = leerArchivo(formulario.Habilitacion);
                byte[] laboral = leerArchivo(formulario.ConstanciaLaboral);
                archivos = new ArchivoHelper(configuracion["Archivos:DirectorioRaiz"], entorno.ContentRootPath);
                carga = Guid.NewGuid().ToString("N");

                SP_RegistrarAdministradorInstitucion_in registro = new SP_RegistrarAdministradorInstitucion_in
                {
                    Nombre = formulario.Nombre,
                    Apellido = formulario.Apellido,
                    Documento = formulario.Documento,
                    Domicilio = formulario.Domicilio,
                    FechaNacimiento = formulario.FechaNacimiento,
                    Clave = formulario.Clave,
                    Email = formulario.Email,
                    Telefono = formulario.Telefono,
                    NombreInstitucion = formulario.NombreInstitucion,
                    CuitInstitucion = formulario.CuitInstitucion,
                    RutaConstanciaFiscal = archivos.guardar(carga, "constancia-fiscal", formulario.ConstanciaFiscal?.FileName ?? "", fiscal),
                    RutaHabilitacion = archivos.guardar(carga, "habilitacion", formulario.Habilitacion?.FileName ?? "", habilitacion),
                    RutaConstanciaLaboral = archivos.guardar(carga, "constancia-laboral", formulario.ConstanciaLaboral?.FileName ?? "", laboral)
                };
                SP_RegistrarAdministradorInstitucionNegocio negocio = new SP_RegistrarAdministradorInstitucionNegocio();
                seIntentoRegistrar = true;
                SP_RegistrarAdministradorInstitucion_out respuesta = negocio.registrar(registro);
                return CreatedAtAction("GetPorId", "Ent_PostulacionesAdminInstitucion", new { id = respuesta.IdPostulacion }, respuesta);
            }
            catch (ArgumentException ex)
            {
                limpiarCarga(archivos, carga);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                if (!seIntentoRegistrar)
                {
                    limpiarCarga(archivos, carga);
                }
                else
                {
                    // Una pérdida de conexión puede ocurrir después del COMMIT del SP.
                    // Se conservan los adjuntos para no borrar documentos de un alta confirmada.
                    logger.LogWarning("Se conservan los documentos de la carga {Carga}; comprobar el resultado del registro antes de limpiarlos.", carga);
                }
                logger.LogError(ex, "Error al ejecutar RegistrarAdministradorInstitucion.");
                return StatusCode(500, new { Message = "No se pudo completar la operación. Intente nuevamente más tarde." });
            }
        }

        private byte[] leerArchivo(IFormFile? archivo)
        {
            using (Stream? contenido = archivo?.OpenReadStream())
            {
                return ArchivoHelper.leerYValidar(contenido, archivo?.FileName, archivo?.Length ?? 0);
            }
        }

        private void limpiarCarga(ArchivoHelper? archivos, string? carga)
        {
            if (archivos == null || carga == null)
            {
                return;
            }
            try
            {
                archivos.eliminarCarga(carga);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "No se pudo limpiar la carga rechazada {Carga}.", carga);
            }
        }
    }
}
