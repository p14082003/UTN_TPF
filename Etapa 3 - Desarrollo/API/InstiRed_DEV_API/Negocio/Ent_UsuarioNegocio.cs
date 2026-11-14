using Negocio.Helpers;
using System.Data;
using Dominio;
using MySqlConnector;

namespace Negocio
{
    public class Ent_UsuarioNegocio
    {
        public List<Ent_Usuario> listar()
        {
            List<Ent_Usuario> lista = new List<Ent_Usuario>();
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("SELECT `id`, `email`, `activo`, `idTipoUsuario` FROM `Usuarios` ORDER BY `id`");
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

        public Ent_Usuario? obtenerPorId(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("El campo Id debe ser mayor que cero.");
            }
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("SELECT `id`, `email`, `activo`, `idTipoUsuario` FROM `Usuarios` WHERE `id` = @clave0");
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

        public int agregar(Ent_Usuario nuevo)
        {
            validar(nuevo);
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("INSERT INTO `Usuarios` (`clave`, `email`, `activo`, `idTipoUsuario`) VALUES (@clave, @email, @activo, @idTipoUsuario)");
                datos.setearParametro("@clave", ContraseñaHelper.generarHash(nuevo.Clave));
                datos.setearParametro("@email", nuevo.Email);
                datos.setearParametro("@activo", nuevo.Activo);
                datos.setearParametro("@idTipoUsuario", nuevo.IdTipoUsuario);
                datos.ejecutarAccion();
                return Convert.ToInt32(datos.UltimoIdInsertado);
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                {
                    throw new ConstraintException("Ya existe un registro de Usuarios con esos datos únicos.", ex);
                }
                if (ex.Number == 1452)
                {
                    throw new ConstraintException("No se puede guardar el registro de Usuarios porque una de las entidades relacionadas no existe.", ex);
                }
                if (ex.Number == 1451)
                {
                    throw new ConstraintException("No se puede modificar o eliminar el registro de Usuarios porque aparece en otras entidades.", ex);
                }
                throw;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        public void actualizar(Ent_Usuario registro)
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
                datos.setearConsulta("UPDATE `Usuarios` SET `clave` = @clave, `email` = @email, `idTipoUsuario` = @idTipoUsuario WHERE `id` = @clave0");
                datos.setearParametro("@clave", ContraseñaHelper.generarHash(registro.Clave));
                datos.setearParametro("@email", registro.Email);
                datos.setearParametro("@idTipoUsuario", registro.IdTipoUsuario);
                datos.setearParametro("@clave0", id);
                if (datos.ejecutarAccion() == 0)
                {
                    throw new KeyNotFoundException("No se encontró el registro de Usuarios que se desea actualizar.");
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                {
                    throw new ConstraintException("Ya existe un registro de Usuarios con esos datos únicos.", ex);
                }
                if (ex.Number == 1452)
                {
                    throw new ConstraintException("No se puede guardar el registro de Usuarios porque una de las entidades relacionadas no existe.", ex);
                }
                if (ex.Number == 1451)
                {
                    throw new ConstraintException("No se puede modificar o eliminar el registro de Usuarios porque aparece en otras entidades.", ex);
                }
                throw;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        public bool cambiarEstado(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("El campo Id debe ser mayor que cero.");
            }
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.iniciarTransaccion();
                datos.setearConsulta("UPDATE `Usuarios` SET `activo` = NOT `activo` WHERE `id` = @id");
                datos.setearParametro("@id", id);
                if (datos.ejecutarAccion() == 0)
                {
                    throw new KeyNotFoundException("No se encontró el registro de Usuarios solicitado.");
                }

                datos.setearConsulta("SELECT `activo` FROM `Usuarios` WHERE `id` = @id");
                datos.setearParametro("@id", id);
                datos.ejecutarLectura();
                MySqlDataReader? lector = datos.Lector;
                if (lector == null)
                {
                    throw new InvalidOperationException("No se inicializó el lector de datos.");
                }
                if (!lector.Read())
                {
                    throw new KeyNotFoundException("No se encontró el registro de Usuarios solicitado.");
                }
                bool nuevoEstado = Convert.ToBoolean(lector["activo"]);
                datos.confirmarTransaccion();
                return nuevoEstado;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        private Ent_Usuario leerRegistro(MySqlDataReader lector)
        {
            Ent_Usuario registro = new Ent_Usuario();
            registro.Id = Convert.ToInt32(lector["id"]);
            registro.Email = (string)lector["email"];
            registro.Activo = Convert.ToBoolean(lector["activo"]);
            registro.IdTipoUsuario = Convert.ToInt32(lector["idTipoUsuario"]);
            return registro;
        }

        private void validar(Ent_Usuario registro)
        {
            if (registro == null)
            {
                throw new ArgumentException("Debe indicar los datos de Usuario.");
            }
            if (string.IsNullOrWhiteSpace(registro.Clave))
            {
                throw new ArgumentException("El campo Clave es obligatorio.");
            }
            if (registro.Clave.Length > 255)
            {
                throw new ArgumentException("El campo Clave no puede superar los 255 caracteres.");
            }
            if (string.IsNullOrWhiteSpace(registro.Email))
            {
                throw new ArgumentException("El campo Email es obligatorio.");
            }
            if (registro.Email.Length > 100)
            {
                throw new ArgumentException("El campo Email no puede superar los 100 caracteres.");
            }
            if (registro.IdTipoUsuario <= 0 || registro.IdTipoUsuario > 127)
            {
                throw new ArgumentException("El campo IdTipoUsuario debe ser mayor que cero y no superar 127.");
            }
        }
    }
}
