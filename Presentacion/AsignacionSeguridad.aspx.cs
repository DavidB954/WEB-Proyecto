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
    public partial class AsignacionSeguridad : PaginaSeguridadBase
    {
        BLL_Usuario bll_usuario = new BLL_Usuario();
        BLL_Permiso bll_permiso = new BLL_Permiso();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                try
                {
                    bll_permiso.SincronizarCatalogo(UsuarioLogueado);

                    CargarUsuarios();
                }
                catch (Exception ex)
                {
                    RegistrarErrorInterno("AsignacionSeguridad.Page_Load", ex);
                    MostrarMensaje(lblMensajeAsignacion, T("No se pudieron cargar los usuarios. Intenta nuevamente mas tarde."), false);
                }
            }
        }

        private void CargarUsuarios()
        {
            gvUsuarios.DataSource = bll_usuario.UsuariosConRol(UsuarioLogueado);
            gvUsuarios.DataBind();
        }

        protected void gvUsuarios_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                int idUsuario = (int)gvUsuarios.DataKeys[gvUsuarios.SelectedIndex].Value;
                CargarAsignacion(idUsuario);

                if (pnlAsignacion.Visible)
                {
                    DesplazarHacia(pnlAsignacion);
                }
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("AsignacionSeguridad.gvUsuarios_SelectedIndexChanged", ex);
                MostrarMensaje(lblMensajeAsignacion, T("No se pudo cargar el usuario seleccionado."), false);
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

            gvPermisosDelUsuario.DataSource = arbol.PermisosDirectos
                .Select(p => new
                {
                    p.Permiso.IdPermiso,
                    p.Permiso.Nombre,
                    p.Permiso.Tipo,
                    Detalle = DescribirPermiso(p.Permiso.Nombre)
                })
                .ToList();
            gvPermisosDelUsuario.DataBind();

            HashSet<int> idsRolesAsignados = new HashSet<int>(arbol.RolesAsignados.Select(r => r.Rol.IdRol));

            ddlRolAAsignar.DataSource = bll_permiso.ObtenerRolesAsignables(UsuarioLogueado).Where(r => !idsRolesAsignados.Contains(r.IdRol)).ToList();
            ddlRolAAsignar.DataTextField = "Nombre";
            ddlRolAAsignar.DataValueField = "IdRol";
            ddlRolAAsignar.DataBind();
            IndicarSiNoHayOpciones(ddlRolAAsignar, btnAsignarRol, T("(ya tiene todos los roles)"));

            CargarComboPaginas(ddlPaginaAAsignar);
            CargarPermisosDePagina(arbol);

            CargarArbolVisual(arbol);

            AplicarModoEdicion(bll_permiso.ObtenerMotivoSoloLectura(UsuarioLogueado, idUsuario));
        }

        private void AplicarModoEdicion(string motivoSoloLectura)
        {
            bool editable = motivoSoloLectura == null;

            pnlAsignarRol.Visible = editable;
            pnlAgregarPermiso.Visible = editable;
            gvRolesDelUsuario.Columns[1].Visible = editable;
            gvPermisosDelUsuario.Columns[2].Visible = editable;

            if (editable)
            {
                lblSoloLectura.Text = string.Empty;
            }
            else
            {
                MostrarMensaje(lblSoloLectura, motivoSoloLectura, false);
            }
        }

        private string DescribirPermiso(string codigo)
        {
            PermisoDefinicion definicion = CatalogoPermisos.Buscar(codigo);

            return definicion == null ? string.Empty : definicion.Pagina + " - " + T(definicion.Descripcion);
        }

        private void CargarPermisosDePagina(UsuarioComponente arbol)
        {
            CargarPermisosDisponibles(ddlPermisoAAsignar, btnAsignarPermiso, bll_permiso.ObtenerPermisos(), ddlPaginaAAsignar.SelectedValue, arbol.TienePermiso);
        }

        protected void ddlPaginaAAsignar_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                int? idUsuario = ObtenerIdUsuarioSeleccionado();
                BE_Usuario usuario = bll_usuario.Usuarios().FirstOrDefault(u => u.IdUsuario == idUsuario);

                if (usuario != null)
                {
                    CargarPermisosDePagina(bll_permiso.ConstruirArbolUsuario(usuario));
                }
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("AsignacionSeguridad.ddlPaginaAAsignar_SelectedIndexChanged", ex);
                MostrarMensaje(lblMensajeAsignacion, T("No se pudieron cargar los permisos de la pagina."), false);
            }
        }

        private void CargarArbolVisual(UsuarioComponente arbol)
        {
            trvArbolUsuario.Nodes.Clear();

            TreeNode nodoRaiz = CrearNodo("Usuario: " + arbol.Usuario.NombreApellido);

            AgregarNodosHijos(nodoRaiz, arbol.Asignaciones, codigo => "Permiso: " + codigo);

            trvArbolUsuario.Nodes.Add(nodoRaiz);
            trvArbolUsuario.ExpandAll();
        }

        private int? ObtenerIdUsuarioSeleccionado()
        {
            return ViewState["IdUsuarioSeleccionado"] as int?;
        }

        protected void btnAsignarRol_Click(object sender, EventArgs e)
        {
            if (!ExigirPermiso("ASIGNAR_ROL_USUARIO"))
            {
                return;
            }

            try
            {
                int? idUsuario = ObtenerIdUsuarioSeleccionado();

                if (idUsuario == null || string.IsNullOrEmpty(ddlRolAAsignar.SelectedValue))
                {
                    return;
                }

                int idRol = int.Parse(ddlRolAAsignar.SelectedValue);

                bll_permiso.AsignarRolAUsuario(UsuarioLogueado, idUsuario.Value, idRol);

                CargarAsignacion(idUsuario.Value);
                CargarUsuarios();

                lblMensajeAsignacion.Text = string.Empty;
                MostrarToast(T("Rol asignado correctamente."), true);
            }
            catch (InvalidOperationException ex)
            {
                MostrarMensaje(lblMensajeAsignacion, ex.Message, false);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("AsignacionSeguridad.btnAsignarRol_Click", ex);
                MostrarMensaje(lblMensajeAsignacion, T("No se pudo asignar el rol."), false);
            }
        }

        protected void gvRolesDelUsuario_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "Quitar")
            {
                return;
            }

            if (!ExigirPermiso("ASIGNAR_ROL_USUARIO"))
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

                int idRol = int.Parse(e.CommandArgument.ToString());

                bll_permiso.QuitarRolDeUsuario(UsuarioLogueado, idUsuario.Value, idRol);

                CargarAsignacion(idUsuario.Value);
                CargarUsuarios();

                lblMensajeAsignacion.Text = string.Empty;
                MostrarToast(T("Rol quitado correctamente."), true);
            }
            catch (InvalidOperationException ex)
            {
                MostrarMensaje(lblMensajeAsignacion, ex.Message, false);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("AsignacionSeguridad.gvRolesDelUsuario_RowCommand", ex);
                MostrarMensaje(lblMensajeAsignacion, T("No se pudo quitar el rol."), false);
            }
        }

        protected void btnAsignarPermiso_Click(object sender, EventArgs e)
        {
            if (!ExigirPermiso("ASIGNAR_PERMISO_USUARIO"))
            {
                return;
            }

            try
            {
                int? idUsuario = ObtenerIdUsuarioSeleccionado();

                if (idUsuario == null || string.IsNullOrEmpty(ddlPermisoAAsignar.SelectedValue))
                {
                    return;
                }

                int idPermiso = int.Parse(ddlPermisoAAsignar.SelectedValue);

                bll_permiso.AsignarPermisoAUsuario(UsuarioLogueado, idUsuario.Value, idPermiso);

                CargarAsignacion(idUsuario.Value);

                lblMensajeAsignacion.Text = string.Empty;
                MostrarToast(T("Permiso asignado correctamente."), true);
            }
            catch (InvalidOperationException ex)
            {
                MostrarMensaje(lblMensajeAsignacion, ex.Message, false);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("AsignacionSeguridad.btnAsignarPermiso_Click", ex);
                MostrarMensaje(lblMensajeAsignacion, T("No se pudo asignar el permiso."), false);
            }
        }

        protected void gvPermisosDelUsuario_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "Quitar")
            {
                return;
            }

            if (!ExigirPermiso("ASIGNAR_PERMISO_USUARIO"))
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

                int idPermiso = int.Parse(e.CommandArgument.ToString());

                bll_permiso.QuitarPermisoDeUsuario(UsuarioLogueado, idUsuario.Value, idPermiso);

                CargarAsignacion(idUsuario.Value);

                lblMensajeAsignacion.Text = string.Empty;
                MostrarToast(T("Permiso quitado correctamente."), true);
            }
            catch (InvalidOperationException ex)
            {
                MostrarMensaje(lblMensajeAsignacion, ex.Message, false);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("AsignacionSeguridad.gvPermisosDelUsuario_RowCommand", ex);
                MostrarMensaje(lblMensajeAsignacion, T("No se pudo quitar el permiso."), false);
            }
        }
    }
}
