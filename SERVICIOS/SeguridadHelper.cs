using BE;
using BE.Seguridad;
using System.Linq;

namespace SERVICIOS
{
    public static class SeguridadHelper
    {
        public static bool TieneAcceso(BE_Usuario usuario, UsuarioComponente permisos, params string[] codigosPermiso)
        {
            if (usuario == null)
            {
                return false;
            }

            if (usuario.EsEmergencia)
            {
                return true;
            }

            return permisos != null && codigosPermiso.Any(permisos.TienePermiso);
        }

        public static bool TieneAccesoPagina(BE_Usuario usuario, UsuarioComponente permisos, string pagina)
        {
            string[] codigos = CatalogoPermisos.DePagina(pagina).Select(p => p.Codigo).ToArray();

            return TieneAcceso(usuario, permisos, codigos);
        }

        public static bool TieneAccesoAccion(BE_Usuario usuario, UsuarioComponente permisos, string codigoAccion)
        {
            PermisoDefinicion definicion = CatalogoPermisos.Buscar(codigoAccion);

            if (definicion == null)
            {
                return TieneAcceso(usuario, permisos, codigoAccion);
            }

            string[] codigos = CatalogoPermisos.DePagina(definicion.Pagina)
                .Where(p => p.Tipo == CatalogoPermisos.TipoPagina)
                .Select(p => p.Codigo)
                .Concat(new[] { definicion.Codigo })
                .ToArray();

            return TieneAcceso(usuario, permisos, codigos);
        }
    }
}
