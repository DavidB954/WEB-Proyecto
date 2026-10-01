using System;
using System.Collections.Generic;
using System.Linq;

namespace BE.Seguridad
{
    // Raiz del arbol Composite para un usuario puntual: agrupa los Roles y/o
    // Permisos que se le asignaron directamente. No es un IComponentePermiso
    // (un usuario no puede "colgar" de otro nodo), solo consume la interfaz.
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

        public IEnumerable<BE_Permiso> ObtenerPermisosEfectivos()
        {
            return asignaciones
                .SelectMany(a => a.ObtenerPermisosEfectivos())
                .GroupBy(p => p.Nombre)
                .Select(g => g.First());
        }

        // Bloquea el caso del enunciado: si el permiso ya llega al usuario
        // (directo o via cualquier rol), no se puede volver a asignar suelto.
        public void AsignarPermiso(PermisoComponente permiso)
        {
            if (permiso == null)
                throw new ArgumentNullException(nameof(permiso));

            if (TienePermiso(permiso.Codigo))
            {
                throw new InvalidOperationException(
                    $"El usuario {usuario.NombreApellido} ya tiene el permiso '{permiso.Codigo}' " +
                    "(directo o a traves de un rol). No se puede asignar de nuevo.");
            }

            asignaciones.Add(permiso);
        }

        // Simetrico: si el rol otorga un permiso que el usuario ya tiene
        // asignado suelto, tambien se rechaza para no duplicar el camino.
        public void AsignarRol(RolComponente rol)
        {
            if (rol == null)
                throw new ArgumentNullException(nameof(rol));

            BE_Permiso duplicado = rol.ObtenerPermisosEfectivos().FirstOrDefault(p => TienePermiso(p.Nombre));

            if (duplicado != null)
            {
                throw new InvalidOperationException(
                    $"No se puede asignar el rol '{rol.Codigo}': el usuario {usuario.NombreApellido} " +
                    $"ya tiene el permiso '{duplicado.Nombre}' asignado (quedaria duplicado por el rol).");
            }

            asignaciones.Add(rol);
        }

        // Usado solo para reconstruir el arbol desde la base, igual que
        // RolComponente.CargarHijo.
        public void CargarAsignacion(IComponentePermiso componente)
        {
            asignaciones.Add(componente ?? throw new ArgumentNullException(nameof(componente)));
        }
    }
}
