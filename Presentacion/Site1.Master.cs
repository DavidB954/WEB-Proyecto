using BE;
using BE.Seguridad;
using BLL;
using SERVICIOS;
using System;
using System.Web;
using System.Web.UI;

namespace Presentacion
{
    public partial class Site1 : System.Web.UI.MasterPage
    {
        BLL_Usuario bll_usuario = new BLL_Usuario();

        protected string MensajeSesionExpirada => HttpUtility.JavaScriptStringEncode(
            GestorIdioma.Traducir("Su sesion ha expirado por inactividad. Sera redirigido al login."), true);

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                try
                {
                    if (Session["Usuario"] != null)
                    {
                        BE_Usuario usuario = (BE_Usuario)Session["Usuario"];
                        lblUsuario.Text = usuario.Nombre;

                        UsuarioComponente permisos = Session["Permisos"] as UsuarioComponente;

                        liUsuarios.Visible = SeguridadHelper.TieneAccesoPagina(usuario, permisos, "CRUD_Usuarios.aspx");
                        liTurnos.Visible = SeguridadHelper.TieneAccesoPagina(usuario, permisos, "Turnos.aspx");
                        liRecetas.Visible = SeguridadHelper.TieneAccesoPagina(usuario, permisos, "Recetas.aspx");
                        liMedicos.Visible = SeguridadHelper.TieneAccesoPagina(usuario, permisos, "Medicos.aspx");
                        liAgendaMedicos.Visible = SeguridadHelper.TieneAccesoPagina(usuario, permisos, "AgendaMedicos.aspx");
                        liBitacora.Visible = SeguridadHelper.TieneAccesoPagina(usuario, permisos, "Bitacora.aspx");
                        liSeguridad.Visible = SeguridadHelper.TieneAccesoPagina(usuario, permisos, "Seguridad.aspx");
                        liIdiomas.Visible = SeguridadHelper.TieneAccesoPagina(usuario, permisos, "FormIdiomas.aspx");
                        liTraducciones.Visible = SeguridadHelper.TieneAccesoPagina(usuario, permisos, "FormTraducciones.aspx");
                        liRolesPermisos.Visible = SeguridadHelper.TieneAccesoPagina(usuario, permisos, "GestionRolesPermisos.aspx");
                        liAsignacionSeguridad.Visible = SeguridadHelper.TieneAccesoPagina(usuario, permisos, "AsignacionSeguridad.aspx");
                    }
                }
                catch (Exception ex)
                {
                    RegistroErrores.Registrar("Site1.Master.Page_Load (menu)", ex);
                }

                CargarIdiomas();
            }
        }

        private void CargarIdiomas()
        {
            try
            {
                GestorIdioma.CargarSelector(ddlIdioma);
            }
            catch (Exception ex)
            {
                RegistroErrores.Registrar("Site1.Master.CargarIdiomas", ex);
                ddlIdioma.Visible = false;
            }
        }

        protected void ddlIdioma_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                int idIdioma = int.Parse(ddlIdioma.SelectedValue);

                GestorIdioma.CambiarIdioma(idIdioma);

                BE_Usuario usuarioLogueado = Session["Usuario"] as BE_Usuario;

                if (usuarioLogueado != null && !usuarioLogueado.EsEmergencia)
                {
                    bll_usuario.ActualizarIdiomaPreferido(usuarioLogueado.IdUsuario, idIdioma);
                    usuarioLogueado.IdIdiomaPreferido = idIdioma;
                }

                Response.Redirect(Request.RawUrl, false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (Exception ex)
            {
                RegistroErrores.Registrar("Site1.Master.ddlIdioma_SelectedIndexChanged", ex);

                string mensaje = System.Web.HttpUtility.JavaScriptStringEncode(GestorIdioma.Traducir("No se pudo cambiar el idioma. Intenta nuevamente mas tarde."));
                ScriptManager.RegisterStartupScript(Page, Page.GetType(), "errorIdioma_" + Guid.NewGuid().ToString("N"), $"mostrarAviso('{mensaje}');", true);
            }
        }
    }
}