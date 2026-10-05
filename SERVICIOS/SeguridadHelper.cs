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

        // Se puede entrar a una pagina teniendo cualquiera de sus permisos del
        // catalogo: el de tipo PAGINA (solo ver) o alguna de sus ACCIONES.
        public static bool TieneAccesoPagina(BE_Usuario usuario, UsuarioComponente permisos, string pagina)
        {
            string[] codigos = CatalogoPermisos.DePagina(pagina).Select(p => p.Codigo).ToArray();

            return TieneAcceso(usuario, permisos, codigos);
        }

        // Una ACCION se permite si el usuario la tiene puntualmente o si tiene el
        // permiso PAGINA de la pagina a la que pertenece (que habilita todo).
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
