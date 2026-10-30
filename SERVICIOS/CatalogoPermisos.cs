using System;
using System.Collections.Generic;
using System.Linq;

namespace SERVICIOS
{
    public class PermisoDefinicion
    {
        public PermisoDefinicion(string codigo, string tipo, string pagina, string descripcion)
        {
            Codigo = codigo;
            Tipo = tipo;
            Pagina = pagina;
            Descripcion = descripcion;
        }

        public string Codigo { get; }

        public string Tipo { get; }

        public string Pagina { get; }

        public string Descripcion { get; }
    }

    // Unico lugar donde se declaran los permisos que existen en el sistema
    //
    // PAGINA: entra a la pagina y ve todos sus controles.
    // ACCION: entra a la pagina y ve solo ese control.
    //
    // Para una pagina nueva:
    //   1. Agregar aca su permiso PAGINA y uno ACCION por cada boton a proteger.
    //   2. En el .aspx, poner data-permiso="CODIGO" en cada boton protegido.
    //   3. En el OnClick de cada boton protegido, llamar a ExigirPermiso("CODIGO").
    public static class CatalogoPermisos
    {
        public const string TipoPagina = "PAGINA";
        public const string TipoAccion = "ACCION";

        public static readonly IReadOnlyList<PermisoDefinicion> Todos = new List<PermisoDefinicion>
        {
            new PermisoDefinicion("ABM_USUARIO",            TipoPagina, "CRUD_Usuarios.aspx",        "Ver usuarios"),
            new PermisoDefinicion("USUARIO_ALTA",           TipoAccion, "CRUD_Usuarios.aspx",        "Crear usuarios"),
            new PermisoDefinicion("USUARIO_MODIFICACION",   TipoAccion, "CRUD_Usuarios.aspx",        "Modificar usuarios"),
            new PermisoDefinicion("USUARIO_BAJA",           TipoAccion, "CRUD_Usuarios.aspx",        "Eliminar usuarios"),

            new PermisoDefinicion("ACCESO_TURNOS",          TipoPagina, "Turnos.aspx",               "Ver turnos"),
            new PermisoDefinicion("TURNO_ALTA",             TipoAccion, "Turnos.aspx",               "Agendar turnos"),
            new PermisoDefinicion("TURNO_CANCELAR",         TipoAccion, "Turnos.aspx",               "Cancelar turnos"),

            new PermisoDefinicion("ACCESO_MEDICOS",         TipoPagina, "Medicos.aspx",              "Ver atencion medica"),
            new PermisoDefinicion("MEDICO_ATENDER",         TipoAccion, "Medicos.aspx",              "Atender pacientes"),

            new PermisoDefinicion("GESTION_AGENDA_MEDICOS", TipoPagina, "AgendaMedicos.aspx",        "Ver agenda y especialidades de medicos"),
            new PermisoDefinicion("MEDICO_DATOS_EDITAR",    TipoAccion, "AgendaMedicos.aspx",        "Modificar matricula y especialidades"),
            new PermisoDefinicion("AGENDA_HORARIO_EDITAR",  TipoAccion, "AgendaMedicos.aspx",        "Agregar/activar/eliminar franjas horarias"),
            new PermisoDefinicion("AGENDA_BLOQUEO_EDITAR",  TipoAccion, "AgendaMedicos.aspx",        "Bloquear dias u horarios puntuales"),
            new PermisoDefinicion("ESPECIALIDAD_ABM",       TipoAccion, "AgendaMedicos.aspx",        "Crear/eliminar especialidades"),

            new PermisoDefinicion("ACCESO_RECETAS",         TipoPagina, "Recetas.aspx",              "Ver recetas"),
            new PermisoDefinicion("VER_BITACORA",           TipoPagina, "Bitacora.aspx",             "Ver bitacora"),
            new PermisoDefinicion("BITACORA_EXPORTAR",      TipoAccion, "Bitacora.aspx",             "Exportar bitacora a XML"),

            new PermisoDefinicion("ACCESO_SEGURIDAD",       TipoPagina, "Seguridad.aspx",            "Ver backup e integridad"),
            new PermisoDefinicion("BACKUP_GENERAR",         TipoAccion, "Seguridad.aspx",            "Generar backup"),
            new PermisoDefinicion("BACKUP_RESTAURAR",       TipoAccion, "Seguridad.aspx",            "Restaurar backup"),
            new PermisoDefinicion("DV_RECALCULAR",          TipoAccion, "Seguridad.aspx",            "Recalcular digitos verificadores"),

            new PermisoDefinicion("GESTION_IDIOMAS",        TipoPagina, "FormIdiomas.aspx",          "Ver idiomas"),
            new PermisoDefinicion("IDIOMA_ALTA",            TipoAccion, "FormIdiomas.aspx",          "Agregar idiomas"),
            new PermisoDefinicion("IDIOMA_BAJA",            TipoAccion, "FormIdiomas.aspx",          "Eliminar idiomas"),

            new PermisoDefinicion("GESTION_TRADUCCIONES",   TipoPagina, "FormTraducciones.aspx",     "Ver traducciones"),
            new PermisoDefinicion("TRADUCCION_EDITAR",      TipoAccion, "FormTraducciones.aspx",     "Guardar traducciones"),
            new PermisoDefinicion("TRADUCCION_EXPORTAR",    TipoAccion, "FormTraducciones.aspx",     "Exportar traducciones a XML"),
            new PermisoDefinicion("TRADUCCION_IMPORTAR",    TipoAccion, "FormTraducciones.aspx",     "Importar traducciones desde XML"),

            new PermisoDefinicion("GESTION_ROLES_PERMISOS", TipoPagina, "GestionRolesPermisos.aspx", "Ver roles"),
            new PermisoDefinicion("ROL_ALTA",               TipoAccion, "GestionRolesPermisos.aspx", "Crear roles"),
            new PermisoDefinicion("ROL_MODIFICACION",       TipoAccion, "GestionRolesPermisos.aspx", "Modificar roles"),
            new PermisoDefinicion("ROL_BAJA",               TipoAccion, "GestionRolesPermisos.aspx", "Eliminar roles"),
            new PermisoDefinicion("ROL_COMPONER",           TipoAccion, "GestionRolesPermisos.aspx", "Agregar/quitar permisos y sub-roles"),

            new PermisoDefinicion("ASIGNACION_SEGURIDAD",   TipoPagina, "AsignacionSeguridad.aspx",  "Ver seguridad de usuarios"),
            new PermisoDefinicion("ASIGNAR_ROL_USUARIO",    TipoAccion, "AsignacionSeguridad.aspx",  "Asignar/quitar roles a usuarios"),
            new PermisoDefinicion("ASIGNAR_PERMISO_USUARIO", TipoAccion, "AsignacionSeguridad.aspx", "Asignar/quitar permisos a usuarios")
        };

        public static PermisoDefinicion Buscar(string codigo)
        {
            return Todos.FirstOrDefault(p => string.Equals(p.Codigo, codigo, StringComparison.OrdinalIgnoreCase));
        }

        public static IEnumerable<PermisoDefinicion> DePagina(string pagina)
        {
            return Todos.Where(p => string.Equals(p.Pagina, pagina, StringComparison.OrdinalIgnoreCase));
        }

        public static bool EsPaginaProtegida(string pagina)
        {
            return DePagina(pagina).Any();
        }
    }
}
