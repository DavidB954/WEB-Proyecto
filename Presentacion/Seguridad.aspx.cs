using BE;
using BLL;
using SERVICIOS;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Presentacion
{
    public partial class Seguridad : System.Web.UI.Page
    {
        BLL_DVV bll_dvv = new BLL_DVV();

        protected void Page_Load(object sender, EventArgs e)
        {
            BE_Usuario usuarioLogueado = Session["Usuario"] as BE_Usuario;

            if (!SeguridadHelper.TieneAcceso(usuarioLogueado, "WEBMASTER"))
            {
                Response.Redirect(usuarioLogueado == null ? "Login.aspx" : "AccesoDenegado.aspx");
                return;
            }

            if (!IsPostBack)
            {
                try
                {
                    MostrarMensajesIntegridad();
                    ConfigurarDisponibilidadBackup();
                    CargarBackupsDisponibles();
                }
                catch (Exception ex)
                {
                    RegistrarErrorInterno("Seguridad.Page_Load", ex);
                    lblMensaje.Text = "No se pudo cargar la pantalla de seguridad. Intentá nuevamente más tarde.";
                    lblMensaje.ForeColor = System.Drawing.Color.Red;
                }
            }
        }

        private void CargarBackupsDisponibles()
        {
            var backups = bll_dvv.ListarBackupsDisponibles();

            ddlBackups.Items.Clear();

            if (backups.Count == 0)
            {
                ddlBackups.Items.Add(new ListItem("No hay backups disponibles", ""));
                btnRestore.Enabled = false;
                return;
            }

            foreach (var nombreArchivo in backups.OrderByDescending(n => n))
            {
                ddlBackups.Items.Add(new ListItem(FormatearNombreBackup(nombreArchivo), nombreArchivo));
            }

            btnRestore.Enabled = true;
        }

        private string FormatearNombreBackup(string nombreArchivo)
        {
            string soloNombre = System.IO.Path.GetFileNameWithoutExtension(nombreArchivo);
            string[] partes = soloNombre.Split('_');

            if (partes.Length >= 3 &&
                DateTime.TryParseExact(partes[partes.Length - 2] + partes[partes.Length - 1], "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fecha))
            {
                return $"{fecha:dd/MM/yyyy HH:mm:ss} ({nombreArchivo})";
            }

            return nombreArchivo;
        }

        private void ConfigurarDisponibilidadBackup()
        {
            bool baseCorrupta = !bll_dvv.EstaIntegra("Usuario")
                             || !bll_dvv.EstaIntegra("Rol")
                             || !bll_dvv.EstaIntegra("Bitacora");

            btnBackUp.Visible = !baseCorrupta;
        }

        private void MostrarMensajesIntegridad()
        {
            var mensajes = Session["MensajesIntegridad"] as List<string>;

            if (mensajes != null && mensajes.Count > 0)
            {
                lstMensajesIntegridad.DataSource = mensajes;
                lstMensajesIntegridad.DataBind();
                pnlIntegridad.Visible = true;

                Session.Remove("MensajesIntegridad");
            }
        }

        protected void btnRecalcular_Click(object sender, EventArgs e)
        {
            try
            {
                bll_dvv.RecalcularDVHFilas("Usuario");
                bll_dvv.RecalcularDVHFilas("Rol");
                bll_dvv.RecalcularDVHFilas("Bitacora");

                bll_dvv.ActualizarDVV("Usuario");
                bll_dvv.ActualizarDVV("Rol");
                bll_dvv.ActualizarDVV("Bitacora");

                CerrarSesionYVolverALogin("Se recalcularon los dígitos verificadores correctamente. Tenés que volver a iniciar sesión.");
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("Seguridad.btnRecalcular_Click", ex);
                lblMensaje.Text = "No se pudieron recalcular los dígitos verificadores. Intentá nuevamente más tarde.";
                lblMensaje.ForeColor = System.Drawing.Color.Red;
            }
        }

        protected void btnBackUp_Click(object sender, EventArgs e)
        {
            try
            {
                BE_Usuario usuarioLogueado = Session["Usuario"] as BE_Usuario;

                string rutaGenerada = bll_dvv.GenerarBackUp(usuarioLogueado);

                lblMensaje.Text = $"Backup generado en: {rutaGenerada}";
                lblMensaje.ForeColor = System.Drawing.Color.Green;

                CargarBackupsDisponibles();
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("Seguridad.btnBackUp_Click", ex);
                lblMensaje.Text = "No se pudo generar el backup. Intentá nuevamente más tarde.";
                lblMensaje.ForeColor = System.Drawing.Color.Red;
            }
        }

        protected void btnRestore_Click(object sender, EventArgs e)
        {
            try
            {
                BE_Usuario usuarioLogueado = Session["Usuario"] as BE_Usuario;

                bll_dvv.RestaurarBackup(usuarioLogueado, ddlBackups.SelectedValue);

                CerrarSesionYVolverALogin("Se restauró la base de datos correctamente. Tenés que volver a iniciar sesión.");
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("Seguridad.btnRestore_Click", ex);
                lblMensaje.Text = "No se pudo restaurar el backup. Intentá nuevamente más tarde.";
                lblMensaje.ForeColor = System.Drawing.Color.Red;
            }
        }


        private void CerrarSesionYVolverALogin(string mensaje)
        {
            Session.Clear();
            Session["MensajeLogout"] = mensaje;
            Response.Redirect("Login.aspx");
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