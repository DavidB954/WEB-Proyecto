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
    public partial class Site1 : System.Web.UI.MasterPage
    {
        BLL_Idioma bll_idioma = new BLL_Idioma();
        BLL_Usuario bll_usuario = new BLL_Usuario();

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

                        liUsuarios.Visible = SeguridadHelper.TieneAcceso(usuario, permisos, "ABM_USUARIO");
                        liTurnos.Visible = SeguridadHelper.TieneAcceso(usuario, permisos, "ACCESO_TURNOS");
                        liRecetas.Visible = SeguridadHelper.TieneAcceso(usuario, permisos, "ACCESO_RECETAS");
                        liMedicos.Visible = SeguridadHelper.TieneAcceso(usuario, permisos, "ACCESO_MEDICOS");
                        liBitacora.Visible = SeguridadHelper.TieneAcceso(usuario, permisos, "VER_BITACORA");
                        liSeguridad.Visible = SeguridadHelper.TieneAcceso(usuario, permisos, "ACCESO_SEGURIDAD");
                        liIdiomas.Visible = SeguridadHelper.TieneAcceso(usuario, permisos, "GESTION_IDIOMAS");
                        liTraducciones.Visible = SeguridadHelper.TieneAcceso(usuario, permisos, "GESTION_TRADUCCIONES");
                        liRolesPermisos.Visible = SeguridadHelper.TieneAcceso(usuario, permisos, "GESTION_ROLES_PERMISOS");
                        liAsignacionSeguridad.Visible = SeguridadHelper.TieneAcceso(usuario, permisos, "ASIGNACION_SEGURIDAD");
                    }
                }
                catch (Exception ex)
                {
                    // Site1.Master se renderiza en casi todas las paginas del sitio:
                    // si esto falla, que se pierda el menu de navegacion es preferible
                    // a que se caiga toda la pagina.
                    RegistrarErrorInterno("Site1.Master.Page_Load (menu)", ex);
                }

                CargarIdiomas();
            }
        }

        private void CargarIdiomas()
        {
            try
            {
                ddlIdioma.DataSource = bll_idioma.ObtenerIdiomasActivos();
                ddlIdioma.DataBind();

                string idIdiomaActual = GestorIdioma.ObtenerIdiomaActual().ToString();

                if (ddlIdioma.Items.FindByValue(idIdiomaActual) != null)
                {
                    ddlIdioma.SelectedValue = idIdiomaActual;
                }
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("Site1.Master.CargarIdiomas", ex);
                ddlIdioma.Visible = false;
            }
        }

        protected void ddlIdioma_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                int idIdioma = int.Parse(ddlIdioma.SelectedValue);

                GestorIdioma.CambiarIdioma(idIdioma);

                // Se persiste en el usuario (no solo en Session) para que la eleccion
                // sobreviva a un cierre de sesion: CerrarSesion.aspx hace Session.Abandon()
                // y con eso se perderia si solo quedara en GestorIdioma/Session.
                BE_Usuario usuarioLogueado = Session["Usuario"] as BE_Usuario;

                if (usuarioLogueado != null && usuarioLogueado.IdUsuario > 0)
                {
                    bll_usuario.ActualizarIdiomaPreferido(usuarioLogueado.IdUsuario, idIdioma);
                    usuarioLogueado.IdIdiomaPreferido = idIdioma;
                }
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("Site1.Master.ddlIdioma_SelectedIndexChanged", ex);

                string mensaje = System.Web.HttpUtility.JavaScriptStringEncode("No se pudo cambiar el idioma. Intenta nuevamente mas tarde.");
                ScriptManager.RegisterStartupScript(Page, Page.GetType(), "errorIdioma_" + Guid.NewGuid().ToString("N"), $"alert('{mensaje}');", true);
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