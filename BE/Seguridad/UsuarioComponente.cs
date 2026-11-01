using System;
using System.Collections.Generic;
using System.Linq;

namespace BE.Seguridad
{
    public class UsuarioComponente
    {
        private readonly BE_Usuario usuario;
        private readonly List<IComponentePermiso> asignaciones = new List<IComponentePermiso>();

        public UsuarioComponente(BE_Usuario usuario)
        {
            this.usuario = usuario ?? throw new ArgumentNullException(nameof(usuario));
        }

        public BE_Usuario Usuario => usuario;

        public IReadOnlyList<IComponentePermiso> Asignaciones => asignaciones;

        public IEnumerable<RolComponente> RolesAsignados => asignaciones.OfType<RolComponente>();

        public IEnumerable<PermisoComponente> PermisosDirectos => asignaciones.OfType<PermisoComponente>();

        public bool TienePermiso(string codigoPermiso)
        {
            return asignaciones.Any(a => a.TienePermiso(codigoPermiso));
        }

        public void AsignarPermiso(PermisoComponente permiso)
        {
            if (permiso == null)
                throw new ArgumentNullException(nameof(permiso));

            if (TienePermiso(permiso.Codigo))
            {
                throw new InvalidOperationException(TextoLocalizado.T(
                    "El usuario {0} ya tiene el permiso '{1}' (directo o a traves de un rol). No se puede asignar de nuevo.",
                    usuario.NombreApellido, permiso.Codigo));
            }

            asignaciones.Add(permiso);
        }

        public void AsignarRol(RolComponente rol)
        {
            if (rol == null)
                throw new ArgumentNullException(nameof(rol));

            BE_Permiso duplicado = rol.ObtenerPermisosEfectivos().FirstOrDefault(p => TienePermiso(p.Nombre));

            if (duplicado != null)
            {
                throw new InvalidOperationException(TextoLocalizado.T(
                    "No se puede asignar el rol '{0}': el usuario {1} ya tiene el permiso '{2}' asignado (quedaria duplicado por el rol).",
                    rol.Codigo, usuario.NombreApellido, duplicado.Nombre));
            }

            asignaciones.Add(rol);
        }

        public void CargarAsignacion(IComponentePermiso componente)
        {
            asignaciones.Add(componente ?? throw new ArgumentNullException(nameof(componente)));
        }
    }
}
