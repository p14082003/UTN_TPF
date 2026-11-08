using System.Data;
using Dominio;
using MySqlConnector;

namespace Negocio
{
    public class PersonaNegocio
    {
        public List<Persona> listar()
        {
            List<Persona> lista = new List<Persona>();
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("SELECT `id`, `nombre`, `apellido`, `documento`, `domicilio`, `fechaNacimiento` FROM `Personas` ORDER BY `id`");
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

        public Persona? obtenerPorId(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("El campo Id debe ser mayor que cero.");
            }
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("SELECT `id`, `nombre`, `apellido`, `documento`, `domicilio`, `fechaNacimiento` FROM `Personas` WHERE `id` = @clave0");
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

        public int agregar(Persona nuevo)
        {
            validar(nuevo);
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("INSERT INTO `Personas` (`nombre`, `apellido`, `documento`, `domicilio`, `fechaNacimiento`) VALUES (@nombre, @apellido, @documento, @domicilio, @fechaNacimiento)");
                datos.setearParametro("@nombre", nuevo.Nombre);
                datos.setearParametro("@apellido", nuevo.Apellido);
                datos.setearParametro("@documento", nuevo.Documento);
                datos.setearParametro("@domicilio", nuevo.Domicilio);
                datos.setearParametro("@fechaNacimiento", nuevo.FechaNacimiento.Date);
                datos.ejecutarAccion();
                return Convert.ToInt32(datos.UltimoIdInsertado);
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                {
                    throw new ConstraintException("Ya existe un registro de Personas con esos datos únicos.", ex);
                }
                if (ex.Number == 1452)
                {
                    throw new ConstraintException("No se puede guardar el registro de Personas porque una de las entidades relacionadas no existe.", ex);
                }
                if (ex.Number == 1451)
                {
                    throw new ConstraintException("No se puede modificar o eliminar el registro de Personas porque aparece en otras entidades.", ex);
                }
                throw;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        public void actualizar(Persona registro)
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
                datos.setearConsulta("UPDATE `Personas` SET `nombre` = @nombre, `apellido` = @apellido, `documento` = @documento, `domicilio` = @domicilio, `fechaNacimiento` = @fechaNacimiento WHERE `id` = @clave0");
                datos.setearParametro("@nombre", registro.Nombre);
                datos.setearParametro("@apellido", registro.Apellido);
                datos.setearParametro("@documento", registro.Documento);
                datos.setearParametro("@domicilio", registro.Domicilio);
                datos.setearParametro("@fechaNacimiento", registro.FechaNacimiento.Date);
                datos.setearParametro("@clave0", id);
                if (datos.ejecutarAccion() == 0)
                {
                    throw new KeyNotFoundException("No se encontró el registro de Personas que se desea actualizar.");
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                {
                    throw new ConstraintException("Ya existe un registro de Personas con esos datos únicos.", ex);
                }
                if (ex.Number == 1452)
                {
                    throw new ConstraintException("No se puede guardar el registro de Personas porque una de las entidades relacionadas no existe.", ex);
                }
                if (ex.Number == 1451)
                {
                    throw new ConstraintException("No se puede modificar o eliminar el registro de Personas porque aparece en otras entidades.", ex);
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
                datos.setearConsulta("DELETE FROM `Personas` WHERE `id` = @clave0");
                datos.setearParametro("@clave0", id);

                if (datos.ejecutarAccion() == 0)
                {
                    throw new KeyNotFoundException("No se encontró el registro de Personas solicitado.");
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                {
                    throw new ConstraintException("Ya existe un registro de Personas con esos datos únicos.", ex);
                }
                if (ex.Number == 1452)
                {
                    throw new ConstraintException("No se puede guardar el registro de Personas porque una de las entidades relacionadas no existe.", ex);
                }
                if (ex.Number == 1451)
                {
                    throw new ConstraintException("No se puede modificar o eliminar el registro de Personas porque aparece en otras entidades.", ex);
                }
                throw;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        private Persona leerRegistro(MySqlDataReader lector)
        {
            Persona registro = new Persona();
            registro.Id = Convert.ToInt32(lector["id"]);
            registro.Nombre = (string)lector["nombre"];
            registro.Apellido = (string)lector["apellido"];
            registro.Documento = (string)lector["documento"];
            registro.Domicilio = (string)lector["domicilio"];
            registro.FechaNacimiento = Convert.ToDateTime(lector["fechaNacimiento"]);
            return registro;
        }

        private void validar(Persona registro)
        {
            if (registro == null)
            {
                throw new ArgumentException("Debe indicar los datos de Persona.");
            }
            if (string.IsNullOrWhiteSpace(registro.Nombre))
            {
                throw new ArgumentException("El campo Nombre es obligatorio.");
            }
            if (registro.Nombre.Length > 50)
            {
                throw new ArgumentException("El campo Nombre no puede superar los 50 caracteres.");
            }
            if (string.IsNullOrWhiteSpace(registro.Apellido))
            {
                throw new ArgumentException("El campo Apellido es obligatorio.");
            }
            if (registro.Apellido.Length > 50)
            {
                throw new ArgumentException("El campo Apellido no puede superar los 50 caracteres.");
            }
            if (string.IsNullOrWhiteSpace(registro.Documento))
            {
                throw new ArgumentException("El campo Documento es obligatorio.");
            }
            if (registro.Documento.Length > 8)
            {
                throw new ArgumentException("El campo Documento no puede superar los 8 caracteres.");
            }
            if (string.IsNullOrWhiteSpace(registro.Domicilio))
            {
                throw new ArgumentException("El campo Domicilio es obligatorio.");
            }
            if (registro.Domicilio.Length > 100)
            {
                throw new ArgumentException("El campo Domicilio no puede superar los 100 caracteres.");
            }
            if (registro.FechaNacimiento.Year < 1000)
            {
                throw new ArgumentException("El campo FechaNacimiento debe contener una fecha válida desde el año 1000.");
            }
        }
    }
}
