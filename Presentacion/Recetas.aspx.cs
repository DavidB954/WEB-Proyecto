using BE;
using BLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;

namespace Presentacion
{
    // Recetas del paciente logueado: los medicamentos que le recetaron en sus
    // atenciones (Medicos.aspx).
    public partial class Recetas : PaginaBase
    {
        BLL_Receta bll_receta = new BLL_Receta();

        protected void Page_Load(object sender, EventArgs e)
        {
            // El acceso a la pagina lo valida PaginaBase segun CatalogoPermisos.

            if (!IsPostBack)
            {
                try
                {
                    spanAyudaVigencia.InnerText = T("Cada receta vale {0} dias desde que se emite.", BLL_Receta.DiasVigencia);
                    CargarRecetas();
                }
                catch (InvalidOperationException ex)
                {
                    pnlRecetas.Visible = false;
                    MostrarMensaje(ex.Message, false);
                }
                catch (Exception ex)
                {
                    RegistrarErrorInterno("Recetas.Page_Load", ex);
                    MostrarMensaje(T("No se pudieron cargar las recetas. Intenta nuevamente mas tarde."), false);
                }
            }
        }

        private void CargarRecetas()
        {
            List<BE_Receta> recetas = bll_receta.ObtenerRecetasPaciente(UsuarioLogueado);

            List<BE_Receta> visibles = rblVista.SelectedValue == "Todas"
                ? recetas
                : recetas.Where(r => r.Vigente).ToList();

            gvRecetas.DataSource = visibles;
            gvRecetas.DataBind();

            // El texto de grilla vacia dice que no hay recetas emitidas: si solo
            // hay vencidas, se aclara para que no parezca que se perdieron.
            if (visibles.Count == 0 && recetas.Count > 0)
            {
                MostrarMensaje(T("No tenes recetas vigentes. Tus {0} receta(s) anteriores estan en \"Todas\".", recetas.Count), true);
            }
        }

        protected void rblVista_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                CargarRecetas();
            }
            catch (InvalidOperationException ex)
            {
                MostrarMensaje(ex.Message, false);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("Recetas.rblVista_SelectedIndexChanged", ex);
                MostrarMensaje(T("No se pudieron cargar las recetas. Intenta nuevamente mas tarde."), false);
            }
        }

        private void MostrarMensaje(string texto, bool exito)
        {
            lblMensaje.Text = System.Web.HttpUtility.HtmlEncode(texto);
            lblMensaje.CssClass = exito ? "msg-form msg-exito" : "msg-form msg-error";
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
