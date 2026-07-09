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
            }
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

                bll_dvv.GenerarBackUp(usuarioLogueado, txtRutaBackup.Text);

                CerrarSesionYVolverALogin("Se generó el backup correctamente. Tenés que volver a iniciar sesión.");
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

        //Estas 3 acciones cambian el estado de la base o su línea base de integridad: forzamos a volver a pasar por el login normal (que vuelve a verificar integridad) en vez de dejar la sesión de Webmaster abierta.
        private void CerrarSesionYVolverALogin(string mensaje)
        {
            Session.Clear();
            Session["MensajeLogout"] = mensaje;
            Response.Redirect("Login.aspx");
        }
    }
}