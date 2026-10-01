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
    public partial class FormTraducciones : PaginaBase
    {
        BLL_Idioma bll_idioma = new BLL_Idioma();

        protected void Page_Load(object sender, EventArgs e)
        {
            BE_Usuario usuarioLogueado = Session["Usuario"] as BE_Usuario;
            UsuarioComponente permisos = Session["Permisos"] as UsuarioComponente;

            if (!SeguridadHelper.TieneAcceso(usuarioLogueado, permisos, "GESTION_TRADUCCIONES"))
            {
                Response.Redirect(usuarioLogueado == null ? "Login.aspx" : "AccesoDenegado.aspx");
                return;
            }

            if (!IsPostBack)
            {
                try
                {
                    CargarIdiomas();
                    RefrescarGrilla();
                }
                catch (Exception ex)
                {
                    RegistrarErrorInterno("FormTraducciones.Page_Load", ex);
                    lblMensaje.Text = "No se pudieron cargar las traducciones. Intenta nuevamente mas tarde.";
                    lblMensaje.ForeColor = System.Drawing.Color.Red;
                }
            }
        }

        private void CargarIdiomas()
        {
            ddlIdiomaEditar.DataSource = bll_idioma.ObtenerIdiomas();
            ddlIdiomaEditar.DataTextField = "Nombre";
            ddlIdiomaEditar.DataValueField = "IdIdioma";
            ddlIdiomaEditar.DataBind();
        }

        private void RefrescarGrilla()
        {
            if (ddlIdiomaEditar.Items.Count == 0)
            {
                return;
            }

            int idIdioma = int.Parse(ddlIdiomaEditar.SelectedValue);

            gvTraducciones.DataSource = bll_idioma.ObtenerTraduccionesParaGrilla(idIdioma);
            gvTraducciones.DataBind();
        }

        private void MostrarToast(string mensaje, bool exito)
        {
            string clase = exito ? "toast-exito" : "toast-error";
            string texto = HttpUtility.JavaScriptStringEncode(mensaje, true);

            string script =
                "(function(){var t=document.createElement('div');t.className='toast " + clase + "';" +
                "t.textContent=" + texto + ";document.body.appendChild(t);" +
                "setTimeout(function(){t.className+=' toast-hide';},3000);" +
                "setTimeout(function(){if(t.parentNode){t.parentNode.removeChild(t);}},3600);})();";

            ScriptManager.RegisterStartupScript(this, GetType(), "toast_" + Guid.NewGuid().ToString("N"), script, true);
        }

        protected void ddlIdiomaEditar_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                RefrescarGrilla();
                lblMensaje.Text = string.Empty;
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("FormTraducciones.ddlIdiomaEditar_SelectedIndexChanged", ex);
                lblMensaje.Text = "No se pudieron cargar las traducciones de ese idioma.";
                lblMensaje.ForeColor = System.Drawing.Color.Red;
            }
        }

        protected void btnGuardarTraducciones_Click(object sender, EventArgs e)
        {
            try
            {
                int idIdioma = int.Parse(ddlIdiomaEditar.SelectedValue);
                BE_Usuario usuarioLogueado = Session["Usuario"] as BE_Usuario;

                foreach (GridViewRow fila in gvTraducciones.Rows)
                {
                    if (fila.RowType != DataControlRowType.DataRow)
                    {
                        continue;
                    }

                    int idClave = (int)gvTraducciones.DataKeys[fila.RowIndex]["IdClave"];
                    TextBox txtTexto = fila.FindControl("txtTexto") as TextBox;

                    if (txtTexto == null)
                    {
                        continue;
                    }

                    bll_idioma.GuardarTraduccion(usuarioLogueado, idIdioma, idClave, txtTexto.Text);
                }

                GestorIdioma.InvalidarCache(idIdioma);

                RefrescarGrilla();

                lblMensaje.Text = string.Empty;
                MostrarToast("Traducciones guardadas correctamente.", true);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("FormTraducciones.btnGuardarTraducciones_Click", ex);
                lblMensaje.Text = "No se pudieron guardar las traducciones. Intenta nuevamente mas tarde.";
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
