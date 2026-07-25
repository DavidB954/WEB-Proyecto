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
            //Se revalida en CADA carga (incluidos los postbacks de Guardar/Modificar/Eliminar/Roles), no solo la
            //primera vez: si la sesión vence mientras el administrador está en esta pantalla, un postback no debe
            //poder ejecutar un alta/baja/modificación de usuarios ni de roles sin sesión válida.
            BE_Usuario usuarioLogueado = Session["Usuario"] as BE_Usuario;

            if (!SeguridadHelper.TieneAcceso(usuarioLogueado, "ADMINISTRADOR"))
            {
                Response.Redirect(usuarioLogueado == null ? "Login.aspx" : "AccesoDenegado.aspx");
                return;
            }

            if (!IsPostBack)
            {
                try
                {
                    CargarRoles();
                    RefrescarGrillas();
                }
                catch (Exception ex)
                {
                    RegistrarErrorInterno("CRUD_Usuarios.Page_Load", ex);
                    lblMensaje.Text = "No se pudieron cargar los datos. Intentá nuevamente más tarde.";
                    lblMensaje.ForeColor = System.Drawing.Color.Red;
                }
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
            chkResetearPassword.Checked = false;

            RefrescarGrillas();
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                //Los RegularExpressionValidator del markup son solo del lado del cliente: si el postback llega
                //con JavaScript deshabilitado (o armado a mano), hay que rechazarlo también acá.
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
                BE_Usuario usuarioLogueado = Session["Usuario"] as BE_Usuario;

                bll_usuario.AgregarUsuario(usuarioLogueado, usuario);

                LimpiarFormularioUsuario();

                lblMensaje.Text = string.Empty;
                MostrarToast("Usuario guardado correctamente.", true);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("CRUD_Usuarios.btnGuardar_Click", ex);
                string mensaje = System.Web.HttpUtility.JavaScriptStringEncode("No se pudo guardar el usuario. Verificá los datos e intentá nuevamente.");
                ScriptManager.RegisterStartupScript(this, GetType(), "errorAlert", $"alert('{mensaje}');", true);
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

            //La contraseña nunca viaja de vuelta al navegador: se deja vacía. Si se quiere cambiar, hay que
            //tildar "Restablecer contraseña" y reescribirla; si no, "Modificar" preserva la actual.
            txtPassword.Text = string.Empty;
            chkResetearPassword.Checked = false;
        }

        protected void btnModificar_Click(object sender, EventArgs e)
        {
            try
            {
                //Los RegularExpressionValidator del markup son solo del lado del cliente: si el postback llega
                //con JavaScript deshabilitado (o armado a mano), hay que rechazarlo también acá.
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

                //"Restablecer contraseña" es opcional: solo si está tildado hace falta reescribirla. Si no,
                //ModificarUsuario preserva la contraseña actual del usuario sin tocarla.
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
                usuario.DNI = txtDNI.Text;
                usuario.Activo = chkActivo.Checked;

                //Este formulario no edita los intentos fallidos: hay que preservar el valor actual en base,
                //si no, al no setearlo acá quedaría en el default de int (0) y CUALQUIER modificación
                //(aunque sea solo corregir el nombre) resetearía silenciosamente el contador de intentos fallidos.
                BE_Usuario usuarioActual = bll_usuario.Usuarios().FirstOrDefault(u => u.IdUsuario == usuario.IdUsuario);
                usuario.IntentosFallidos = usuarioActual?.IntentosFallidos ?? 0;

                //El DVH lo calcula la capa de negocio (BLL_Usuario). Si no se restablece la contraseña, BLL_Usuario
                //ignora usuario.HashPassword y preserva el valor actual (ya hasheado) tal como está en la base.
                BE_Usuario usuarioLogueado = Session["Usuario"] as BE_Usuario;

                bll_usuario.ModificarUsuario(usuarioLogueado, usuario, restablecerPassword);

                LimpiarFormularioUsuario();

                lblMensaje.Text = string.Empty;
                MostrarToast("Usuario modificado correctamente.", true);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("CRUD_Usuarios.btnModificar_Click", ex);
                lblMensaje.Text = "No se pudo modificar el usuario. Intentá nuevamente más tarde.";
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
                lblMensaje.Text = "No se pudo eliminar el usuario. Intentá nuevamente más tarde.";
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

                BE_Usuario usuarioLogueado = Session["Usuario"] as BE_Usuario;

                bll_rol.AsignarRol(usuarioLogueado, idUsuarioDestino, idRol);

                RefrescarGrillas();

                lblMensajeRol.Text = string.Empty;
                MostrarToast("Rol asignado correctamente.", true);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("CRUD_Usuarios.btnGuardarRol_Click", ex);
                lblMensajeRol.Text = "No se pudo asignar el rol. Intentá nuevamente más tarde.";
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

                BE_Usuario usuarioLogueado = Session["Usuario"] as BE_Usuario;

                bll_rol.ModificarRol(usuarioLogueado, idUsuarioDestino, idRol);

                RefrescarGrillas();

                lblMensajeRol.Text = string.Empty;
                MostrarToast("Rol modificado correctamente.", true);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("CRUD_Usuarios.btnModificarRol_Click", ex);
                lblMensajeRol.Text = "No se pudo modificar el rol. Intentá nuevamente más tarde.";
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

                BE_Usuario usuarioLogueado = Session["Usuario"] as BE_Usuario;

                //Eliminamos el rol que realmente tiene (no el seleccionado en el combo, que podría ser otro).
                bll_rol.EliminarRol(usuarioLogueado, idUsuarioDestino, rolActual.IdRol);

                RefrescarGrillas();

                lblMensajeRol.Text = string.Empty;
                MostrarToast($"Se quitó el rol {rolActual.Nombre} al usuario.", true);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("CRUD_Usuarios.btnEliminarRol_Click", ex);
                lblMensajeRol.Text = "No se pudo quitar el rol. Intentá nuevamente más tarde.";
                lblMensajeRol.ForeColor = System.Drawing.Color.Red;
            }
        }

        //Deja rastro en el mismo log que usa Global.asax, sin mostrarle al usuario el detalle interno de la excepción.
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
                //Si ni el log funciona, no hay nada más para hacer acá.
            }
        }
    }
}
