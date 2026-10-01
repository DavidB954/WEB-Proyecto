using BE;
using BE.Seguridad;
using BLL;
using SERVICIOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Presentacion
{
    public partial class AsignacionSeguridad : PaginaBase
    {
        BLL_Usuario bll_usuario = new BLL_Usuario();
        BLL_Rol bll_rol = new BLL_Rol();
        BLL_Permiso bll_permiso = new BLL_Permiso();

        protected void Page_Load(object sender, EventArgs e)
        {
            BE_Usuario usuarioLogueado = Session["Usuario"] as BE_Usuario;
            UsuarioComponente permisos = Session["Permisos"] as UsuarioComponente;

            if (!SeguridadHelper.TieneAcceso(usuarioLogueado, permisos, "ASIGNACION_SEGURIDAD"))
            {
                Response.Redirect(usuarioLogueado == null ? "Login.aspx" : "AccesoDenegado.aspx");
                return;
            }

            if (!IsPostBack)
            {
                try
                {
                    CargarUsuarios();
                }
                catch (Exception ex)
                {
                    RegistrarErrorInterno("AsignacionSeguridad.Page_Load", ex);
                    lblMensajeAsignacion.Text = "No se pudieron cargar los usuarios. Intenta nuevamente mas tarde.";
                    lblMensajeAsignacion.ForeColor = System.Drawing.Color.Red;
                }
            }
        }

        private void CargarUsuarios()
        {
            gvUsuarios.DataSource = bll_usuario.UsuariosConRol();
            gvUsuarios.DataBind();
        }

        private void MostrarToast(string mensaje, bool exito)
        {
            string clase = exito ? "toast-exito" : "toast-error";
            string texto = System.Web.HttpUtility.JavaScriptStringEncode(mensaje, true);

            string script =
                "(function(){var t=document.createElement('div');t.className='toast " + clase + "';" +
                "t.textContent=" + texto + ";document.body.appendChild(t);" +
                "setTimeout(function(){t.className+=' toast-hide';},3000);" +
                "setTimeout(function(){if(t.parentNode){t.parentNode.removeChild(t);}},3600);})();";

            ScriptManager.RegisterStartupScript(this, GetType(), "toast_" + Guid.NewGuid().ToString("N"), script, true);
        }

        protected void gvUsuarios_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                int idUsuario = (int)gvUsuarios.DataKeys[gvUsuarios.SelectedIndex].Value;
                CargarAsignacion(idUsuario);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("AsignacionSeguridad.gvUsuarios_SelectedIndexChanged", ex);
                lblMensajeAsignacion.Text = "No se pudo cargar el usuario seleccionado.";
                lblMensajeAsignacion.ForeColor = System.Drawing.Color.Red;
            }
        }

        private void CargarAsignacion(int idUsuario)
        {
            BE_Usuario usuario = bll_usuario.Usuarios().FirstOrDefault(u => u.IdUsuario == idUsuario);

            if (usuario == null)
            {
                pnlAsignacion.Visible = false;
                pnlSinUsuarioSeleccionado.Visible = true;
                return;
            }

            pnlSinUsuarioSeleccionado.Visible = false;
            pnlAsignacion.Visible = true;

            ViewState["IdUsuarioSeleccionado"] = idUsuario;
            txtUsuarioSeleccionado.Text = usuario.NombreApellido;

            UsuarioComponente arbol = bll_permiso.ConstruirArbolUsuario(usuario);

            gvRolesDelUsuario.DataSource = arbol.RolesAsignados.Select(r => r.Rol).ToList();
            gvRolesDelUsuario.DataBind();

            gvPermisosDelUsuario.DataSource = arbol.PermisosDirectos.Select(p => p.Permiso).ToList();
            gvPermisosDelUsuario.DataBind();

            HashSet<int> idsRolesAsignados = new HashSet<int>(arbol.RolesAsignados.Select(r => r.Rol.IdRol));

            ddlRolAAsignar.DataSource = bll_rol.ObtenerRoles().Where(r => !idsRolesAsignados.Contains(r.IdRol)).ToList();
            ddlRolAAsignar.DataTextField = "Nombre";
            ddlRolAAsignar.DataValueField = "IdRol";
            ddlRolAAsignar.DataBind();

            HashSet<int> idsPermisosDirectos = new HashSet<int>(arbol.PermisosDirectos.Select(p => p.Permiso.IdPermiso));

            ddlPermisoAAsignar.DataSource = bll_permiso.ObtenerPermisos().Where(p => !idsPermisosDirectos.Contains(p.IdPermiso)).ToList();
            ddlPermisoAAsignar.DataTextField = "Nombre";
            ddlPermisoAAsignar.DataValueField = "IdPermiso";
            ddlPermisoAAsignar.DataBind();

            CargarArbolVisual(arbol);
        }

        private void CargarArbolVisual(UsuarioComponente arbol)
        {
            trvArbolUsuario.Nodes.Clear();

            TreeNode nodoRaiz = new TreeNode("Usuario: " + arbol.Usuario.NombreApellido);

            foreach (IComponentePermiso asignacion in arbol.Asignaciones)
            {
                if (asignacion is PermisoComponente permiso)
                {
                    nodoRaiz.ChildNodes.Add(new TreeNode("Permiso: " + permiso.Codigo));
                }
                else if (asignacion is RolComponente rol)
                {
                    TreeNode nodoRol = new TreeNode("Rol: " + rol.Codigo);
                    AgregarNodosHijos(nodoRol, rol);
                    nodoRaiz.ChildNodes.Add(nodoRol);
                }
            }

            trvArbolUsuario.Nodes.Add(nodoRaiz);
            trvArbolUsuario.ExpandAll();
        }

        private void AgregarNodosHijos(TreeNode nodoPadre, RolComponente rol)
        {
            foreach (IComponentePermiso hijo in rol.Hijos)
            {
                if (hijo is PermisoComponente permisoHijo)
                {
                    nodoPadre.ChildNodes.Add(new TreeNode("Permiso: " + permisoHijo.Codigo));
                }
                else if (hijo is RolComponente rolHijo)
                {
                    TreeNode nodoRolHijo = new TreeNode("Rol: " + rolHijo.Codigo);
                    AgregarNodosHijos(nodoRolHijo, rolHijo);
                    nodoPadre.ChildNodes.Add(nodoRolHijo);
                }
            }
        }

        private int? ObtenerIdUsuarioSeleccionado()
        {
            return ViewState["IdUsuarioSeleccionado"] as int?;
        }

        protected void btnAsignarRol_Click(object sender, EventArgs e)
        {
            try
            {
                int? idUsuario = ObtenerIdUsuarioSeleccionado();

                if (idUsuario == null || string.IsNullOrEmpty(ddlRolAAsignar.SelectedValue))
                {
                    return;
                }

                BE_Usuario usuarioLogueado = Session["Usuario"] as BE_Usuario;
                int idRol = int.Parse(ddlRolAAsignar.SelectedValue);

                bll_permiso.AsignarRolAUsuario(usuarioLogueado, idUsuario.Value, idRol);

                CargarAsignacion(idUsuario.Value);
                CargarUsuarios();

                lblMensajeAsignacion.Text = string.Empty;
                MostrarToast("Rol asignado correctamente.", true);
            }
            catch (InvalidOperationException ex)
            {
                lblMensajeAsignacion.Text = ex.Message;
                lblMensajeAsignacion.ForeColor = System.Drawing.Color.Red;
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("AsignacionSeguridad.btnAsignarRol_Click", ex);
                lblMensajeAsignacion.Text = "No se pudo asignar el rol.";
                lblMensajeAsignacion.ForeColor = System.Drawing.Color.Red;
            }
        }

        protected void gvRolesDelUsuario_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "Quitar")
            {
                return;
            }

            try
            {
                int? idUsuario = ObtenerIdUsuarioSeleccionado();

                if (idUsuario == null)
                {
                    return;
                }

                BE_Usuario usuarioLogueado = Session["Usuario"] as BE_Usuario;
                int idRol = int.Parse(e.CommandArgument.ToString());

                bll_permiso.QuitarRolDeUsuario(usuarioLogueado, idUsuario.Value, idRol);

                CargarAsignacion(idUsuario.Value);
                CargarUsuarios();

                lblMensajeAsignacion.Text = string.Empty;
                MostrarToast("Rol quitado correctamente.", true);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("AsignacionSeguridad.gvRolesDelUsuario_RowCommand", ex);
                lblMensajeAsignacion.Text = "No se pudo quitar el rol.";
                lblMensajeAsignacion.ForeColor = System.Drawing.Color.Red;
            }
        }

        protected void btnAsignarPermiso_Click(object sender, EventArgs e)
        {
            try
            {
                int? idUsuario = ObtenerIdUsuarioSeleccionado();

                if (idUsuario == null || string.IsNullOrEmpty(ddlPermisoAAsignar.SelectedValue))
                {
                    return;
                }

                BE_Usuario usuarioLogueado = Session["Usuario"] as BE_Usuario;
                int idPermiso = int.Parse(ddlPermisoAAsignar.SelectedValue);

                bll_permiso.AsignarPermisoAUsuario(usuarioLogueado, idUsuario.Value, idPermiso);

                CargarAsignacion(idUsuario.Value);

                lblMensajeAsignacion.Text = string.Empty;
                MostrarToast("Permiso asignado correctamente.", true);
            }
            catch (InvalidOperationException ex)
            {
                lblMensajeAsignacion.Text = ex.Message;
                lblMensajeAsignacion.ForeColor = System.Drawing.Color.Red;
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("AsignacionSeguridad.btnAsignarPermiso_Click", ex);
                lblMensajeAsignacion.Text = "No se pudo asignar el permiso.";
                lblMensajeAsignacion.ForeColor = System.Drawing.Color.Red;
            }
        }

        protected void gvPermisosDelUsuario_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "Quitar")
            {
                return;
            }

            try
            {
                int? idUsuario = ObtenerIdUsuarioSeleccionado();

                if (idUsuario == null)
                {
                    return;
                }

                BE_Usuario usuarioLogueado = Session["Usuario"] as BE_Usuario;
                int idPermiso = int.Parse(e.CommandArgument.ToString());

                bll_permiso.QuitarPermisoDeUsuario(usuarioLogueado, idUsuario.Value, idPermiso);

                CargarAsignacion(idUsuario.Value);

                lblMensajeAsignacion.Text = string.Empty;
                MostrarToast("Permiso quitado correctamente.", true);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("AsignacionSeguridad.gvPermisosDelUsuario_RowCommand", ex);
                lblMensajeAsignacion.Text = "No se pudo quitar el permiso.";
                lblMensajeAsignacion.ForeColor = System.Drawing.Color.Red;
            }
        }

        private void RegistrarErrorInterno(string origen, Exception ex)
        {
            try
            {
                string carpetaLogs = Server.MapPath("~/App_Data");
                System.IO.Directory.CreateDirectory(carpetaLogs);
                System.IO.File.AppendAllText(
                    System.IO.Path.Combine(carpetaLogs, "errores.log"),
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {origen} - {ex}{Environment.NewLine}");
            }
            catch
            {
            }
        }
    }
}
