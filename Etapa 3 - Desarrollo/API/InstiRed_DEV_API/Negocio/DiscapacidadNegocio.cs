using System.Data;
using Dominio;
using MySqlConnector;

namespace Negocio
{
    public class DiscapacidadNegocio
    {
        public List<Discapacidad> listar()
        {
            List<Discapacidad> lista = new List<Discapacidad>();
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("SELECT `idInstitucion`, `movilidad`, `visual`, `auditiva`, `verbal`, `intelectual`, `sindromeDown`, `autismo` FROM `Discapacidades` ORDER BY `idInstitucion`");
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

        public Discapacidad? obtenerPorId(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("El campo IdInstitucion debe ser mayor que cero.");
            }
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("SELECT `idInstitucion`, `movilidad`, `visual`, `auditiva`, `verbal`, `intelectual`, `sindromeDown`, `autismo` FROM `Discapacidades` WHERE `idInstitucion` = @clave0");
                datos.setearParametro("@clave0", id);
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

        public int agregar(Discapacidad nuevo)
        {
            validar(nuevo);
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("INSERT INTO `Discapacidades` (`idInstitucion`, `movilidad`, `visual`, `auditiva`, `verbal`, `intelectual`, `sindromeDown`, `autismo`) VALUES (@idInstitucion, @movilidad, @visual, @auditiva, @verbal, @intelectual, @sindromeDown, @autismo)");
                datos.setearParametro("@idInstitucion", nuevo.IdInstitucion);
                datos.setearParametro("@movilidad", nuevo.Movilidad);
                datos.setearParametro("@visual", nuevo.Visual);
                datos.setearParametro("@auditiva", nuevo.Auditiva);
                datos.setearParametro("@verbal", nuevo.Verbal);
                datos.setearParametro("@intelectual", nuevo.Intelectual);
                datos.setearParametro("@sindromeDown", nuevo.SindromeDown);
                datos.setearParametro("@autismo", nuevo.Autismo);
                datos.ejecutarAccion();
                return nuevo.IdInstitucion;
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                {
                    throw new ConstraintException("Ya existe un registro de Discapacidades con esos datos únicos.", ex);
                }
                if (ex.Number == 1452)
                {
                    throw new ConstraintException("No se puede guardar el registro de Discapacidades porque una de las entidades relacionadas no existe.", ex);
                }
                if (ex.Number == 1451)
                {
                    throw new ConstraintException("No se puede modificar o eliminar el registro de Discapacidades porque aparece en otras entidades.", ex);
                }
                throw;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        public void actualizar(Discapacidad registro)
        {
            validar(registro);
            int id = registro.IdInstitucion;
            if (id <= 0)
            {
                throw new ArgumentException("El campo IdInstitucion debe ser mayor que cero.");
            }
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("UPDATE `Discapacidades` SET `movilidad` = @movilidad, `visual` = @visual, `auditiva` = @auditiva, `verbal` = @verbal, `intelectual` = @intelectual, `sindromeDown` = @sindromeDown, `autismo` = @autismo WHERE `idInstitucion` = @clave0");
                datos.setearParametro("@movilidad", registro.Movilidad);
                datos.setearParametro("@visual", registro.Visual);
                datos.setearParametro("@auditiva", registro.Auditiva);
                datos.setearParametro("@verbal", registro.Verbal);
                datos.setearParametro("@intelectual", registro.Intelectual);
                datos.setearParametro("@sindromeDown", registro.SindromeDown);
                datos.setearParametro("@autismo", registro.Autismo);
                datos.setearParametro("@clave0", id);
                if (datos.ejecutarAccion() == 0)
                {
                    throw new KeyNotFoundException("No se encontró el registro de Discapacidades que se desea actualizar.");
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                {
                    throw new ConstraintException("Ya existe un registro de Discapacidades con esos datos únicos.", ex);
                }
                if (ex.Number == 1452)
                {
                    throw new ConstraintException("No se puede guardar el registro de Discapacidades porque una de las entidades relacionadas no existe.", ex);
                }
                if (ex.Number == 1451)
                {
                    throw new ConstraintException("No se puede modificar o eliminar el registro de Discapacidades porque aparece en otras entidades.", ex);
                }
                throw;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        public void eliminar(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("El campo IdInstitucion debe ser mayor que cero.");
            }
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("DELETE FROM `Discapacidades` WHERE `idInstitucion` = @clave0");
                datos.setearParametro("@clave0", id);

                if (datos.ejecutarAccion() == 0)
                {
                    throw new KeyNotFoundException("No se encontró el registro de Discapacidades solicitado.");
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                {
                    throw new ConstraintException("Ya existe un registro de Discapacidades con esos datos únicos.", ex);
                }
                if (ex.Number == 1452)
                {
                    throw new ConstraintException("No se puede guardar el registro de Discapacidades porque una de las entidades relacionadas no existe.", ex);
                }
                if (ex.Number == 1451)
                {
                    throw new ConstraintException("No se puede modificar o eliminar el registro de Discapacidades porque aparece en otras entidades.", ex);
                }
                throw;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        private Discapacidad leerRegistro(MySqlDataReader lector)
        {
            Discapacidad registro = new Discapacidad();
            registro.IdInstitucion = Convert.ToInt32(lector["idInstitucion"]);
            registro.Movilidad = Convert.ToBoolean(lector["movilidad"]);
            registro.Visual = Convert.ToBoolean(lector["visual"]);
            registro.Auditiva = Convert.ToBoolean(lector["auditiva"]);
            registro.Verbal = Convert.ToBoolean(lector["verbal"]);
            registro.Intelectual = Convert.ToBoolean(lector["intelectual"]);
            registro.SindromeDown = Convert.ToBoolean(lector["sindromeDown"]);
            registro.Autismo = Convert.ToBoolean(lector["autismo"]);
            return registro;
        }

        private void validar(Discapacidad registro)
        {
            if (registro == null)
            {
                throw new ArgumentException("Debe indicar los datos de Discapacidad.");
            }
            if (registro.IdInstitucion <= 0)
            {
                throw new ArgumentException("El campo IdInstitucion debe ser mayor que cero.");
            }
        }
    }
}
