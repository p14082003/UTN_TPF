using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Negocio.Helpers
{
    public class ArchivoHelper
    {
        public const int MaximoBytesPorArchivo = 5 * 1024 * 1024;
        private const string CarpetaAdministrador = "postulaciones-admin";
        private const string CarpetaAlumno = "postulaciones-alumnos";
        private const string CarpetaImagen = "imagenes-instituciones";
        private const string PatronAdministrador = @"\Apostulaciones-admin/[a-f0-9]{32}/(constancia-fiscal|habilitacion|constancia-laboral)\.(pdf|jpg|jpeg|png)\z";
        private const string PatronAlumno = @"\Apostulaciones-alumnos/[a-f0-9]{32}/documento-[0-9]+\.(pdf|jpg|jpeg|png)\z";
        private const string PatronImagen = @"\Aimagenes-instituciones/[a-f0-9]{32}/imagen\.(jpg|jpeg|png)\z";
        private readonly string raiz;

        public ArchivoHelper(string? directorioRaiz, string directorioAplicacion)
        {
            if (string.IsNullOrWhiteSpace(directorioRaiz) || !Path.IsPathFullyQualified(directorioRaiz))
            {
                throw new InvalidOperationException("Debe configurar Archivos:DirectorioRaiz con una ruta absoluta fuera del proyecto.");
            }
            raiz = Path.GetFullPath(directorioRaiz);
            string aplicacion = Path.GetFullPath(directorioAplicacion);
            if (estaDentro(aplicacion, raiz))
            {
                throw new InvalidOperationException("La carpeta de archivos debe estar fuera del proyecto de la API.");
            }
        }

        public static byte[] leerYValidar(Stream? archivo, string? nombre, long longitud,
            string mensajeArchivoVacio = "Debe adjuntar los tres documentos y ninguno puede estar vacío.")
        {
            if (archivo == null || longitud <= 0)
            {
                throw new ArgumentException(mensajeArchivoVacio);
            }
            if (longitud > MaximoBytesPorArchivo)
            {
                throw new ArgumentException("Cada archivo puede pesar como máximo 5 MB.");
            }
            using (MemoryStream copia = new MemoryStream())
            {
                byte[] buffer = new byte[81920];
                int leidos;
                while ((leidos = archivo.Read(buffer, 0, buffer.Length)) > 0)
                {
                    if (copia.Length + leidos > MaximoBytesPorArchivo)
                    {
                        throw new ArgumentException("Cada archivo puede pesar como máximo 5 MB.");
                    }
                    copia.Write(buffer, 0, leidos);
                }
                byte[] contenido = copia.ToArray();
                validar(nombre, contenido);
                return contenido;
            }
        }

        public static void validar(string? nombre, byte[] contenido)
        {
            if (contenido == null || contenido.Length == 0)
            {
                throw new ArgumentException("El archivo no puede estar vacío.");
            }
            if (contenido.Length > MaximoBytesPorArchivo)
            {
                throw new ArgumentException("Cada archivo puede pesar como máximo 5 MB.");
            }
            string extension = Path.GetExtension(nombre ?? "").ToLowerInvariant();
            bool valido = false;
            if (extension == ".pdf")
            {
                valido = contenido.Length >= 12 &&
                    Encoding.ASCII.GetString(contenido, 0, 5) == "%PDF-" &&
                    Encoding.ASCII.GetString(contenido, Math.Max(0, contenido.Length - 1024), Math.Min(1024, contenido.Length)).Contains("%%EOF");
            }
            else if (extension == ".jpg" || extension == ".jpeg")
            {
                valido = contenido.Length >= 5 && contenido[0] == 0xFF && contenido[1] == 0xD8 &&
                    contenido[2] == 0xFF && contenido[^2] == 0xFF && contenido[^1] == 0xD9;
            }
            else if (extension == ".png")
            {
                byte[] inicio = { 137, 80, 78, 71, 13, 10, 26, 10 };
                byte[] final = { 0, 0, 0, 0, 73, 69, 78, 68, 174, 66, 96, 130 };
                valido = contenido.Length >= 45 && contenido.Take(8).SequenceEqual(inicio) &&
                    contenido.Skip(contenido.Length - 12).SequenceEqual(final);
            }
            else
            {
                throw new ArgumentException("Solo se permiten archivos PDF, JPG y PNG.");
            }
            if (!valido)
            {
                throw new ArgumentException("La firma del archivo no corresponde a un PDF, JPG o PNG válido para su extensión.");
            }
        }

        public string guardar(string carga, string tipo, string nombreOriginal, byte[] contenido)
        {
            validarTipo(tipo);
            return guardarArchivo(carga, CarpetaAdministrador, tipo, nombreOriginal, contenido, PatronAdministrador);
        }

        public string guardarDocumentoAlumno(string carga, int numero, string nombreOriginal, byte[] contenido)
        {
            string nombre = "documento-" + numero.ToString(CultureInfo.InvariantCulture);
            return guardarArchivo(carga, CarpetaAlumno, nombre, nombreOriginal, contenido, PatronAlumno);
        }

        public string guardarImagenInstitucion(string carga, string nombreOriginal, byte[] contenido)
        {
            string extension = Path.GetExtension(nombreOriginal).ToLowerInvariant();
            if (extension != ".jpg" && extension != ".jpeg" && extension != ".png")
            {
                throw new ArgumentException("La imagen debe ser JPG, JPEG o PNG.");
            }
            return guardarArchivo(carga, CarpetaImagen, "imagen", nombreOriginal, contenido, PatronImagen);
        }

        private string guardarArchivo(string carga, string categoria, string nombre, string nombreOriginal,
            byte[] contenido, string patron)
        {
            validar(nombreOriginal, contenido);
            string carpeta = directorioCarga(carga, categoria);
            string extension = Path.GetExtension(nombreOriginal).ToLowerInvariant();
            string relativa = categoria + "/" + carga + "/" + nombre + extension;
            string absoluta = resolverRuta(relativa, patron);
            comprobarEnlaces(carpeta);
            Directory.CreateDirectory(carpeta);
            comprobarEnlaces(absoluta);
            using (FileStream archivo = new FileStream(absoluta, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                archivo.Write(contenido, 0, contenido.Length);
            }
            return relativa;
        }

        public FileStream abrir(string rutaRelativa)
        {
            return abrirArchivo(rutaRelativa, PatronAdministrador);
        }

        public FileStream abrirDocumentoAlumno(string rutaRelativa)
        {
            return abrirArchivo(rutaRelativa, PatronAlumno);
        }

        public FileStream abrirImagenInstitucion(string rutaRelativa)
        {
            return abrirArchivo(rutaRelativa, PatronImagen);
        }

        private FileStream abrirArchivo(string rutaRelativa, string patron)
        {
            string absoluta = resolverRuta(rutaRelativa, patron);
            comprobarEnlaces(absoluta);
            return new FileStream(absoluta, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        public void eliminarCarga(string carga)
        {
            eliminarCarpetaCarga(carga, CarpetaAdministrador);
        }

        public void eliminarCargaAlumno(string carga)
        {
            eliminarCarpetaCarga(carga, CarpetaAlumno);
        }

        public void eliminarCargaImagenInstitucion(string carga)
        {
            eliminarCarpetaCarga(carga, CarpetaImagen);
        }

        public static bool esRutaImagenInstitucion(string rutaRelativa)
        {
            return Regex.IsMatch(rutaRelativa, PatronImagen);
        }

        public void eliminarImagenInstitucion(string rutaRelativa)
        {
            string absoluta = resolverRuta(rutaRelativa, PatronImagen);
            comprobarEnlaces(absoluta);
            try
            {
                File.Delete(absoluta);
            }
            catch (DirectoryNotFoundException)
            {
                // Si falta la carpeta, el archivo ya no está en disco.
            }

            string? carpeta = Path.GetDirectoryName(absoluta);
            if (carpeta != null && Directory.Exists(carpeta) && !Directory.EnumerateFileSystemEntries(carpeta).Any())
            {
                Directory.Delete(carpeta);
            }
        }

        private void eliminarCarpetaCarga(string carga, string categoria)
        {
            string carpeta = directorioCarga(carga, categoria);
            comprobarEnlaces(carpeta);
            if (Directory.Exists(carpeta))
            {
                Directory.Delete(carpeta, true);
            }
        }

        public static void validarTipo(string tipo)
        {
            if (tipo != "constancia-fiscal" && tipo != "habilitacion" && tipo != "constancia-laboral")
            {
                throw new ArgumentException("El tipo debe ser constancia-fiscal, habilitacion o constancia-laboral.");
            }
        }

        public static string obtenerContentType(string ruta)
        {
            return Path.GetExtension(ruta).ToLowerInvariant() switch
            {
                ".pdf" => "application/pdf",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => throw new FileNotFoundException("El documento no está disponible.")
            };
        }

        private string directorioCarga(string carga, string categoria)
        {
            if (!Regex.IsMatch(carga, "^[a-f0-9]{32}\\z"))
            {
                throw new ArgumentException("El identificador de la carga no es válido.");
            }
            return Path.Combine(raiz, categoria, carga);
        }

        private string resolverRuta(string relativa, string patron)
        {
            if (string.IsNullOrWhiteSpace(relativa) || !Regex.IsMatch(relativa, patron))
            {
                throw new FileNotFoundException("El documento no está disponible.");
            }
            string absoluta = Path.GetFullPath(Path.Combine(raiz, relativa.Replace('/', Path.DirectorySeparatorChar)));
            if (!estaDentro(raiz, absoluta))
            {
                throw new FileNotFoundException("El documento no está disponible.");
            }
            return absoluta;
        }

        private void comprobarEnlaces(string destino)
        {
            string relativa = Path.GetRelativePath(raiz, destino);
            string actual = raiz;
            foreach (string parte in relativa.Split(Path.DirectorySeparatorChar))
            {
                actual = Path.Combine(actual, parte);
                if ((File.Exists(actual) || Directory.Exists(actual)) &&
                    (File.GetAttributes(actual) & FileAttributes.ReparsePoint) != 0)
                {
                    throw new IOException("No se permiten enlaces en la carpeta de documentos.");
                }
            }
        }

        private static bool estaDentro(string carpeta, string destino)
        {
            StringComparison comparacion = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            string baseNormalizada = Path.TrimEndingDirectorySeparator(carpeta);
            return string.Equals(baseNormalizada, Path.TrimEndingDirectorySeparator(destino), comparacion) ||
                destino.StartsWith(Path.EndsInDirectorySeparator(baseNormalizada) ? baseNormalizada : baseNormalizada + Path.DirectorySeparatorChar, comparacion);
        }
    }
}
