SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO
IF OBJECT_ID('dbo.Usuario') IS NOT NULL
BEGIN
    RAISERROR('La base ya tiene las tablas del sistema: este script es solo para una base vacia.', 16, 1);
    SET NOEXEC ON;
END
GO

CREATE TABLE dbo.[AtencionMedica] (
    [IdAtencion] INT IDENTITY(1,1) NOT NULL,
    [IdPaciente] INT NOT NULL,
    [IdMedico] INT NOT NULL,
    [IdTurno] INT NOT NULL,
    [FechaHora] DATETIME NOT NULL,
    [MotivoConsulta] VARCHAR(255) NULL,
    [Diagnostico] VARCHAR(500) NULL,
    [Observaciones] VARCHAR(1000) NULL,
    [DVH] VARCHAR(255) NULL,
    CONSTRAINT [PK__Atencion__223C362ECE9422F0] PRIMARY KEY CLUSTERED ([IdAtencion])
);
GO

CREATE UNIQUE NONCLUSTERED INDEX [UX_AtencionMedica_Turno] ON dbo.[AtencionMedica] ([IdTurno]);
GO

CREATE NONCLUSTERED INDEX [IX_AtencionMedica_Paciente] ON dbo.[AtencionMedica] ([IdPaciente], [FechaHora]);
GO

CREATE NONCLUSTERED INDEX [IX_AtencionMedica_Medico] ON dbo.[AtencionMedica] ([IdMedico]);
GO

CREATE TABLE dbo.[Bitacora] (
    [IdBitacora] INT IDENTITY(1,1) NOT NULL,
    [IdUsuario] INT NULL,
    [FechaHora] DATETIME NOT NULL CONSTRAINT [DF__Bitacora__FechaH__4D5F7D71] DEFAULT (getdate()),
    [Accion] VARCHAR(50) NOT NULL,
    [Descripcion] VARCHAR(1000) NULL,
    [DireccionIP] VARCHAR(50) NULL,
    [DVH] VARCHAR(1000) NULL,
    [Modulo] VARCHAR(50) NULL,
    [NombreMaquina] VARCHAR(100) NULL,
    [Criticidad] VARCHAR(10) NOT NULL CONSTRAINT [DF_Bitacora_Criticidad] DEFAULT ('BAJA'),
    CONSTRAINT [PK__Bitacora__ED3A1B1397A899FB] PRIMARY KEY CLUSTERED ([IdBitacora])
);
GO

CREATE NONCLUSTERED INDEX [IX_Bitacora_Usuario] ON dbo.[Bitacora] ([IdUsuario]);
GO

CREATE TABLE dbo.[ClaveTexto] (
    [IdClave] INT IDENTITY(1,1) NOT NULL,
    [Pagina] VARCHAR(100) NOT NULL,
    [ControlId] VARCHAR(100) NOT NULL,
    [TextoBase] NVARCHAR(300) NOT NULL,
    CONSTRAINT [PK__ClaveTex__003FE7C5C8662D1C] PRIMARY KEY CLUSTERED ([IdClave]),
    CONSTRAINT [UQ_ClaveTexto] UNIQUE NONCLUSTERED ([Pagina], [ControlId])
);
GO

CREATE TABLE dbo.[DigitoVerificadorVertical] (
    [NombreTabla] VARCHAR(50) NOT NULL,
    [DVV] VARCHAR(255) NOT NULL,
    [FechaActualizacion] DATETIME NOT NULL CONSTRAINT [DF__DigitoVer__Fecha__4F47C5E3] DEFAULT (getdate()),
    [IdsVigentes] NVARCHAR(MAX) NULL,
    CONSTRAINT [PK__DigitoVe__5BCFB3895AED9F99] PRIMARY KEY CLUSTERED ([NombreTabla])
);
GO

CREATE TABLE dbo.[Especialidad] (
    [IdEspecialidad] INT IDENTITY(1,1) NOT NULL,
    [NombreEspecialidad] VARCHAR(30) NOT NULL,
    CONSTRAINT [PK__Especial__693FA0AF6E610195] PRIMARY KEY CLUSTERED ([IdEspecialidad]),
    CONSTRAINT [UQ__Especial__448A2514C15422F8] UNIQUE NONCLUSTERED ([NombreEspecialidad])
);
GO

CREATE UNIQUE NONCLUSTERED INDEX [UQ_Especialidad_Nombre] ON dbo.[Especialidad] ([NombreEspecialidad]);
GO

CREATE TABLE dbo.[EstadoIntegridad] (
    [IdEstadoIntegridad] INT IDENTITY(1,1) NOT NULL,
    [NombreTabla] VARCHAR(50) NOT NULL,
    [Fecha] DATETIME NOT NULL CONSTRAINT [DF__EstadoInt__Fecha__503BEA1C] DEFAULT (getdate()),
    [Resultado] VARCHAR(20) NOT NULL,
    [Observacion] VARCHAR(500) NULL,
    CONSTRAINT [PK__EstadoIn__68DEF4C2BEEE561B] PRIMARY KEY CLUSTERED ([IdEstadoIntegridad])
);
GO

CREATE TABLE dbo.[EstadoTurno] (
    [IdEstadoTurno] INT IDENTITY(1,1) NOT NULL,
    [Nombre] VARCHAR(30) NOT NULL,
    CONSTRAINT [PK__EstadoTu__BB9A37DFA92625C9] PRIMARY KEY CLUSTERED ([IdEstadoTurno]),
    CONSTRAINT [UQ__EstadoTu__75E3EFCFB8555058] UNIQUE NONCLUSTERED ([Nombre])
);
GO

CREATE TABLE dbo.[Idioma] (
    [IdIdioma] INT IDENTITY(1,1) NOT NULL,
    [Codigo] VARCHAR(10) NOT NULL,
    [Nombre] VARCHAR(50) NOT NULL,
    [Activo] BIT NOT NULL CONSTRAINT [DF__Idioma__Activo__76619304] DEFAULT ((1)),
    [PorDefecto] BIT NOT NULL CONSTRAINT [DF__Idioma__PorDefec__7755B73D] DEFAULT ((0)),
    CONSTRAINT [PK__Idioma__C867BD364AE5DF31] PRIMARY KEY CLUSTERED ([IdIdioma]),
    CONSTRAINT [UQ_Idioma_Codigo] UNIQUE NONCLUSTERED ([Codigo])
);
GO

CREATE TABLE dbo.[Medico] (
    [IdMedico] INT NOT NULL,
    [Matricula] INT NOT NULL,
    [HorarioDesde] TIME(7) NOT NULL,
    [HorarioHasta] TIME(7) NOT NULL,
    [DVH] VARCHAR(255) NULL,
    CONSTRAINT [PK__Medico__C326E652275D5F9F] PRIMARY KEY CLUSTERED ([IdMedico])
);
GO

CREATE TABLE dbo.[MedicoBloqueo] (
    [IdBloqueo] INT IDENTITY(1,1) NOT NULL,
    [IdMedico] INT NOT NULL,
    [Fecha] DATE NOT NULL,
    [HoraDesde] TIME(0) NULL,
    [HoraHasta] TIME(0) NULL,
    [Motivo] VARCHAR(100) NULL,
    [DVH] VARCHAR(255) NULL,
    CONSTRAINT [PK_MedicoBloqueo] PRIMARY KEY CLUSTERED ([IdBloqueo]),
    CONSTRAINT [CK_MedicoBloqueo_Rango] CHECK ([HoraDesde] IS NULL AND [HoraHasta] IS NULL OR [HoraDesde] IS NOT NULL AND [HoraHasta] IS NOT NULL AND [HoraHasta]>[HoraDesde])
);
GO

CREATE NONCLUSTERED INDEX [IX_MedicoBloqueo_Medico] ON dbo.[MedicoBloqueo] ([IdMedico], [Fecha]);
GO

CREATE TABLE dbo.[MedicoEspecialidad] (
    [IdMedico] INT NOT NULL,
    [IdEspecialidad] INT NOT NULL,
    CONSTRAINT [PK_MedicoEspecialidad] PRIMARY KEY CLUSTERED ([IdMedico], [IdEspecialidad])
);
GO

CREATE TABLE dbo.[MedicoHorario] (
    [IdHorario] INT IDENTITY(1,1) NOT NULL,
    [IdMedico] INT NOT NULL,
    [DiaSemana] TINYINT NOT NULL,
    [HoraDesde] TIME(0) NOT NULL,
    [HoraHasta] TIME(0) NOT NULL,
    [DuracionTurno] INT NOT NULL,
    [Activo] BIT NOT NULL CONSTRAINT [DF_MedicoHorario_Activo] DEFAULT ((1)),
    [DVH] VARCHAR(255) NULL,
    CONSTRAINT [PK_MedicoHorario] PRIMARY KEY CLUSTERED ([IdHorario]),
    CONSTRAINT [CK_MedicoHorario_Dia] CHECK ([DiaSemana]>=(1) AND [DiaSemana]<=(7)),
    CONSTRAINT [CK_MedicoHorario_Rango] CHECK ([HoraHasta]>[HoraDesde]),
    CONSTRAINT [CK_MedicoHorario_Duracion] CHECK ([DuracionTurno]>=(5) AND [DuracionTurno]<=(240))
);
GO

CREATE NONCLUSTERED INDEX [IX_MedicoHorario_Medico] ON dbo.[MedicoHorario] ([IdMedico], [DiaSemana]);
GO

CREATE TABLE dbo.[Paciente] (
    [IdPaciente] INT NOT NULL,
    [NumeroHistoriaClinica] VARCHAR(20) NOT NULL,
    [FechaNacimiento] DATE NOT NULL,
    [Telefono] VARCHAR(20) NULL,
    [DVH] VARCHAR(255) NULL,
    CONSTRAINT [PK__Paciente__C93DB49B3785FC53] PRIMARY KEY CLUSTERED ([IdPaciente]),
    CONSTRAINT [UQ__Paciente__48983771287C8E7A] UNIQUE NONCLUSTERED ([NumeroHistoriaClinica])
);
GO

CREATE TABLE dbo.[Permiso] (
    [IdPermiso] INT IDENTITY(1,1) NOT NULL,
    [Nombre] VARCHAR(30) NOT NULL,
    [Tipo] VARCHAR(20) NOT NULL,
    [DVH] VARCHAR(255) NULL,
    CONSTRAINT [PK__Permiso__0D626EC8BEB6CA62] PRIMARY KEY CLUSTERED ([IdPermiso]),
    CONSTRAINT [UQ_Permiso_Nombre] UNIQUE NONCLUSTERED ([Nombre])
);
GO

CREATE TABLE dbo.[Receta] (
    [IdReceta] INT IDENTITY(1,1) NOT NULL,
    [IdAtencion] INT NOT NULL,
    [Medicamento] VARCHAR(30) NOT NULL,
    [Observaciones] VARCHAR(255) NULL,
    [DVH] VARCHAR(255) NULL,
    CONSTRAINT [PK__Receta__2CEFF15705491EC2] PRIMARY KEY CLUSTERED ([IdReceta])
);
GO

CREATE NONCLUSTERED INDEX [IX_Receta_Atencion] ON dbo.[Receta] ([IdAtencion]);
GO

CREATE TABLE dbo.[Rol] (
    [IdRol] INT IDENTITY(1,1) NOT NULL,
    [Nombre] VARCHAR(30) NOT NULL,
    [DVH] VARCHAR(255) NULL,
    CONSTRAINT [PK__Rol__2A49584CE6984D56] PRIMARY KEY CLUSTERED ([IdRol]),
    CONSTRAINT [UQ_Rol_Nombre] UNIQUE NONCLUSTERED ([Nombre])
);
GO

CREATE TABLE dbo.[Rol_Permiso] (
    [IdRol] INT NOT NULL,
    [IdPermiso] INT NOT NULL,
    CONSTRAINT [PK_RolPermiso] PRIMARY KEY CLUSTERED ([IdRol], [IdPermiso])
);
GO

CREATE TABLE dbo.[Rol_Rol] (
    [idPadre] INT NOT NULL,
    [idHijo] INT NOT NULL,
    CONSTRAINT [PK__Rol_Rol__8FFC4C403792E0F2] PRIMARY KEY CLUSTERED ([idPadre], [idHijo]),
    CONSTRAINT [CK_Rol_Rol_NoAutoreferencia] CHECK ([idPadre]<>[idHijo])
);
GO

CREATE TABLE dbo.[Traduccion] (
    [IdIdioma] INT NOT NULL,
    [IdClave] INT NOT NULL,
    [Texto] NVARCHAR(300) NOT NULL,
    CONSTRAINT [PK_Traduccion] PRIMARY KEY CLUSTERED ([IdIdioma], [IdClave])
);
GO

CREATE TABLE dbo.[Turno] (
    [IdTurno] INT IDENTITY(1,1) NOT NULL,
    [IdPaciente] INT NOT NULL,
    [IdMedico] INT NOT NULL,
    [FechaHora] DATETIME NOT NULL,
    [IdEstadoTurno] INT NOT NULL,
    [DVH] VARCHAR(255) NULL,
    CONSTRAINT [PK__Turno__C1ECF79A663831AC] PRIMARY KEY CLUSTERED ([IdTurno])
);
GO

CREATE UNIQUE NONCLUSTERED INDEX [UX_Turno_Medico_FechaHora_Activo] ON dbo.[Turno] ([IdMedico], [FechaHora]) WHERE ([IdEstadoTurno]<>(2));
GO

CREATE UNIQUE NONCLUSTERED INDEX [UX_Turno_Paciente_FechaHora_Activo] ON dbo.[Turno] ([IdPaciente], [FechaHora]) WHERE ([IdEstadoTurno]<>(2));
GO

CREATE TABLE dbo.[Usuario] (
    [IdUsuario] INT IDENTITY(1,1) NOT NULL,
    [Nombre] VARCHAR(30) NOT NULL,
    [Apellido] VARCHAR(30) NOT NULL,
    [Email] VARCHAR(30) NOT NULL,
    [HashPassword] VARCHAR(255) NOT NULL,
    [DNI] VARCHAR(10) NOT NULL,
    [DVH] VARCHAR(255) NULL,
    [IntentosFallidos] INT NOT NULL CONSTRAINT [DF__Usuario__Intento__51300E55] DEFAULT ((0)),
    [Activo] BIT NOT NULL CONSTRAINT [DF__Usuario__Activo__5224328E] DEFAULT ((1)),
    [IdIdiomaPreferido] INT NULL,
    CONSTRAINT [PK__Usuario__5B65BF979BF406CD] PRIMARY KEY CLUSTERED ([IdUsuario]),
    CONSTRAINT [UQ__Usuario__A9D1053474944382] UNIQUE NONCLUSTERED ([Email]),
    CONSTRAINT [UQ__Usuario__C035B8DD7F5938F0] UNIQUE NONCLUSTERED ([DNI])
);
GO

CREATE TABLE dbo.[UsuarioPermiso] (
    [IdUsuario] INT NOT NULL,
    [IdPermiso] INT NOT NULL,
    CONSTRAINT [PK_UsuarioPermiso] PRIMARY KEY CLUSTERED ([IdUsuario], [IdPermiso])
);
GO

CREATE TABLE dbo.[UsuarioRol] (
    [IdUsuario] INT NOT NULL,
    [IdRol] INT NOT NULL,
    CONSTRAINT [PK_UsuarioRol] PRIMARY KEY CLUSTERED ([IdUsuario], [IdRol])
);
GO

ALTER TABLE dbo.[Receta] ADD CONSTRAINT [FK_Receta_Atencion] FOREIGN KEY ([IdAtencion]) REFERENCES dbo.[AtencionMedica] ([IdAtencion]);
GO

ALTER TABLE dbo.[MedicoEspecialidad] ADD CONSTRAINT [FK_MedicoEspecialidad_Especialidad] FOREIGN KEY ([IdEspecialidad]) REFERENCES dbo.[Especialidad] ([IdEspecialidad]);
GO

ALTER TABLE dbo.[Turno] ADD CONSTRAINT [FK_Turno_Estado] FOREIGN KEY ([IdEstadoTurno]) REFERENCES dbo.[EstadoTurno] ([IdEstadoTurno]);
GO

ALTER TABLE dbo.[MedicoHorario] ADD CONSTRAINT [FK_MedicoHorario_Medico] FOREIGN KEY ([IdMedico]) REFERENCES dbo.[Medico] ([IdMedico]);
GO

ALTER TABLE dbo.[MedicoBloqueo] ADD CONSTRAINT [FK_MedicoBloqueo_Medico] FOREIGN KEY ([IdMedico]) REFERENCES dbo.[Medico] ([IdMedico]);
GO

ALTER TABLE dbo.[MedicoEspecialidad] ADD CONSTRAINT [FK_MedicoEspecialidad_Medico] FOREIGN KEY ([IdMedico]) REFERENCES dbo.[Medico] ([IdMedico]);
GO

ALTER TABLE dbo.[AtencionMedica] ADD CONSTRAINT [FK_Atencion_Paciente] FOREIGN KEY ([IdPaciente]) REFERENCES dbo.[Paciente] ([IdPaciente]);
GO

ALTER TABLE dbo.[Turno] ADD CONSTRAINT [FK_Turno_Paciente] FOREIGN KEY ([IdPaciente]) REFERENCES dbo.[Paciente] ([IdPaciente]);
GO

ALTER TABLE dbo.[UsuarioPermiso] ADD CONSTRAINT [FK_UsuarioPermiso_Permiso] FOREIGN KEY ([IdPermiso]) REFERENCES dbo.[Permiso] ([IdPermiso]);
GO

ALTER TABLE dbo.[Rol_Permiso] ADD CONSTRAINT [FK_RolPermiso_Permiso] FOREIGN KEY ([IdPermiso]) REFERENCES dbo.[Permiso] ([IdPermiso]);
GO

ALTER TABLE dbo.[Rol_Permiso] ADD CONSTRAINT [FK_RolPermiso_Rol] FOREIGN KEY ([IdRol]) REFERENCES dbo.[Rol] ([IdRol]);
GO

ALTER TABLE dbo.[Rol_Rol] ADD CONSTRAINT [FK__Rol_Rol__idHijo__5D95E53A] FOREIGN KEY ([idHijo]) REFERENCES dbo.[Rol] ([IdRol]);
GO

ALTER TABLE dbo.[Rol_Rol] ADD CONSTRAINT [FK__Rol_Rol__idPadre__5E8A0973] FOREIGN KEY ([idPadre]) REFERENCES dbo.[Rol] ([IdRol]);
GO

ALTER TABLE dbo.[UsuarioRol] ADD CONSTRAINT [FK_UsuarioRol_Rol] FOREIGN KEY ([IdRol]) REFERENCES dbo.[Rol] ([IdRol]);
GO

ALTER TABLE dbo.[AtencionMedica] ADD CONSTRAINT [FK_Atencion_Turno] FOREIGN KEY ([IdTurno]) REFERENCES dbo.[Turno] ([IdTurno]);
GO

ALTER TABLE dbo.[UsuarioPermiso] ADD CONSTRAINT [FK_UsuarioPermiso_Usuario] FOREIGN KEY ([IdUsuario]) REFERENCES dbo.[Usuario] ([IdUsuario]);
GO

ALTER TABLE dbo.[Medico] ADD CONSTRAINT [FK_Medico_Usuario] FOREIGN KEY ([IdMedico]) REFERENCES dbo.[Usuario] ([IdUsuario]);
GO

ALTER TABLE dbo.[Paciente] ADD CONSTRAINT [FK_Paciente_Usuario] FOREIGN KEY ([IdPaciente]) REFERENCES dbo.[Usuario] ([IdUsuario]);
GO

ALTER TABLE dbo.[UsuarioRol] ADD CONSTRAINT [FK_UsuarioRol_Usuario] FOREIGN KEY ([IdUsuario]) REFERENCES dbo.[Usuario] ([IdUsuario]);
GO

ALTER TABLE dbo.[Traduccion] ADD CONSTRAINT [FK_Traduccion_Idioma] FOREIGN KEY ([IdIdioma]) REFERENCES dbo.[Idioma] ([IdIdioma]) ON DELETE CASCADE;
GO

ALTER TABLE dbo.[Usuario] ADD CONSTRAINT [FK_Usuario_Idioma] FOREIGN KEY ([IdIdiomaPreferido]) REFERENCES dbo.[Idioma] ([IdIdioma]) ON DELETE SET NULL;
GO

ALTER TABLE dbo.[Traduccion] ADD CONSTRAINT [FK_Traduccion_ClaveTexto] FOREIGN KEY ([IdClave]) REFERENCES dbo.[ClaveTexto] ([IdClave]) ON DELETE CASCADE;
GO

CREATE PROCEDURE [dbo].[FiltrarBitacora]
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

SET IDENTITY_INSERT dbo.[Idioma] ON;
INSERT INTO dbo.[Idioma] ([IdIdioma], [Codigo], [Nombre], [Activo], [PorDefecto]) VALUES (1, N'es', N'Espanol', 1, 1);
INSERT INTO dbo.[Idioma] ([IdIdioma], [Codigo], [Nombre], [Activo], [PorDefecto]) VALUES (2, N'en', N'English', 1, 0);
SET IDENTITY_INSERT dbo.[Idioma] OFF;
GO

SET IDENTITY_INSERT dbo.[EstadoTurno] ON;
INSERT INTO dbo.[EstadoTurno] ([IdEstadoTurno], [Nombre]) VALUES (3, N'Atendido');
INSERT INTO dbo.[EstadoTurno] ([IdEstadoTurno], [Nombre]) VALUES (4, N'Ausente');
INSERT INTO dbo.[EstadoTurno] ([IdEstadoTurno], [Nombre]) VALUES (2, N'Cancelado');
INSERT INTO dbo.[EstadoTurno] ([IdEstadoTurno], [Nombre]) VALUES (1, N'Reservado');
SET IDENTITY_INSERT dbo.[EstadoTurno] OFF;
GO

SET IDENTITY_INSERT dbo.[Especialidad] ON;
INSERT INTO dbo.[Especialidad] ([IdEspecialidad], [NombreEspecialidad]) VALUES (1, N'Cardiologia');
INSERT INTO dbo.[Especialidad] ([IdEspecialidad], [NombreEspecialidad]) VALUES (2, N'Clinica Medica');
INSERT INTO dbo.[Especialidad] ([IdEspecialidad], [NombreEspecialidad]) VALUES (3, N'Dermatologia');
INSERT INTO dbo.[Especialidad] ([IdEspecialidad], [NombreEspecialidad]) VALUES (4, N'Pediatria');
INSERT INTO dbo.[Especialidad] ([IdEspecialidad], [NombreEspecialidad]) VALUES (5, N'Traumatologia');
SET IDENTITY_INSERT dbo.[Especialidad] OFF;
GO

SET IDENTITY_INSERT dbo.[Permiso] ON;
INSERT INTO dbo.[Permiso] ([IdPermiso], [Nombre], [Tipo], [DVH]) VALUES (4, N'ABM_USUARIO', N'PAGINA', N'84527f7b584e4f4dd5490d91c56d117c163f364586fe23d9c800e80ab6c829c7');
INSERT INTO dbo.[Permiso] ([IdPermiso], [Nombre], [Tipo], [DVH]) VALUES (5, N'VER_BITACORA', N'PAGINA', NULL);
INSERT INTO dbo.[Permiso] ([IdPermiso], [Nombre], [Tipo], [DVH]) VALUES (6, N'USUARIO_BAJA', N'ACCION', N'617170405ed63a04f88cb11db226b874484d3e1f6506a804e87145c49e7cb348');
INSERT INTO dbo.[Permiso] ([IdPermiso], [Nombre], [Tipo], [DVH]) VALUES (7, N'ACCESO_RECETAS', N'PAGINA', NULL);
INSERT INTO dbo.[Permiso] ([IdPermiso], [Nombre], [Tipo], [DVH]) VALUES (8, N'ACCESO_TURNOS', N'PAGINA', NULL);
INSERT INTO dbo.[Permiso] ([IdPermiso], [Nombre], [Tipo], [DVH]) VALUES (9, N'ACCESO_SEGURIDAD', N'PAGINA', NULL);
INSERT INTO dbo.[Permiso] ([IdPermiso], [Nombre], [Tipo], [DVH]) VALUES (10, N'GESTION_IDIOMAS', N'PAGINA', NULL);
INSERT INTO dbo.[Permiso] ([IdPermiso], [Nombre], [Tipo], [DVH]) VALUES (11, N'GESTION_TRADUCCIONES', N'PAGINA', NULL);
INSERT INTO dbo.[Permiso] ([IdPermiso], [Nombre], [Tipo], [DVH]) VALUES (12, N'GESTION_ROLES_PERMISOS', N'PAGINA', NULL);
INSERT INTO dbo.[Permiso] ([IdPermiso], [Nombre], [Tipo], [DVH]) VALUES (13, N'ASIGNACION_SEGURIDAD', N'PAGINA', NULL);
INSERT INTO dbo.[Permiso] ([IdPermiso], [Nombre], [Tipo], [DVH]) VALUES (15, N'USUARIO_ALTA', N'ACCION', N'0f5451439638689b0b6d7fcd58317188ebb989a0e3067a3a982181bea0a4802f');
INSERT INTO dbo.[Permiso] ([IdPermiso], [Nombre], [Tipo], [DVH]) VALUES (16, N'USUARIO_MODIFICACION', N'ACCION', N'31868321625cdd564f2196d6684418aa75b3cb5357dd381896dd56bd4ca6dcfc');
INSERT INTO dbo.[Permiso] ([IdPermiso], [Nombre], [Tipo], [DVH]) VALUES (17, N'ACCESO_MEDICOS', N'PAGINA', N'bf3da339e13feb302764ce9c8945f2cc396c475777bcaf8859f390d621bdcc9d');
INSERT INTO dbo.[Permiso] ([IdPermiso], [Nombre], [Tipo], [DVH]) VALUES (18, N'TURNO_ALTA', N'ACCION', N'2b2828c69cab0cf5547c10cbf8e7f9ac09c007194887db8160f820b9944bccd0');
INSERT INTO dbo.[Permiso] ([IdPermiso], [Nombre], [Tipo], [DVH]) VALUES (19, N'TURNO_CANCELAR', N'ACCION', N'c63a9846271acaf3cd3d4e58a7c8d7fe3fd62d867f7532217b9a9c2b99c08e8e');
INSERT INTO dbo.[Permiso] ([IdPermiso], [Nombre], [Tipo], [DVH]) VALUES (20, N'MEDICO_ATENDER', N'ACCION', N'074934b83d2b2d97465dd3d6e10b6228560bc7e4b180749f3c4e9a3867b47158');
INSERT INTO dbo.[Permiso] ([IdPermiso], [Nombre], [Tipo], [DVH]) VALUES (21, N'BACKUP_GENERAR', N'ACCION', N'c32fa02676f66113e9fec727bb26723fa1db3f32182c6f1f6ef0870f66566011');
INSERT INTO dbo.[Permiso] ([IdPermiso], [Nombre], [Tipo], [DVH]) VALUES (22, N'BACKUP_RESTAURAR', N'ACCION', N'81fb0137c66bf1d4374a92c2539176a520c014140d419bb459f04ae5508ef57b');
INSERT INTO dbo.[Permiso] ([IdPermiso], [Nombre], [Tipo], [DVH]) VALUES (23, N'DV_RECALCULAR', N'ACCION', N'392df9e1a63248e8705de80864a09390f186b6725da49f082f80d971ba4459c5');
INSERT INTO dbo.[Permiso] ([IdPermiso], [Nombre], [Tipo], [DVH]) VALUES (24, N'IDIOMA_ALTA', N'ACCION', N'a3fd830bec49c6f0033140b0be4f6a69104cf0b81c76d95210a892c0f974d476');
INSERT INTO dbo.[Permiso] ([IdPermiso], [Nombre], [Tipo], [DVH]) VALUES (25, N'IDIOMA_BAJA', N'ACCION', N'235a2a61472acf130d18f73c39a5849916a90ba866e0280e8378f197dade9d1b');
INSERT INTO dbo.[Permiso] ([IdPermiso], [Nombre], [Tipo], [DVH]) VALUES (26, N'TRADUCCION_EDITAR', N'ACCION', N'c66a162b1c3096033d331dcd733bca30407a83f0987f432d0eabfd679eed5c4e');
INSERT INTO dbo.[Permiso] ([IdPermiso], [Nombre], [Tipo], [DVH]) VALUES (27, N'ROL_ALTA', N'ACCION', N'4e1c65768ed61d409ae41401fc963edc114e6c1d67f19dfd0df3b2d9dfa41ddb');
INSERT INTO dbo.[Permiso] ([IdPermiso], [Nombre], [Tipo], [DVH]) VALUES (28, N'ROL_MODIFICACION', N'ACCION', N'e47a7ffa6d6b128b5cb02cfceaf39afcc93433291fd54f23311a2b925bc5dc05');
INSERT INTO dbo.[Permiso] ([IdPermiso], [Nombre], [Tipo], [DVH]) VALUES (29, N'ROL_BAJA', N'ACCION', N'e3d8bcf124f9e4c11640a8290c888f0c545cd673eac531ceee9329fe694d9e36');
INSERT INTO dbo.[Permiso] ([IdPermiso], [Nombre], [Tipo], [DVH]) VALUES (30, N'ROL_COMPONER', N'ACCION', N'aba5970fbb132ef727bb09aa7dc72dd6e850b61f89c9a1a4c3fa4c13b6e7035b');
INSERT INTO dbo.[Permiso] ([IdPermiso], [Nombre], [Tipo], [DVH]) VALUES (31, N'ASIGNAR_ROL_USUARIO', N'ACCION', N'62e55fa24a5042de6c84cffbbc97ac299e091ff74dac7355ef1b8fe93e5713d5');
INSERT INTO dbo.[Permiso] ([IdPermiso], [Nombre], [Tipo], [DVH]) VALUES (32, N'ASIGNAR_PERMISO_USUARIO', N'ACCION', N'432588831e8cad6ceb88f1602f9185fa73917b5baec04301fb3e096fdbd34fa4');
INSERT INTO dbo.[Permiso] ([IdPermiso], [Nombre], [Tipo], [DVH]) VALUES (33, N'BITACORA_EXPORTAR', N'ACCION', N'323e510b1a19920172a76dcacb208fc03ec94c61c551368e8963318d50e1ecb6');
INSERT INTO dbo.[Permiso] ([IdPermiso], [Nombre], [Tipo], [DVH]) VALUES (34, N'GESTION_AGENDA_MEDICOS', N'PAGINA', N'd65c82e369de0708d06a1f81c33bacb9b1ccd802634058aa35d6df741905cdd2');
INSERT INTO dbo.[Permiso] ([IdPermiso], [Nombre], [Tipo], [DVH]) VALUES (35, N'MEDICO_DATOS_EDITAR', N'ACCION', N'3043d78f2e80458f081715b99a412a31d19cffe9b7d62c4ded10ea605d6cb8cd');
INSERT INTO dbo.[Permiso] ([IdPermiso], [Nombre], [Tipo], [DVH]) VALUES (36, N'AGENDA_HORARIO_EDITAR', N'ACCION', N'242e9b8c04e5459cdb3be28e25e2679d9d757d8ee92474dee560cb8deecd9988');
INSERT INTO dbo.[Permiso] ([IdPermiso], [Nombre], [Tipo], [DVH]) VALUES (37, N'AGENDA_BLOQUEO_EDITAR', N'ACCION', N'66269069cdb1615352f09038be6721f182b88c198db23243b18fc0f721b9de31');
INSERT INTO dbo.[Permiso] ([IdPermiso], [Nombre], [Tipo], [DVH]) VALUES (38, N'ESPECIALIDAD_ABM', N'ACCION', N'9feb84c1d3564ac3525aeb146d8601ceb05fea6a0d0bba6710d83c57be5fe523');
INSERT INTO dbo.[Permiso] ([IdPermiso], [Nombre], [Tipo], [DVH]) VALUES (41, N'BITACORA_IMPORTAR', N'ACCION', N'53b5cd7138eed81ea97d523e0df929a147794e7d84146045c9cc2bb58ae3952c');
SET IDENTITY_INSERT dbo.[Permiso] OFF;
GO

SET IDENTITY_INSERT dbo.[Rol] ON;
INSERT INTO dbo.[Rol] ([IdRol], [Nombre], [DVH]) VALUES (1, N'MEDICO', N'b239c0a026651133b9bb6a1f1d6a23b5fe2036ba286ccbf32ec3f59625df5f0c');
INSERT INTO dbo.[Rol] ([IdRol], [Nombre], [DVH]) VALUES (2, N'PACIENTE', N'e27f59695619c2593c36cfb35b94c7a44ce62e938c08275d30c0489d3e8f503c');
INSERT INTO dbo.[Rol] ([IdRol], [Nombre], [DVH]) VALUES (3, N'WEBMASTER', N'7ca8d10082563d99acc5331b760cba33c97e5714deafc48651ea6d452148b867');
INSERT INTO dbo.[Rol] ([IdRol], [Nombre], [DVH]) VALUES (4, N'ADMINISTRADOR', N'a721c6dd3ed4b6ce941111b79241bf09a85d80b914212ac02af58efefec4b9a1');
SET IDENTITY_INSERT dbo.[Rol] OFF;
GO

INSERT INTO dbo.[Rol_Permiso] ([IdRol], [IdPermiso]) VALUES (1, 17);
INSERT INTO dbo.[Rol_Permiso] ([IdRol], [IdPermiso]) VALUES (2, 7);
INSERT INTO dbo.[Rol_Permiso] ([IdRol], [IdPermiso]) VALUES (2, 8);
INSERT INTO dbo.[Rol_Permiso] ([IdRol], [IdPermiso]) VALUES (3, 5);
INSERT INTO dbo.[Rol_Permiso] ([IdRol], [IdPermiso]) VALUES (3, 9);
INSERT INTO dbo.[Rol_Permiso] ([IdRol], [IdPermiso]) VALUES (3, 10);
INSERT INTO dbo.[Rol_Permiso] ([IdRol], [IdPermiso]) VALUES (3, 11);
INSERT INTO dbo.[Rol_Permiso] ([IdRol], [IdPermiso]) VALUES (3, 12);
INSERT INTO dbo.[Rol_Permiso] ([IdRol], [IdPermiso]) VALUES (3, 13);
INSERT INTO dbo.[Rol_Permiso] ([IdRol], [IdPermiso]) VALUES (4, 4);
INSERT INTO dbo.[Rol_Permiso] ([IdRol], [IdPermiso]) VALUES (4, 31);
INSERT INTO dbo.[Rol_Permiso] ([IdRol], [IdPermiso]) VALUES (4, 32);
INSERT INTO dbo.[Rol_Permiso] ([IdRol], [IdPermiso]) VALUES (4, 33);
INSERT INTO dbo.[Rol_Permiso] ([IdRol], [IdPermiso]) VALUES (4, 34);
GO

INSERT INTO dbo.[DigitoVerificadorVertical] ([NombreTabla], [DVV], [FechaActualizacion], [IdsVigentes]) VALUES (N'AtencionMedica', N'', N'2026-05-25T23:21:44.060', NULL);
INSERT INTO dbo.[DigitoVerificadorVertical] ([NombreTabla], [DVV], [FechaActualizacion], [IdsVigentes]) VALUES (N'Bitacora', N'630c7c2261b6f1a35443ac02436bc0dc7c957f6180cf9922d47324e9870ddd38', N'2026-10-30T19:56:10.200', N'37,38,41,42,45,46,47,1042,1043,1044,1045,1046,1047,1048,1049,1050,1051,1052,1053,1054,1055,1056,1057,1058,1059,1060,1061,1062,1063,1064,1065,1066,1067,1068,1069,1070,1071,1072,1073,1074,1075,1076,1077,1078,1079,1080,1081,1082,1083,1084,1085,1086,2083,2084,2085,2086,2087,2088,2089,2090,2091,2092,2093,2094,2095,2096,2097,2098,2099,2100,2101,2102,2103,2104,2105,2106,2107,2108,2109,2110,2111,2112,2113,2114,2115,2116,2117,2118,2119,2120,2121,2122,2123,2124,2125,2126,2127,2128,2129,2130,2131,2132,2133,2134,2135,2136,2137,2138,2139,2140,2141,2142,2143,2144,2145,2146,2147,2148,2149,2150,2151,2152,2153,2154,2155,2156,2157,2158,2159,2160,2161,2162,2163,2164,2165,2166,2167,2168,2169,2170,2171,2172,2173,2174,2175,2176,2177,2178,2179,2180,2181,2182,2183,2184,2185,2186,2187,2188,2189,2190,2191,2192,2193,2194,2195,2196,2197,2198,2199,2200,2201,2202,2203,2204,2205,2206,2207,2208,2209,2210,2211,2212,2213,2214,2215,2216,2217,2218,2219,2220,2221,2222,2223,2224,2225,2226,2227,2228,2229,2230,2231,2232,2233,2234,2235,2236,2237,2238,2239,2240,2241,2242,2243,2244,2245,2246,2247,2248,2249,2250,2251,2252,2253,2254,2255,2256,2257,2258,2259,2260,2261,2262,2263,2264,2265,2266,2267,2268,2269,2270,2271,2272,2273,2274,2275,2276,2277,2278,2279,2280,2281,3261,3262,3263,3264,4261,4262,4263,4264,4265,4266,4267,4268,4269,4270,4271,4272,4273,4274,4275,4276,4277,4278,4279,4280,4281,4282,4283,4284,4285,4286,4287,4288,4289,4290,4291,4292,4293,4294,4295,4296,4297,4298,4299,4300,4301,4302,4303,4304,4305,4306,4307,4308,4309,4310,4311,4312,4313,4314,4315,4316,4317,4318,4319,4320,4321,4322');
INSERT INTO dbo.[DigitoVerificadorVertical] ([NombreTabla], [DVV], [FechaActualizacion], [IdsVigentes]) VALUES (N'Medico', N'', N'2026-05-25T23:21:44.060', NULL);
INSERT INTO dbo.[DigitoVerificadorVertical] ([NombreTabla], [DVV], [FechaActualizacion], [IdsVigentes]) VALUES (N'Paciente', N'', N'2026-05-25T23:21:44.060', NULL);
INSERT INTO dbo.[DigitoVerificadorVertical] ([NombreTabla], [DVV], [FechaActualizacion], [IdsVigentes]) VALUES (N'Permiso', N'', N'2026-05-25T23:21:44.060', NULL);
INSERT INTO dbo.[DigitoVerificadorVertical] ([NombreTabla], [DVV], [FechaActualizacion], [IdsVigentes]) VALUES (N'Receta', N'', N'2026-05-25T23:21:44.060', NULL);
INSERT INTO dbo.[DigitoVerificadorVertical] ([NombreTabla], [DVV], [FechaActualizacion], [IdsVigentes]) VALUES (N'Rol', N'a4da04db6ca76b04fe68178fc0a331e46cbd661c09520230a6825743aaa034fe', N'2026-10-31T19:18:00.170', N'1,2,3,4');
INSERT INTO dbo.[DigitoVerificadorVertical] ([NombreTabla], [DVV], [FechaActualizacion], [IdsVigentes]) VALUES (N'Turno', N'', N'2026-05-25T23:21:44.060', NULL);
INSERT INTO dbo.[DigitoVerificadorVertical] ([NombreTabla], [DVV], [FechaActualizacion], [IdsVigentes]) VALUES (N'Usuario', N'7f91ab4e6f788e858bf3144c9d6a4d2aa070ccc8fd50f2f383dd4c622bb690ce', N'2026-10-21T14:20:13.733', N'7,0,14,19,21,22');
GO

UPDATE dbo.DigitoVerificadorVertical
SET DVV = 'e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855', IdsVigentes = NULL
WHERE NombreTabla IN ('Usuario', 'Bitacora');
GO
SET NOEXEC OFF;
GO
