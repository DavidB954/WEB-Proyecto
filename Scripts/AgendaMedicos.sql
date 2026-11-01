USE GestionWEB;
GO

IF OBJECT_ID('dbo.MedicoHorario', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.MedicoHorario (
        IdHorario     INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_MedicoHorario PRIMARY KEY,
        IdMedico      INT        NOT NULL CONSTRAINT FK_MedicoHorario_Medico REFERENCES dbo.Medico(IdMedico),
        DiaSemana     TINYINT    NOT NULL,
        HoraDesde     TIME(0)    NOT NULL,
        HoraHasta     TIME(0)    NOT NULL,
        DuracionTurno INT        NOT NULL,
        Activo        BIT        NOT NULL CONSTRAINT DF_MedicoHorario_Activo DEFAULT (1),
        DVH           VARCHAR(255) NULL,
        CONSTRAINT CK_MedicoHorario_Dia      CHECK (DiaSemana BETWEEN 1 AND 7),
        CONSTRAINT CK_MedicoHorario_Rango    CHECK (HoraHasta > HoraDesde),
        CONSTRAINT CK_MedicoHorario_Duracion CHECK (DuracionTurno BETWEEN 5 AND 240)
    );

    CREATE INDEX IX_MedicoHorario_Medico ON dbo.MedicoHorario (IdMedico, DiaSemana);
END
GO

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

INSERT INTO dbo.Especialidad (NombreEspecialidad)
SELECT v.Nombre
FROM (VALUES ('Cardiologia'), ('Clinica Medica'), ('Dermatologia'), ('Pediatria'), ('Traumatologia')) v(Nombre)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Especialidad e WHERE e.NombreEspecialidad = v.Nombre);
GO
