using System;
using System.Collections.Generic;
using System.Linq;

namespace BE.Seguridad
{
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

        public void AgregarHijo(IComponentePermiso hijo)
        {
            if (hijo == null)
                throw new ArgumentNullException(nameof(hijo));

            if (hijo is RolComponente rolHijo && rolHijo.ContieneRol(rol.IdRol))
            {
                throw new InvalidOperationException(TextoLocalizado.T(
                    "No se puede agregar el rol '{0}' dentro de '{1}': generaria una referencia circular (el rol '{1}' ya forma parte de los sub-roles de '{0}').",
                    rolHijo.Codigo, rol.Nombre));
            }

            hijos.Add(hijo);
        }

        public void CargarHijo(IComponentePermiso hijo)
        {
            hijos.Add(hijo ?? throw new ArgumentNullException(nameof(hijo)));
        }

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
