using BE;
using BLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;

namespace Presentacion
{
    public partial class FormTraducciones : PaginaBase
    {
        BLL_Idioma bll_idioma = new BLL_Idioma();

        protected void Page_Load(object sender, EventArgs e)
        {
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
                    MostrarMensaje(lblMensaje, T("No se pudieron cargar las traducciones. Intenta nuevamente mas tarde."), false);
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
                MostrarMensaje(lblMensaje, T("No se pudieron cargar las traducciones de ese idioma."), false);
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
                MostrarMensaje(lblMensaje, T("No se pudieron cargar las traducciones de esa pagina."), false);
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

                    if (!textosActuales.TryGetValue(idClave, out string textoActual) || textoActual != txtTexto.Text)
                    {
                        bll_idioma.GuardarTraduccion(UsuarioLogueado, idIdioma, idClave, txtTexto.Text);
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
                MostrarMensaje(lblMensaje, T("No se pudieron guardar las traducciones. Intenta nuevamente mas tarde."), false);
            }
        }
    }
}
