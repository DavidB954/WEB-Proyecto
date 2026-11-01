using SERVICIOS;
using System;

namespace Presentacion
{
    public partial class Menu : PaginaBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (UsuarioLogueado == null)
            {
                Response.Redirect("Login.aspx");
                return;
            }

            cardTurnos.Visible = SeguridadHelper.TieneAccesoPagina(UsuarioLogueado, PermisosUsuario, "Turnos.aspx");
            cardUsuarios.Visible = SeguridadHelper.TieneAccesoPagina(UsuarioLogueado, PermisosUsuario, "CRUD_Usuarios.aspx");
            cardRecetas.Visible = SeguridadHelper.TieneAccesoPagina(UsuarioLogueado, PermisosUsuario, "Recetas.aspx");
            cardMedicos.Visible = SeguridadHelper.TieneAccesoPagina(UsuarioLogueado, PermisosUsuario, "Medicos.aspx");
            cardAgendaMedicos.Visible = SeguridadHelper.TieneAccesoPagina(UsuarioLogueado, PermisosUsuario, "AgendaMedicos.aspx");
            cardBitacora.Visible = SeguridadHelper.TieneAccesoPagina(UsuarioLogueado, PermisosUsuario, "Bitacora.aspx");
            cardSeguridad.Visible = SeguridadHelper.TieneAccesoPagina(UsuarioLogueado, PermisosUsuario, "Seguridad.aspx");
            cardIdiomas.Visible = SeguridadHelper.TieneAccesoPagina(UsuarioLogueado, PermisosUsuario, "FormIdiomas.aspx");
            cardTraducciones.Visible = SeguridadHelper.TieneAccesoPagina(UsuarioLogueado, PermisosUsuario, "FormTraducciones.aspx");
            cardRolesPermisos.Visible = SeguridadHelper.TieneAccesoPagina(UsuarioLogueado, PermisosUsuario, "GestionRolesPermisos.aspx");
            cardAsignacionSeguridad.Visible = SeguridadHelper.TieneAccesoPagina(UsuarioLogueado, PermisosUsuario, "AsignacionSeguridad.aspx");
        }
    }
}