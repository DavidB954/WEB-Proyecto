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
                case AccionBitacora.IDIOMA_BAJA:
                    return ALTA;

                case AccionBitacora.LOGIN_INCORRECTO:
                case AccionBitacora.USUARIO_ALTA:
                case AccionBitacora.USUARIO_MODIFICACION:
                case AccionBitacora.USUARIO_RESTAURACION:
                case AccionBitacora.ROL_ALTA:
                case AccionBitacora.ROL_MODIFICACION:
                case AccionBitacora.ROL_ASIGNADO_USUARIO:
                case AccionBitacora.ROL_QUITADO_USUARIO:
                case AccionBitacora.PERMISO_ALTA:
                case AccionBitacora.PERMISO_MODIFICACION:
                case AccionBitacora.PERMISO_ASIGNADO_USUARIO:
                case AccionBitacora.PERMISO_QUITADO_USUARIO:
                case AccionBitacora.ROL_PERMISO_AGREGADO:
                case AccionBitacora.ROL_PERMISO_QUITADO:
                case AccionBitacora.ROL_SUBROL_AGREGADO:
                case AccionBitacora.ROL_SUBROL_QUITADO:
                case AccionBitacora.BACKUP_GENERADO:
                case AccionBitacora.IDIOMA_ALTA:
                case AccionBitacora.TRADUCCION_MODIFICACION:
                    return MEDIA;

                default:
                    return BAJA;
            }
        }
    }
}
