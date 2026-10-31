-- Permisos de cada rol del sistema segun su funcion.
-- Idempotente: deja Rol_Permiso de estos cuatro roles exactamente como dice la
-- tabla de abajo (agrega lo que falta y quita lo que sobra). No toca otros
-- roles ni los permisos asignados directamente a usuarios (UsuarioPermiso).
--
-- Se usan permisos de tipo PAGINA: cada uno habilita la pagina completa con
-- todas sus acciones (ver SeguridadHelper.TieneAccesoAccion), asi que no hace
-- falta asignar las ACCIONES por separado.
--
-- Corre por fuera de la aplicacion, asi que no queda en la Bitacora. Los
-- usuarios conectados ven el cambio cuando vuelven a iniciar sesion (los
-- permisos se cargan en la sesion al loguearse).
USE GestionWEB;
GO

DECLARE @Matriz TABLE (Rol VARCHAR(30), Permiso VARCHAR(30));

INSERT INTO @Matriz (Rol, Permiso) VALUES
    -- Paciente: pide/cancela sus turnos y consulta sus recetas.
    ('PACIENTE',      'ACCESO_TURNOS'),
    ('PACIENTE',      'ACCESO_RECETAS'),

    -- Medico: atiende sus turnos, registra atenciones y recetas.
    ('MEDICO',        'ACCESO_MEDICOS'),

    -- Administrativo: usuarios, que rol tiene cada uno, agenda de medicos y
    -- consulta de bitacora.
    ('ADMINISTRADOR', 'ABM_USUARIO'),
    ('ADMINISTRADOR', 'ASIGNACION_SEGURIDAD'),
    ('ADMINISTRADOR', 'GESTION_AGENDA_MEDICOS'),
    ('ADMINISTRADOR', 'VER_BITACORA'),

    -- Webmaster: seguridad tecnica (backup, integridad, bitacora), diseno de
    -- roles y permisos, e idiomas/traducciones.
    ('WEBMASTER',     'ACCESO_SEGURIDAD'),
    ('WEBMASTER',     'VER_BITACORA'),
    ('WEBMASTER',     'GESTION_ROLES_PERMISOS'),
    ('WEBMASTER',     'ASIGNACION_SEGURIDAD'),
    ('WEBMASTER',     'GESTION_IDIOMAS'),
    ('WEBMASTER',     'GESTION_TRADUCCIONES');

-- Si falta un rol o un permiso se corta antes de tocar nada.
IF EXISTS (SELECT 1 FROM @Matriz m WHERE NOT EXISTS (SELECT 1 FROM dbo.Rol r WHERE r.Nombre = m.Rol))
   OR EXISTS (SELECT 1 FROM @Matriz m WHERE NOT EXISTS (SELECT 1 FROM dbo.Permiso p WHERE p.Nombre = m.Permiso))
BEGIN
    RAISERROR('Falta algun rol o permiso de la matriz. Sincroniza el catalogo de permisos desde GestionRolesPermisos.aspx.', 16, 1);
    RETURN;
END

BEGIN TRAN;

DELETE rp
FROM dbo.Rol_Permiso rp
JOIN dbo.Rol r     ON r.IdRol = rp.IdRol
JOIN dbo.Permiso p ON p.IdPermiso = rp.IdPermiso
WHERE r.Nombre IN (SELECT DISTINCT Rol FROM @Matriz)
  AND NOT EXISTS (SELECT 1 FROM @Matriz m WHERE m.Rol = r.Nombre AND m.Permiso = p.Nombre);

PRINT CAST(@@ROWCOUNT AS VARCHAR) + ' permiso(s) quitado(s).';

INSERT INTO dbo.Rol_Permiso (IdRol, IdPermiso)
SELECT r.IdRol, p.IdPermiso
FROM @Matriz m
JOIN dbo.Rol r     ON r.Nombre = m.Rol
JOIN dbo.Permiso p ON p.Nombre = m.Permiso
WHERE NOT EXISTS (SELECT 1 FROM dbo.Rol_Permiso rp WHERE rp.IdRol = r.IdRol AND rp.IdPermiso = p.IdPermiso);

PRINT CAST(@@ROWCOUNT AS VARCHAR) + ' permiso(s) agregado(s).';

COMMIT;
GO
