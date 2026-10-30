-- Agenda de medicos: franjas horarias semanales y bloqueos puntuales.
-- Idempotente: se puede correr mas de una vez sobre GestionWEB.
USE GestionWEB;
GO

-- Franja semanal en la que atiende un medico (ej: lunes de 08:00 a 12:00,
-- turnos cada 15 min). Un medico puede tener varias franjas el mismo dia
-- siempre que no se superpongan. Activo = 0 deja la franja cargada pero sin
-- generar turnos (el medico no atiende en ese horario por ahora).
IF OBJECT_ID('dbo.MedicoHorario', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.MedicoHorario (
        IdHorario     INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_MedicoHorario PRIMARY KEY,
        IdMedico      INT        NOT NULL CONSTRAINT FK_MedicoHorario_Medico REFERENCES dbo.Medico(IdMedico),
        DiaSemana     TINYINT    NOT NULL,  -- 1 = Lunes ... 7 = Domingo
        HoraDesde     TIME(0)    NOT NULL,
        HoraHasta     TIME(0)    NOT NULL,
        DuracionTurno INT        NOT NULL,  -- minutos
        Activo        BIT        NOT NULL CONSTRAINT DF_MedicoHorario_Activo DEFAULT (1),
        DVH           VARCHAR(255) NULL,
        CONSTRAINT CK_MedicoHorario_Dia      CHECK (DiaSemana BETWEEN 1 AND 7),
        CONSTRAINT CK_MedicoHorario_Rango    CHECK (HoraHasta > HoraDesde),
        CONSTRAINT CK_MedicoHorario_Duracion CHECK (DuracionTurno BETWEEN 5 AND 240)
    );

    CREATE INDEX IX_MedicoHorario_Medico ON dbo.MedicoHorario (IdMedico, DiaSemana);
END
GO

-- Excepciones a la agenda semanal: un dia completo (HoraDesde/HoraHasta NULL)
-- o un rango de horas de una fecha puntual en el que el medico no atiende.
IF OBJECT_ID('dbo.MedicoBloqueo', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.MedicoBloqueo (
        IdBloqueo  INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_MedicoBloqueo PRIMARY KEY,
        IdMedico   INT          NOT NULL CONSTRAINT FK_MedicoBloqueo_Medico REFERENCES dbo.Medico(IdMedico),
        Fecha      DATE         NOT NULL,
        HoraDesde  TIME(0)      NULL,
        HoraHasta  TIME(0)      NULL,
        Motivo     VARCHAR(100) NULL,
        DVH        VARCHAR(255) NULL,
        CONSTRAINT CK_MedicoBloqueo_Rango CHECK (
            (HoraDesde IS NULL AND HoraHasta IS NULL) OR
            (HoraDesde IS NOT NULL AND HoraHasta IS NOT NULL AND HoraHasta > HoraDesde))
    );

    CREATE INDEX IX_MedicoBloqueo_Medico ON dbo.MedicoBloqueo (IdMedico, Fecha);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_Especialidad_Nombre')
    CREATE UNIQUE INDEX UQ_Especialidad_Nombre ON dbo.Especialidad (NombreEspecialidad);
GO

-- Especialidades iniciales (las mismas que mostraba Turnos.aspx).
INSERT INTO dbo.Especialidad (NombreEspecialidad)
SELECT v.Nombre
FROM (VALUES ('Cardiologia'), ('Clinica Medica'), ('Dermatologia'), ('Pediatria'), ('Traumatologia')) v(Nombre)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Especialidad e WHERE e.NombreEspecialidad = v.Nombre);
GO
