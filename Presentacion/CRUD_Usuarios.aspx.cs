using BE;
using BE.Seguridad;
using BLL;
using SERVICIOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Presentacion
{
    public partial class WebForm1 : PaginaBase
    {
        BLL_Usuario bll_usuario = new BLL_Usuario();

        protected void Page_Load(object sender, EventArgs e)
        {
            BE_Usuario usuarioLogueado = Session["Usuario"] as BE_Usuario;
            UsuarioComponente permisos = Session["Permisos"] as UsuarioComponente;

            if (!SeguridadHelper.TieneAcceso(usuarioLogueado, permisos, "ABM_USUARIO"))
            {
                Response.Redirect(usuarioLogueado == null ? "Login.aspx" : "AccesoDenegado.aspx");
                return;
            }

            if (!IsPostBack)
            {
                try
                {
                    RefrescarGrillas();
                }
                catch (Exception ex)
                {
                    RegistrarErrorInterno("CRUD_Usuarios.Page_Load", ex);
                    lblMensaje.Text = "No se pudieron cargar los datos. Intenta nuevamente mas tarde.";
                    lblMensaje.ForeColor = System.Drawing.Color.Red;
                }
            }
        }

        private void RefrescarGrillas()
        {
            List<BE_Usuario> usuarios = bll_usuario.UsuariosConRol();

            gvUsuarios.DataSource = usuarios;
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
                    lblMensaje.Text = "Debe completar todos los campos antes de guardar.";
                    lblMensaje.ForeColor = System.Drawing.Color.Red;
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

                BE_Usuario usuarioLogueado = Session["Usuario"] as BE_Usuario;

                bll_usuario.AgregarUsuario(usuarioLogueado, usuario);

                LimpiarFormularioUsuario();

                lblMensaje.Text = string.Empty;
                MostrarToast("Usuario guardado correctamente.", true);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("CRUD_Usuarios.btnGuardar_Click", ex);
                string mensaje = System.Web.HttpUtility.JavaScriptStringEncode("No se pudo guardar el usuario. Verifica los datos e intenta nuevamente.");
                ScriptManager.RegisterStartupScript(this, GetType(), "errorAlert", $"alert('{mensaje}');", true);
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
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("CRUD_Usuarios.gvUsuarios_SelectedIndexChanged", ex);
                lblMensaje.Text = "No se pudo cargar el usuario seleccionado.";
                lblMensaje.ForeColor = System.Drawing.Color.Red;
            }
        }

        protected void btnModificar_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Page.IsValid)
                {
                    return;
                }

                if (string.IsNullOrWhiteSpace(hiddenIdUsuario.Value))
                {
                    lblMensaje.Text = "Seleccione un usuario de la lista antes de modificar.";
                    lblMensaje.ForeColor = System.Drawing.Color.Red;
                    return;
                }

                bool restablecerPassword = chkResetearPassword.Checked;

                if (restablecerPassword && string.IsNullOrWhiteSpace(txtPassword.Text))
                {
                    lblMensaje.Text = "Ingrese la nueva contraseña para restablecerla.";
                    lblMensaje.ForeColor = System.Drawing.Color.Red;
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

                BE_Usuario usuarioLogueado = Session["Usuario"] as BE_Usuario;

                bll_usuario.ModificarUsuario(usuarioLogueado, usuario, restablecerPassword);

                LimpiarFormularioUsuario();

                lblMensaje.Text = string.Empty;
                MostrarToast("Usuario modificado correctamente.", true);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("CRUD_Usuarios.btnModificar_Click", ex);
                lblMensaje.Text = "No se pudo modificar el usuario. Intenta nuevamente mas tarde.";
                lblMensaje.ForeColor = System.Drawing.Color.Red;
            }
        }

        protected void btnEliminar_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(hiddenIdUsuario.Value))
                {
                    lblMensaje.Text = "Seleccione un usuario de la lista antes de eliminar.";
                    lblMensaje.ForeColor = System.Drawing.Color.Red;
                    return;
                }

                int id = int.Parse(hiddenIdUsuario.Value);
                BE_Usuario usuarioLogueado = Session["Usuario"] as BE_Usuario;

                bll_usuario.EliminarUsuario(usuarioLogueado, id);

                LimpiarFormularioUsuario();

                lblMensaje.Text = string.Empty;
                MostrarToast("Usuario eliminado correctamente.", true);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("CRUD_Usuarios.btnEliminar_Click", ex);
                lblMensaje.Text = "No se pudo eliminar el usuario. Intenta nuevamente mas tarde.";
                lblMensaje.ForeColor = System.Drawing.Color.Red;
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
