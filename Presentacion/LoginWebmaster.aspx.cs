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
                //Login acotado para el estado comprometido: valida existencia + contraseña + rol WEBMASTER, sin refrescar el DVV (no "cura" la manipulación).
                BE_LoginResultado resultado = bll_Usu.ValidarWebmaster(txtEmail.Text, txtPassword.Text);

                //Si el login normal falla, probamos las credenciales de emergencia: es el caso en que el atacante borró al único Webmaster y nadie más puede recuperar el sistema.
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

                //Aunque las credenciales sean correctas, mientras la base esté comprometida solo puede ingresar el Webmaster.
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

        //Deja rastro en el mismo log que usa Global.asax, sin mostrarle al usuario el detalle interno de la excepción.
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
                //Si ni el log funciona, no hay nada más para hacer acá.
            }
        }
    }
}
