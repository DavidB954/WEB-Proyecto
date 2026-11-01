using BE;
using SERVICIOS;
using System.Collections.Generic;

namespace BLL
{
    internal static class CadenasIntegridad
    {
        public static string CadenaUsuario(BE_Usuario usuario)
        {
            string activo = usuario.Activo ? "1" : "0";
            return $"{usuario.Nombre}|{usuario.Apellido}|{usuario.Email}|{usuario.HashPassword}|{usuario.DNI}|{usuario.IntentosFallidos}|{activo}";
        }

        public static string CadenaRol(BE_Rol rol)
        {
            return $"{rol.IdRol}|{rol.Nombre}";
        }

        public static string CadenaBitacora(BE_Bitacora bitacora)
        {
            return $"{bitacora.IdUsuario}|{bitacora.FechaHora}|{bitacora.Accion}|{bitacora.Modulo}|{bitacora.IP}|{bitacora.Descripcion}|{bitacora.NombreMaquina}|{bitacora.Criticidad}";
        }

        public static string CalcularDVV(IEnumerable<string> firmasDeFilas)
        {
            return HashHelper.GenerarHash(string.Concat(firmasDeFilas));
        }
    }
}
