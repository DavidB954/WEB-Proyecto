using BE;
using BE.Seguridad;
using SERVICIOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Presentacion
{
    public partial class Menu : PaginaBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            BE_Usuario usuarioLogueado = Session["Usuario"] as BE_Usuario;

            if (usuarioLogueado == null)
            {
                Response.Redirect("Login.aspx");
                return;
            }

            UsuarioComponente permisos = Session["Permisos"] as UsuarioComponente;

            cardTurnos.Visible = SeguridadHelper.TieneAccesoPagina(usuarioLogueado, permisos, "Turnos.aspx");
            cardUsuarios.Visible = SeguridadHelper.TieneAccesoPagina(usuarioLogueado, permisos, "CRUD_Usuarios.aspx");
            cardRecetas.Visible = SeguridadHelper.TieneAccesoPagina(usuarioLogueado, permisos, "Recetas.aspx");
            cardMedicos.Visible = SeguridadHelper.TieneAccesoPagina(usuarioLogueado, permisos, "Medicos.aspx");
            cardBitacora.Visible = SeguridadHelper.TieneAccesoPagina(usuarioLogueado, permisos, "Bitacora.aspx");
            cardSeguridad.Visible = SeguridadHelper.TieneAccesoPagina(usuarioLogueado, permisos, "Seguridad.aspx");
            cardRolesPermisos.Visible = SeguridadHelper.TieneAccesoPagina(usuarioLogueado, permisos, "GestionRolesPermisos.aspx");
            cardAsignacionSeguridad.Visible = SeguridadHelper.TieneAccesoPagina(usuarioLogueado, permisos, "AsignacionSeguridad.aspx");
        }

    }
}