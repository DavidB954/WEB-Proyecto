USE GestionWEB;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_WARNINGS ON;
SET ANSI_PADDING ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET ARITHABORT ON;
GO

DECLARE @HashPassword123 VARCHAR(64) = 'b7e66a201bd19cea5b19974524dc6cc2bd7eec57bbd2f19eaab3095a04043111';

DECLARE @Medicos TABLE (Email VARCHAR(30) PRIMARY KEY, Nombre VARCHAR(30), Apellido VARCHAR(30), DNI VARCHAR(10), Matricula INT);
INSERT INTO @Medicos VALUES
    ('lf@gmail.com', 'Laura',    'Fernandez', '27100101', 10101),
    ('mg@gmail.com', 'Martin',   'Gomez',     '25100102', 10102),
    ('st@gmail.com', 'Sofia',    'Torres',    '30100103', 10103),
    ('pr@gmail.com', 'Pablo',    'Romero',    '28100104', 10104),
    ('vc@gmail.com', 'Valeria',  'Castro',    '31100105', 10105),
    ('ds@gmail.com', 'Diego',    'Suarez',    '26100106', 10106);

DECLARE @MedicoEspecialidades TABLE (Email VARCHAR(30), Especialidad VARCHAR(30));
INSERT INTO @MedicoEspecialidades VALUES
    ('lf@gmail.com', 'Clinica Medica'),
    ('mg@gmail.com', 'Cardiologia'),
    ('st@gmail.com', 'Dermatologia'),
    ('pr@gmail.com', 'Pediatria'),
    ('pr@gmail.com', 'Clinica Medica'),
    ('vc@gmail.com', 'Traumatologia'),
    ('ds@gmail.com', 'Cardiologia'),
    ('ds@gmail.com', 'Clinica Medica');

DECLARE @Franjas TABLE (Email VARCHAR(30), Dias VARCHAR(20), Desde TIME(0), Hasta TIME(0), Duracion INT);
INSERT INTO @Franjas VALUES
    ('lf@gmail.com', '1,2,3,4,5,6,7', '08:00', '20:00', 30),
    ('mg@gmail.com', '1,3,5',         '08:00', '12:00', 30),
    ('st@gmail.com', '2,4',           '14:00', '18:00', 20),
    ('pr@gmail.com', '1,2,3,4,5',     '09:00', '13:00', 15),
    ('vc@gmail.com', '1,3',           '15:00', '19:00', 30),
    ('ds@gmail.com', '2,4',           '08:00', '12:00', 30);

DECLARE @Pacientes TABLE (Email VARCHAR(30) PRIMARY KEY, Nombre VARCHAR(30), Apellido VARCHAR(30), DNI VARCHAR(10), FechaNacimiento DATE, Telefono VARCHAR(20));
INSERT INTO @Pacientes VALUES
    ('al@gmail.com', 'Ana',      'Lopez',   '22200201', '1974-03-12', '11 5550-0201'),
    ('md@gmail.com', 'Marcos',   'Diaz',    '33200202', '1988-07-25', '11 5550-0202'),
    ('lh@gmail.com', 'Lucia',    'Herrera', '38200203', '1995-11-03', '11 5550-0203'),
    ('ta@gmail.com', 'Tomas',    'Acosta',  '55200204', '2016-05-18', '11 5550-0204'),
    ('cv@gmail.com', 'Camila',   'Vega',    '43200205', '2001-09-30', '11 5550-0205'),
    ('fm@gmail.com', 'Federico', 'Molina',  '18200206', '1967-01-22', '11 5550-0206'),
    ('js@gmail.com', 'Julieta',  'Sosa',    '53200207', '2014-02-14', '11 5550-0207'),
    ('nb@gmail.com', 'Nicolas',  'Benitez', '29200208', '1980-12-08', '11 5550-0208');

DECLARE @Turnos TABLE (Clave INT PRIMARY KEY, Paciente VARCHAR(30), Medico VARCHAR(30), Semana INT NULL, Dia INT NULL, Hora VARCHAR(5), Estado INT, FechaHora DATETIME NULL);
INSERT INTO @Turnos (Clave, Paciente, Medico, Semana, Dia, Hora, Estado) VALUES
    ( 1, 'al@gmail.com', 'lf@gmail.com',  -60, 1, '09:00', 3),
    ( 2, 'al@gmail.com', 'mg@gmail.com',  -56, 1, '09:00', 3),
    ( 3, 'al@gmail.com', 'mg@gmail.com',  -44, 3, '10:00', 3),
    ( 4, 'al@gmail.com', 'st@gmail.com',  -36, 2, '15:00', 3),
    ( 5, 'al@gmail.com', 'vc@gmail.com',  -30, 3, '16:00', 3),
    ( 6, 'al@gmail.com', 'vc@gmail.com',  -20, 1, '15:30', 3),
    ( 7, 'al@gmail.com', 'st@gmail.com',  -16, 2, '14:20', 4),
    ( 8, 'al@gmail.com', 'mg@gmail.com',  -12, 5, '08:30', 3),
    ( 9, 'al@gmail.com', 'lf@gmail.com',   -9, 3, '10:30', 2),
    (10, 'al@gmail.com', 'lf@gmail.com',   -6, 4, '11:00', 3),
    (11, 'al@gmail.com', 'lf@gmail.com',   -2, 2, '10:00', 3),
    (12, 'al@gmail.com', 'lf@gmail.com', NULL, NULL, '09:30', 1),
    (13, 'al@gmail.com', 'mg@gmail.com',    2, 1, '09:30', 1),
    (14, 'al@gmail.com', 'st@gmail.com',    3, 4, '14:40', 1),
    (15, 'al@gmail.com', 'vc@gmail.com',    5, 3, '15:30', 1),
    (16, 'md@gmail.com', 'ds@gmail.com',  -10, 2, '08:30', 3),
    (17, 'md@gmail.com', 'lf@gmail.com',   -3, 1, '12:00', 3),
    (18, 'md@gmail.com', 'lf@gmail.com', NULL, NULL, '11:30', 1),
    (19, 'md@gmail.com', 'mg@gmail.com',    1, 3, '08:00', 1),
    (20, 'lh@gmail.com', 'st@gmail.com',   -8, 4, '14:00', 3),
    (21, 'lh@gmail.com', 'st@gmail.com',   -1, 2, '15:20', 4),
    (22, 'lh@gmail.com', 'lf@gmail.com', NULL, NULL, '17:00', 1),
    (23, 'lh@gmail.com', 'st@gmail.com',    2, 4, '15:00', 1),
    (24, 'ta@gmail.com', 'pr@gmail.com',  -14, 1, '09:30', 3),
    (25, 'ta@gmail.com', 'pr@gmail.com',   -4, 5, '10:00', 3),
    (26, 'ta@gmail.com', 'pr@gmail.com',    1, 2, '09:15', 1),
    (27, 'cv@gmail.com', 'vc@gmail.com',   -5, 3, '15:00', 3),
    (28, 'cv@gmail.com', 'vc@gmail.com',   -2, 1, '16:00', 3),
    (29, 'cv@gmail.com', 'lf@gmail.com',    1, 4, '10:00', 2),
    (30, 'cv@gmail.com', 'vc@gmail.com',    2, 3, '17:00', 1),
    (31, 'fm@gmail.com', 'mg@gmail.com',  -18, 5, '09:00', 3),
    (32, 'fm@gmail.com', 'mg@gmail.com',   -6, 1, '11:00', 3),
    (33, 'fm@gmail.com', 'ds@gmail.com',    1, 4, '09:00', 1),
    (34, 'fm@gmail.com', 'mg@gmail.com',    3, 1, '08:30', 1),
    (35, 'js@gmail.com', 'pr@gmail.com',   -7, 3, '11:00', 3),
    (36, 'js@gmail.com', 'pr@gmail.com',    1, 1, '10:30', 1),
    (37, 'nb@gmail.com', 'ds@gmail.com',   -4, 4, '10:00', 3),
    (38, 'nb@gmail.com', 'lf@gmail.com',   -1, 5, '08:30', 1),
    (39, 'nb@gmail.com', 'st@gmail.com',    4, 2, '16:00', 1);

DECLARE @Atenciones TABLE (ClaveTurno INT PRIMARY KEY, Motivo VARCHAR(255), Diagnostico VARCHAR(500), Observaciones VARCHAR(1000));
INSERT INTO @Atenciones VALUES
    ( 1, 'Control anual de salud.', 'Dislipemia leve. Colesterol LDL 165 mg/dL. Presion arterial 138/88 mmHg.', 'Antecedentes: madre hipertensa, padre con infarto a los 62 anos. No fuma. Alergia a la penicilina referida en la infancia, sin confirmar. Se indica dieta y actividad fisica. Se deriva a cardiologia por los valores de presion.'),
    ( 2, 'Derivada por clinica medica. Palpitaciones ocasionales y presion elevada.', 'Hipertension arterial esencial, estadio 1. ECG en ritmo sinusal, sin alteraciones.', 'Presion arterial 146/92 mmHg. Se solicita ecocardiograma y MAPA de 24 horas. Se inicia tratamiento y se pide registro domiciliario de la presion.'),
    ( 3, 'Control de hipertension con estudios.', 'Hipertension arterial controlada. Ecocardiograma normal, fraccion de eyeccion 62%.', 'Presion arterial 126/80 mmHg. MAPA con buen descenso nocturno. Refiere que no volvieron las palpitaciones. Continua con igual tratamiento.'),
    ( 4, 'Lunar en la espalda que cambio de color.', 'Nevo melanocitico benigno. Dermatoscopia sin criterios de malignidad.', 'Se indica proteccion solar diaria y control anual con mapeo de lunares. Consultar antes si cambia de forma, color o tamano.'),
    ( 5, 'Dolor lumbar de dos semanas, sin traumatismo previo.', 'Lumbalgia mecanica. Sin compromiso neurologico. Radiografia con leve rectificacion lumbar.', 'Se indica reposo relativo y calor local. Se evita el uso prolongado de antiinflamatorios por la hipertension. Se solicitan 10 sesiones de kinesiologia.'),
    ( 6, 'Control de lumbalgia despues de la kinesiologia.', 'Lumbalgia mecanica resuelta.', 'Sin dolor al examen y con movilidad completa. Se indican ejercicios de fortalecimiento del tronco y natacion. Alta traumatologica.'),
    ( 8, 'Control de hipertension. Registros domiciliarios en aumento.', 'Hipertension arterial con control parcial. Colesterol LDL 148 mg/dL.', 'Presion arterial 142/90 mmHg. Se ajusta la dosis de enalapril y se suma estatina. Control de funcion renal e ionograma en 4 semanas: resultados a evaluar por clinica medica.'),
    (10, 'Tos con catarro, fiebre de 38 grados y dolor de garganta desde hace 4 dias.', 'Bronquitis aguda. Saturacion 97%, sin signos de neumonia.', 'Se confirma antecedente de alergia a la penicilina: se indica azitromicina. Abundante hidratacion. Volver a consultar si hay falta de aire o persiste la fiebre a las 72 horas.'),
    (11, 'Control posterior a la bronquitis y lectura de laboratorio.', 'Bronquitis aguda resuelta. Hipertension arterial y dislipemia controladas. Funcion renal e ionograma normales.', 'Presion arterial 124/78 mmHg. Colesterol LDL 112 mg/dL. Glucemia 94 mg/dL. Continua con el tratamiento actual. Se solicita mamografia anual. Vacuna antigripal indicada.'),
    (16, 'Dolor en el pecho al hacer ejercicio.', 'Dolor toracico de origen muscular. ECG y ergometria normales.', 'Se descarta causa cardiaca. Se indica retomar el ejercicio de forma progresiva.'),
    (17, 'Acidez y ardor despues de las comidas.', 'Reflujo gastroesofagico leve.', 'Se indica cenar temprano, reducir cafe y evitar comidas grasas.'),
    (20, 'Granos en la cara que no mejoran.', 'Acne moderado.', 'Se indica limpieza dos veces al dia y protector solar libre de aceites. Control en 3 meses.'),
    (24, 'Fiebre y dolor de garganta. Viene con la madre.', 'Faringitis bacteriana. Hisopado rapido positivo para estreptococo.', 'Peso 32 kg. Sin alergias conocidas. Reposo escolar por 48 horas desde el inicio del antibiotico.'),
    (25, 'Control de salud escolar.', 'Nino sano. Peso y talla en percentil 50.', 'Vacunas al dia. Agudeza visual normal. Se entrega certificado de aptitud fisica.'),
    (27, 'Torcedura de tobillo derecho jugando al voley.', 'Esguince de tobillo derecho grado II. Radiografia sin fractura.', 'Se indica inmovilizacion con bota walker por 2 semanas, reposo, hielo y elevacion.'),
    (28, 'Control de esguince de tobillo.', 'Esguince en resolucion. Edema minimo y buena estabilidad.', 'Se retira la bota. Se indican 10 sesiones de kinesiologia y volver al deporte en 4 semanas.'),
    (31, 'Control cardiologico. Antecedente de colocacion de stent en 2021.', 'Enfermedad coronaria cronica estable.', 'Sin angina. Ergometria sin signos de isquemia. Continua con doble control anual.'),
    (32, 'Control cardiologico y lectura de laboratorio.', 'Enfermedad coronaria cronica estable. Colesterol LDL 68 mg/dL.', 'Objetivo de colesterol alcanzado. Presion arterial 122/76 mmHg. Continua con igual tratamiento.'),
    (35, 'Tos seca y silbidos en el pecho por la noche.', 'Asma leve intermitente.', 'Peso 36 kg. Se ensena a la madre el uso de aerocamara. Evitar humo de tabaco y alfombras.'),
    (37, 'Presion alta en un control laboral.', 'Hipertension arterial de reciente diagnostico.', 'Presion arterial 148/94 mmHg. Se solicita laboratorio completo y se indica reducir la sal.');

DECLARE @Recetas TABLE (Orden INT IDENTITY(1,1) PRIMARY KEY, ClaveTurno INT, Medicamento VARCHAR(30), Indicaciones VARCHAR(255));
INSERT INTO @Recetas (ClaveTurno, Medicamento, Indicaciones) VALUES
    ( 2, 'Enalapril 10 mg', '1 comprimido por la manana, todos los dias.'),
    ( 5, 'Paracetamol 1 g', '1 comprimido cada 8 horas si hay dolor, hasta 5 dias.'),
    ( 5, 'Ciclobenzaprina 10 mg', '1 comprimido por la noche durante 7 dias. Puede dar somnolencia.'),
    ( 8, 'Enalapril 20 mg', '1 comprimido por la manana. Reemplaza al de 10 mg.'),
    ( 8, 'Atorvastatina 20 mg', '1 comprimido por la noche, todos los dias.'),
    (10, 'Azitromicina 500 mg', '1 comprimido por dia durante 3 dias.'),
    (10, 'Paracetamol 500 mg', '1 comprimido cada 8 horas si hay fiebre.'),
    (17, 'Omeprazol 20 mg', '1 capsula en ayunas durante 30 dias.'),
    (20, 'Peroxido de benzoilo 5% gel', 'Aplicar una capa fina por la noche sobre la piel limpia.'),
    (24, 'Amoxicilina 500 mg/5 ml', '7,5 ml cada 12 horas durante 10 dias.'),
    (24, 'Ibuprofeno 4% suspension', '8 ml cada 8 horas si hay fiebre o dolor.'),
    (27, 'Ibuprofeno 600 mg', '1 comprimido cada 8 horas con comida, hasta 5 dias.'),
    (31, 'Aspirina 100 mg', '1 comprimido despues del almuerzo, todos los dias.'),
    (31, 'Atorvastatina 40 mg', '1 comprimido por la noche, todos los dias.'),
    (31, 'Bisoprolol 5 mg', '1 comprimido por la manana, todos los dias.'),
    (35, 'Salbutamol aerosol', '2 disparos con aerocamara si hay tos o silbidos, hasta cada 6 horas.'),
    (37, 'Losartan 50 mg', '1 comprimido por la manana, todos los dias.');

DECLARE @DvvAlmacenado VARCHAR(255) = (SELECT DVV FROM dbo.DigitoVerificadorVertical WHERE NombreTabla = 'Usuario');
DECLARE @DvvActual VARCHAR(255);

SELECT @DvvActual = LOWER(CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', ISNULL(STRING_AGG(CAST(f.Firma AS VARCHAR(MAX)), '') WITHIN GROUP (ORDER BY f.IdUsuario), '')), 2))
FROM (
    SELECT IdUsuario, LOWER(CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', CAST(CONCAT(Nombre, '|', Apellido, '|', Email, '|', HashPassword, '|', DNI, '|', IntentosFallidos, '|', CASE WHEN Activo = 1 THEN '1' ELSE '0' END) AS VARCHAR(MAX))), 2)) AS Firma
    FROM dbo.Usuario
) f;

IF @DvvAlmacenado IS NULL OR @DvvAlmacenado <> @DvvActual
BEGIN
    RAISERROR('El DVV de Usuario ya no coincide con los datos: revisar la integridad antes de cargar la demo. No se cargo nada.', 16, 1);
    RETURN;
END

IF EXISTS (SELECT 1 FROM dbo.Usuario u WHERE u.Email IN (SELECT Email FROM @Medicos UNION SELECT Email FROM @Pacientes)
                                          OR u.DNI IN (SELECT DNI FROM @Medicos UNION SELECT DNI FROM @Pacientes))
   OR EXISTS (SELECT 1 FROM dbo.Medico m WHERE m.Matricula IN (SELECT Matricula FROM @Medicos))
BEGIN
    RAISERROR('Ya existe alguno de los usuarios de la demo (mail, DNI o matricula). No se cargo nada.', 16, 1);
    RETURN;
END

DECLARE @Hoy DATETIME = CAST(CAST(GETDATE() AS DATE) AS DATETIME);
DECLARE @Lunes DATETIME = DATEADD(DAY, -(DATEDIFF(DAY, '19000101', @Hoy) % 7), @Hoy);

UPDATE @Turnos
SET FechaHora = CASE WHEN Semana IS NULL THEN @Hoy ELSE DATEADD(DAY, Semana * 7 + Dia - 1, @Lunes) END + CAST(Hora AS DATETIME);

BEGIN TRANSACTION;

    INSERT INTO dbo.Usuario (Nombre, Apellido, Email, HashPassword, DNI, IntentosFallidos, Activo)
    SELECT Nombre, Apellido, Email, @HashPassword123, DNI, 0, 1 FROM @Medicos
    UNION ALL
    SELECT Nombre, Apellido, Email, @HashPassword123, DNI, 0, 1 FROM @Pacientes;

    UPDATE u
    SET DVH = LOWER(CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', CAST(CONCAT(u.Nombre, '|', u.Apellido, '|', u.Email, '|', u.HashPassword, '|', u.DNI, '|', u.IntentosFallidos, '|1') AS VARCHAR(MAX))), 2))
    FROM dbo.Usuario u
    WHERE u.Email IN (SELECT Email FROM @Medicos UNION SELECT Email FROM @Pacientes);

    INSERT INTO dbo.UsuarioRol (IdUsuario, IdRol)
    SELECT u.IdUsuario, r.IdRol
    FROM @Medicos m
    JOIN dbo.Usuario u ON u.Email = m.Email
    JOIN dbo.Rol r ON r.Nombre = 'MEDICO';

    INSERT INTO dbo.UsuarioRol (IdUsuario, IdRol)
    SELECT u.IdUsuario, r.IdRol
    FROM @Pacientes p
    JOIN dbo.Usuario u ON u.Email = p.Email
    JOIN dbo.Rol r ON r.Nombre = 'PACIENTE';

    INSERT INTO dbo.Medico (IdMedico, Matricula, HorarioDesde, HorarioHasta, DVH)
    SELECT u.IdUsuario, m.Matricula, f.Desde, f.Hasta,
           LOWER(CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', CAST(CONCAT(u.IdUsuario, '|', m.Matricula) AS VARCHAR(MAX))), 2))
    FROM @Medicos m
    JOIN dbo.Usuario u ON u.Email = m.Email
    JOIN @Franjas f ON f.Email = m.Email;

    INSERT INTO dbo.MedicoEspecialidad (IdMedico, IdEspecialidad)
    SELECT u.IdUsuario, e.IdEspecialidad
    FROM @MedicoEspecialidades me
    JOIN dbo.Usuario u ON u.Email = me.Email
    JOIN dbo.Especialidad e ON e.NombreEspecialidad = me.Especialidad;

    INSERT INTO dbo.MedicoHorario (IdMedico, DiaSemana, HoraDesde, HoraHasta, DuracionTurno, Activo)
    SELECT u.IdUsuario, CAST(d.value AS TINYINT), f.Desde, f.Hasta, f.Duracion, 1
    FROM @Franjas f
    JOIN dbo.Usuario u ON u.Email = f.Email
    CROSS APPLY STRING_SPLIT(f.Dias, ',') d;

    UPDATE h
    SET DVH = LOWER(CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', CAST(CONCAT(h.IdHorario, '|', h.IdMedico, '|', h.DiaSemana, '|', CONVERT(VARCHAR(5), h.HoraDesde, 108), '|', CONVERT(VARCHAR(5), h.HoraHasta, 108), '|', h.DuracionTurno, '|1') AS VARCHAR(MAX))), 2))
    FROM dbo.MedicoHorario h
    JOIN dbo.Usuario u ON u.IdUsuario = h.IdMedico
    WHERE u.Email IN (SELECT Email FROM @Medicos);

    INSERT INTO dbo.Paciente (IdPaciente, NumeroHistoriaClinica, FechaNacimiento, Telefono, DVH)
    SELECT u.IdUsuario, x.Hc, p.FechaNacimiento, p.Telefono,
           LOWER(CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', CAST(CONCAT(u.IdUsuario, '|', x.Hc, '|', CONVERT(VARCHAR(10), p.FechaNacimiento, 23), '|', p.Telefono) AS VARCHAR(MAX))), 2))
    FROM @Pacientes p
    JOIN dbo.Usuario u ON u.Email = p.Email
    CROSS APPLY (SELECT 'HC-' + RIGHT('000000' + CAST(u.IdUsuario AS VARCHAR(10)), 6) AS Hc) x;

    DECLARE @TurnosCreados TABLE (Clave INT PRIMARY KEY, IdTurno INT);

    MERGE dbo.Turno AS destino
    USING (
        SELECT t.Clave, up.IdUsuario AS IdPaciente, um.IdUsuario AS IdMedico, t.FechaHora, t.Estado
        FROM @Turnos t
        JOIN dbo.Usuario up ON up.Email = t.Paciente
        JOIN dbo.Usuario um ON um.Email = t.Medico
    ) AS origen
    ON 1 = 0
    WHEN NOT MATCHED THEN
        INSERT (IdPaciente, IdMedico, FechaHora, IdEstadoTurno)
        VALUES (origen.IdPaciente, origen.IdMedico, origen.FechaHora, origen.Estado)
    OUTPUT origen.Clave, inserted.IdTurno INTO @TurnosCreados (Clave, IdTurno);

    UPDATE t
    SET DVH = LOWER(CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', CAST(CONCAT(t.IdTurno, '|', t.IdPaciente, '|', t.IdMedico, '|', CONVERT(VARCHAR(16), t.FechaHora, 120), '|', t.IdEstadoTurno) AS VARCHAR(MAX))), 2))
    FROM dbo.Turno t
    JOIN @TurnosCreados c ON c.IdTurno = t.IdTurno;

    INSERT INTO dbo.AtencionMedica (IdPaciente, IdMedico, IdTurno, FechaHora, MotivoConsulta, Diagnostico, Observaciones)
    SELECT t.IdPaciente, t.IdMedico, t.IdTurno, DATEADD(MINUTE, 12, t.FechaHora), a.Motivo, a.Diagnostico, a.Observaciones
    FROM @Atenciones a
    JOIN @TurnosCreados c ON c.Clave = a.ClaveTurno
    JOIN dbo.Turno t ON t.IdTurno = c.IdTurno;

    UPDATE a
    SET DVH = LOWER(CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', CAST(CONCAT(a.IdAtencion, '|', a.IdPaciente, '|', a.IdMedico, '|', a.IdTurno, '|', CONVERT(VARCHAR(19), a.FechaHora, 120), '|', a.MotivoConsulta, '|', a.Diagnostico, '|', a.Observaciones) AS VARCHAR(MAX))), 2))
    FROM dbo.AtencionMedica a
    JOIN @TurnosCreados c ON c.IdTurno = a.IdTurno;

    INSERT INTO dbo.Receta (IdAtencion, Medicamento, Observaciones)
    SELECT a.IdAtencion, r.Medicamento, r.Indicaciones
    FROM @Recetas r
    JOIN @TurnosCreados c ON c.Clave = r.ClaveTurno
    JOIN dbo.AtencionMedica a ON a.IdTurno = c.IdTurno
    ORDER BY r.Orden;

    UPDATE r
    SET DVH = LOWER(CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', CAST(CONCAT(r.IdReceta, '|', r.IdAtencion, '|', r.Medicamento, '|', r.Observaciones) AS VARCHAR(MAX))), 2))
    FROM dbo.Receta r
    JOIN dbo.AtencionMedica a ON a.IdAtencion = r.IdAtencion
    JOIN @TurnosCreados c ON c.IdTurno = a.IdTurno;

    UPDATE dbo.DigitoVerificadorVertical
    SET DVV = (
            SELECT LOWER(CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', STRING_AGG(CAST(f.Firma AS VARCHAR(MAX)), '') WITHIN GROUP (ORDER BY f.IdUsuario)), 2))
            FROM (
                SELECT IdUsuario, LOWER(CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', CAST(CONCAT(Nombre, '|', Apellido, '|', Email, '|', HashPassword, '|', DNI, '|', IntentosFallidos, '|', CASE WHEN Activo = 1 THEN '1' ELSE '0' END) AS VARCHAR(MAX))), 2)) AS Firma
                FROM dbo.Usuario
            ) f
        ),
        FechaActualizacion = GETDATE()
    WHERE NombreTabla = 'Usuario';

COMMIT TRANSACTION;

PRINT 'Datos de demostracion cargados.';
GO
