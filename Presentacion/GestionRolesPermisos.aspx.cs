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
    public partial class GestionRolesPermisos : PaginaBase
    {
        BLL_Permiso bll_permiso = new BLL_Permiso();
        BLL_Rol bll_rol = new BLL_Rol();

        protected void Page_Load(object sender, EventArgs e)
        {
            BE_Usuario usuarioLogueado = Session["Usuario"] as BE_Usuario;
            UsuarioComponente permisos = Session["Permisos"] as UsuarioComponente;

            if (!SeguridadHelper.TieneAcceso(usuarioLogueado, permisos, "GESTION_ROLES_PERMISOS"))
            {
                Response.Redirect(usuarioLogueado == null ? "Login.aspx" : "AccesoDenegado.aspx");
                return;
            }

            if (!IsPostBack)
            {
                try
                {
                    CargarGrillas();
                }
                catch (Exception ex)
                {
                    RegistrarErrorInterno("GestionRolesPermisos.Page_Load", ex);
                    lblMensajePermiso.Text = "No se pudieron cargar los datos. Intenta nuevamente mas tarde.";
                    lblMensajePermiso.ForeColor = System.Drawing.Color.Red;
                }
            }
        }

        private void CargarGrillas()
        {
            gvPermisos.DataSource = bll_permiso.ObtenerPermisos();
            gvPermisos.DataBind();

            gvRoles.DataSource = bll_rol.ObtenerRoles();
            gvRoles.DataBind();
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

        // ==================== ABM Permiso ====================

        private void LimpiarFormularioPermiso()
        {
            txtNombrePermiso.Text = string.Empty;
            ddlTipoPermiso.SelectedIndex = 0;
            hiddenIdPermiso.Value = string.Empty;

            CargarGrillas();
        }

        protected void btnGuardarPermiso_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtNombrePermiso.Text))
                {
                    lblMensajePermiso.Text = "Debe ingresar un nombre para el permiso.";
                    lblMensajePermiso.ForeColor = System.Drawing.Color.Red;
                    return;
                }

                BE_Usuario usuarioLogueado = Session["Usuario"] as BE_Usuario;

                BE_Permiso permiso = new BE_Permiso
                {
                    Nombre = txtNombrePermiso.Text.Trim(),
                    Tipo = ddlTipoPermiso.SelectedValue
                };

                bll_permiso.AgregarPermiso(usuarioLogueado, permiso);

                LimpiarFormularioPermiso();

                lblMensajePermiso.Text = string.Empty;
                MostrarToast("Permiso guardado correctamente.", true);
            }
            catch (InvalidOperationException ex)
            {
                lblMensajePermiso.Text = ex.Message;
                lblMensajePermiso.ForeColor = System.Drawing.Color.Red;
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("GestionRolesPermisos.btnGuardarPermiso_Click", ex);
                lblMensajePermiso.Text = "No se pudo guardar el permiso. Intenta nuevamente mas tarde.";
                lblMensajePermiso.ForeColor = System.Drawing.Color.Red;
            }
        }

        protected void gvPermisos_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                GridViewRow fila = gvPermisos.SelectedRow;

                hiddenIdPermiso.Value = fila.Cells[1].Text;
                txtNombrePermiso.Text = fila.Cells[2].Text;
                ddlTipoPermiso.SelectedValue = fila.Cells[3].Text;
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("GestionRolesPermisos.gvPermisos_SelectedIndexChanged", ex);
                lblMensajePermiso.Text = "No se pudo cargar el permiso seleccionado.";
                lblMensajePermiso.ForeColor = System.Drawing.Color.Red;
            }
        }

        protected void btnModificarPermiso_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(hiddenIdPermiso.Value))
                {
                    lblMensajePermiso.Text = "Seleccione un permiso de la lista antes de modificar.";
                    lblMensajePermiso.ForeColor = System.Drawing.Color.Red;
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtNombrePermiso.Text))
                {
                    lblMensajePermiso.Text = "Debe ingresar un nombre para el permiso.";
                    lblMensajePermiso.ForeColor = System.Drawing.Color.Red;
                    return;
                }

                BE_Usuario usuarioLogueado = Session["Usuario"] as BE_Usuario;

                BE_Permiso permiso = new BE_Permiso
                {
                    IdPermiso = int.Parse(hiddenIdPermiso.Value),
                    Nombre = txtNombrePermiso.Text.Trim(),
                    Tipo = ddlTipoPermiso.SelectedValue
                };

                bll_permiso.ModificarPermiso(usuarioLogueado, permiso);

                LimpiarFormularioPermiso();

                lblMensajePermiso.Text = string.Empty;
                MostrarToast("Permiso modificado correctamente.", true);
            }
            catch (InvalidOperationException ex)
            {
                lblMensajePermiso.Text = ex.Message;
                lblMensajePermiso.ForeColor = System.Drawing.Color.Red;
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("GestionRolesPermisos.btnModificarPermiso_Click", ex);
                lblMensajePermiso.Text = "No se pudo modificar el permiso. Intenta nuevamente mas tarde.";
                lblMensajePermiso.ForeColor = System.Drawing.Color.Red;
            }
        }

        protected void btnEliminarPermiso_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(hiddenIdPermiso.Value))
                {
                    lblMensajePermiso.Text = "Seleccione un permiso de la lista antes de eliminar.";
                    lblMensajePermiso.ForeColor = System.Drawing.Color.Red;
                    return;
                }

                BE_Usuario usuarioLogueado = Session["Usuario"] as BE_Usuario;

                bll_permiso.EliminarPermiso(usuarioLogueado, int.Parse(hiddenIdPermiso.Value));

                LimpiarFormularioPermiso();

                lblMensajePermiso.Text = string.Empty;
                MostrarToast("Permiso eliminado correctamente.", true);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("GestionRolesPermisos.btnEliminarPermiso_Click", ex);
                lblMensajePermiso.Text = "No se pudo eliminar el permiso. Verifique que no este en uso por ningun rol o usuario.";
                lblMensajePermiso.ForeColor = System.Drawing.Color.Red;
            }
        }

        protected void btnLimpiarPermiso_Click(object sender, EventArgs e)
        {
            LimpiarFormularioPermiso();
        }

        // ==================== ABM Rol ====================

        private void LimpiarFormularioRol()
        {
            txtNombreRol.Text = string.Empty;
            hiddenIdRol.Value = string.Empty;

            pnlComposicion.Visible = false;
            pnlSinRolSeleccionado.Visible = true;

            CargarGrillas();
        }

        protected void btnGuardarRol_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtNombreRol.Text))
                {
                    lblMensajeRol.Text = "Debe ingresar un nombre para el rol.";
                    lblMensajeRol.ForeColor = System.Drawing.Color.Red;
                    return;
                }

                BE_Usuario usuarioLogueado = Session["Usuario"] as BE_Usuario;

                BE_Rol rol = new BE_Rol { Nombre = txtNombreRol.Text.Trim() };

                bll_rol.AgregarRol(usuarioLogueado, rol);

                LimpiarFormularioRol();

                lblMensajeRol.Text = string.Empty;
                MostrarToast("Rol guardado correctamente.", true);
            }
            catch (InvalidOperationException ex)
            {
                lblMensajeRol.Text = ex.Message;
                lblMensajeRol.ForeColor = System.Drawing.Color.Red;
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("GestionRolesPermisos.btnGuardarRol_Click", ex);
                lblMensajeRol.Text = "No se pudo guardar el rol. Intenta nuevamente mas tarde.";
                lblMensajeRol.ForeColor = System.Drawing.Color.Red;
            }
        }

        protected void gvRoles_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                GridViewRow fila = gvRoles.SelectedRow;

                hiddenIdRol.Value = fila.Cells[1].Text;
                txtNombreRol.Text = fila.Cells[2].Text;

                CargarComposicion(int.Parse(hiddenIdRol.Value));
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("GestionRolesPermisos.gvRoles_SelectedIndexChanged", ex);
                lblMensajeRol.Text = "No se pudo cargar el rol seleccionado.";
                lblMensajeRol.ForeColor = System.Drawing.Color.Red;
            }
        }

        protected void btnModificarRol_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(hiddenIdRol.Value))
                {
                    lblMensajeRol.Text = "Seleccione un rol de la lista antes de modificar.";
                    lblMensajeRol.ForeColor = System.Drawing.Color.Red;
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtNombreRol.Text))
                {
                    lblMensajeRol.Text = "Debe ingresar un nombre para el rol.";
                    lblMensajeRol.ForeColor = System.Drawing.Color.Red;
                    return;
                }

                BE_Usuario usuarioLogueado = Session["Usuario"] as BE_Usuario;

                BE_Rol rol = new BE_Rol
                {
                    IdRol = int.Parse(hiddenIdRol.Value),
                    Nombre = txtNombreRol.Text.Trim()
                };

                bll_rol.ModificarRol(usuarioLogueado, rol);

                LimpiarFormularioRol();

                lblMensajeRol.Text = string.Empty;
                MostrarToast("Rol modificado correctamente.", true);
            }
            catch (InvalidOperationException ex)
            {
                lblMensajeRol.Text = ex.Message;
                lblMensajeRol.ForeColor = System.Drawing.Color.Red;
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("GestionRolesPermisos.btnModificarRol_Click", ex);
                lblMensajeRol.Text = "No se pudo modificar el rol. Intenta nuevamente mas tarde.";
                lblMensajeRol.ForeColor = System.Drawing.Color.Red;
            }
        }

        protected void btnEliminarRol_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(hiddenIdRol.Value))
                {
                    lblMensajeRol.Text = "Seleccione un rol de la lista antes de eliminar.";
                    lblMensajeRol.ForeColor = System.Drawing.Color.Red;
                    return;
                }

                BE_Usuario usuarioLogueado = Session["Usuario"] as BE_Usuario;

                bll_rol.EliminarRol(usuarioLogueado, int.Parse(hiddenIdRol.Value));

                LimpiarFormularioRol();

                lblMensajeRol.Text = string.Empty;
                MostrarToast("Rol eliminado correctamente.", true);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("GestionRolesPermisos.btnEliminarRol_Click", ex);
                lblMensajeRol.Text = "No se pudo eliminar el rol. Verifique que no este en uso por ningun usuario u otro rol.";
                lblMensajeRol.ForeColor = System.Drawing.Color.Red;
            }
        }

        protected void btnLimpiarRol_Click(object sender, EventArgs e)
        {
            LimpiarFormularioRol();
        }

        // ==================== Composicion del Rol (Composite) ====================

        private void CargarComposicion(int idRol)
        {
            pnlSinRolSeleccionado.Visible = false;
            pnlComposicion.Visible = true;

            RolComponente arbol = bll_rol.ConstruirArbolRol(idRol);

            txtRolComponiendo.Text = arbol.Codigo;

            gvPermisosDelRol.DataSource = arbol.PermisosDirectos.Select(p => p.Permiso).ToList();
            gvPermisosDelRol.DataBind();

            gvSubRolesDelRol.DataSource = arbol.SubRoles.Select(r => r.Rol).ToList();
            gvSubRolesDelRol.DataBind();

            HashSet<int> idsPermisosDirectos = new HashSet<int>(arbol.PermisosDirectos.Select(p => p.Permiso.IdPermiso));

            ddlPermisoAAgregar.DataSource = bll_permiso.ObtenerPermisos().Where(p => !idsPermisosDirectos.Contains(p.IdPermiso)).ToList();
            ddlPermisoAAgregar.DataTextField = "Nombre";
            ddlPermisoAAgregar.DataValueField = "IdPermiso";
            ddlPermisoAAgregar.DataBind();

            HashSet<int> idsSubRolesDirectos = new HashSet<int>(arbol.SubRoles.Select(r => r.Rol.IdRol));

            ddlSubRolAAgregar.DataSource = bll_rol.ObtenerRoles().Where(r => r.IdRol != idRol && !idsSubRolesDirectos.Contains(r.IdRol)).ToList();
            ddlSubRolAAgregar.DataTextField = "Nombre";
            ddlSubRolAAgregar.DataValueField = "IdRol";
            ddlSubRolAAgregar.DataBind();

            CargarArbolVisual(arbol);
        }

        private void CargarArbolVisual(RolComponente arbol)
        {
            trvArbolRol.Nodes.Clear();

            TreeNode nodoRaiz = new TreeNode("Rol: " + arbol.Codigo);
            AgregarNodosHijos(nodoRaiz, arbol);
            trvArbolRol.Nodes.Add(nodoRaiz);

            trvArbolRol.ExpandAll();
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

        protected void btnAgregarPermisoARol_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(hiddenIdRol.Value) || string.IsNullOrEmpty(ddlPermisoAAgregar.SelectedValue))
                {
                    return;
                }

                BE_Usuario usuarioLogueado = Session["Usuario"] as BE_Usuario;
                int idRol = int.Parse(hiddenIdRol.Value);
                int idPermiso = int.Parse(ddlPermisoAAgregar.SelectedValue);

                bll_rol.AgregarPermisoARol(usuarioLogueado, idRol, idPermiso);

                CargarComposicion(idRol);

                lblMensajeComposicion.Text = string.Empty;
                MostrarToast("Permiso agregado al rol.", true);
            }
            catch (InvalidOperationException ex)
            {
                lblMensajeComposicion.Text = ex.Message;
                lblMensajeComposicion.ForeColor = System.Drawing.Color.Red;
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("GestionRolesPermisos.btnAgregarPermisoARol_Click", ex);
                lblMensajeComposicion.Text = "No se pudo agregar el permiso al rol.";
                lblMensajeComposicion.ForeColor = System.Drawing.Color.Red;
            }
        }

        protected void gvPermisosDelRol_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "Quitar")
            {
                return;
            }

            try
            {
                BE_Usuario usuarioLogueado = Session["Usuario"] as BE_Usuario;
                int idRol = int.Parse(hiddenIdRol.Value);
                int idPermiso = int.Parse(e.CommandArgument.ToString());

                bll_rol.QuitarPermisoDeRol(usuarioLogueado, idRol, idPermiso);

                CargarComposicion(idRol);

                lblMensajeComposicion.Text = string.Empty;
                MostrarToast("Permiso quitado del rol.", true);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("GestionRolesPermisos.gvPermisosDelRol_RowCommand", ex);
                lblMensajeComposicion.Text = "No se pudo quitar el permiso del rol.";
                lblMensajeComposicion.ForeColor = System.Drawing.Color.Red;
            }
        }

        protected void btnAgregarSubRol_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(hiddenIdRol.Value) || string.IsNullOrEmpty(ddlSubRolAAgregar.SelectedValue))
                {
                    return;
                }

                BE_Usuario usuarioLogueado = Session["Usuario"] as BE_Usuario;
                int idRolPadre = int.Parse(hiddenIdRol.Value);
                int idRolHijo = int.Parse(ddlSubRolAAgregar.SelectedValue);

                bll_rol.AgregarSubRol(usuarioLogueado, idRolPadre, idRolHijo);

                CargarComposicion(idRolPadre);

                lblMensajeComposicion.Text = string.Empty;
                MostrarToast("Sub-rol agregado.", true);
            }
            catch (InvalidOperationException ex)
            {
                lblMensajeComposicion.Text = ex.Message;
                lblMensajeComposicion.ForeColor = System.Drawing.Color.Red;
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("GestionRolesPermisos.btnAgregarSubRol_Click", ex);
                lblMensajeComposicion.Text = "No se pudo agregar el sub-rol.";
                lblMensajeComposicion.ForeColor = System.Drawing.Color.Red;
            }
        }

        protected void gvSubRolesDelRol_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "Quitar")
            {
                return;
            }

            try
            {
                BE_Usuario usuarioLogueado = Session["Usuario"] as BE_Usuario;
                int idRolPadre = int.Parse(hiddenIdRol.Value);
                int idRolHijo = int.Parse(e.CommandArgument.ToString());

                bll_rol.QuitarSubRol(usuarioLogueado, idRolPadre, idRolHijo);

                CargarComposicion(idRolPadre);

                lblMensajeComposicion.Text = string.Empty;
                MostrarToast("Sub-rol quitado.", true);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("GestionRolesPermisos.gvSubRolesDelRol_RowCommand", ex);
                lblMensajeComposicion.Text = "No se pudo quitar el sub-rol.";
                lblMensajeComposicion.ForeColor = System.Drawing.Color.Red;
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
