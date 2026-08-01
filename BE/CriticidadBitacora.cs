using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE
{
    public static class CriticidadBitacora
    {
        public const string ALTA = "ALTA";
        public const string MEDIA = "MEDIA";
        public const string BAJA = "BAJA";

        public static string Obtener(AccionBitacora accion)
        {
            switch (accion)
            {
                case AccionBitacora.LOGIN_BLOQUEADO:
                case AccionBitacora.INTEGRIDAD_ERROR:
                case AccionBitacora.USUARIO_BAJA:
                case AccionBitacora.USUARIO_RESTABLECIMIENTO_PASSWORD:
                case AccionBitacora.ROL_BAJA:
                case AccionBitacora.PERMISO_BAJA:
                case AccionBitacora.BACKUP_RESTAURADO:
                    return ALTA;

                case AccionBitacora.LOGIN_INCORRECTO:
                case AccionBitacora.USUARIO_ALTA:
                case AccionBitacora.USUARIO_MODIFICACION:
                case AccionBitacora.USUARIO_RESTAURACION:
                case AccionBitacora.ROL_ALTA:
                case AccionBitacora.ROL_ASIGNADO_USUARIO:
                case AccionBitacora.ROL_QUITADO_USUARIO:
                case AccionBitacora.PERMISO_ALTA:
                case AccionBitacora.BACKUP_GENERADO:
                    return MEDIA;

                default:
                    return BAJA;
            }
        }
    }
}
