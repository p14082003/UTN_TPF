using Negocio.Helpers;
using System.Data;
using Dominio;
using MySqlConnector;

namespace Negocio
{
    public class Ent_TelefonoNegocio
    {
        public List<Ent_Telefono> listar()
        {
            List<Ent_Telefono> lista = new List<Ent_Telefono>();
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("SELECT `id`, `idInstitucion`, `idAdministrador`, `idResponsable`, `telefono` FROM `Telefonos` ORDER BY `id`");
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

        public Ent_Telefono? obtenerPorId(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("El campo Id debe ser mayor que cero.");
            }
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("SELECT `id`, `idInstitucion`, `idAdministrador`, `idResponsable`, `telefono` FROM `Telefonos` WHERE `id` = @clave0");
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

        public int agregar(Ent_Telefono nuevo)
        {
            validar(nuevo);
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("INSERT INTO `Telefonos` (`idInstitucion`, `idAdministrador`, `idResponsable`, `telefono`) VALUES (@idInstitucion, @idAdministrador, @idResponsable, @telefono)");
                datos.setearParametro("@idInstitucion", nuevo.IdInstitucion);
                datos.setearParametro("@idAdministrador", nuevo.IdAdministrador);
                datos.setearParametro("@idResponsable", nuevo.IdResponsable);
                datos.setearParametro("@telefono", nuevo.NumeroTelefono);
                datos.ejecutarAccion();
                return Convert.ToInt32(datos.UltimoIdInsertado);
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                {
                    throw new ConstraintException("Ya existe un registro de Telefonos con esos datos únicos.", ex);
                }
                if (ex.Number == 1452)
                {
                    throw new ConstraintException("No se puede guardar el registro de Telefonos porque una de las entidades relacionadas no existe.", ex);
                }
                if (ex.Number == 1451)
                {
                    throw new ConstraintException("No se puede modificar o eliminar el registro de Telefonos porque aparece en otras entidades.", ex);
                }
                throw;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        public void actualizar(Ent_Telefono registro)
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
                datos.setearConsulta("UPDATE `Telefonos` SET `idInstitucion` = @idInstitucion, `idAdministrador` = @idAdministrador, `idResponsable` = @idResponsable, `telefono` = @telefono WHERE `id` = @clave0");
                datos.setearParametro("@idInstitucion", registro.IdInstitucion);
                datos.setearParametro("@idAdministrador", registro.IdAdministrador);
                datos.setearParametro("@idResponsable", registro.IdResponsable);
                datos.setearParametro("@telefono", registro.NumeroTelefono);
                datos.setearParametro("@clave0", id);
                if (datos.ejecutarAccion() == 0)
                {
                    throw new KeyNotFoundException("No se encontró el registro de Telefonos que se desea actualizar.");
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                {
                    throw new ConstraintException("Ya existe un registro de Telefonos con esos datos únicos.", ex);
                }
                if (ex.Number == 1452)
                {
                    throw new ConstraintException("No se puede guardar el registro de Telefonos porque una de las entidades relacionadas no existe.", ex);
                }
                if (ex.Number == 1451)
                {
                    throw new ConstraintException("No se puede modificar o eliminar el registro de Telefonos porque aparece en otras entidades.", ex);
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
                datos.setearConsulta("DELETE FROM `Telefonos` WHERE `id` = @clave0");
                datos.setearParametro("@clave0", id);

                if (datos.ejecutarAccion() == 0)
                {
                    throw new KeyNotFoundException("No se encontró el registro de Telefonos solicitado.");
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                {
                    throw new ConstraintException("Ya existe un registro de Telefonos con esos datos únicos.", ex);
                }
                if (ex.Number == 1452)
                {
                    throw new ConstraintException("No se puede guardar el registro de Telefonos porque una de las entidades relacionadas no existe.", ex);
                }
                if (ex.Number == 1451)
                {
                    throw new ConstraintException("No se puede modificar o eliminar el registro de Telefonos porque aparece en otras entidades.", ex);
                }
                throw;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        private Ent_Telefono leerRegistro(MySqlDataReader lector)
        {
            Ent_Telefono registro = new Ent_Telefono();
            registro.Id = Convert.ToInt32(lector["id"]);
            if (lector["idInstitucion"] != DBNull.Value)
            {
                registro.IdInstitucion = Convert.ToInt32(lector["idInstitucion"]);
            }
            if (lector["idAdministrador"] != DBNull.Value)
            {
                registro.IdAdministrador = Convert.ToInt32(lector["idAdministrador"]);
            }
            if (lector["idResponsable"] != DBNull.Value)
            {
                registro.IdResponsable = Convert.ToInt32(lector["idResponsable"]);
            }
            registro.NumeroTelefono = (string)lector["telefono"];
            return registro;
        }

        private void validar(Ent_Telefono registro)
        {
            if (registro == null)
            {
                throw new ArgumentException("Debe indicar los datos de Telefono.");
            }
            if (registro.IdInstitucion != null && (registro.IdInstitucion <= 0))
            {
                throw new ArgumentException("El campo IdInstitucion debe ser mayor que cero o quedar sin especificar.");
            }
            if (registro.IdAdministrador != null && (registro.IdAdministrador <= 0))
            {
                throw new ArgumentException("El campo IdAdministrador debe ser mayor que cero o quedar sin especificar.");
            }
            if (registro.IdResponsable != null && (registro.IdResponsable <= 0))
            {
                throw new ArgumentException("El campo IdResponsable debe ser mayor que cero o quedar sin especificar.");
            }
            if (string.IsNullOrWhiteSpace(registro.NumeroTelefono))
            {
                throw new ArgumentException("El campo NumeroTelefono es obligatorio.");
            }
            if (registro.NumeroTelefono.Length > 30)
            {
                throw new ArgumentException("El campo NumeroTelefono no puede superar los 30 caracteres.");
            }
            if (registro.IdInstitucion == null && registro.IdAdministrador == null && registro.IdResponsable == null)
            {
                throw new ArgumentException("El teléfono debe estar asociado al menos a una institución, un administrador o un responsable.");
            }
        }
    }
}
