USE GestionWEB;
GO

DECLARE @Matriz TABLE (Rol VARCHAR(30), Permiso VARCHAR(30));

INSERT INTO @Matriz (Rol, Permiso) VALUES
    ('PACIENTE',      'ACCESO_TURNOS'),
    ('PACIENTE',      'ACCESO_RECETAS'),

    ('MEDICO',        'ACCESO_MEDICOS'),

    ('ADMINISTRADOR', 'ABM_USUARIO'),
    ('ADMINISTRADOR', 'ASIGNAR_ROL_USUARIO'),
    ('ADMINISTRADOR', 'GESTION_AGENDA_MEDICOS'),
    ('ADMINISTRADOR', 'BITACORA_EXPORTAR'),

    ('WEBMASTER',     'ACCESO_SEGURIDAD'),
    ('WEBMASTER',     'VER_BITACORA'),
    ('WEBMASTER',     'GESTION_ROLES_PERMISOS'),
    ('WEBMASTER',     'ASIGNACION_SEGURIDAD'),
    ('WEBMASTER',     'GESTION_IDIOMAS'),
    ('WEBMASTER',     'GESTION_TRADUCCIONES');

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
