using Negocio.Helpers;

namespace WebAPI.Helpers
{
    internal static class ArchivoFormularioHelper
    {
        public static byte[] leer(IFormFile? archivo)
        {
            using (Stream? contenido = archivo?.OpenReadStream())
            {
                return ArchivoHelper.leerYValidar(contenido, archivo?.FileName, archivo?.Length ?? 0,
                    "Debe adjuntar un archivo que no esté vacío.");
            }
        }

        public static void limpiarCarga(string carga, Action<string> eliminar, ILogger logger)
        {
            try
            {
                eliminar(carga);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "No se pudo limpiar la carga rechazada {Carga}.", carga);
            }
        }
    }
}
