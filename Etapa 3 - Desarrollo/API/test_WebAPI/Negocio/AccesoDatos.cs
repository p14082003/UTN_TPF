using System.Text.Json;
using MySqlConnector;

namespace Negocio
{
    public class AccesoDatos
    {
        private MySqlConnection conexion;
        private MySqlCommand comando;
        private MySqlDataReader? lector;

        public MySqlDataReader? Lector
        {
            get
            {
                return lector;
            }
        }
        public AccesoDatos()
        {
            // Valor de contraseña externalizado para no exponerlo en github
            string localJsonConfig = File.ReadAllText("appsettings.Local.json");
            using (JsonDocument document = JsonDocument.Parse(localJsonConfig))
            {
                string cadenaConexion = document.RootElement.GetProperty("MySql").GetString()
                    ?? throw new InvalidOperationException("Falta configurar MySql.");

                conexion = new MySqlConnection(cadenaConexion);
            }
            comando = new MySqlCommand();
        }

        public void setearConsulta(string consulta)
        {
            comando.CommandText = consulta;
        }

        public void ejecutarLectura()
        {
            comando.Connection = conexion;
            conexion.Open();
            lector = comando.ExecuteReader();
        }

        public void ejecutarAccion()
        {
            comando.Connection = conexion;
            conexion.Open();
            comando.ExecuteNonQuery();
        }

        public void setearParametro(string nombre, object valor)
        {
            comando.Parameters.AddWithValue(nombre, valor);
        }

        public void cerrarConexion()
        {
            if (lector != null)
            {
                lector.Close();
            }
            conexion.Close();
        }
    }
}
