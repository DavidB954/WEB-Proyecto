-- =============================================================================
-- Agrega la columna IdsVigentes a DigitoVerificadorVertical (la usa DAL_DVV.
-- ObtenerIdsVigentes/ActualizarIdsVigentes, referenciada desde el código pero
-- nunca migrada a la base real: sin esta columna, DetectarBitacoraEliminada
-- tira una excepción "Invalid column name 'IdsVigentes'" apenas se llega a
-- llamarla, y esa excepción termina en el catch genérico de Login.aspx.cs).
--
-- También siembra una base inicial para "Bitacora" y "Usuario" con los IDs
-- que existen HOY, para que filas que ya faltaban de antes (ej. usuarios de
-- prueba borrados a mano durante el desarrollo, mucho antes de que existiera
-- este chequeo) no se reporten para siempre como "eliminación sospechosa".
-- Esto NO toca el DVH de ninguna fila: una manipulación de contenido ya en
-- curso (ej. una Descripcion de Bitácora pisada a mano) se sigue detectando
-- igual, porque esa comparación es por fila y no usa IdsVigentes.
--
-- IMPORTANTE - orden de ejecución:
--   1) Ejecutar este script completo contra la base de datos.
--   2) No hace falta "Recalcular dígitos" para que esto tome efecto: la
--      siembra de IdsVigentes ya queda hecha acá mismo.
-- =============================================================================

ALTER TABLE DigitoVerificadorVertical
ADD IdsVigentes NVARCHAR(MAX) NULL;
GO

UPDATE DigitoVerificadorVertical
SET IdsVigentes = (
    SELECT STRING_AGG(CAST(IdBitacora AS VARCHAR(20)), ',')
    FROM Bitacora
)
WHERE NombreTabla = 'Bitacora';
GO

UPDATE DigitoVerificadorVertical
SET IdsVigentes = (
    SELECT STRING_AGG(CAST(IdUsuario AS VARCHAR(20)), ',')
    FROM Usuario
)
WHERE NombreTabla = 'Usuario';
GO
