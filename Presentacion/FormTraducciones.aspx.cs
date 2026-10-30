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
            // El acceso a la pagina lo valida PaginaBase segun CatalogoPermisos.

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
                    lblMensaje.Text = T("No se pudieron cargar las traducciones. Intenta nuevamente mas tarde.");
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

            List<BE_Traduccion> traducciones = bll_idioma.ObtenerTraduccionesParaGrilla(idIdioma);

            CargarPaginas(traducciones);

            gvTraducciones.DataSource = traducciones.Where(t => t.Pagina == ddlPaginaFiltro.SelectedValue).ToList();
            gvTraducciones.DataBind();
        }

        // Se edita de a una pagina: con todo el catalogo junto, el postback de la
        // grilla supera el limite de campos de ASP.NET (MaxHttpCollectionKeys).
        private void CargarPaginas(List<BE_Traduccion> traducciones)
        {
            string paginaActual = ddlPaginaFiltro.SelectedValue;

            ddlPaginaFiltro.DataSource = traducciones.Select(t => t.Pagina).Distinct().OrderBy(p => p).ToList();
            ddlPaginaFiltro.DataBind();

            if (ddlPaginaFiltro.Items.FindByValue(paginaActual) != null)
            {
                ddlPaginaFiltro.SelectedValue = paginaActual;
            }
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
                lblMensaje.Text = T("No se pudieron cargar las traducciones de ese idioma.");
                lblMensaje.ForeColor = System.Drawing.Color.Red;
            }
        }

        protected void ddlPaginaFiltro_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                RefrescarGrilla();
                lblMensaje.Text = string.Empty;
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("FormTraducciones.ddlPaginaFiltro_SelectedIndexChanged", ex);
                lblMensaje.Text = T("No se pudieron cargar las traducciones de esa pagina.");
                lblMensaje.ForeColor = System.Drawing.Color.Red;
            }
        }

        protected void btnExportarXml_Click(object sender, EventArgs e)
        {
            if (!ExigirPermiso("TRADUCCION_EXPORTAR"))
            {
                return;
            }

            string xml;

            try
            {
                xml = bll_idioma.GenerarXmlTraducciones(UsuarioLogueado, int.Parse(ddlIdiomaEditar.SelectedValue));
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("FormTraducciones.btnExportarXml_Click", ex);
                lblMensaje.Text = T("No se pudieron exportar las traducciones. Intenta nuevamente mas tarde.");
                lblMensaje.ForeColor = System.Drawing.Color.Red;
                return;
            }

            DescargarXml(xml, $"Traducciones_{ddlIdiomaEditar.SelectedItem.Text}_{DateTime.Now:yyyy-MM-dd_HHmm}.xml");
        }

        protected void btnImportarXml_Click(object sender, EventArgs e)
        {
            if (!ExigirPermiso("TRADUCCION_IMPORTAR"))
            {
                return;
            }

            try
            {
                if (!fuImportarXml.HasFile || !string.Equals(System.IO.Path.GetExtension(fuImportarXml.FileName), ".xml", StringComparison.OrdinalIgnoreCase))
                {
                    lblMensaje.Text = T("Selecciona un archivo .xml para importar.");
                    lblMensaje.ForeColor = System.Drawing.Color.Red;
                    return;
                }

                int idIdioma = int.Parse(ddlIdiomaEditar.SelectedValue);

                BE_ResultadoImportacion resultado = bll_idioma.ImportarXmlTraducciones(UsuarioLogueado, idIdioma, fuImportarXml.PostedFile.InputStream);

                GestorIdioma.InvalidarCache(idIdioma);

                RefrescarGrilla();

                lblMensaje.Text = T("Importacion terminada: {0} actualizadas, {1} sin cambios, {2} omitidas.", resultado.Actualizadas, resultado.SinCambios, resultado.Omitidas);
                lblMensaje.ForeColor = System.Drawing.Color.Green;
            }
            catch (InvalidOperationException ex)
            {
                lblMensaje.Text = ex.Message;
                lblMensaje.ForeColor = System.Drawing.Color.Red;
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("FormTraducciones.btnImportarXml_Click", ex);
                lblMensaje.Text = T("No se pudieron importar las traducciones. Intenta nuevamente mas tarde.");
                lblMensaje.ForeColor = System.Drawing.Color.Red;
            }
        }

        protected void btnGuardarTraducciones_Click(object sender, EventArgs e)
        {
            if (!ExigirPermiso("TRADUCCION_EDITAR"))
            {
                return;
            }

            try
            {
                int idIdioma = int.Parse(ddlIdiomaEditar.SelectedValue);
                BE_Usuario usuarioLogueado = Session["Usuario"] as BE_Usuario;

                Dictionary<int, string> textosActuales = bll_idioma.ObtenerTraduccionesParaGrilla(idIdioma)
                    .ToDictionary(t => t.IdClave, t => t.Texto ?? string.Empty);

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

                    // Solo lo que cambio: cada guardado deja un evento en la bitacora.
                    if (!textosActuales.TryGetValue(idClave, out string textoActual) || textoActual != txtTexto.Text)
                    {
                        bll_idioma.GuardarTraduccion(usuarioLogueado, idIdioma, idClave, txtTexto.Text);
                    }
                }

                GestorIdioma.InvalidarCache(idIdioma);

                RefrescarGrilla();

                lblMensaje.Text = string.Empty;
                MostrarToast(T("Traducciones guardadas correctamente."), true);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("FormTraducciones.btnGuardarTraducciones_Click", ex);
                lblMensaje.Text = T("No se pudieron guardar las traducciones. Intenta nuevamente mas tarde.");
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
