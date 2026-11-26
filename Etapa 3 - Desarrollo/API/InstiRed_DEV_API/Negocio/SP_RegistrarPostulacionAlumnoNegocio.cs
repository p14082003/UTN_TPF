using Dominio.DTOs;
using MySqlConnector;
using Negocio.Helpers;
using System.Text.Json;

namespace Negocio
{
    public class SP_RegistrarPostulacionAlumnoNegocio
    {
        public SP_RegistrarPostulacionAlumno_out registrar(SP_RegistrarPostulacionAlumno_in registro)
        {
            validar(registro);
            return ProcedimientoHelper.ejecutar("sp_Registrar_Postulacion_Alumno", datos =>
            {
                datos.setearParametro("@p_idResponsable", registro.IdResponsable);
                datos.setearParametro("@p_idAlumno", registro.IdAlumno);
                datos.setearParametro("@p_idCurso", registro.IdCurso);
                datos.setearParametro("@p_listaDocumentos", JsonSerializer.Serialize(registro.ListaDocumentos));
                MySqlDataReader lector = ProcedimientoHelper.leerFila(datos);
                return new SP_RegistrarPostulacionAlumno_out
                {
                    IdPostulacion = Convert.ToInt32(lector["IdPostulacion"]),
                    FechaPostulacion = Convert.ToDateTime(lector["FechaPostulacion"]),
                    CodigoSeguimiento = (string)lector["CodigoSeguimiento"],
                    IdCurso = Convert.ToInt32(lector["IdCurso"]),
                    IdAlumno = Convert.ToInt32(lector["IdAlumno"]),
                    IdEstado = Convert.ToInt32(lector["IdEstado"]),
                    IdInstitucion = Convert.ToInt32(lector["IdInstitucion"]),
                    Nombre = (string)lector["Nombre"],
                    Apellido = (string)lector["Apellido"],
                    Documento = (string)lector["Documento"],
                    Descripcion = (string)lector["Descripcion"]
                };
            });
        }

        private void validar(SP_RegistrarPostulacionAlumno_in registro)
        {
            if (registro == null)
            {
                throw new ArgumentException("Debe indicar los datos de la operación.");
            }
            if (registro.IdResponsable <= 0)
            {
                throw new ArgumentException("El campo IdResponsable debe ser mayor que cero.");
            }
            if (registro.IdAlumno <= 0)
            {
                throw new ArgumentException("El campo IdAlumno debe ser mayor que cero.");
            }
            if (registro.IdCurso <= 0)
            {
                throw new ArgumentException("El campo IdCurso debe ser mayor que cero.");
            }
            validarDocumentos(registro.ListaDocumentos?.Select(documento => documento.Descripcion));
        }

        public static void validarDocumentos(IEnumerable<string>? descripciones)
        {
            var documentos = (descripciones ?? Enumerable.Empty<string>())
                .Select(descripcion => descripcion?.Trim() ?? "").ToList();
            if (documentos.Any(descripcion => descripcion.Length == 0 || descripcion.Length > 100))
            {
                throw new ArgumentException("Cada documento requiere una descripción de hasta 100 caracteres.");
            }
            string[] obligatorios = { "DNI del alumno - Frente", "DNI del alumno - Dorso", "Partida de nacimiento" };
            var unicos = obligatorios.Append("Certificado de estudios");
            var repetidos = unicos.Where(tipo => documentos.Count(descripcion =>
                string.Equals(descripcion, tipo, StringComparison.OrdinalIgnoreCase)) > 1).ToList();
            if (repetidos.Count > 0)
            {
                throw new ArgumentException("Estos documentos solo se pueden adjuntar una vez: " + string.Join(", ", repetidos) + ".");
            }
            var faltantes = obligatorios.Where(tipo => !documentos.Contains(tipo, StringComparer.OrdinalIgnoreCase)).ToList();
            if (faltantes.Count > 0)
            {
                throw new ArgumentException("Faltan documentos obligatorios: " + string.Join(", ", faltantes) + ".");
            }
        }
    }
}
