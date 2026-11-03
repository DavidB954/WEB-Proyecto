# Sistema de Gestión Médica

Aplicación web para la gestión de un centro médico: turnos, agenda de médicos, atenciones, recetas y administración de usuarios, con un módulo de seguridad completo (roles y permisos, bitácora de auditoría e integridad de datos).

Proyecto desarrollado para la materia **Metodologías y Desarrollos WEB**.

## Tecnologías

- **C# / ASP.NET Web Forms** (.NET Framework 4.7.2)
- **SQL Server** (ADO.NET con consultas parametrizadas)
- **Web Service SOAP** (ASMX) + cliente de consola
- HTML, CSS y JavaScript

## Arquitectura

Arquitectura en capas:

| Capa | Proyecto | Responsabilidad |
|------|----------|-----------------|
| Presentación | `Presentacion` | Páginas Web Forms, master page y web service |
| Negocio | `BLL` | Reglas de negocio, validaciones, bitácora, integridad |
| Datos | `DAL` | Único acceso a la base de datos (SQL parametrizado) |
| Entidades | `BE` | Objetos de negocio |
| Servicios | `SERVICIOS` | Hashing, cifrado, permisos, multi-idioma, PDF |
| Cliente WS | `ClienteBitacoraWS` | Cliente de consola que consume el web service |

## Funcionalidades

**Gestión médica**
- Solicitud y cancelación de turnos por parte de los pacientes
- Agenda de médicos (días, horarios y duración de turnos)
- Registro de atención médica y emisión de recetas
- Consulta de recetas por paciente

**Seguridad**
- Login con contraseñas hasheadas (SHA-256), bloqueo por intentos fallidos
- Roles y permisos con patrón **Composite** (roles que contienen sub-roles y permisos, asignables a nivel de página o de botón)
- **Bitácora** de eventos con criticidad, módulo, filtros, paginación y exportación/importación XML
- **Dígitos verificadores** (DVH/DVV) para detectar modificaciones de datos por fuera del sistema
- Backup y restore de la base de datos
- Manejo centralizado de errores

**Multi-idioma**
- Traducciones administrables desde la aplicación (patrón **Observer**)
- Alta, baja y modificación de idiomas

**Integración**
- Web service SOAP para iniciar sesión y exportar la bitácora en XML

## Cómo ejecutarlo

1. Requisitos: Visual Studio con soporte para ASP.NET (.NET Framework 4.7.2) y SQL Server (Express o superior).
2. Crear la base de datos ejecutando `Scripts/CrearBase.sql`.
3. Ejecutar el resto de los scripts de `Scripts/` (permisos, traducciones y datos de demo).
4. Si tu instancia de SQL Server no es la local por defecto, ajustar la cadena de conexión en `DAL/DAL_Conexion.cs`.
5. Abrir `WEB-Proyecto.slnx`, establecer `Presentacion` como proyecto de inicio y ejecutar.

> Los usuarios de demo no tienen contraseñas publicadas. Para probar la aplicación, crear un usuario desde la base o restablecer la contraseña de uno existente.

## Autor

David Bellido
