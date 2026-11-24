using Dominio.DTOs;
using MySqlConnector;
using Negocio.Helpers;

namespace Negocio
{
    public class VW_InstitucionesNegocio
    {
        public List<VW_Instituciones_out> listar(int? idTipoInstitucion = null, bool? estado = null, string? nombre = null,
            int? idNivel = null, int? anio = null, int? cicloLectivo = null, int? idTurno = null,
            bool soloConVacantes = false, string orden = "nombre")
        {
            if (idTipoInstitucion != null && (idTipoInstitucion <= 0 || idTipoInstitucion > 127))
            {
                throw new ArgumentException("El filtro idTipoInstitucion debe ser mayor que cero y no superar 127.");
            }
            if (nombre != null && (string.IsNullOrWhiteSpace(nombre) || nombre.Trim().Length > 100))
            {
                throw new ArgumentException("El filtro nombre no puede estar vacío ni superar los 100 caracteres.");
            }
            if (idNivel.HasValue && (idNivel < 1 || idNivel > 3))
            {
                throw new ArgumentException("Seleccioná un nivel escolar válido.");
            }
            if (idTipoInstitucion == 4 && idNivel.HasValue)
            {
                throw new ArgumentException("Los institutos de idiomas no tienen un nivel escolar normalizado.");
            }
            if (anio.HasValue && (!idNivel.HasValue || anio < (idNivel == 1 ? 3 : 1) ||
                anio > (idNivel == 1 ? 5 : idNivel == 2 ? 7 : 6)))
            {
                throw new ArgumentException("Seleccioná una sala, grado o año válido para el nivel elegido.");
            }
            int anioActual = DateTime.Today.Year;
            if (cicloLectivo.HasValue && cicloLectivo != anioActual && cicloLectivo != anioActual + 1)
            {
                throw new ArgumentException("El ciclo lectivo debe ser el año actual o el próximo.");
            }
            if (idTurno.HasValue && (idTurno <= 0 || idTurno > 127))
            {
                throw new ArgumentException("Seleccioná un turno válido.");
            }
            if (orden != "nombre" && orden != "valoracion")
            {
                throw new ArgumentException("El orden debe ser nombre o valoracion.");
            }
            List<VW_Instituciones_out> lista = new List<VW_Instituciones_out>();
            AccesoDatos datos = new AccesoDatos();
            try
            {
                string consulta = @"
                    SELECT i.idInstitucion, i.nombre, i.direccion, i.email, i.cuit, i.latitud, i.longitud,
                           i.descripcion, i.estado, i.idTipoInstitucion, i.tipoInstitucion, i.movilidad,
                           i.visual, i.auditiva, i.verbal, i.intelectual, i.sindromeDown, i.autismo,
                           i.idiomas, i.telefonos, i.promedioCalificacion, i.cantidadResenias
                    FROM vw_Instituciones i
                    WHERE (@idTipoInstitucion IS NULL OR i.idTipoInstitucion = @idTipoInstitucion)
                      AND (@estado IS NULL OR i.estado = @estado)
                      AND (@nombre IS NULL OR i.nombre LIKE CONCAT('%', @nombre, '%'))
                      AND (@idNivel IS NULL
                           OR (@idNivel = 1 AND i.idTipoInstitucion IN (1, 5, 7, 8))
                           OR (@idNivel = 2 AND i.idTipoInstitucion IN (2, 5, 6, 7))
                           OR (@idNivel = 3 AND i.idTipoInstitucion IN (3, 6, 7, 8)))";

                if (anio.HasValue || cicloLectivo.HasValue || idTurno.HasValue || soloConVacantes)
                {
                    // Todos los criterios deben cumplirse en el mismo curso.
                    consulta += @"
                      AND EXISTS (
                          SELECT 1 FROM Cursos c
                          WHERE c.idInstitucion = i.idInstitucion
                            AND c.cicloLectivo IN (@anioActual, @anioProximo)
                            AND (@idNivel IS NULL OR c.idNivel = @idNivel)
                            AND (@anio IS NULL OR c.anio = @anio)
                            AND (@cicloLectivo IS NULL OR c.cicloLectivo = @cicloLectivo)
                            AND (@idTurno IS NULL OR c.idTurno = @idTurno)
                            AND (@soloConVacantes = FALSE OR c.capacidad > c.inscriptos)
                      )";
                }
                consulta += orden == "valoracion"
                    ? " ORDER BY i.promedioCalificacion DESC, i.cantidadResenias DESC, i.nombre, i.idInstitucion"
                    : " ORDER BY i.nombre, i.idInstitucion";
                datos.setearConsulta(consulta);
                datos.setearParametro("@idTipoInstitucion", idTipoInstitucion);
                datos.setearParametro("@estado", estado);
                datos.setearParametro("@nombre", nombre?.Trim());
                datos.setearParametro("@idNivel", idNivel);
                datos.setearParametro("@anio", anio);
                datos.setearParametro("@cicloLectivo", cicloLectivo);
                datos.setearParametro("@idTurno", idTurno);
                datos.setearParametro("@soloConVacantes", soloConVacantes);
                datos.setearParametro("@anioActual", anioActual);
                datos.setearParametro("@anioProximo", anioActual + 1);
                datos.ejecutarLectura();
                MySqlDataReader? lector = datos.Lector;
                if (lector == null)
                {
                    throw new InvalidOperationException("No se inicializó el lector de datos.");
                }
                while (lector.Read())
                {
                    lista.Add(leerRegistro(lector));
                }
                return lista;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        public VW_Instituciones_out? obtenerPorId(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("El ID debe ser mayor que cero.");
            }
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("SELECT idInstitucion, nombre, direccion, email, cuit, latitud, longitud, descripcion, estado, idTipoInstitucion, tipoInstitucion, movilidad, visual, auditiva, verbal, intelectual, sindromeDown, autismo, idiomas, telefonos, promedioCalificacion, cantidadResenias FROM vw_Instituciones WHERE idInstitucion = @id");
                datos.setearParametro("@id", id);
                datos.ejecutarLectura();
                MySqlDataReader? lector = datos.Lector;
                if (lector == null)
                {
                    throw new InvalidOperationException("No se inicializó el lector de datos.");
                }
                if (lector.Read())
                {
                    return leerRegistro(lector);
                }
                return null;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        private VW_Instituciones_out leerRegistro(MySqlDataReader lector)
        {
            VW_Instituciones_out registro = new VW_Instituciones_out();
            registro.IdInstitucion = Convert.ToInt32(lector["idInstitucion"]);
            registro.Nombre = (string)lector["nombre"];
            registro.Direccion = (string)lector["direccion"];
            registro.Email = (string)lector["email"];
            registro.Cuit = (string)lector["cuit"];
            registro.Latitud = Convert.ToDouble(lector["latitud"]);
            registro.Longitud = Convert.ToDouble(lector["longitud"]);
            registro.Descripcion = (string)lector["descripcion"];
            registro.Estado = Convert.ToBoolean(lector["estado"]);
            registro.IdTipoInstitucion = Convert.ToInt32(lector["idTipoInstitucion"]);
            registro.TipoInstitucion = (string)lector["tipoInstitucion"];
            if (lector["movilidad"] != DBNull.Value)
            {
                registro.Movilidad = Convert.ToBoolean(lector["movilidad"]);
            }
            if (lector["visual"] != DBNull.Value)
            {
                registro.Visual = Convert.ToBoolean(lector["visual"]);
            }
            if (lector["auditiva"] != DBNull.Value)
            {
                registro.Auditiva = Convert.ToBoolean(lector["auditiva"]);
            }
            if (lector["verbal"] != DBNull.Value)
            {
                registro.Verbal = Convert.ToBoolean(lector["verbal"]);
            }
            if (lector["intelectual"] != DBNull.Value)
            {
                registro.Intelectual = Convert.ToBoolean(lector["intelectual"]);
            }
            if (lector["sindromeDown"] != DBNull.Value)
            {
                registro.SindromeDown = Convert.ToBoolean(lector["sindromeDown"]);
            }
            if (lector["autismo"] != DBNull.Value)
            {
                registro.Autismo = Convert.ToBoolean(lector["autismo"]);
            }
            if (lector["idiomas"] != DBNull.Value)
            {
                registro.Idiomas = (string)lector["idiomas"];
            }
            if (lector["telefonos"] != DBNull.Value)
            {
                registro.Telefonos = (string)lector["telefonos"];
            }
            if (lector["promedioCalificacion"] != DBNull.Value)
            {
                registro.PromedioCalificacion = Convert.ToDecimal(lector["promedioCalificacion"]);
            }
            registro.CantidadResenias = Convert.ToInt64(lector["cantidadResenias"]);
            return registro;
        }
    }
}
