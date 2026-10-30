-- Permite borrar usuarios fisicamente (CRUD_Usuarios) conservando el historial.
--  - Bitacora: sus filas conservan el IdUsuario del usuario eliminado (no se
--    pone en NULL porque el DVH de cada fila incluye el IdUsuario).
--  - Turno / AtencionMedica: al borrar un medico se conservan sus turnos
--    atendidos y atenciones, que son la historia clinica de otros pacientes.
-- Los datos del usuario como paciente y la agenda del medico los borra
-- DAL_Usuario.EliminarUsuario dentro de una transaccion.
-- Idempotente: se puede correr mas de una vez.

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Bitacora_Usuario')
    ALTER TABLE dbo.Bitacora DROP CONSTRAINT FK_Bitacora_Usuario;

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Turno_Medico')
    ALTER TABLE dbo.Turno DROP CONSTRAINT FK_Turno_Medico;

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Atencion_Medico')
    ALTER TABLE dbo.AtencionMedica DROP CONSTRAINT FK_Atencion_Medico;
GO

-- Sin las FK, los filtros por usuario/medico necesitan su propio indice.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Bitacora_Usuario' AND object_id = OBJECT_ID('dbo.Bitacora'))
    CREATE INDEX IX_Bitacora_Usuario ON dbo.Bitacora (IdUsuario);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AtencionMedica_Medico' AND object_id = OBJECT_ID('dbo.AtencionMedica'))
    CREATE INDEX IX_AtencionMedica_Medico ON dbo.AtencionMedica (IdMedico);
GO
