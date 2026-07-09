using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE
{
    public enum AccionBitacora
    {
        //Utilizamos un enum para poder definir las acciones que se van a registrar en la bitacora. Enum es la mejor opcion para esto porque nos permite tener un conjunto de valores predefinidos y evitar errores de tipeo al registrar las acciones.

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
        IDIOMA_ALTA,
        IDIOMA_BAJA,
        MODULO_GENERAL

    }
}
