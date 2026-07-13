using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE
{
    //Mapeo Accion -> Criticidad que se aplica al momento de registrar el evento; el valor se guarda
    //en la columna Criticidad de la Bitacora. Cambiar este switch solo afecta a los eventos futuros:
    //los ya registrados conservan la criticidad con la que se grabaron (está protegida por el DVH).
    //Debe coincidir con el CASE del script Scripts/AgregarCriticidadBitacora.sql que clasificó las filas históricas.
    public static class CriticidadBitacora
    {
        public const string ALTA = "ALTA";
        public const string MEDIA = "MEDIA";
        public const string BAJA = "BAJA";

        public static string Obtener(AccionBitacora accion)
        {
            switch (accion)
            {
                //Eventos de seguridad o que alteran/eliminan datos de forma sensible.
                case AccionBitacora.LOGIN_BLOQUEADO:
                case AccionBitacora.INTEGRIDAD_ERROR:
                case AccionBitacora.USUARIO_BAJA:
                case AccionBitacora.USUARIO_RESTABLECIMIENTO_PASSWORD:
                case AccionBitacora.ROL_BAJA:
                case AccionBitacora.PERMISO_BAJA:
                case AccionBitacora.BACKUP_RESTAURADO:
                    return ALTA;

                //Cambios de configuración/datos que conviene auditar pero no son incidentes.
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

                //Operatoria normal (LOGIN_INTENTO, LOGIN_OK, LOGOUT, MODULO_GENERAL, etc.).
                default:
                    return BAJA;
            }
        }
    }
}
