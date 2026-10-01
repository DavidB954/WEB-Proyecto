using System.Collections.Generic;

namespace BE.Seguridad
{
    public interface IComponentePermiso
    {
        string Codigo { get; }

        bool TienePermiso(string codigoPermiso);

        IEnumerable<BE_Permiso> ObtenerPermisosEfectivos();
    }
}
