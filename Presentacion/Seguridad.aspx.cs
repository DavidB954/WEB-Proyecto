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
    public partial class Seguridad : System.Web.UI.Page
    {
        BLL_DVV bll_dvv = new BLL_DVV();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BE_Usuario usuarioLogueado = Session["Usuario"] as BE_Usuario;

                if (!SeguridadHelper.TieneAcceso(usuarioLogueado, "WEBMASTER"))
                {
                    Response.Redirect(usuarioLogueado == null ? "Login.aspx" : "AccesoDenegado.aspx");
                    return;
                }

                MostrarMensajesIntegridad();
                ConfigurarDisponibilidadBackup();
            }
        }

        //Si alguna tabla protegida está corrupta, "Generar BackUp" se oculta: no tiene sentido respaldar datos comprometidos.
        //Solo quedan disponibles "Restaurar BD" (volver a un backup sano) y "Recalcular DV" (aceptar el estado actual como válido).
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

                //Se muestra una sola vez; la próxima verificación (siguiente login) vuelve a completarlo si el problema persiste.
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
                lblMensaje.Text = ex.Message;
                lblMensaje.ForeColor = System.Drawing.Color.Red;
            }
        }

        protected void btnBackUp_Click(object sender, EventArgs e)
        {
            try
            {
                BE_Usuario usuarioLogueado = Session["Usuario"] as BE_Usuario;

                string rutaGenerada = bll_dvv.GenerarBackUp(usuarioLogueado, txtRutaBackup.Text);

                CerrarSesionYVolverALogin($"Backup generado en: {rutaGenerada}. Tenés que volver a iniciar sesión.");
            }
            catch (Exception ex)
            {
                lblMensaje.Text = ex.Message;
                lblMensaje.ForeColor = System.Drawing.Color.Red;
            }
        }

        protected void btnRestore_Click(object sender, EventArgs e)
        {
            try
            {
                BE_Usuario usuarioLogueado = Session["Usuario"] as BE_Usuario;

                bll_dvv.RestaurarBackup(usuarioLogueado, txtRutaBackup.Text);

                CerrarSesionYVolverALogin("Se restauró la base de datos correctamente. Tenés que volver a iniciar sesión.");
            }
            catch (Exception ex)
            {
                lblMensaje.Text = ex.Message;
                lblMensaje.ForeColor = System.Drawing.Color.Red;
            }
        }

        
        private void CerrarSesionYVolverALogin(string mensaje)
        {
            Session.Clear();
            Session["MensajeLogout"] = mensaje;
            Response.Redirect("Login.aspx");
        }
    }
}