using Negocio.Helpers;
using System.Data;
using Dominio;
using MySqlConnector;

namespace Negocio
{
    public class Ent_PostulacionAdminInstitucionNegocio
    {
        public List<Ent_PostulacionAdminInstitucion> listar()
        {
            List<Ent_PostulacionAdminInstitucion> lista = new List<Ent_PostulacionAdminInstitucion>();
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("SELECT `id`, `fecha`, `codigoSeguimiento`, `idAdministradorInstitucion`, `idEstado`, `nombreInstitucion`, `cuitInstitucion`, `rutaConstanciaFiscal`, `rutaHabilitacion`, `rutaConstanciaLaboral`, `motivoRechazo` FROM `PostulacionesAdminInstitucion` ORDER BY `id`");
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

        public Ent_PostulacionAdminInstitucion? obtenerPorId(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("El campo Id debe ser mayor que cero.");
            }
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("SELECT `id`, `fecha`, `codigoSeguimiento`, `idAdministradorInstitucion`, `idEstado`, `nombreInstitucion`, `cuitInstitucion`, `rutaConstanciaFiscal`, `rutaHabilitacion`, `rutaConstanciaLaboral`, `motivoRechazo` FROM `PostulacionesAdminInstitucion` WHERE `id` = @clave0");
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

        public int agregar(Ent_PostulacionAdminInstitucion nuevo)
        {
            validar(nuevo);
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("INSERT INTO `PostulacionesAdminInstitucion` (`fecha`, `codigoSeguimiento`, `idAdministradorInstitucion`, `idEstado`, `nombreInstitucion`, `cuitInstitucion`, `rutaConstanciaFiscal`, `rutaHabilitacion`, `rutaConstanciaLaboral`, `motivoRechazo`) VALUES (@fecha, @codigoSeguimiento, @idAdministradorInstitucion, @idEstado, @nombreInstitucion, @cuitInstitucion, @rutaConstanciaFiscal, @rutaHabilitacion, @rutaConstanciaLaboral, @motivoRechazo)");
                datos.setearParametro("@fecha", nuevo.Fecha.Date);
                datos.setearParametro("@codigoSeguimiento", nuevo.CodigoSeguimiento);
                datos.setearParametro("@idAdministradorInstitucion", nuevo.IdAdministradorInstitucion);
                datos.setearParametro("@idEstado", nuevo.IdEstado);
                datos.setearParametro("@nombreInstitucion", nuevo.NombreInstitucion);
                datos.setearParametro("@cuitInstitucion", nuevo.CuitInstitucion);
                datos.setearParametro("@rutaConstanciaFiscal", nuevo.RutaConstanciaFiscal);
                datos.setearParametro("@rutaHabilitacion", nuevo.RutaHabilitacion);
                datos.setearParametro("@rutaConstanciaLaboral", nuevo.RutaConstanciaLaboral);
                datos.setearParametro("@motivoRechazo", nuevo.MotivoRechazo);
                datos.ejecutarAccion();
                return Convert.ToInt32(datos.UltimoIdInsertado);
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                {
                    throw new ConstraintException("Ya existe un registro de PostulacionesAdminInstitucion con esos datos únicos.", ex);
                }
                if (ex.Number == 1452)
                {
                    throw new ConstraintException("No se puede guardar el registro de PostulacionesAdminInstitucion porque una de las entidades relacionadas no existe.", ex);
                }
                if (ex.Number == 1451)
                {
                    throw new ConstraintException("No se puede modificar o eliminar el registro de PostulacionesAdminInstitucion porque aparece en otras entidades.", ex);
                }
                throw;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        public void actualizar(Ent_PostulacionAdminInstitucion registro)
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
                datos.setearConsulta("UPDATE `PostulacionesAdminInstitucion` SET `fecha` = @fecha, `codigoSeguimiento` = @codigoSeguimiento, `idAdministradorInstitucion` = @idAdministradorInstitucion, `idEstado` = @idEstado, `nombreInstitucion` = @nombreInstitucion, `cuitInstitucion` = @cuitInstitucion, `rutaConstanciaFiscal` = @rutaConstanciaFiscal, `rutaHabilitacion` = @rutaHabilitacion, `rutaConstanciaLaboral` = @rutaConstanciaLaboral, `motivoRechazo` = @motivoRechazo WHERE `id` = @clave0");
                datos.setearParametro("@fecha", registro.Fecha.Date);
                datos.setearParametro("@codigoSeguimiento", registro.CodigoSeguimiento);
                datos.setearParametro("@idAdministradorInstitucion", registro.IdAdministradorInstitucion);
                datos.setearParametro("@idEstado", registro.IdEstado);
                datos.setearParametro("@nombreInstitucion", registro.NombreInstitucion);
                datos.setearParametro("@cuitInstitucion", registro.CuitInstitucion);
                datos.setearParametro("@rutaConstanciaFiscal", registro.RutaConstanciaFiscal);
                datos.setearParametro("@rutaHabilitacion", registro.RutaHabilitacion);
                datos.setearParametro("@rutaConstanciaLaboral", registro.RutaConstanciaLaboral);
                datos.setearParametro("@motivoRechazo", registro.MotivoRechazo);
                datos.setearParametro("@clave0", id);
                if (datos.ejecutarAccion() == 0)
                {
                    throw new KeyNotFoundException("No se encontró el registro de PostulacionesAdminInstitucion que se desea actualizar.");
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                {
                    throw new ConstraintException("Ya existe un registro de PostulacionesAdminInstitucion con esos datos únicos.", ex);
                }
                if (ex.Number == 1452)
                {
                    throw new ConstraintException("No se puede guardar el registro de PostulacionesAdminInstitucion porque una de las entidades relacionadas no existe.", ex);
                }
                if (ex.Number == 1451)
                {
                    throw new ConstraintException("No se puede modificar o eliminar el registro de PostulacionesAdminInstitucion porque aparece en otras entidades.", ex);
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
                datos.setearConsulta("DELETE FROM `PostulacionesAdminInstitucion` WHERE `id` = @clave0");
                datos.setearParametro("@clave0", id);

                if (datos.ejecutarAccion() == 0)
                {
                    throw new KeyNotFoundException("No se encontró el registro de PostulacionesAdminInstitucion solicitado.");
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                {
                    throw new ConstraintException("Ya existe un registro de PostulacionesAdminInstitucion con esos datos únicos.", ex);
                }
                if (ex.Number == 1452)
                {
                    throw new ConstraintException("No se puede guardar el registro de PostulacionesAdminInstitucion porque una de las entidades relacionadas no existe.", ex);
                }
                if (ex.Number == 1451)
                {
                    throw new ConstraintException("No se puede modificar o eliminar el registro de PostulacionesAdminInstitucion porque aparece en otras entidades.", ex);
                }
                throw;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        private Ent_PostulacionAdminInstitucion leerRegistro(MySqlDataReader lector)
        {
            Ent_PostulacionAdminInstitucion registro = new Ent_PostulacionAdminInstitucion();
            registro.Id = Convert.ToInt32(lector["id"]);
            registro.Fecha = Convert.ToDateTime(lector["fecha"]);
            registro.CodigoSeguimiento = (string)lector["codigoSeguimiento"];
            registro.IdAdministradorInstitucion = Convert.ToInt32(lector["idAdministradorInstitucion"]);
            registro.IdEstado = Convert.ToInt32(lector["idEstado"]);
            registro.NombreInstitucion = (string)lector["nombreInstitucion"];
            registro.CuitInstitucion = (string)lector["cuitInstitucion"];
            registro.RutaConstanciaFiscal = (string)lector["rutaConstanciaFiscal"];
            registro.RutaHabilitacion = (string)lector["rutaHabilitacion"];
            registro.RutaConstanciaLaboral = (string)lector["rutaConstanciaLaboral"];
            if (lector["motivoRechazo"] != DBNull.Value)
            {
                registro.MotivoRechazo = (string)lector["motivoRechazo"];
            }
            return registro;
        }

        private void validar(Ent_PostulacionAdminInstitucion registro)
        {
            if (registro == null)
            {
                throw new ArgumentException("Debe indicar los datos de PostulacionAdminInstitucion.");
            }
            if (registro.Fecha.Year < 1000)
            {
                throw new ArgumentException("El campo Fecha debe contener una fecha válida desde el año 1000.");
            }
            if (string.IsNullOrWhiteSpace(registro.CodigoSeguimiento))
            {
                throw new ArgumentException("El campo CodigoSeguimiento es obligatorio.");
            }
            if (registro.CodigoSeguimiento.Length > 30)
            {
                throw new ArgumentException("El campo CodigoSeguimiento no puede superar los 30 caracteres.");
            }
            if (registro.IdAdministradorInstitucion <= 0)
            {
                throw new ArgumentException("El campo IdAdministradorInstitucion debe ser mayor que cero.");
            }
            if (registro.IdEstado <= 0)
            {
                throw new ArgumentException("El campo IdEstado debe ser mayor que cero.");
            }
            if (string.IsNullOrWhiteSpace(registro.NombreInstitucion))
            {
                throw new ArgumentException("El campo NombreInstitucion es obligatorio.");
            }
            if (registro.NombreInstitucion.Length > 100)
            {
                throw new ArgumentException("El campo NombreInstitucion no puede superar los 100 caracteres.");
            }
            if (string.IsNullOrWhiteSpace(registro.CuitInstitucion))
            {
                throw new ArgumentException("El campo CuitInstitucion es obligatorio.");
            }
            if (registro.CuitInstitucion.Length > 11)
            {
                throw new ArgumentException("El campo CuitInstitucion no puede superar los 11 caracteres.");
            }
            if (string.IsNullOrWhiteSpace(registro.RutaConstanciaFiscal))
            {
                throw new ArgumentException("El campo RutaConstanciaFiscal es obligatorio.");
            }
            if (registro.RutaConstanciaFiscal.Length > 255)
            {
                throw new ArgumentException("El campo RutaConstanciaFiscal no puede superar los 255 caracteres.");
            }
            if (string.IsNullOrWhiteSpace(registro.RutaHabilitacion))
            {
                throw new ArgumentException("El campo RutaHabilitacion es obligatorio.");
            }
            if (registro.RutaHabilitacion.Length > 255)
            {
                throw new ArgumentException("El campo RutaHabilitacion no puede superar los 255 caracteres.");
            }
            if (string.IsNullOrWhiteSpace(registro.RutaConstanciaLaboral))
            {
                throw new ArgumentException("El campo RutaConstanciaLaboral es obligatorio.");
            }
            if (registro.RutaConstanciaLaboral.Length > 255)
            {
                throw new ArgumentException("El campo RutaConstanciaLaboral no puede superar los 255 caracteres.");
            }
            if (registro.MotivoRechazo != null && registro.MotivoRechazo.Length > 255)
            {
                throw new ArgumentException("El campo MotivoRechazo no puede superar los 255 caracteres.");
            }
        }
    }
}
