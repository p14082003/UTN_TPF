using System;
using System.Web.SessionState;

namespace Presentacion.Auth
{
    [Serializable]
    internal class UsuarioSesion
    {
        public const string Clave = "Usuario";
        public const int Responsable = 1;
        public const int AdministradorInstitucion = 2;
        public const int DevCo = 3;

        public bool Ok { get; set; }
        public int IdUsuario { get; set; }
        public int IdTipoUsuario { get; set; }
        public int? IdResponsable { get; set; }
        public int? IdAlumno { get; set; }
        public int? IdInstitucion { get; set; }
        public int? IdAdministradorInstitucion { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public int? IdPersona { get; set; }

        public static UsuarioSesion Obtener(HttpSessionState sesion)
        {
            var usuario = sesion?[Clave] as UsuarioSesion;
            return usuario != null && usuario.Ok && usuario.IdUsuario > 0 ? usuario : null;
        }

        public static bool TieneRol(HttpSessionState sesion, int rol)
        {
            return Obtener(sesion)?.IdTipoUsuario == rol;
        }
    }
}
