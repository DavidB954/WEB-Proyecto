using System;
using System.Collections.Generic;

namespace BE.Seguridad
{
    // Leaf del Composite: un permiso concreto (ej: ABM_USUARIO, ACCESO_MEDICOS).
    // No tiene hijos, asi que no puede participar de un ciclo.
    public class PermisoComponente : IComponentePermiso
    {
        private readonly BE_Permiso permiso;

        public PermisoComponente(BE_Permiso permiso)
        {
            this.permiso = permiso ?? throw new ArgumentNullException(nameof(permiso));
        }

        public BE_Permiso Permiso => permiso;

        public string Codigo => permiso.Nombre;

        public bool TienePermiso(string codigoPermiso)
        {
            return string.Equals(Codigo, codigoPermiso, StringComparison.OrdinalIgnoreCase);
        }

        public IEnumerable<BE_Permiso> ObtenerPermisosEfectivos()
        {
            yield return permiso;
        }
    }
}
