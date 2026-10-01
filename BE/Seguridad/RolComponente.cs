using System;
using System.Collections.Generic;
using System.Linq;

namespace BE.Seguridad
{
    // Composite del Composite: un rol agrupa Permisos (leaf) y/o
    // otros Roles (composite), formando un arbol.
    public class RolComponente : IComponentePermiso
    {
        private readonly BE_Rol rol;
        private readonly List<IComponentePermiso> hijos = new List<IComponentePermiso>();

        public RolComponente(BE_Rol rol)
        {
            this.rol = rol ?? throw new ArgumentNullException(nameof(rol));
        }

        public BE_Rol Rol => rol;

        public string Codigo => rol.Nombre;

        public IReadOnlyList<IComponentePermiso> Hijos => hijos;

        public IEnumerable<RolComponente> SubRoles => hijos.OfType<RolComponente>();

        public IEnumerable<PermisoComponente> PermisosDirectos => hijos.OfType<PermisoComponente>();

        // Unico punto de entrada para armar el arbol: si agregar "hijo" cierra
        // un ciclo (hijo ya contiene a este rol en su propia descendencia),
        // se rechaza en vez de guardar una referencia circular.
        public void AgregarHijo(IComponentePermiso hijo)
        {
            if (hijo == null)
                throw new ArgumentNullException(nameof(hijo));

            if (hijo is RolComponente rolHijo && rolHijo.ContieneRol(rol.IdRol))
            {
                throw new InvalidOperationException(
                    $"No se puede agregar el rol '{rolHijo.Codigo}' dentro de '{rol.Nombre}': " +
                    $"generaria una referencia circular (el rol '{rol.Nombre}' ya forma parte de los sub-roles de '{rolHijo.Codigo}').");
            }

            hijos.Add(hijo);
        }

        // Usado solo para reconstruir el arbol desde la base (los datos ya
        // pasaron la validacion de AgregarHijo cuando se guardaron): evita
        // relanzar la comprobacion de ciclos al simplemente leer.
        public void CargarHijo(IComponentePermiso hijo)
        {
            hijos.Add(hijo ?? throw new ArgumentNullException(nameof(hijo)));
        }

        // true si idRol es este rol o aparece en algun sub-rol descendiente.
        public bool ContieneRol(int idRol)
        {
            if (rol.IdRol == idRol)
                return true;

            return hijos.OfType<RolComponente>().Any(hijo => hijo.ContieneRol(idRol));
        }

        public bool TienePermiso(string codigoPermiso)
        {
            return hijos.Any(hijo => hijo.TienePermiso(codigoPermiso));
        }

        public IEnumerable<BE_Permiso> ObtenerPermisosEfectivos()
        {
            return hijos.SelectMany(hijo => hijo.ObtenerPermisosEfectivos());
        }
    }
}
