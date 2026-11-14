using Negocio.Helpers;
using System.Data;
using Dominio;
using MySqlConnector;

namespace Negocio
{
    public class Ent_AdministradorInstitucionNegocio
    {
        public List<Ent_AdministradorInstitucion> listar()
        {
            List<Ent_AdministradorInstitucion> lista = new List<Ent_AdministradorInstitucion>();
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("SELECT `id`, `idPersona`, `idUsuario`, `idInstitucion` FROM `AdministradoresInstitucion` ORDER BY `id`");
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

        public Ent_AdministradorInstitucion? obtenerPorId(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("El campo Id debe ser mayor que cero.");
            }
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("SELECT `id`, `idPersona`, `idUsuario`, `idInstitucion` FROM `AdministradoresInstitucion` WHERE `id` = @clave0");
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

        public int agregar(Ent_AdministradorInstitucion nuevo)
        {
            validar(nuevo);
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("INSERT INTO `AdministradoresInstitucion` (`idPersona`, `idUsuario`, `idInstitucion`) VALUES (@idPersona, @idUsuario, @idInstitucion)");
                datos.setearParametro("@idPersona", nuevo.IdPersona);
                datos.setearParametro("@idUsuario", nuevo.IdUsuario);
                datos.setearParametro("@idInstitucion", nuevo.IdInstitucion);
                datos.ejecutarAccion();
                return Convert.ToInt32(datos.UltimoIdInsertado);
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                {
                    throw new ConstraintException("Ya existe un registro de AdministradoresInstitucion con esos datos únicos.", ex);
                }
                if (ex.Number == 1452)
                {
                    throw new ConstraintException("No se puede guardar el registro de AdministradoresInstitucion porque una de las entidades relacionadas no existe.", ex);
                }
                if (ex.Number == 1451)
                {
                    throw new ConstraintException("No se puede modificar o eliminar el registro de AdministradoresInstitucion porque aparece en otras entidades.", ex);
                }
                throw;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        public void actualizar(Ent_AdministradorInstitucion registro)
        {
            validar(registro);
            int id = registro.Id;
            if (id <= 0)
            {
                throw new ArgumentException("El campo Id debe ser mayor que cero.");
            }
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("UPDATE `AdministradoresInstitucion` SET `idPersona` = @idPersona, `idUsuario` = @idUsuario, `idInstitucion` = @idInstitucion WHERE `id` = @clave0");
                datos.setearParametro("@idPersona", registro.IdPersona);
                datos.setearParametro("@idUsuario", registro.IdUsuario);
                datos.setearParametro("@idInstitucion", registro.IdInstitucion);
                datos.setearParametro("@clave0", id);
                if (datos.ejecutarAccion() == 0)
                {
                    throw new KeyNotFoundException("No se encontró el registro de AdministradoresInstitucion que se desea actualizar.");
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                {
                    throw new ConstraintException("Ya existe un registro de AdministradoresInstitucion con esos datos únicos.", ex);
                }
                if (ex.Number == 1452)
                {
                    throw new ConstraintException("No se puede guardar el registro de AdministradoresInstitucion porque una de las entidades relacionadas no existe.", ex);
                }
                if (ex.Number == 1451)
                {
                    throw new ConstraintException("No se puede modificar o eliminar el registro de AdministradoresInstitucion porque aparece en otras entidades.", ex);
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
                throw new ArgumentException("El campo Id debe ser mayor que cero.");
            }
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("DELETE FROM `AdministradoresInstitucion` WHERE `id` = @clave0");
                datos.setearParametro("@clave0", id);

                if (datos.ejecutarAccion() == 0)
                {
                    throw new KeyNotFoundException("No se encontró el registro de AdministradoresInstitucion solicitado.");
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                {
                    throw new ConstraintException("Ya existe un registro de AdministradoresInstitucion con esos datos únicos.", ex);
                }
                if (ex.Number == 1452)
                {
                    throw new ConstraintException("No se puede guardar el registro de AdministradoresInstitucion porque una de las entidades relacionadas no existe.", ex);
                }
                if (ex.Number == 1451)
                {
                    throw new ConstraintException("No se puede modificar o eliminar el registro de AdministradoresInstitucion porque aparece en otras entidades.", ex);
                }
                throw;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        private Ent_AdministradorInstitucion leerRegistro(MySqlDataReader lector)
        {
            Ent_AdministradorInstitucion registro = new Ent_AdministradorInstitucion();
            registro.Id = Convert.ToInt32(lector["id"]);
            registro.IdPersona = Convert.ToInt32(lector["idPersona"]);
            registro.IdUsuario = Convert.ToInt32(lector["idUsuario"]);
            if (lector["idInstitucion"] != DBNull.Value)
            {
                registro.IdInstitucion = Convert.ToInt32(lector["idInstitucion"]);
            }
            return registro;
        }

        private void validar(Ent_AdministradorInstitucion registro)
        {
            if (registro == null)
            {
                throw new ArgumentException("Debe indicar los datos de AdministradorInstitucion.");
            }
            if (registro.IdPersona <= 0)
            {
                throw new ArgumentException("El campo IdPersona debe ser mayor que cero.");
            }
            if (registro.IdUsuario <= 0)
            {
                throw new ArgumentException("El campo IdUsuario debe ser mayor que cero.");
            }
            if (registro.IdInstitucion != null && (registro.IdInstitucion <= 0))
            {
                throw new ArgumentException("El campo IdInstitucion debe ser mayor que cero o quedar sin especificar.");
            }
        }
    }
}
