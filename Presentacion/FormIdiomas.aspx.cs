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
    public partial class FormIdiomas : PaginaBase
    {
        BLL_Idioma bll_idioma = new BLL_Idioma();

        protected void Page_Load(object sender, EventArgs e)
        {
            // El acceso a la pagina lo valida PaginaBase segun CatalogoPermisos.

            if (!IsPostBack)
            {
                try
                {
                    RefrescarGrilla();
                }
                catch (Exception ex)
                {
                    RegistrarErrorInterno("FormIdiomas.Page_Load", ex);
                    lblMensaje.Text = T("No se pudieron cargar los idiomas. Intenta nuevamente mas tarde.");
                    lblMensaje.ForeColor = System.Drawing.Color.Red;
                }
            }
        }

        private void RefrescarGrilla()
        {
            gvIdiomas.DataSource = bll_idioma.ObtenerIdiomas();
            gvIdiomas.DataBind();
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

        private void LimpiarFormulario()
        {
            txtCodigo.Text = string.Empty;
            txtNombre.Text = string.Empty;
            hiddenIdIdioma.Value = string.Empty;

            RefrescarGrilla();
        }

        protected void btnAgregar_Click(object sender, EventArgs e)
        {
            if (!ExigirPermiso("IDIOMA_ALTA"))
            {
                return;
            }

            try
            {
                if (string.IsNullOrWhiteSpace(txtCodigo.Text) || string.IsNullOrWhiteSpace(txtNombre.Text))
                {
                    lblMensaje.Text = T("Debe completar el codigo y el nombre del idioma.");
                    lblMensaje.ForeColor = System.Drawing.Color.Red;
                    return;
                }

                BE_Idioma idioma = new BE_Idioma
                {
                    Codigo = txtCodigo.Text.Trim(),
                    Nombre = txtNombre.Text.Trim()
                };

                BE_Usuario usuarioLogueado = Session["Usuario"] as BE_Usuario;

                bll_idioma.AgregarIdioma(usuarioLogueado, idioma);

                LimpiarFormulario();

                lblMensaje.Text = string.Empty;
                MostrarToast(T("Idioma agregado correctamente."), true);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("FormIdiomas.btnAgregar_Click", ex);
                lblMensaje.Text = T("No se pudo agregar el idioma. Verifica los datos e intenta nuevamente.");
                lblMensaje.ForeColor = System.Drawing.Color.Red;
            }
        }

        protected void gvIdiomas_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                GridViewRow fila = gvIdiomas.SelectedRow;

                if (fila == null)
                {
                    return;
                }

                hiddenIdIdioma.Value = fila.Cells[1].Text;
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("FormIdiomas.gvIdiomas_SelectedIndexChanged", ex);
                lblMensaje.Text = T("No se pudo seleccionar el idioma.");
                lblMensaje.ForeColor = System.Drawing.Color.Red;
            }
        }

        protected void btnEliminar_Click(object sender, EventArgs e)
        {
            if (!ExigirPermiso("IDIOMA_BAJA"))
            {
                return;
            }

            try
            {
                if (string.IsNullOrWhiteSpace(hiddenIdIdioma.Value))
                {
                    lblMensaje.Text = T("Seleccione un idioma de la lista antes de eliminar.");
                    lblMensaje.ForeColor = System.Drawing.Color.Red;
                    return;
                }

                int idIdioma = int.Parse(hiddenIdIdioma.Value);
                int idIdiomaEnUso = GestorIdioma.ObtenerIdiomaActual();

                BE_Usuario usuarioLogueado = Session["Usuario"] as BE_Usuario;

                bll_idioma.EliminarIdioma(usuarioLogueado, idIdioma, idIdiomaEnUso);

                LimpiarFormulario();

                lblMensaje.Text = string.Empty;
                MostrarToast(T("Idioma eliminado correctamente."), true);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("FormIdiomas.btnEliminar_Click", ex);
                lblMensaje.Text = T("No se pudo eliminar el idioma. Intenta nuevamente mas tarde.");
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
