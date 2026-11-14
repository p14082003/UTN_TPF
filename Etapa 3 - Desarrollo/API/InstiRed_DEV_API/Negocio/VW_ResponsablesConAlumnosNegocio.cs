using Dominio.DTOs;
using MySqlConnector;
using Negocio.Helpers;

namespace Negocio
{
    public class VW_ResponsablesConAlumnosNegocio
    {
        public List<VW_ResponsablesConAlumnos_out> listar(int? idResponsable = null, int? idAlumno = null)
        {
            if (idResponsable != null && (idResponsable <= 0))
            {
                throw new ArgumentException("El filtro idResponsable debe ser mayor que cero.");
            }
            if (idAlumno != null && (idAlumno <= 0))
            {
                throw new ArgumentException("El filtro idAlumno debe ser mayor que cero.");
            }
            List<VW_ResponsablesConAlumnos_out> lista = new List<VW_ResponsablesConAlumnos_out>();
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("SELECT idResponsable, nombreResponsable, apellidoResponsable, docResponsable, emailResponsable, usuarioActivo, idAlumno, nombreAlumno, apellidoAlumno, docAlumno, rol, idCurso, institucion FROM vw_Responsables_Con_Alumnos WHERE (@idResponsable IS NULL OR idResponsable = @idResponsable) AND (@idAlumno IS NULL OR idAlumno = @idAlumno) ORDER BY idResponsable, idAlumno, idCurso");
                datos.setearParametro("@idResponsable", idResponsable);
                datos.setearParametro("@idAlumno", idAlumno);
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

        private VW_ResponsablesConAlumnos_out leerRegistro(MySqlDataReader lector)
        {
            VW_ResponsablesConAlumnos_out registro = new VW_ResponsablesConAlumnos_out();
            registro.IdResponsable = Convert.ToInt32(lector["idResponsable"]);
            registro.NombreResponsable = (string)lector["nombreResponsable"];
            registro.ApellidoResponsable = (string)lector["apellidoResponsable"];
            registro.DocResponsable = (string)lector["docResponsable"];
            registro.EmailResponsable = (string)lector["emailResponsable"];
            registro.UsuarioActivo = Convert.ToBoolean(lector["usuarioActivo"]);
            if (lector["idAlumno"] != DBNull.Value)
            {
                registro.IdAlumno = Convert.ToInt32(lector["idAlumno"]);
            }
            if (lector["nombreAlumno"] != DBNull.Value)
            {
                registro.NombreAlumno = (string)lector["nombreAlumno"];
            }
            if (lector["apellidoAlumno"] != DBNull.Value)
            {
                registro.ApellidoAlumno = (string)lector["apellidoAlumno"];
            }
            if (lector["docAlumno"] != DBNull.Value)
            {
                registro.DocAlumno = (string)lector["docAlumno"];
            }
            if (lector["rol"] != DBNull.Value)
            {
                registro.Rol = (string)lector["rol"];
            }
            if (lector["idCurso"] != DBNull.Value)
            {
                registro.IdCurso = Convert.ToInt32(lector["idCurso"]);
            }
            if (lector["institucion"] != DBNull.Value)
            {
                registro.Institucion = (string)lector["institucion"];
            }
            return registro;
        }
    }
}
