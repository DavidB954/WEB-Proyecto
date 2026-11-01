using BE;
using BE.Seguridad;
using BLL;
using SERVICIOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;

namespace Presentacion
{
    public partial class GestionRolesPermisos : PaginaSeguridadBase
    {
        BLL_Permiso bll_permiso = new BLL_Permiso();
        BLL_Rol bll_rol = new BLL_Rol();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                try
                {
                    bll_permiso.SincronizarCatalogo(UsuarioLogueado);

                    CargarGrillas();
                }
                catch (Exception ex)
                {
                    RegistrarErrorInterno("GestionRolesPermisos.Page_Load", ex);
                    MostrarMensaje(lblMensajeRol, T("No se pudieron cargar los datos. Intenta nuevamente mas tarde."), false);
                }
            }
        }

        private void CargarGrillas()
        {
            gvRoles.DataSource = bll_rol.ObtenerRoles();
            gvRoles.DataBind();
        }

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
                    MostrarMensaje(lblMensajeRol, T("Debe ingresar un nombre para el rol."), false);
                    return;
                }

                BE_Rol rol = new BE_Rol { Nombre = txtNombreRol.Text.Trim() };

                bll_rol.AgregarRol(UsuarioLogueado, rol);

                LimpiarFormularioRol();

                lblMensajeRol.Text = string.Empty;
                MostrarToast(T("Rol guardado correctamente."), true);
            }
            catch (InvalidOperationException ex)
            {
                MostrarMensaje(lblMensajeRol, ex.Message, false);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("GestionRolesPermisos.btnGuardarRol_Click", ex);
                MostrarMensaje(lblMensajeRol, T("No se pudo guardar el rol. Intenta nuevamente mas tarde."), false);
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

                DesplazarHacia(pnlComposicion);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("GestionRolesPermisos.gvRoles_SelectedIndexChanged", ex);
                MostrarMensaje(lblMensajeRol, T("No se pudo cargar el rol seleccionado."), false);
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
                    MostrarMensaje(lblMensajeRol, T("Seleccione un rol de la lista antes de modificar."), false);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtNombreRol.Text))
                {
                    MostrarMensaje(lblMensajeRol, T("Debe ingresar un nombre para el rol."), false);
                    return;
                }

                BE_Rol rol = new BE_Rol
                {
                    IdRol = int.Parse(hiddenIdRol.Value),
                    Nombre = txtNombreRol.Text.Trim()
                };

                bll_rol.ModificarRol(UsuarioLogueado, rol);

                LimpiarFormularioRol();

                lblMensajeRol.Text = string.Empty;
                MostrarToast(T("Rol modificado correctamente."), true);
            }
            catch (InvalidOperationException ex)
            {
                MostrarMensaje(lblMensajeRol, ex.Message, false);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("GestionRolesPermisos.btnModificarRol_Click", ex);
                MostrarMensaje(lblMensajeRol, T("No se pudo modificar el rol. Intenta nuevamente mas tarde."), false);
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
                    MostrarMensaje(lblMensajeRol, T("Seleccione un rol de la lista antes de eliminar."), false);
                    return;
                }

                bll_rol.EliminarRol(UsuarioLogueado, int.Parse(hiddenIdRol.Value));

                LimpiarFormularioRol();

                lblMensajeRol.Text = string.Empty;
                MostrarToast(T("Rol eliminado correctamente."), true);
            }
            catch (InvalidOperationException ex)
            {
                MostrarMensaje(lblMensajeRol, ex.Message, false);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("GestionRolesPermisos.btnEliminarRol_Click", ex);
                MostrarMensaje(lblMensajeRol, T("No se pudo eliminar el rol. Intenta nuevamente mas tarde."), false);
            }
        }

        protected void btnLimpiarRol_Click(object sender, EventArgs e)
        {
            LimpiarFormularioRol();
        }

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

            CargarComboPaginas(ddlPaginaAAgregar);
            CargarPermisosDePagina(arbol);

            HashSet<int> idsSubRolesDirectos = new HashSet<int>(arbol.SubRoles.Select(r => r.Rol.IdRol));

            ddlSubRolAAgregar.DataSource = bll_rol.ObtenerRoles().Where(r => r.IdRol != idRol && !idsSubRolesDirectos.Contains(r.IdRol)).ToList();
            ddlSubRolAAgregar.DataTextField = "Nombre";
            ddlSubRolAAgregar.DataValueField = "IdRol";
            ddlSubRolAAgregar.DataBind();
            IndicarSiNoHayOpciones(ddlSubRolAAgregar, btnAgregarSubRol, T("(no quedan roles para agregar)"));

            CargarArbolVisual(arbol);
        }

        private void CargarPermisosDePagina(RolComponente arbol)
        {
            CargarPermisosDisponibles(ddlPermisoAAgregar, btnAgregarPermisoARol, bll_permiso.ObtenerPermisos(), ddlPaginaAAgregar.SelectedValue, arbol.TienePermiso);
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
                MostrarMensaje(lblMensajeComposicion, T("No se pudieron cargar los permisos de la pagina."), false);
            }
        }

        private void CargarArbolVisual(RolComponente arbol)
        {
            trvArbolRol.Nodes.Clear();

            TreeNode nodoRaiz = CrearNodo("Rol: " + arbol.Codigo);
            AgregarNodosHijos(nodoRaiz, arbol.Hijos, DescribirPermiso);
            trvArbolRol.Nodes.Add(nodoRaiz);

            trvArbolRol.ExpandAll();
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

                int idRol = int.Parse(hiddenIdRol.Value);
                int idPermiso = int.Parse(ddlPermisoAAgregar.SelectedValue);

                bll_rol.AgregarPermisoARol(UsuarioLogueado, idRol, idPermiso);

                CargarComposicion(idRol);

                lblMensajeComposicion.Text = string.Empty;
                MostrarToast(T("Permiso agregado al rol."), true);
            }
            catch (InvalidOperationException ex)
            {
                MostrarMensaje(lblMensajeComposicion, ex.Message, false);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("GestionRolesPermisos.btnAgregarPermisoARol_Click", ex);
                MostrarMensaje(lblMensajeComposicion, T("No se pudo agregar el permiso al rol."), false);
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
                int idRol = int.Parse(hiddenIdRol.Value);
                int idPermiso = int.Parse(e.CommandArgument.ToString());

                bll_rol.QuitarPermisoDeRol(UsuarioLogueado, idRol, idPermiso);

                CargarComposicion(idRol);

                lblMensajeComposicion.Text = string.Empty;
                MostrarToast(T("Permiso quitado del rol."), true);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("GestionRolesPermisos.gvPermisosDelRol_RowCommand", ex);
                MostrarMensaje(lblMensajeComposicion, T("No se pudo quitar el permiso del rol."), false);
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

                int idRolPadre = int.Parse(hiddenIdRol.Value);
                int idRolHijo = int.Parse(ddlSubRolAAgregar.SelectedValue);

                bll_rol.AgregarSubRol(UsuarioLogueado, idRolPadre, idRolHijo);

                CargarComposicion(idRolPadre);

                lblMensajeComposicion.Text = string.Empty;
                MostrarToast(T("Sub-rol agregado."), true);
            }
            catch (InvalidOperationException ex)
            {
                MostrarMensaje(lblMensajeComposicion, ex.Message, false);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("GestionRolesPermisos.btnAgregarSubRol_Click", ex);
                MostrarMensaje(lblMensajeComposicion, T("No se pudo agregar el sub-rol."), false);
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
                int idRolPadre = int.Parse(hiddenIdRol.Value);
                int idRolHijo = int.Parse(e.CommandArgument.ToString());

                bll_rol.QuitarSubRol(UsuarioLogueado, idRolPadre, idRolHijo);

                CargarComposicion(idRolPadre);

                lblMensajeComposicion.Text = string.Empty;
                MostrarToast(T("Sub-rol quitado."), true);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("GestionRolesPermisos.gvSubRolesDelRol_RowCommand", ex);
                MostrarMensaje(lblMensajeComposicion, T("No se pudo quitar el sub-rol."), false);
            }
        }
    }
}
