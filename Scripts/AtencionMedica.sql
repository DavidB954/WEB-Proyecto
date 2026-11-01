USE GestionWEB;
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_AtencionMedica_Turno')
    CREATE UNIQUE INDEX UX_AtencionMedica_Turno ON dbo.AtencionMedica (IdTurno);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AtencionMedica_Paciente')
    CREATE INDEX IX_AtencionMedica_Paciente ON dbo.AtencionMedica (IdPaciente, FechaHora);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Receta_Atencion')
    CREATE INDEX IX_Receta_Atencion ON dbo.Receta (IdAtencion);
GO

DELETE t
FROM dbo.Traduccion t
JOIN dbo.ClaveTexto c ON c.IdClave = t.IdClave
WHERE c.Pagina = 'WebForm2' AND c.ControlId = 'gvTurnosMedicos_col3' AND c.TextoBase <> 'Historia clinica';

UPDATE dbo.ClaveTexto
SET TextoBase = 'Historia clinica'
WHERE Pagina = 'WebForm2' AND ControlId = 'gvTurnosMedicos_col3' AND TextoBase <> 'Historia clinica';
GO
