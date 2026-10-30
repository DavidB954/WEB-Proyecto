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
            // El acceso a la pagina lo valida PaginaBase segun CatalogoPermisos.

            if (!IsPostBack)
            {
                try
                {
                    // Los permisos no se crean a mano: se dan de alta desde
                    // CatalogoPermisos para que se puedan agregar a los roles.
                    bll_permiso.SincronizarCatalogo(UsuarioLogueado);

                    CargarGrillas();
                }
                catch (Exception ex)
                {
                    RegistrarErrorInterno("GestionRolesPermisos.Page_Load", ex);
                    lblMensajeRol.Text = T("No se pudieron cargar los datos. Intenta nuevamente mas tarde.");
                    lblMensajeRol.ForeColor = System.Drawing.Color.Red;
                }
            }
        }

        private void CargarGrillas()
        {
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
            if (!ExigirPermiso("ROL_ALTA"))
            {
                return;
            }

            try
            {
                if (string.IsNullOrWhiteSpace(txtNombreRol.Text))
                {
                    lblMensajeRol.Text = T("Debe ingresar un nombre para el rol.");
                    lblMensajeRol.ForeColor = System.Drawing.Color.Red;
                    return;
                }

                BE_Usuario usuarioLogueado = Session["Usuario"] as BE_Usuario;

                BE_Rol rol = new BE_Rol { Nombre = txtNombreRol.Text.Trim() };

                bll_rol.AgregarRol(usuarioLogueado, rol);

                LimpiarFormularioRol();

                lblMensajeRol.Text = string.Empty;
                MostrarToast(T("Rol guardado correctamente."), true);
            }
            catch (InvalidOperationException ex)
            {
                lblMensajeRol.Text = ex.Message;
                lblMensajeRol.ForeColor = System.Drawing.Color.Red;
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("GestionRolesPermisos.btnGuardarRol_Click", ex);
                lblMensajeRol.Text = T("No se pudo guardar el rol. Intenta nuevamente mas tarde.");
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
                lblMensajeRol.Text = T("No se pudo cargar el rol seleccionado.");
                lblMensajeRol.ForeColor = System.Drawing.Color.Red;
            }
        }

        protected void btnModificarRol_Click(object sender, EventArgs e)
        {
            if (!ExigirPermiso("ROL_MODIFICACION"))
            {
                return;
            }

            try
            {
                if (string.IsNullOrWhiteSpace(hiddenIdRol.Value))
                {
                    lblMensajeRol.Text = T("Seleccione un rol de la lista antes de modificar.");
                    lblMensajeRol.ForeColor = System.Drawing.Color.Red;
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtNombreRol.Text))
                {
                    lblMensajeRol.Text = T("Debe ingresar un nombre para el rol.");
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
                MostrarToast(T("Rol modificado correctamente."), true);
            }
            catch (InvalidOperationException ex)
            {
                lblMensajeRol.Text = ex.Message;
                lblMensajeRol.ForeColor = System.Drawing.Color.Red;
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("GestionRolesPermisos.btnModificarRol_Click", ex);
                lblMensajeRol.Text = T("No se pudo modificar el rol. Intenta nuevamente mas tarde.");
                lblMensajeRol.ForeColor = System.Drawing.Color.Red;
            }
        }

        protected void btnEliminarRol_Click(object sender, EventArgs e)
        {
            if (!ExigirPermiso("ROL_BAJA"))
            {
                return;
            }

            try
            {
                if (string.IsNullOrWhiteSpace(hiddenIdRol.Value))
                {
                    lblMensajeRol.Text = T("Seleccione un rol de la lista antes de eliminar.");
                    lblMensajeRol.ForeColor = System.Drawing.Color.Red;
                    return;
                }

                BE_Usuario usuarioLogueado = Session["Usuario"] as BE_Usuario;

                bll_rol.EliminarRol(usuarioLogueado, int.Parse(hiddenIdRol.Value));

                LimpiarFormularioRol();

                lblMensajeRol.Text = string.Empty;
                MostrarToast(T("Rol eliminado correctamente."), true);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("GestionRolesPermisos.btnEliminarRol_Click", ex);
                lblMensajeRol.Text = T("No se pudo eliminar el rol. Verifique que no este en uso por ningun usuario u otro rol.");
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

            gvPermisosDelRol.DataSource = arbol.PermisosDirectos
                .Select(p => new
                {
                    p.Permiso.IdPermiso,
                    p.Permiso.Nombre,
                    p.Permiso.Tipo,
                    Pagina = CatalogoPermisos.Buscar(p.Permiso.Nombre)?.Pagina ?? "-"
                })
                .ToList();
            gvPermisosDelRol.DataBind();

            gvSubRolesDelRol.DataSource = arbol.SubRoles.Select(r => r.Rol).ToList();
            gvSubRolesDelRol.DataBind();

            CargarPaginas();
            CargarPermisosDePagina(arbol);

            HashSet<int> idsSubRolesDirectos = new HashSet<int>(arbol.SubRoles.Select(r => r.Rol.IdRol));

            ddlSubRolAAgregar.DataSource = bll_rol.ObtenerRoles().Where(r => r.IdRol != idRol && !idsSubRolesDirectos.Contains(r.IdRol)).ToList();
            ddlSubRolAAgregar.DataTextField = "Nombre";
            ddlSubRolAAgregar.DataValueField = "IdRol";
            ddlSubRolAAgregar.DataBind();

            CargarArbolVisual(arbol);
        }

        // Conserva la pagina elegida entre recargas (ej: despues de agregar un
        // permiso se sigue viendo la misma pagina).
        private void CargarPaginas()
        {
            string paginaSeleccionada = ddlPaginaAAgregar.SelectedValue;

            ddlPaginaAAgregar.Items.Clear();

            foreach (string pagina in CatalogoPermisos.Todos.Select(p => p.Pagina).Distinct())
            {
                ddlPaginaAAgregar.Items.Add(new ListItem(pagina, pagina));
            }

            if (ddlPaginaAAgregar.Items.FindByValue(paginaSeleccionada) != null)
            {
                ddlPaginaAAgregar.SelectedValue = paginaSeleccionada;
            }
        }

        // Permisos de la pagina elegida: primero "toda la pagina" (PAGINA) y despues
        // cada ACCION por separado. Se omiten los que el rol ya tiene directos.
        private void CargarPermisosDePagina(RolComponente arbol)
        {
            HashSet<string> nombresDirectos = new HashSet<string>(
                arbol.PermisosDirectos.Select(p => p.Codigo), StringComparer.OrdinalIgnoreCase);

            Dictionary<string, int> idsPorNombre = bll_permiso.ObtenerPermisos()
                .GroupBy(p => p.Nombre, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First().IdPermiso, StringComparer.OrdinalIgnoreCase);

            IEnumerable<PermisoDefinicion> definiciones = CatalogoPermisos.DePagina(ddlPaginaAAgregar.SelectedValue)
                .OrderBy(p => p.Tipo == CatalogoPermisos.TipoPagina ? 0 : 1);

            ddlPermisoAAgregar.Items.Clear();

            foreach (PermisoDefinicion definicion in definiciones)
            {
                if (nombresDirectos.Contains(definicion.Codigo) || !idsPorNombre.TryGetValue(definicion.Codigo, out int idPermiso))
                {
                    continue;
                }

                string texto = definicion.Tipo == CatalogoPermisos.TipoPagina
                    ? T("Toda la pagina ({0})", definicion.Codigo)
                    : T("Solo: {0} ({1})", T(definicion.Descripcion), definicion.Codigo);

                ddlPermisoAAgregar.Items.Add(new ListItem(texto, idPermiso.ToString()));
            }
        }

        protected void ddlPaginaAAgregar_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(hiddenIdRol.Value))
                {
                    return;
                }

                CargarPermisosDePagina(bll_rol.ConstruirArbolRol(int.Parse(hiddenIdRol.Value)));
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("GestionRolesPermisos.ddlPaginaAAgregar_SelectedIndexChanged", ex);
                lblMensajeComposicion.Text = T("No se pudieron cargar los permisos de la pagina.");
                lblMensajeComposicion.ForeColor = System.Drawing.Color.Red;
            }
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
                    nodoPadre.ChildNodes.Add(new TreeNode(DescribirPermiso(permisoHijo.Codigo)));
                }
                else if (hijo is RolComponente rolHijo)
                {
                    TreeNode nodoRolHijo = new TreeNode("Rol: " + rolHijo.Codigo);
                    AgregarNodosHijos(nodoRolHijo, rolHijo);
                    nodoPadre.ChildNodes.Add(nodoRolHijo);
                }
            }
        }

        private string DescribirPermiso(string codigo)
        {
            PermisoDefinicion definicion = CatalogoPermisos.Buscar(codigo);

            if (definicion == null)
            {
                return T("Permiso: {0}", codigo);
            }

            return definicion.Tipo == CatalogoPermisos.TipoPagina
                ? T("Pagina completa: {0} ({1})", definicion.Pagina, definicion.Codigo)
                : T("Accion: {0} en {1} ({2})", T(definicion.Descripcion), definicion.Pagina, definicion.Codigo);
        }

        protected void btnAgregarPermisoARol_Click(object sender, EventArgs e)
        {
            if (!ExigirPermiso("ROL_COMPONER"))
            {
                return;
            }

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
                MostrarToast(T("Permiso agregado al rol."), true);
            }
            catch (InvalidOperationException ex)
            {
                lblMensajeComposicion.Text = ex.Message;
                lblMensajeComposicion.ForeColor = System.Drawing.Color.Red;
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("GestionRolesPermisos.btnAgregarPermisoARol_Click", ex);
                lblMensajeComposicion.Text = T("No se pudo agregar el permiso al rol.");
                lblMensajeComposicion.ForeColor = System.Drawing.Color.Red;
            }
        }

        protected void gvPermisosDelRol_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "Quitar")
            {
                return;
            }

            if (!ExigirPermiso("ROL_COMPONER"))
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
                MostrarToast(T("Permiso quitado del rol."), true);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("GestionRolesPermisos.gvPermisosDelRol_RowCommand", ex);
                lblMensajeComposicion.Text = T("No se pudo quitar el permiso del rol.");
                lblMensajeComposicion.ForeColor = System.Drawing.Color.Red;
            }
        }

        protected void btnAgregarSubRol_Click(object sender, EventArgs e)
        {
            if (!ExigirPermiso("ROL_COMPONER"))
            {
                return;
            }

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
                MostrarToast(T("Sub-rol agregado."), true);
            }
            catch (InvalidOperationException ex)
            {
                lblMensajeComposicion.Text = ex.Message;
                lblMensajeComposicion.ForeColor = System.Drawing.Color.Red;
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("GestionRolesPermisos.btnAgregarSubRol_Click", ex);
                lblMensajeComposicion.Text = T("No se pudo agregar el sub-rol.");
                lblMensajeComposicion.ForeColor = System.Drawing.Color.Red;
            }
        }

        protected void gvSubRolesDelRol_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "Quitar")
            {
                return;
            }

            if (!ExigirPermiso("ROL_COMPONER"))
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
                MostrarToast(T("Sub-rol quitado."), true);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("GestionRolesPermisos.gvSubRolesDelRol_RowCommand", ex);
                lblMensajeComposicion.Text = T("No se pudo quitar el sub-rol.");
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
