using Dominio.DTOs;
using MySqlConnector;
using Negocio.Helpers;

namespace Negocio
{
    public class SP_RegistrarCursoNegocio
    {
        public SP_RegistrarCurso_out registrar(SP_RegistrarCurso_in registro)
        {
            if (registro.Anio < sbyte.MinValue || registro.Anio > sbyte.MaxValue)
            {
                throw new ArgumentException("El año ingresado está fuera de rango.");
            }
            if (registro.Capacidad < 1 || registro.Capacidad > short.MaxValue)
            {
                throw new ArgumentException("Capacidad de alumnos inválida.");
            }
            if (registro.Inscriptos < 0 || registro.Inscriptos > registro.Capacidad)
            {
                throw new ArgumentException("Cantidad de alumnos inscriptos inválida.");
            }

            return ProcedimientoHelper.ejecutar("sp_Registrar_Curso", datos =>
            {
                datos.setearParametro("@p_idAdministradorInstitucion", registro.IdAdministradorInstitucion);
                datos.setearParametro("@P_idTurno", registro.IdTurno);
                datos.setearParametro("@p_cicloLectivo", registro.CicloLectivo);
                datos.setearParametro("@p_idNivel", registro.IdNivel);
                datos.setearParametro("@p_anio", registro.Anio);
                datos.setearParametro("@p_nivelPersonalizado", registro.NivelPersonalizado);
                datos.setearParametro("@p_division", registro.Division);
                datos.setearParametro("@p_capacidad", registro.Capacidad);
                datos.setearParametro("@p_inscriptos", registro.Inscriptos);

                MySqlDataReader lector = ProcedimientoHelper.leerFila(datos);
                SP_RegistrarCurso_out curso = new SP_RegistrarCurso_out
                {
                    Id = Convert.ToInt32(lector["Id"]),
                    IdInstitucion = Convert.ToInt32(lector["IdInstitucion"]),
                    IdTurno = Convert.ToInt32(lector["IdTurno"]),
                    CicloLectivo = Convert.ToInt32(lector["CicloLectivo"]),
                    Division = (string)lector["Division"],
                    Capacidad = Convert.ToInt32(lector["Capacidad"]),
                    Inscriptos = Convert.ToInt32(lector["Inscriptos"])
                };
                if (lector["IdNivel"] != DBNull.Value)
                {
                    curso.IdNivel = Convert.ToInt32(lector["IdNivel"]);
                }
                if (lector["Anio"] != DBNull.Value)
                {
                    curso.Anio = Convert.ToInt32(lector["Anio"]);
                }
                if (lector["NivelPersonalizado"] != DBNull.Value)
                {
                    curso.NivelPersonalizado = (string)lector["NivelPersonalizado"];
                }
                return curso;
            });
        }
    }
}
