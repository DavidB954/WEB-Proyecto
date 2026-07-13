using BE;
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
    public partial class WebForm1 : System.Web.UI.Page
    {
        BLL_Usuario bll_usuario = new BLL_Usuario();
        BLL_Rol bll_rol = new BLL_Rol();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BE_Usuario usuarioLogueado = Session["Usuario"] as BE_Usuario;

                if (!SeguridadHelper.TieneAcceso(usuarioLogueado, "ADMINISTRADOR"))
                {
                    Response.Redirect(usuarioLogueado == null ? "Login.aspx" : "AccesoDenegado.aspx");
                    return;
                }

                CargarRoles();
                RefrescarGrillas();
            }
        }

        //Rebinda las dos grillas (ABM y roles) con los datos actuales. Se llama tras cualquier alta/baja/modificación para que el rol nuevo aparezca al toque.
        private void RefrescarGrillas()
        {
            List<BE_Usuario> usuarios = bll_usuario.UsuariosConRol();

            gvUsuarios.DataSource = usuarios;
            gvUsuarios.DataBind();

            gvUsuariosRoles.DataSource = usuarios;
            gvUsuariosRoles.DataBind();
        }

        private void CargarRoles()
        {
            ddlRoles.DataSource = bll_rol.ObtenerRoles();
            ddlRoles.DataTextField = "Nombre";
            ddlRoles.DataValueField = "IdRol";
            ddlRoles.DataBind();

            //Opción por defecto para que no quede preseleccionado el primer rol de la lista.
            ddlRoles.Items.Insert(0, new ListItem("-- Seleccionar Rol --", ""));
        }

        //Muestra un mensaje flotante (toast) que se cierra solo, en vez de dejar un texto fijo en la página.
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

        // =====================================================================
        //  ABM DE USUARIOS
        // =====================================================================

        //Limpia el formulario de ABM y refresca las grillas.
        private void LimpiarFormularioUsuario()
        {
            txtNombre.Text = string.Empty;
            txtApellido.Text = string.Empty;
            txtDNI.Text = string.Empty;
            txtEmail.Text = string.Empty;
            txtPassword.Text = string.Empty;
            hiddenIdUsuario.Value = string.Empty;
            chkActivo.Checked = true;

            RefrescarGrillas();
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtNombre.Text) ||
                string.IsNullOrWhiteSpace(txtApellido.Text) ||
                string.IsNullOrWhiteSpace(txtDNI.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text) ||
                string.IsNullOrWhiteSpace(txtPassword.Text))
                {
                    lblMensaje.Text = "Debe completar todos los campos antes de guardar.";
                    lblMensaje.ForeColor = System.Drawing.Color.Red;
                    return; // corta la ejecución, no guarda
                }

                BE_Usuario usuario = new BE_Usuario();

                usuario.Nombre = txtNombre.Text;
                usuario.Apellido = txtApellido.Text;
                usuario.Email = txtEmail.Text;
                usuario.HashPassword = txtPassword.Text;
                usuario.DNI = txtDNI.Text;
                usuario.IntentosFallidos = 0;
                usuario.Activo = chkActivo.Checked;

                //El DVH lo calcula la capa de negocio (BLL_Usuario) con la contraseña ya hasheada; acá no se arma.
                BE_Usuario usuarioLogueado = (BE_Usuario)Session["Usuario"];

                bll_usuario.AgregarUsuario(usuarioLogueado, usuario);

                LimpiarFormularioUsuario();

                lblMensaje.Text = string.Empty;
                MostrarToast("Usuario guardado correctamente.", true);
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "errorAlert", $"alert('Error: {ex.Message}');", true);
            }
        }

        protected void gvUsuarios_SelectedIndexChanged(object sender, EventArgs e)
        {
            GridViewRow fila = gvUsuarios.SelectedRow;

            hiddenIdUsuario.Value = fila.Cells[1].Text;
            txtNombre.Text = fila.Cells[2].Text;
            txtApellido.Text = fila.Cells[3].Text;
            txtDNI.Text = fila.Cells[4].Text;
            txtEmail.Text = fila.Cells[5].Text;

            //La columna Activo es un CheckBoxField: el control real está dentro de la celda (índice 8, después de Rol e Intentos Fallidos).
            if (fila.Cells[8].Controls.Count > 0 && fila.Cells[8].Controls[0] is CheckBox chk)
            {
                chkActivo.Checked = chk.Checked;
            }

            //La contraseña nunca viaja de vuelta al navegador: se deja vacía y hay que reescribirla para modificar.
            txtPassword.Text = string.Empty;
        }

        protected void btnModificar_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(hiddenIdUsuario.Value))
                {
                    lblMensaje.Text = "Seleccione un usuario de la lista antes de modificar.";
                    lblMensaje.ForeColor = System.Drawing.Color.Red;
                    return;
                }

                //La contraseña no vuelve del navegador tras seleccionar una fila. Si se guardara vacía, se hashearía "" y el usuario no podría loguearse: por eso se exige reescribirla.
                if (string.IsNullOrWhiteSpace(txtPassword.Text))
                {
                    lblMensaje.Text = "Debe reingresar la contraseña para modificar el usuario.";
                    lblMensaje.ForeColor = System.Drawing.Color.Red;
                    return;
                }

                BE_Usuario usuario = new BE_Usuario();

                usuario.IdUsuario = int.Parse(hiddenIdUsuario.Value);
                usuario.Nombre = txtNombre.Text;
                usuario.Apellido = txtApellido.Text;
                usuario.Email = txtEmail.Text;
                usuario.HashPassword = txtPassword.Text;
                usuario.DNI = txtDNI.Text;
                usuario.Activo = chkActivo.Checked;

                //El DVH lo calcula la capa de negocio (BLL_Usuario) con la contraseña ya hasheada; acá no se arma.
                BE_Usuario usuarioLogueado = (BE_Usuario)Session["Usuario"];

                bll_usuario.ModificarUsuario(usuarioLogueado, usuario);

                LimpiarFormularioUsuario();

                lblMensaje.Text = string.Empty;
                MostrarToast("Usuario modificado correctamente.", true);
            }
            catch (Exception ex)
            {
                lblMensaje.Text = ex.Message;
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
                BE_Usuario usuarioLogueado = (BE_Usuario)Session["Usuario"];

                bll_usuario.EliminarUsuario(usuarioLogueado, id);

                LimpiarFormularioUsuario();

                lblMensaje.Text = string.Empty;
                MostrarToast("Usuario eliminado correctamente.", true);
            }
            catch (Exception ex)
            {
                lblMensaje.Text = ex.Message;
                lblMensaje.ForeColor = System.Drawing.Color.Red;
            }
        }

        // =====================================================================
        //  ASIGNACIÓN DE ROLES
        // =====================================================================

        protected void gvUsuariosRoles_SelectedIndexChanged(object sender, EventArgs e)
        {
            GridViewRow fila = gvUsuariosRoles.SelectedRow;

            //El Id viaja por DataKeyNames (no como columna visible), así que lo tomamos de DataKeys.
            hiddenIdUsuarioRol.Value = gvUsuariosRoles.DataKeys[fila.RowIndex].Value.ToString();

            //Nombre + Apellido de las columnas 1 y 2 (0 es el botón Seleccionar).
            txtUsuarioRol.Text = $"{fila.Cells[1].Text} {fila.Cells[2].Text}";

            //Preseleccionamos en el combo el rol que ya tiene; si no tiene, dejamos "-- Seleccionar Rol --".
            BE_Rol rolActual = bll_rol.ObtenerRolDeUsuario(int.Parse(hiddenIdUsuarioRol.Value));
            ddlRoles.SelectedValue = rolActual != null ? rolActual.IdRol.ToString() : "";
        }

        //Valida que haya un usuario seleccionado en la grilla de roles. Devuelve false y muestra mensaje si no.
        private bool HayUsuarioRolSeleccionado()
        {
            if (string.IsNullOrWhiteSpace(hiddenIdUsuarioRol.Value))
            {
                lblMensajeRol.Text = "Seleccione un usuario de la lista antes de operar sobre su rol.";
                lblMensajeRol.ForeColor = System.Drawing.Color.Red;
                return false;
            }
            return true;
        }

        //Valida que se haya elegido un rol del combo (no la opción "-- Seleccionar Rol --").
        private bool HayRolSeleccionado()
        {
            if (string.IsNullOrEmpty(ddlRoles.SelectedValue))
            {
                lblMensajeRol.Text = "Seleccione un rol del listado.";
                lblMensajeRol.ForeColor = System.Drawing.Color.Red;
                return false;
            }
            return true;
        }

        protected void btnGuardarRol_Click(object sender, EventArgs e)
        {
            try
            {
                if (!HayUsuarioRolSeleccionado() || !HayRolSeleccionado())
                {
                    return;
                }

                int idUsuarioDestino = int.Parse(hiddenIdUsuarioRol.Value);
                int idRol = int.Parse(ddlRoles.SelectedValue);

                //Si el usuario ya tiene exactamente ese rol, avisamos y no hacemos nada (evita reasignar lo mismo).
                BE_Rol rolActual = bll_rol.ObtenerRolDeUsuario(idUsuarioDestino);
                if (rolActual != null && rolActual.IdRol == idRol)
                {
                    lblMensajeRol.Text = $"El usuario ya tiene asignado el rol {rolActual.Nombre}.";
                    lblMensajeRol.ForeColor = System.Drawing.Color.Red;
                    return;
                }

                BE_Usuario usuarioLogueado = (BE_Usuario)Session["Usuario"];

                bll_rol.AsignarRol(usuarioLogueado, idUsuarioDestino, idRol);

                RefrescarGrillas();

                lblMensajeRol.Text = string.Empty;
                MostrarToast("Rol asignado correctamente.", true);
            }
            catch (Exception ex)
            {
                lblMensajeRol.Text = ex.Message;
                lblMensajeRol.ForeColor = System.Drawing.Color.Red;
            }
        }

        protected void btnModificarRol_Click(object sender, EventArgs e)
        {
            try
            {
                if (!HayUsuarioRolSeleccionado() || !HayRolSeleccionado())
                {
                    return;
                }

                int idUsuarioDestino = int.Parse(hiddenIdUsuarioRol.Value);
                int idRol = int.Parse(ddlRoles.SelectedValue);

                BE_Rol rolActual = bll_rol.ObtenerRolDeUsuario(idUsuarioDestino);

                if (rolActual == null)
                {
                    lblMensajeRol.Text = "El usuario no tiene un rol para modificar. Use 'Asignar Rol'.";
                    lblMensajeRol.ForeColor = System.Drawing.Color.Red;
                    return;
                }

                if (rolActual.IdRol == idRol)
                {
                    lblMensajeRol.Text = $"El usuario ya tiene asignado el rol {rolActual.Nombre}.";
                    lblMensajeRol.ForeColor = System.Drawing.Color.Red;
                    return;
                }

                BE_Usuario usuarioLogueado = (BE_Usuario)Session["Usuario"];

                bll_rol.ModificarRol(usuarioLogueado, idUsuarioDestino, idRol);

                RefrescarGrillas();

                lblMensajeRol.Text = string.Empty;
                MostrarToast("Rol modificado correctamente.", true);
            }
            catch (Exception ex)
            {
                lblMensajeRol.Text = ex.Message;
                lblMensajeRol.ForeColor = System.Drawing.Color.Red;
            }
        }

        protected void btnEliminarRol_Click(object sender, EventArgs e)
        {
            try
            {
                if (!HayUsuarioRolSeleccionado())
                {
                    return;
                }

                int idUsuarioDestino = int.Parse(hiddenIdUsuarioRol.Value);

                BE_Rol rolActual = bll_rol.ObtenerRolDeUsuario(idUsuarioDestino);

                if (rolActual == null)
                {
                    lblMensajeRol.Text = "El usuario no tiene ningún rol asignado.";
                    lblMensajeRol.ForeColor = System.Drawing.Color.Red;
                    return;
                }

                BE_Usuario usuarioLogueado = (BE_Usuario)Session["Usuario"];

                //Eliminamos el rol que realmente tiene (no el seleccionado en el combo, que podría ser otro).
                bll_rol.EliminarRol(usuarioLogueado, idUsuarioDestino, rolActual.IdRol);

                RefrescarGrillas();

                lblMensajeRol.Text = string.Empty;
                MostrarToast($"Se quitó el rol {rolActual.Nombre} al usuario.", true);
            }
            catch (Exception ex)
            {
                lblMensajeRol.Text = ex.Message;
                lblMensajeRol.ForeColor = System.Drawing.Color.Red;
            }
        }
    }
}
