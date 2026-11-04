using Dominio;

namespace Negocio
{
    public class UsuarioNegocio
    {
        public List<Usuario> listar()
        {
            List<Usuario> lista = new List<Usuario>();
            AccesoDatos datos = new AccesoDatos();

            try
            {
                datos.setearConsulta("SELECT id, name, surname, age FROM users");
                datos.ejecutarLectura();
                
                var lector = datos.Lector;
                if (lector == null)
                {
                    throw new InvalidOperationException("No se inicializó el lector de datos.");
                }

                while (lector.Read())
                {
                    lista.Add(new Usuario
                    {
                        Id = (int)lector["id"],
                        Name = (string)lector["name"],
                        Surname = (string)lector["surname"],
                        Age = (int)lector["age"]
                    });
                }
                return lista;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        public void agregar(Usuario nuevo)
        {
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("INSERT INTO users (name, surname, age) VALUES (@name, @surname, @age)");
                datos.setearParametro("@name", nuevo.Name);
                datos.setearParametro("@surname", nuevo.Surname);
                datos.setearParametro("@age", nuevo.Age);
                datos.ejecutarAccion();
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        public void eliminar(int id)
        {
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("DELETE FROM users WHERE id = @id");
                datos.setearParametro("@id", id);
                datos.ejecutarAccion();
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        public void actualizar(Usuario usuario)
        {
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("UPDATE users SET name = @name, surname = @surname, age = @age WHERE id = @id");
                datos.setearParametro("@id", usuario.Id);
                datos.setearParametro("@name", usuario.Name);
                datos.setearParametro("@surname", usuario.Surname);
                datos.setearParametro("@age", usuario.Age);
                datos.ejecutarAccion();
            }
            finally
            {
                datos.cerrarConexion();
            }
        }
    }
}
