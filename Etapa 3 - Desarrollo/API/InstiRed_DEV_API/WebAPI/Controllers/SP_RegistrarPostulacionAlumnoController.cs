using Dominio.DTOs;
using Microsoft.AspNetCore.Mvc;
using Negocio;
using Negocio.Helpers;
using WebAPI.Helpers;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("sp/RegistrarPostulacionAlumno")]
    public class SP_RegistrarPostulacionAlumnoController : ControllerBase
    {
        private readonly IConfiguration configuracion;
        private readonly IWebHostEnvironment entorno;
        private readonly ILogger<SP_RegistrarPostulacionAlumnoController> logger;

        public SP_RegistrarPostulacionAlumnoController(IConfiguration configuracion, IWebHostEnvironment entorno,
            ILogger<SP_RegistrarPostulacionAlumnoController> logger)
        {
            this.configuracion = configuracion;
            this.entorno = entorno;
            this.logger = logger;
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(20 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 20 * 1024 * 1024)]
        [ProducesResponseType(typeof(SP_RegistrarPostulacionAlumno_out), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Post([FromForm] SP_RegistrarPostulacionAlumno_form formulario)
        {
            ArchivoHelper? archivos = null;
            string carga = Guid.NewGuid().ToString("N");
            bool seIntentoRegistrar = false;

            try
            {
                if (formulario.Documentos.Count == 0)
                {
                    throw new ArgumentException("Debe adjuntar al menos un documento.");
                }
                foreach (SP_DocumentoPostulacionAlumno_form documento in formulario.Documentos)
                {
                    if (string.IsNullOrWhiteSpace(documento.Descripcion) || documento.Descripcion.Trim().Length > 100)
                    {
                        throw new ArgumentException("Cada documento requiere una descripción de hasta 100 caracteres.");
                    }
                }
                archivos = new ArchivoHelper(configuracion["Archivos:DirectorioRaiz"], entorno.ContentRootPath);
                SP_RegistrarPostulacionAlumno_in registro = new SP_RegistrarPostulacionAlumno_in
                {
                    IdResponsable = formulario.IdResponsable,
                    IdAlumno = formulario.IdAlumno,
                    IdCurso = formulario.IdCurso
                };
                for (int indice = 0; indice < formulario.Documentos.Count; indice++)
                {
                    SP_DocumentoPostulacionAlumno_form documento = formulario.Documentos[indice];
                    byte[] contenido = ArchivoFormularioHelper.leer(documento.Archivo);
                    string ruta = archivos.guardarDocumentoAlumno(carga, indice + 1, documento.Archivo?.FileName ?? "", contenido);
                    registro.ListaDocumentos.Add(new SP_DocumentoPostulacionAlumno_in
                    {
                        Descripcion = documento.Descripcion.Trim(),
                        Ruta = ruta
                    });
                }

                SP_RegistrarPostulacionAlumnoNegocio negocio = new SP_RegistrarPostulacionAlumnoNegocio();
                seIntentoRegistrar = true;
                SP_RegistrarPostulacionAlumno_out respuesta = negocio.registrar(registro);
                return CreatedAtAction("GetPorId", "Ent_PostulacionesAlumnos", new { id = respuesta.IdPostulacion }, respuesta);
            }
            catch (ArgumentException ex)
            {
                if (archivos != null)
                {
                    ArchivoFormularioHelper.limpiarCarga(carga, archivos.eliminarCargaAlumno, logger);
                }
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                if (archivos != null && !seIntentoRegistrar)
                {
                    ArchivoFormularioHelper.limpiarCarga(carga, archivos.eliminarCargaAlumno, logger);
                }
                if (seIntentoRegistrar)
                {
                    logger.LogWarning("Se conservan los archivos de la carga {Carga}; comprobar el resultado antes de limpiarlos.", carga);
                }
                logger.LogError(ex, "Error al ejecutar RegistrarPostulacionAlumno.");
                return StatusCode(500, new { Message = "No se pudo completar la operación. Intente nuevamente más tarde." });
            }
        }
    }
}
