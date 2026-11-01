using BLL;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI.WebControls;

namespace Presentacion
{
    public partial class Seguridad : PaginaBase
    {
        BLL_DVV bll_dvv = new BLL_DVV();

        protected void Page_Load(object sender, EventArgs e)
        {
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
                    MostrarMensaje(lblMensaje, T("No se pudo cargar la pantalla de seguridad. Intenta nuevamente mas tarde."), false);
                }
            }
        }

        private void CargarBackupsDisponibles()
        {
            var backups = bll_dvv.ListarBackupsDisponibles();

            ddlBackups.Items.Clear();

            if (backups.Count == 0)
            {
                ddlBackups.Items.Add(new ListItem(T("No hay backups disponibles"), ""));
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
            if (!ExigirPermiso("DV_RECALCULAR"))
            {
                return;
            }

            try
            {
                bll_dvv.RecalcularDVHFilas("Usuario");
                bll_dvv.RecalcularDVHFilas("Rol");
                bll_dvv.RecalcularDVHFilas("Bitacora");

                bll_dvv.ActualizarDVV("Usuario");
                bll_dvv.ActualizarDVV("Rol");
                bll_dvv.ActualizarDVV("Bitacora");

                CerrarSesionYVolverALogin(T("Se recalcularon los digitos verificadores correctamente. Tenes que volver a iniciar sesion."));
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("Seguridad.btnRecalcular_Click", ex);
                MostrarMensaje(lblMensaje, T("No se pudieron recalcular los digitos verificadores. Intenta nuevamente mas tarde."), false);
            }
        }

        protected void btnBackUp_Click(object sender, EventArgs e)
        {
            if (!ExigirPermiso("BACKUP_GENERAR"))
            {
                return;
            }

            try
            {
                string rutaGenerada = bll_dvv.GenerarBackUp(UsuarioLogueado);

                MostrarMensaje(lblMensaje, T("Backup generado en: {0}", rutaGenerada), true);

                CargarBackupsDisponibles();
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("Seguridad.btnBackUp_Click", ex);
                MostrarMensaje(lblMensaje, T("No se pudo generar el backup. Intenta nuevamente mas tarde."), false);
            }
        }

        protected void btnRestore_Click(object sender, EventArgs e)
        {
            if (!ExigirPermiso("BACKUP_RESTAURAR"))
            {
                return;
            }

            try
            {
                bll_dvv.RestaurarBackup(UsuarioLogueado, ddlBackups.SelectedValue);

                CerrarSesionYVolverALogin(T("Se restauro la base de datos correctamente. Tenes que volver a iniciar sesion."));
            }
            catch (InvalidOperationException ex)
            {
                MostrarMensaje(lblMensaje, ex.Message, false);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("Seguridad.btnRestore_Click", ex);
                MostrarMensaje(lblMensaje, T("No se pudo restaurar el backup. Intenta nuevamente mas tarde."), false);
            }
        }

        private void CerrarSesionYVolverALogin(string mensaje)
        {
            Session.Clear();
            Session["MensajeLogout"] = mensaje;

            Response.Redirect("Login.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}