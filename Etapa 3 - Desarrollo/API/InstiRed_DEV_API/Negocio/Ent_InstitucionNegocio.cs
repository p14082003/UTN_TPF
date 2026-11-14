using Negocio.Helpers;
using System.Data;
using Dominio;
using MySqlConnector;

namespace Negocio
{
    public class Ent_InstitucionNegocio
    {
        public List<Ent_Institucion> listar()
        {
            List<Ent_Institucion> lista = new List<Ent_Institucion>();
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("SELECT `id`, `nombre`, `cuit`, `direccion`, `email`, `latitud`, `longitud`, `descripcion`, `idTipoInstitucion`, `estado` FROM `Instituciones` ORDER BY `id`");
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

        public Ent_Institucion? obtenerPorId(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("El campo Id debe ser mayor que cero.");
            }
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("SELECT `id`, `nombre`, `cuit`, `direccion`, `email`, `latitud`, `longitud`, `descripcion`, `idTipoInstitucion`, `estado` FROM `Instituciones` WHERE `id` = @clave0");
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

        public int agregar(Ent_Institucion nuevo)
        {
            validar(nuevo);
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("INSERT INTO `Instituciones` (`nombre`, `cuit`, `direccion`, `email`, `latitud`, `longitud`, `descripcion`, `idTipoInstitucion`, `estado`) VALUES (@nombre, @cuit, @direccion, @email, @latitud, @longitud, @descripcion, @idTipoInstitucion, @estado)");
                datos.setearParametro("@nombre", nuevo.Nombre);
                datos.setearParametro("@cuit", nuevo.Cuit);
                datos.setearParametro("@direccion", nuevo.Direccion);
                datos.setearParametro("@email", nuevo.Email);
                datos.setearParametro("@latitud", nuevo.Latitud);
                datos.setearParametro("@longitud", nuevo.Longitud);
                datos.setearParametro("@descripcion", nuevo.Descripcion);
                datos.setearParametro("@idTipoInstitucion", nuevo.IdTipoInstitucion);
                datos.setearParametro("@estado", nuevo.Estado);
                datos.ejecutarAccion();
                return Convert.ToInt32(datos.UltimoIdInsertado);
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                {
                    throw new ConstraintException("Ya existe un registro de Instituciones con esos datos únicos.", ex);
                }
                if (ex.Number == 1452)
                {
                    throw new ConstraintException("No se puede guardar el registro de Instituciones porque una de las entidades relacionadas no existe.", ex);
                }
                if (ex.Number == 1451)
                {
                    throw new ConstraintException("No se puede modificar o eliminar el registro de Instituciones porque aparece en otras entidades.", ex);
                }
                throw;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        public void actualizar(Ent_Institucion registro)
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
                datos.setearConsulta("UPDATE `Instituciones` SET `nombre` = @nombre, `cuit` = @cuit, `direccion` = @direccion, `email` = @email, `latitud` = @latitud, `longitud` = @longitud, `descripcion` = @descripcion, `idTipoInstitucion` = @idTipoInstitucion WHERE `id` = @clave0");
                datos.setearParametro("@nombre", registro.Nombre);
                datos.setearParametro("@cuit", registro.Cuit);
                datos.setearParametro("@direccion", registro.Direccion);
                datos.setearParametro("@email", registro.Email);
                datos.setearParametro("@latitud", registro.Latitud);
                datos.setearParametro("@longitud", registro.Longitud);
                datos.setearParametro("@descripcion", registro.Descripcion);
                datos.setearParametro("@idTipoInstitucion", registro.IdTipoInstitucion);
                datos.setearParametro("@clave0", id);
                if (datos.ejecutarAccion() == 0)
                {
                    throw new KeyNotFoundException("No se encontró el registro de Instituciones que se desea actualizar.");
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                {
                    throw new ConstraintException("Ya existe un registro de Instituciones con esos datos únicos.", ex);
                }
                if (ex.Number == 1452)
                {
                    throw new ConstraintException("No se puede guardar el registro de Instituciones porque una de las entidades relacionadas no existe.", ex);
                }
                if (ex.Number == 1451)
                {
                    throw new ConstraintException("No se puede modificar o eliminar el registro de Instituciones porque aparece en otras entidades.", ex);
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
                datos.setearConsulta("UPDATE `Instituciones` SET `estado` = NOT `estado` WHERE `id` = @id");
                datos.setearParametro("@id", id);
                if (datos.ejecutarAccion() == 0)
                {
                    throw new KeyNotFoundException("No se encontró el registro de Instituciones solicitado.");
                }

                datos.setearConsulta("SELECT `estado` FROM `Instituciones` WHERE `id` = @id");
                datos.setearParametro("@id", id);
                datos.ejecutarLectura();
                MySqlDataReader? lector = datos.Lector;
                if (lector == null)
                {
                    throw new InvalidOperationException("No se inicializó el lector de datos.");
                }
                if (!lector.Read())
                {
                    throw new KeyNotFoundException("No se encontró el registro de Instituciones solicitado.");
                }
                bool nuevoEstado = Convert.ToBoolean(lector["estado"]);
                datos.confirmarTransaccion();
                return nuevoEstado;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        private Ent_Institucion leerRegistro(MySqlDataReader lector)
        {
            Ent_Institucion registro = new Ent_Institucion();
            registro.Id = Convert.ToInt32(lector["id"]);
            registro.Nombre = (string)lector["nombre"];
            registro.Cuit = (string)lector["cuit"];
            registro.Direccion = (string)lector["direccion"];
            registro.Email = (string)lector["email"];
            registro.Latitud = Convert.ToDouble(lector["latitud"]);
            registro.Longitud = Convert.ToDouble(lector["longitud"]);
            registro.Descripcion = (string)lector["descripcion"];
            registro.IdTipoInstitucion = Convert.ToInt32(lector["idTipoInstitucion"]);
            registro.Estado = Convert.ToBoolean(lector["estado"]);
            return registro;
        }

        private void validar(Ent_Institucion registro)
        {
            if (registro == null)
            {
                throw new ArgumentException("Debe indicar los datos de Institucion.");
            }
            if (string.IsNullOrWhiteSpace(registro.Nombre))
            {
                throw new ArgumentException("El campo Nombre es obligatorio.");
            }
            if (registro.Nombre.Length > 100)
            {
                throw new ArgumentException("El campo Nombre no puede superar los 100 caracteres.");
            }
            if (string.IsNullOrWhiteSpace(registro.Cuit))
            {
                throw new ArgumentException("El campo Cuit es obligatorio.");
            }
            if (registro.Cuit.Length > 11)
            {
                throw new ArgumentException("El campo Cuit no puede superar los 11 caracteres.");
            }
            if (string.IsNullOrWhiteSpace(registro.Direccion))
            {
                throw new ArgumentException("El campo Direccion es obligatorio.");
            }
            if (registro.Direccion.Length > 100)
            {
                throw new ArgumentException("El campo Direccion no puede superar los 100 caracteres.");
            }
            if (string.IsNullOrWhiteSpace(registro.Email))
            {
                throw new ArgumentException("El campo Email es obligatorio.");
            }
            if (registro.Email.Length > 100)
            {
                throw new ArgumentException("El campo Email no puede superar los 100 caracteres.");
            }
            if (double.IsNaN(registro.Latitud) || double.IsInfinity(registro.Latitud))
            {
                throw new ArgumentException("El campo Latitud debe ser un número finito.");
            }
            if (double.IsNaN(registro.Longitud) || double.IsInfinity(registro.Longitud))
            {
                throw new ArgumentException("El campo Longitud debe ser un número finito.");
            }
            if (string.IsNullOrWhiteSpace(registro.Descripcion))
            {
                throw new ArgumentException("El campo Descripcion es obligatorio.");
            }
            if (registro.Descripcion.Length > 3000)
            {
                throw new ArgumentException("El campo Descripcion no puede superar los 3000 caracteres.");
            }
            if (registro.IdTipoInstitucion <= 0 || registro.IdTipoInstitucion > 127)
            {
                throw new ArgumentException("El campo IdTipoInstitucion debe ser mayor que cero y no superar 127.");
            }
        }
    }
}
