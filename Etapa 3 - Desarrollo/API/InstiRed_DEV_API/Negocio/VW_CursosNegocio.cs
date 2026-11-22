using Dominio.DTOs;
using MySqlConnector;
using Negocio.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio
{
    public class VW_CursosNegocio
    {
        // Traer cursos por id de institucion
        public List<VW_Cursos_out> obtenerPorInstitucion(int idInstitucion)
        {
            if(idInstitucion <= 0)
            {
                throw new ArgumentException("El idInstitucion debe ser mayor a cero");
            }

            List<VW_Cursos_out> lista = new List<VW_Cursos_out>();

            AccesoDatos datos = new AccesoDatos();

            try
            {
                string consulta = "SELECT idCurso, idInstitucion, cicloLectivo, idNivel, nivel, anio, nivelPersonalizado, division, idTurno, turno, capacidad, inscriptos, vacantesDisponibles " +
                                  "FROM vw_Cursos " +
                                  "WHERE idInstitucion = @idInstitucion " +
                                  "ORDER BY cicloLectivo DESC, anio, division";

                datos.setearConsulta(consulta);
                datos.setearParametro("@idInstitucion", idInstitucion);
                datos.ejecutarLectura();

                MySqlDataReader? lector = datos.Lector;

                if(lector == null)
                {
                    throw new InvalidOperationException("Error al inicializar el lector de datos");
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

        private VW_Cursos_out leerRegistro(MySqlDataReader lector)
        {
            VW_Cursos_out registro = new VW_Cursos_out();

            registro.IdCurso = Convert.ToInt32(lector["idCurso"]);
            registro.IdInstitucion = Convert.ToInt32(lector["idInstitucion"]);
            registro.CicloLectivo = Convert.ToInt32(lector["cicloLectivo"]);
            registro.Division = (string)lector["division"];
            registro.IdTurno = Convert.ToInt32(lector["idTurno"]);
            registro.Turno = (string)lector["turno"];
            registro.Capacidad = Convert.ToInt32(lector["capacidad"]);
            registro.Inscriptos = Convert.ToInt32(lector["inscriptos"]);
            registro.VacantesDisponibles = Convert.ToInt32(lector["vacantesDisponibles"]);

            // Valido los campos null de la DB
            if (lector["idNivel"] != DBNull.Value)
            {
                registro.IdNivel = Convert.ToInt32(lector["idNivel"]);
            }

            if (lector["nivel"] != DBNull.Value)
            {
                registro.Nivel = (string)lector["nivel"];
            }

            if (lector["anio"] != DBNull.Value)
            {
                registro.Anio = Convert.ToInt32(lector["anio"]);
            }

            if (lector["nivelPersonalizado"] != DBNull.Value)
            {
                registro.NivelPersonalizado =
                    (string)lector["nivelPersonalizado"];
            }

            return registro;
        }
    }
}
