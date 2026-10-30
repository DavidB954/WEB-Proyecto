-- Atencion medica: una sola atencion por turno y ajuste de la grilla de Medicos.aspx.
-- Idempotente: se puede correr mas de una vez sobre GestionWEB.
-- Correr despues de TurnosPacientes.sql.
USE GestionWEB;
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- Un turno se atiende una sola vez: si dos postbacks guardan la misma
-- atencion a la vez, el segundo falla en lugar de duplicarla.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_AtencionMedica_Turno')
    CREATE UNIQUE INDEX UX_AtencionMedica_Turno ON dbo.AtencionMedica (IdTurno);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AtencionMedica_Paciente')
    CREATE INDEX IX_AtencionMedica_Paciente ON dbo.AtencionMedica (IdPaciente, FechaHora);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Receta_Atencion')
    CREATE INDEX IX_Receta_Atencion ON dbo.Receta (IdAtencion);
GO

-- La columna 3 de la grilla de turnos del medico era "Obra Social" (dato de
-- prueba que no existe en la base) y pasa a ser "Historia clinica". Las
-- traducciones se guardan por indice de columna, asi que se borran las viejas
-- para que no se siga mostrando "Obra Social"; hay que volver a traducirla
-- desde FormTraducciones.aspx.
DELETE t
FROM dbo.Traduccion t
JOIN dbo.ClaveTexto c ON c.IdClave = t.IdClave
WHERE c.Pagina = 'WebForm2' AND c.ControlId = 'gvTurnosMedicos_col3' AND c.TextoBase <> 'Historia clinica';

UPDATE dbo.ClaveTexto
SET TextoBase = 'Historia clinica'
WHERE Pagina = 'WebForm2' AND ControlId = 'gvTurnosMedicos_col3' AND TextoBase <> 'Historia clinica';
GO
