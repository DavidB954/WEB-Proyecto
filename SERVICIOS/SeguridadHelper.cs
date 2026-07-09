using BE;
using System.Linq;

namespace SERVICIOS
{
    public static class SeguridadHelper
    {
        //Devuelve true solo si hay un usuario logueado y su rol está entre los permitidos para la página.
        public static bool TieneAcceso(BE_Usuario usuario, params string[] rolesPermitidos)
        {
            return usuario != null && rolesPermitidos.Contains(usuario.NombreRol);
        }
    }
}
