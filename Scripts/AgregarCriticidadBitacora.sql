-- =============================================================================
-- Agrega la columna Criticidad a la tabla Bitacora y actualiza el SP de filtrado.
--
-- IMPORTANTE - orden de ejecución:
--   1) Ejecutar este script completo contra la base de datos.
--   2) Iniciar sesión en la aplicación como WEBMASTER, ir a Seguridad y presionar
--      "Recalcular dígitos": la criticidad ahora forma parte de la fórmula del DVH,
--      por lo que los dígitos guardados quedan desactualizados hasta recalcularlos.
--      (Si no se recalcula, la próxima verificación de integridad va a fallar.)
-- =============================================================================

-- 1. Columna nueva. NOT NULL con default para que el insert de la app nunca grabe null.
ALTER TABLE Bitacora
ADD Criticidad VARCHAR(10) NOT NULL CONSTRAINT DF_Bitacora_Criticidad DEFAULT 'BAJA';
GO

-- 2. Clasifica los eventos ya registrados.
--    El mapeo tiene que coincidir con BE.CriticidadBitacora.Obtener (si se cambia uno, cambiar el otro).
UPDATE Bitacora
SET Criticidad = CASE
    WHEN Accion IN ('LOGIN_BLOQUEADO', 'INTEGRIDAD_ERROR', 'USUARIO_BAJA',
                    'USUARIO_RESTABLECIMIENTO_PASSWORD', 'ROL_BAJA', 'PERMISO_BAJA',
                    'BACKUP_RESTAURADO')
        THEN 'ALTA'
    WHEN Accion IN ('LOGIN_INCORRECTO', 'USUARIO_ALTA', 'USUARIO_MODIFICACION',
                    'USUARIO_RESTAURACION', 'ROL_ALTA', 'ROL_ASIGNADO_USUARIO',
                    'ROL_QUITADO_USUARIO', 'PERMISO_ALTA', 'IDIOMA_ALTA',
                    'IDIOMA_BAJA', 'BACKUP_GENERADO')
        THEN 'MEDIA'
    ELSE 'BAJA' -- LOGIN_INTENTO, LOGIN_OK, LOGOUT, MODULO_GENERAL
END;
GO

-- 3. SP de filtrado: se agrega el parámetro @Criticidad, su condición en el WHERE
--    y la columna Criticidad en el SELECT (la grilla la muestra).
--    Si tu versión actual del SP difiere en algo (por ejemplo en cómo compara la IP),
--    conservá tu lógica y sumale solo esas tres cosas.
ALTER PROCEDURE dbo.FiltrarBitacora
    @Desde      DATETIME    = NULL,
    @Hasta      DATETIME    = NULL,
    @IdUsuario  INT         = NULL,
    @Modulo     VARCHAR(30) = NULL,
    @IP         VARCHAR(45) = NULL,
    @Criticidad VARCHAR(10) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT IdBitacora,
           IdUsuario,
           FechaHora,
           Accion,
           Criticidad,
           Modulo,
           DireccionIP AS IP,
           Descripcion,
           NombreMaquina
    FROM Bitacora
    WHERE (@Desde      IS NULL OR FechaHora >= @Desde)
      AND (@Hasta      IS NULL OR FechaHora <= @Hasta)
      AND (@IdUsuario  IS NULL OR IdUsuario = @IdUsuario)
      AND (@Modulo     IS NULL OR Modulo = @Modulo)
      AND (@IP         IS NULL OR DireccionIP = @IP)
      AND (@Criticidad IS NULL OR Criticidad = @Criticidad);
END
GO
