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
    public partial class LoginWebmaster : System.Web.UI.Page
    {
        BLL_Usuario bll_Usu = new BLL_Usuario();

        protected void Page_Load(object sender, EventArgs e)
        {
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
                    lblMensaje.Text = "La base de datos tiene un problema de integridad. Solo Webmaster puede ingresar en este momento.";
                    return;
                }

                Session["Usuario"] = resultado.Usuario;
                Response.Redirect("Seguridad.aspx");
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("LoginWebmaster.btnLogin_Click", ex);
                lblMensaje.Text = "No se pudo iniciar sesión. Intentá nuevamente más tarde.";
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
