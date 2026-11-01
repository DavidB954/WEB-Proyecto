using BE;
using BLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;

namespace Presentacion
{
    public partial class Recetas : PaginaBase
    {
        BLL_Receta bll_receta = new BLL_Receta();

        protected void Page_Load(object sender, EventArgs e)
        {
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
                    MostrarMensaje(lblMensaje, ex.Message, false);
                }
                catch (Exception ex)
                {
                    RegistrarErrorInterno("Recetas.Page_Load", ex);
                    MostrarMensaje(lblMensaje, T("No se pudieron cargar las recetas. Intenta nuevamente mas tarde."), false);
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

            if (visibles.Count == 0 && recetas.Count > 0)
            {
                MostrarMensaje(lblMensaje, T("No tenes recetas vigentes. Tus {0} receta(s) anteriores estan en \"Todas\".", recetas.Count), true);
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
                MostrarMensaje(lblMensaje, ex.Message, false);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("Recetas.rblVista_SelectedIndexChanged", ex);
                MostrarMensaje(lblMensaje, T("No se pudieron cargar las recetas. Intenta nuevamente mas tarde."), false);
            }
        }

        protected void gvRecetas_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "DescargarPdf")
            {
                return;
            }

            byte[] pdf;
            int idReceta;

            try
            {
                idReceta = int.Parse(Convert.ToString(e.CommandArgument));

                pdf = bll_receta.GenerarPdfReceta(UsuarioLogueado, idReceta);
            }
            catch (InvalidOperationException ex)
            {
                MostrarMensaje(lblMensaje, ex.Message, false);
                return;
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("Recetas.gvRecetas_RowCommand", ex);
                MostrarMensaje(lblMensaje, T("No se pudo generar el PDF de la receta. Intenta nuevamente mas tarde."), false);
                return;
            }

            DescargarPdf(pdf, $"Receta_{idReceta}.pdf");
        }
    }
}
