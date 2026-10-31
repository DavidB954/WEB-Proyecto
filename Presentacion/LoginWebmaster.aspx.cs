using BE;
using BLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Presentacion
{
    public partial class LoginWebmaster : PaginaBase
    {
        BLL_Usuario bll_Usu = new BLL_Usuario();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarSelectorIdioma();
            }
        }

        protected void ddlIdioma_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                GestorIdioma.CambiarIdioma(int.Parse(ddlIdioma.SelectedValue));

                // Se recarga para regenerar todo en el idioma elegido, igual que Site1.Master.
                Response.Redirect(Request.RawUrl, false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("LoginWebmaster.ddlIdioma_SelectedIndexChanged", ex);
                lblMensaje.Text = T("No se pudo cambiar el idioma. Intenta nuevamente mas tarde.");
            }
        }

        private void CargarSelectorIdioma()
        {
            try
            {
                GestorIdioma.CargarSelector(ddlIdioma);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("LoginWebmaster.CargarSelectorIdioma", ex);
                ddlIdioma.Visible = false;
            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                BE_LoginResultado resultado = bll_Usu.ValidarWebmaster(txtEmail.Text, txtPassword.Text);

                if (resultado.Usuario == null)
                {
                    BE_LoginResultado emergencia = bll_Usu.LoginEmergencia(txtEmail.Text, txtPassword.Text);

                    if (emergencia.Usuario != null)
                    {
                        resultado = emergencia;
                    }
                }

                if (resultado.Usuario == null)
                {
                    lblMensaje.Text = resultado.Mensaje;
                    return;
                }

                if (resultado.Usuario.NombreRol != "WEBMASTER")
                {
                    lblMensaje.Text = T("La base de datos tiene un problema de integridad. Solo Webmaster puede ingresar en este momento.");
                    return;
                }

                Session["Usuario"] = resultado.Usuario;

                if (resultado.Usuario.IdUsuario > 0)
                {
                    Session["Permisos"] = new BLL_Permiso().ConstruirArbolUsuario(resultado.Usuario);
                }

                if (resultado.Usuario.IdIdiomaPreferido.HasValue)
                {
                    GestorIdioma.CambiarIdioma(resultado.Usuario.IdIdiomaPreferido.Value);
                }

                // Sin ThreadAbortException: ver Login.btnLogin_Click.
                Response.Redirect("Seguridad.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("LoginWebmaster.btnLogin_Click", ex);
                lblMensaje.Text = T("No se pudo iniciar sesion. Intenta nuevamente mas tarde.");
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
