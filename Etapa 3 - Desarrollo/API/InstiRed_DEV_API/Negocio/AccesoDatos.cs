using System.Data;
using System.Text.Json;
using MySqlConnector;

namespace Negocio
{
    public class AccesoDatos
    {
        private MySqlConnection conexion;
        private MySqlCommand comando;
        private MySqlDataReader? lector;
        private MySqlTransaction? transaccion;

        public MySqlDataReader? Lector
        {
            get
            {
                return lector;
            }
        }

        public long UltimoIdInsertado
        {
            get
            {
                return comando.LastInsertedId;
            }
        }

        public AccesoDatos()
        {
            string rutaConfiguracion = Path.Combine(AppContext.BaseDirectory, "appsettings.Local.json");
            if (!File.Exists(rutaConfiguracion))
            {
                throw new InvalidOperationException("Falta el archivo de configuración appsettings.Local.json.");
            }

            string cadenaConexion;
            try
            {
                string contenido = File.ReadAllText(rutaConfiguracion);
                using (JsonDocument document = JsonDocument.Parse(contenido))
                {
                    if (document.RootElement.ValueKind != JsonValueKind.Object ||
                        !document.RootElement.TryGetProperty("MySql", out JsonElement propiedad) ||
                        propiedad.ValueKind != JsonValueKind.String)
                    {
                        throw new InvalidOperationException("Falta configurar la conexión MySql.");
                    }

                    string? valor = propiedad.GetString();
                    if (string.IsNullOrWhiteSpace(valor))
                    {
                        throw new InvalidOperationException("Falta configurar la conexión MySql.");
                    }
                    cadenaConexion = valor;
                }
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("El archivo appsettings.Local.json no tiene un formato JSON válido.", ex);
            }

            MySqlConnectionStringBuilder configuracion = new MySqlConnectionStringBuilder(cadenaConexion);
            // Un UPDATE devuelve las filas encontradas, aunque sus valores no cambien.
            configuracion.UseAffectedRows = false;
            conexion = new MySqlConnection(configuracion.ConnectionString);
            comando = new MySqlCommand();
            comando.Connection = conexion;
        }

        public void setearConsulta(string consulta)
        {
            if (lector != null)
            {
                lector.Dispose();
                lector = null;
            }

            comando.Parameters.Clear();
            comando.CommandText = consulta;
        }

        public void ejecutarLectura()
        {
            if (conexion.State == ConnectionState.Closed)
            {
                conexion.Open();
            }
            lector = comando.ExecuteReader();
        }

        public int ejecutarAccion()
        {
            if (conexion.State == ConnectionState.Closed)
            {
                conexion.Open();
            }
            return comando.ExecuteNonQuery();
        }

        public void iniciarTransaccion()
        {
            if (transaccion != null)
            {
                throw new InvalidOperationException("Ya hay una transacción iniciada.");
            }
            if (conexion.State == ConnectionState.Closed)
            {
                conexion.Open();
            }
            transaccion = conexion.BeginTransaction();
            comando.Transaction = transaccion;
        }

        public void confirmarTransaccion()
        {
            if (transaccion == null)
            {
                throw new InvalidOperationException("No hay una transacción iniciada.");
            }
            if (lector != null)
            {
                lector.Dispose();
                lector = null;
            }
            transaccion.Commit();
            transaccion.Dispose();
            transaccion = null;
            comando.Transaction = null;
        }

        public void setearParametro(string nombre, object? valor)
        {
            if (valor == null)
            {
                valor = DBNull.Value;
            }
            comando.Parameters.AddWithValue(nombre, valor);
        }

        public void cerrarConexion()
        {
            try
            {
                if (lector != null)
                {
                    lector.Dispose();
                    lector = null;
                }
            }
            finally
            {
                try
                {
                    // Si no se confirmó, Dispose revierte la transacción pendiente.
                    if (transaccion != null)
                    {
                        transaccion.Dispose();
                        transaccion = null;
                    }
                }
                finally
                {
                    try
                    {
                        comando.Dispose();
                    }
                    finally
                    {
                        conexion.Dispose();
                    }
                }
            }
        }
    }
}
