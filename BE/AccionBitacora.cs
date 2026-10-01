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
        ROL_MODIFICACION,
        ROL_BAJA,
        ROL_ASIGNADO_USUARIO,
        ROL_QUITADO_USUARIO,
        PERMISO_ALTA,
        PERMISO_MODIFICACION,
        PERMISO_BAJA,
        PERMISO_ASIGNADO_USUARIO,
        PERMISO_QUITADO_USUARIO,
        ROL_PERMISO_AGREGADO,
        ROL_PERMISO_QUITADO,
        ROL_SUBROL_AGREGADO,
        ROL_SUBROL_QUITADO,
        MODULO_GENERAL,
        IDIOMA_ALTA,
        IDIOMA_BAJA,
        IDIOMA_CAMBIO,
        TRADUCCION_MODIFICACION,

        ACCION_INVALIDA
    }
}
