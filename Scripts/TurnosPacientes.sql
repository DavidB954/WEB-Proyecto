USE GestionWEB;
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

SET IDENTITY_INSERT dbo.EstadoTurno ON;

INSERT INTO dbo.EstadoTurno (IdEstadoTurno, Nombre)
SELECT v.Id, v.Nombre
FROM (VALUES (1, 'Reservado'), (2, 'Cancelado'), (3, 'Atendido'), (4, 'Ausente')) v(Id, Nombre)
WHERE NOT EXISTS (SELECT 1 FROM dbo.EstadoTurno e WHERE e.IdEstadoTurno = v.Id);

SET IDENTITY_INSERT dbo.EstadoTurno OFF;
GO

IF EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = 'UQ_Turno_Medico_FechaHora')
    ALTER TABLE dbo.Turno DROP CONSTRAINT UQ_Turno_Medico_FechaHora;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_Turno_Medico_FechaHora_Activo')
    CREATE UNIQUE INDEX UX_Turno_Medico_FechaHora_Activo
        ON dbo.Turno (IdMedico, FechaHora)
        WHERE IdEstadoTurno <> 2;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_Turno_Paciente_FechaHora_Activo')
    CREATE UNIQUE INDEX UX_Turno_Paciente_FechaHora_Activo
        ON dbo.Turno (IdPaciente, FechaHora)
        WHERE IdEstadoTurno <> 2;
GO
