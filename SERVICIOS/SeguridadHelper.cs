using BE;
using System.Linq;

namespace SERVICIOS
{
    public static class SeguridadHelper
    {
        public static bool TieneAcceso(BE_Usuario usuario, params string[] rolesPermitidos)
        {
            return usuario != null && rolesPermitidos.Contains(usuario.NombreRol);
        }
    }
}
