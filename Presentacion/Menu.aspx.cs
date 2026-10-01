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

            cardTurnos.Visible = SeguridadHelper.TieneAcceso(usuarioLogueado, permisos, "ACCESO_TURNOS");
            cardUsuarios.Visible = SeguridadHelper.TieneAcceso(usuarioLogueado, permisos, "ABM_USUARIO");
            cardRecetas.Visible = SeguridadHelper.TieneAcceso(usuarioLogueado, permisos, "ACCESO_RECETAS");
            cardMedicos.Visible = SeguridadHelper.TieneAcceso(usuarioLogueado, permisos, "ACCESO_MEDICOS");
            cardBitacora.Visible = SeguridadHelper.TieneAcceso(usuarioLogueado, permisos, "VER_BITACORA");
            cardSeguridad.Visible = SeguridadHelper.TieneAcceso(usuarioLogueado, permisos, "ACCESO_SEGURIDAD");
            cardRolesPermisos.Visible = SeguridadHelper.TieneAcceso(usuarioLogueado, permisos, "GESTION_ROLES_PERMISOS");
            cardAsignacionSeguridad.Visible = SeguridadHelper.TieneAcceso(usuarioLogueado, permisos, "ASIGNACION_SEGURIDAD");
        }
    }
}