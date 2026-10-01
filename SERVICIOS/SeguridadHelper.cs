using BE;
using BE.Seguridad;
using System.Linq;

namespace SERVICIOS
{
    public static class SeguridadHelper
    {
        // IdUsuario == 0 identifica al login de emergencia (BLL_Usuario.LoginEmergencia):
        // no tiene fila en la base ni arbol de permisos posible, asi que se le da acceso
        // total. Es la misma logica de "romper vidrio" que ya usaba el chequeo por rol.
        public static bool TieneAcceso(BE_Usuario usuario, UsuarioComponente permisos, params string[] codigosPermiso)
        {
            if (usuario == null)
            {
                return false;
            }

            if (usuario.IdUsuario == 0)
            {
                return true;
            }

            return permisos != null && codigosPermiso.Any(permisos.TienePermiso);
        }
    }
}
