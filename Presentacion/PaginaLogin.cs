using BE;
using BLL;
using System;
using System.Web.UI.WebControls;

namespace Presentacion
{
    public class PaginaLogin : PaginaBase
    {
        private const string ClaveIdiomaElegidoEnLogin = "IdiomaElegidoEnLogin";

        protected void CargarSelectorIdioma(DropDownList selector)
        {
            try
            {
                GestorIdioma.CargarSelector(selector);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno(GetType().BaseType.Name + ".CargarSelectorIdioma", ex);
                selector.Visible = false;
            }
        }

        protected void CambiarIdioma(DropDownList selector, Label lblMensaje)
        {
            try
            {
                GestorIdioma.CambiarIdioma(int.Parse(selector.SelectedValue));
                Session[ClaveIdiomaElegidoEnLogin] = true;

                Response.Redirect(Request.RawUrl, false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno(GetType().BaseType.Name + ".ddlIdioma_SelectedIndexChanged", ex);
                lblMensaje.Text = T("No se pudo cambiar el idioma. Intenta nuevamente mas tarde.");
            }
        }

        protected void AbrirSesion(BE_Usuario usuario, string paginaDestino)
        {
            Session["Usuario"] = usuario;

            Session["Permisos"] = new BLL_Permiso().ConstruirArbolUsuario(usuario);

            AplicarIdiomaDelUsuario(usuario);

            Response.Redirect(paginaDestino, false);
            Context.ApplicationInstance.CompleteRequest();
        }

        private void AplicarIdiomaDelUsuario(BE_Usuario usuario)
        {
            bool eligioIdiomaEnLogin = Session[ClaveIdiomaElegidoEnLogin] != null;
            Session.Remove(ClaveIdiomaElegidoEnLogin);

            if (!eligioIdiomaEnLogin)
            {
                if (usuario.IdIdiomaPreferido.HasValue)
                {
                    GestorIdioma.CambiarIdioma(usuario.IdIdiomaPreferido.Value);
                }

                return;
            }

            try
            {
                int idIdioma = GestorIdioma.ObtenerIdiomaActual();

                new BLL_Usuario().ActualizarIdiomaPreferido(usuario.IdUsuario, idIdioma);
                usuario.IdIdiomaPreferido = idIdioma;
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno(GetType().BaseType.Name + ".AplicarIdiomaDelUsuario", ex);
            }
        }
    }
}
