using BE;
using BLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Presentacion
{
    public partial class WebForm1 : PaginaBase
    {
        BLL_Usuario bll_usuario = new BLL_Usuario();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                try
                {
                    RefrescarGrillas();
                }
                catch (Exception ex)
                {
                    RegistrarErrorInterno("CRUD_Usuarios.Page_Load", ex);
                    MostrarMensaje(lblMensaje, T("No se pudieron cargar los datos. Intenta nuevamente mas tarde."), false);
                }
            }
        }

        private void RefrescarGrillas()
        {
            List<BE_Usuario> usuarios = bll_usuario.UsuariosConRol(UsuarioLogueado);

            gvUsuarios.DataSource = usuarios;
            gvUsuarios.DataBind();
        }

        private void LimpiarFormularioUsuario()
        {
            txtNombre.Text = string.Empty;
            txtApellido.Text = string.Empty;
            txtDNI.Text = string.Empty;
            txtEmail.Text = string.Empty;
            txtPassword.Text = string.Empty;
            hiddenIdUsuario.Value = string.Empty;
            chkActivo.Checked = true;
            chkResetearPassword.Checked = false;
            chkResetearIntentos.Checked = false;

            RefrescarGrillas();
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            if (!ExigirPermiso("USUARIO_ALTA"))
            {
                return;
            }

            try
            {
                if (!Page.IsValid)
                {
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtNombre.Text) ||
                string.IsNullOrWhiteSpace(txtApellido.Text) ||
                string.IsNullOrWhiteSpace(txtDNI.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text) ||
                string.IsNullOrWhiteSpace(txtPassword.Text))
                {
                    MostrarMensaje(lblMensaje, T("Debe completar todos los campos antes de guardar."), false);
                    return;
                }

                BE_Usuario usuario = new BE_Usuario();

                usuario.Nombre = txtNombre.Text;
                usuario.Apellido = txtApellido.Text;
                usuario.Email = txtEmail.Text;
                usuario.HashPassword = txtPassword.Text;
                usuario.DNI = txtDNI.Text.Trim();
                usuario.IntentosFallidos = 0;
                usuario.Activo = chkActivo.Checked;

                bll_usuario.AgregarUsuario(UsuarioLogueado, usuario);

                LimpiarFormularioUsuario();

                lblMensaje.Text = string.Empty;
                MostrarToast(T("Usuario guardado correctamente."), true);
            }
            catch (InvalidOperationException ex)
            {
                MostrarMensaje(lblMensaje, ex.Message, false);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("CRUD_Usuarios.btnGuardar_Click", ex);
                string mensaje = System.Web.HttpUtility.JavaScriptStringEncode(T("No se pudo guardar el usuario. Verifica los datos e intenta nuevamente."));
                ScriptManager.RegisterStartupScript(this, GetType(), "errorAlert", $"mostrarAviso('{mensaje}');", true);
            }
        }

        protected void gvUsuarios_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                GridViewRow fila = gvUsuarios.SelectedRow;

                hiddenIdUsuario.Value = fila.Cells[1].Text;
                txtNombre.Text = fila.Cells[2].Text;
                txtApellido.Text = fila.Cells[3].Text;
                txtDNI.Text = fila.Cells[4].Text;
                txtEmail.Text = fila.Cells[5].Text;

                if (fila.Cells[8].Controls.Count > 0 && fila.Cells[8].Controls[0] is CheckBox chk)
                {
                    chkActivo.Checked = chk.Checked;
                }

                txtPassword.Text = string.Empty;
                chkResetearPassword.Checked = false;
                chkResetearIntentos.Checked = false;

                DesplazarHacia(hGestionUsuarios);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("CRUD_Usuarios.gvUsuarios_SelectedIndexChanged", ex);
                MostrarMensaje(lblMensaje, T("No se pudo cargar el usuario seleccionado."), false);
            }
        }

        protected void btnModificar_Click(object sender, EventArgs e)
        {
            if (!ExigirPermiso("USUARIO_MODIFICACION"))
            {
                return;
            }

            try
            {
                if (!Page.IsValid)
                {
                    return;
                }

                if (string.IsNullOrWhiteSpace(hiddenIdUsuario.Value))
                {
                    MostrarMensaje(lblMensaje, T("Seleccione un usuario de la lista antes de modificar."), false);
                    return;
                }

                bool restablecerPassword = chkResetearPassword.Checked;

                if (restablecerPassword && string.IsNullOrWhiteSpace(txtPassword.Text))
                {
                    MostrarMensaje(lblMensaje, T("Ingrese la nueva contraseña para restablecerla."), false);
                    return;
                }

                BE_Usuario usuario = new BE_Usuario();

                usuario.IdUsuario = int.Parse(hiddenIdUsuario.Value);
                usuario.Nombre = txtNombre.Text;
                usuario.Apellido = txtApellido.Text;
                usuario.Email = txtEmail.Text;
                usuario.HashPassword = txtPassword.Text;
                usuario.DNI = txtDNI.Text.Trim();
                usuario.Activo = chkActivo.Checked;

                if (chkResetearIntentos.Checked)
                {
                    usuario.IntentosFallidos = 0;
                }
                else
                {
                    BE_Usuario usuarioActual = bll_usuario.Usuarios().FirstOrDefault(u => u.IdUsuario == usuario.IdUsuario);
                    usuario.IntentosFallidos = usuarioActual?.IntentosFallidos ?? 0;
                }

                bll_usuario.ModificarUsuario(UsuarioLogueado, usuario, restablecerPassword);

                LimpiarFormularioUsuario();

                lblMensaje.Text = string.Empty;
                MostrarToast(T("Usuario modificado correctamente."), true);
            }
            catch (InvalidOperationException ex)
            {
                MostrarMensaje(lblMensaje, ex.Message, false);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("CRUD_Usuarios.btnModificar_Click", ex);
                MostrarMensaje(lblMensaje, T("No se pudo modificar el usuario. Intenta nuevamente mas tarde."), false);
            }
        }

        protected void btnEliminar_Click(object sender, EventArgs e)
        {
            if (!ExigirPermiso("USUARIO_BAJA"))
            {
                return;
            }

            try
            {
                if (string.IsNullOrWhiteSpace(hiddenIdUsuario.Value))
                {
                    MostrarMensaje(lblMensaje, T("Seleccione un usuario de la lista antes de eliminar."), false);
                    return;
                }

                int id = int.Parse(hiddenIdUsuario.Value);
                bll_usuario.EliminarUsuario(UsuarioLogueado, id);

                LimpiarFormularioUsuario();

                lblMensaje.Text = string.Empty;
                MostrarToast(T("Usuario eliminado correctamente."), true);
            }
            catch (InvalidOperationException ex)
            {
                MostrarMensaje(lblMensaje, ex.Message, false);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("CRUD_Usuarios.btnEliminar_Click", ex);
                MostrarMensaje(lblMensaje, T("No se pudo eliminar el usuario. Intenta nuevamente mas tarde."), false);
            }
        }
    }
}
