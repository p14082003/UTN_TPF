using System.Data;
using Dominio;
using MySqlConnector;

namespace Negocio
{
    public class IdiomaInstitucionNegocio
    {
        public List<IdiomaInstitucion> listar()
        {
            List<IdiomaInstitucion> lista = new List<IdiomaInstitucion>();
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("SELECT `idInstitucion`, `idIdioma` FROM `IdiomaInstitucion` ORDER BY `idInstitucion`, `idIdioma`");
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

        public IdiomaInstitucion? obtenerPorId(int idInstitucion, int idIdioma)
        {
            if (idInstitucion <= 0)
            {
                throw new ArgumentException("El campo IdInstitucion debe ser mayor que cero.");
            }
            if (idIdioma <= 0 || idIdioma > 127)
            {
                throw new ArgumentException("El campo IdIdioma debe ser mayor que cero y no superar 127.");
            }
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("SELECT `idInstitucion`, `idIdioma` FROM `IdiomaInstitucion` WHERE `idInstitucion` = @clave0 AND `idIdioma` = @clave1");
                datos.setearParametro("@clave0", idInstitucion);
                datos.setearParametro("@clave1", idIdioma);
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

        public void agregar(IdiomaInstitucion nuevo)
        {
            validar(nuevo);
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("INSERT INTO `IdiomaInstitucion` (`idInstitucion`, `idIdioma`) VALUES (@idInstitucion, @idIdioma)");
                datos.setearParametro("@idInstitucion", nuevo.IdInstitucion);
                datos.setearParametro("@idIdioma", nuevo.IdIdioma);
                datos.ejecutarAccion();

            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                {
                    throw new ConstraintException("Ya existe un registro de IdiomaInstitucion con esos datos únicos.", ex);
                }
                if (ex.Number == 1452)
                {
                    throw new ConstraintException("No se puede guardar el registro de IdiomaInstitucion porque una de las entidades relacionadas no existe.", ex);
                }
                if (ex.Number == 1451)
                {
                    throw new ConstraintException("No se puede modificar o eliminar el registro de IdiomaInstitucion porque aparece en otras entidades.", ex);
                }
                throw;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        public void actualizar(int idInstitucion, int idIdioma, IdiomaInstitucion registro)
        {
            validar(registro);
            if (idInstitucion <= 0)
            {
                throw new ArgumentException("El campo IdInstitucion debe ser mayor que cero.");
            }
            if (idIdioma <= 0 || idIdioma > 127)
            {
                throw new ArgumentException("El campo IdIdioma debe ser mayor que cero y no superar 127.");
            }
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("UPDATE `IdiomaInstitucion` SET `idInstitucion` = @idInstitucion, `idIdioma` = @idIdioma WHERE `idInstitucion` = @clave0 AND `idIdioma` = @clave1");
                datos.setearParametro("@idInstitucion", registro.IdInstitucion);
                datos.setearParametro("@idIdioma", registro.IdIdioma);
                datos.setearParametro("@clave0", idInstitucion);
                datos.setearParametro("@clave1", idIdioma);
                if (datos.ejecutarAccion() == 0)
                {
                    throw new KeyNotFoundException("No se encontró el registro de IdiomaInstitucion que se desea actualizar.");
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                {
                    throw new ConstraintException("Ya existe un registro de IdiomaInstitucion con esos datos únicos.", ex);
                }
                if (ex.Number == 1452)
                {
                    throw new ConstraintException("No se puede guardar el registro de IdiomaInstitucion porque una de las entidades relacionadas no existe.", ex);
                }
                if (ex.Number == 1451)
                {
                    throw new ConstraintException("No se puede modificar o eliminar el registro de IdiomaInstitucion porque aparece en otras entidades.", ex);
                }
                throw;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        public void eliminar(int idInstitucion, int idIdioma)
        {
            if (idInstitucion <= 0)
            {
                throw new ArgumentException("El campo IdInstitucion debe ser mayor que cero.");
            }
            if (idIdioma <= 0 || idIdioma > 127)
            {
                throw new ArgumentException("El campo IdIdioma debe ser mayor que cero y no superar 127.");
            }
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("DELETE FROM `IdiomaInstitucion` WHERE `idInstitucion` = @clave0 AND `idIdioma` = @clave1");
                datos.setearParametro("@clave0", idInstitucion);
                datos.setearParametro("@clave1", idIdioma);

                if (datos.ejecutarAccion() == 0)
                {
                    throw new KeyNotFoundException("No se encontró el registro de IdiomaInstitucion solicitado.");
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                {
                    throw new ConstraintException("Ya existe un registro de IdiomaInstitucion con esos datos únicos.", ex);
                }
                if (ex.Number == 1452)
                {
                    throw new ConstraintException("No se puede guardar el registro de IdiomaInstitucion porque una de las entidades relacionadas no existe.", ex);
                }
                if (ex.Number == 1451)
                {
                    throw new ConstraintException("No se puede modificar o eliminar el registro de IdiomaInstitucion porque aparece en otras entidades.", ex);
                }
                throw;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        private IdiomaInstitucion leerRegistro(MySqlDataReader lector)
        {
            IdiomaInstitucion registro = new IdiomaInstitucion();
            registro.IdInstitucion = Convert.ToInt32(lector["idInstitucion"]);
            registro.IdIdioma = Convert.ToInt32(lector["idIdioma"]);
            return registro;
        }

        private void validar(IdiomaInstitucion registro)
        {
            if (registro == null)
            {
                throw new ArgumentException("Debe indicar los datos de IdiomaInstitucion.");
            }
            if (registro.IdInstitucion <= 0)
            {
                throw new ArgumentException("El campo IdInstitucion debe ser mayor que cero.");
            }
            if (registro.IdIdioma <= 0 || registro.IdIdioma > 127)
            {
                throw new ArgumentException("El campo IdIdioma debe ser mayor que cero y no superar 127.");
            }
        }
    }
}
