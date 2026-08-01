using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE
{
    public enum AccionBitacora
    {

        LOGIN_INTENTO,
        LOGIN_INCORRECTO,
        LOGIN_BLOQUEADO,
        LOGIN_OK,
        LOGOUT,
        USUARIO_ALTA,
        USUARIO_MODIFICACION,
        USUARIO_BAJA,
        USUARIO_RESTAURACION,
        USUARIO_RESTABLECIMIENTO_PASSWORD,
        INTEGRIDAD_ERROR,
        BACKUP_GENERADO,
        BACKUP_RESTAURADO,
        ROL_ALTA,
        ROL_BAJA,
        ROL_ASIGNADO_USUARIO,
        ROL_QUITADO_USUARIO,
        PERMISO_ALTA,
        PERMISO_BAJA,
        MODULO_GENERAL,

        ACCION_INVALIDA
    }
}
