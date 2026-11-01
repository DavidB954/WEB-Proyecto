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

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Bitacora_Usuario' AND object_id = OBJECT_ID('dbo.Bitacora'))
    CREATE INDEX IX_Bitacora_Usuario ON dbo.Bitacora (IdUsuario);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AtencionMedica_Medico' AND object_id = OBJECT_ID('dbo.AtencionMedica'))
    CREATE INDEX IX_AtencionMedica_Medico ON dbo.AtencionMedica (IdMedico);
GO
