using BE;
using BLL;
using System;
using System.Web;
using System.Web.UI.WebControls;

namespace Presentacion
{
    public partial class FormIdiomas : PaginaBase
    {
        BLL_Idioma bll_idioma = new BLL_Idioma();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                try
                {
                    RefrescarGrilla();
                }
                catch (Exception ex)
                {
                    RegistrarErrorInterno("FormIdiomas.Page_Load", ex);
                    MostrarMensaje(lblMensaje, T("No se pudieron cargar los idiomas. Intenta nuevamente mas tarde."), false);
                }
            }
        }

        private void RefrescarGrilla()
        {
            gvIdiomas.DataSource = bll_idioma.ObtenerIdiomas();
            gvIdiomas.DataBind();
        }

        private void LimpiarFormulario()
        {
            txtCodigo.Text = string.Empty;
            txtNombre.Text = string.Empty;
            hiddenIdIdioma.Value = string.Empty;
            gvIdiomas.SelectedIndex = -1;

            MostrarBotonesDeEdicion(false);

            RefrescarGrilla();
        }

        private void MostrarBotonesDeEdicion(bool editando)
        {
            btnAgregar.Visible = !editando;
            btnModificar.Visible = editando;
            btnEliminar.Visible = editando;
            btnLimpiar.Visible = editando;
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
                    MostrarMensaje(lblMensaje, T("Debe completar el codigo y el nombre del idioma."), false);
                    return;
                }

                BE_Idioma idioma = new BE_Idioma
                {
                    Codigo = txtCodigo.Text.Trim(),
                    Nombre = txtNombre.Text.Trim()
                };

                bll_idioma.AgregarIdioma(UsuarioLogueado, idioma);

                LimpiarFormulario();

                lblMensaje.Text = string.Empty;
                MostrarToast(T("Idioma agregado correctamente."), true);
            }
            catch (InvalidOperationException ex)
            {
                MostrarMensaje(lblMensaje, ex.Message, false);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("FormIdiomas.btnAgregar_Click", ex);
                MostrarMensaje(lblMensaje, T("No se pudo agregar el idioma. Verifica los datos e intenta nuevamente."), false);
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

                hiddenIdIdioma.Value = HttpUtility.HtmlDecode(fila.Cells[1].Text);
                txtCodigo.Text = HttpUtility.HtmlDecode(fila.Cells[2].Text);
                txtNombre.Text = HttpUtility.HtmlDecode(fila.Cells[3].Text);

                MostrarBotonesDeEdicion(true);

                lblMensaje.Text = string.Empty;

                DesplazarHacia(hGestionIdiomas);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("FormIdiomas.gvIdiomas_SelectedIndexChanged", ex);
                MostrarMensaje(lblMensaje, T("No se pudo seleccionar el idioma."), false);
            }
        }

        protected void btnModificar_Click(object sender, EventArgs e)
        {
            if (!ExigirPermiso("IDIOMA_MODIFICACION"))
            {
                return;
            }

            try
            {
                if (string.IsNullOrWhiteSpace(hiddenIdIdioma.Value))
                {
                    MostrarMensaje(lblMensaje, T("Seleccione un idioma de la lista antes de modificar."), false);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtCodigo.Text) || string.IsNullOrWhiteSpace(txtNombre.Text))
                {
                    MostrarMensaje(lblMensaje, T("Debe completar el codigo y el nombre del idioma."), false);
                    return;
                }

                BE_Idioma idioma = new BE_Idioma
                {
                    IdIdioma = int.Parse(hiddenIdIdioma.Value),
                    Codigo = txtCodigo.Text.Trim(),
                    Nombre = txtNombre.Text.Trim()
                };

                bll_idioma.ModificarIdioma(UsuarioLogueado, idioma);

                LimpiarFormulario();

                lblMensaje.Text = string.Empty;
                MostrarToast(T("Idioma modificado correctamente."), true);
            }
            catch (InvalidOperationException ex)
            {
                MostrarMensaje(lblMensaje, ex.Message, false);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("FormIdiomas.btnModificar_Click", ex);
                MostrarMensaje(lblMensaje, T("No se pudo modificar el idioma. Intenta nuevamente mas tarde."), false);
            }
        }

        protected void btnLimpiar_Click(object sender, EventArgs e)
        {
            try
            {
                LimpiarFormulario();
                lblMensaje.Text = string.Empty;
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("FormIdiomas.btnLimpiar_Click", ex);
                MostrarMensaje(lblMensaje, T("No se pudieron cargar los idiomas. Intenta nuevamente mas tarde."), false);
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
                    MostrarMensaje(lblMensaje, T("Seleccione un idioma de la lista antes de eliminar."), false);
                    return;
                }

                int idIdioma = int.Parse(hiddenIdIdioma.Value);
                int idIdiomaEnUso = GestorIdioma.ObtenerIdiomaActual();

                bll_idioma.EliminarIdioma(UsuarioLogueado, idIdioma, idIdiomaEnUso);

                LimpiarFormulario();

                lblMensaje.Text = string.Empty;
                MostrarToast(T("Idioma eliminado correctamente."), true);
            }
            catch (InvalidOperationException ex)
            {
                MostrarMensaje(lblMensaje, ex.Message, false);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("FormIdiomas.btnEliminar_Click", ex);
                MostrarMensaje(lblMensaje, T("No se pudo eliminar el idioma. Intenta nuevamente mas tarde."), false);
            }
        }
    }
}
