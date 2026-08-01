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
    public partial class Login : System.Web.UI.Page
    {
        BLL_Usuario bll_Usu = new BLL_Usuario();
        BLL_DVV bll_dvv = new BLL_DVV();
        BE_LoginResultado Obj_Usuario = new BE_LoginResultado();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!VerificarIntegridadOAbortar())
            {
                return;
            }

            if (!IsPostBack)
            {
                if (Session["MensajeLogout"] != null)
                {
                    string mensajeLogout = Session["MensajeLogout"] as string;
                    Session.Remove("MensajeLogout");
                    MostrarToast(mensajeLogout);
                }
            }
        }

        private bool VerificarIntegridadOAbortar()
        {
            var mensajes = new List<string>();

            try
            {
                mensajes.AddRange(bll_dvv.DetectarCambiosBitacoraSiempre());

                foreach (var tabla in new[] { "Usuario", "Rol" })
                {
                    if (!bll_dvv.VerificarIntegridad(tabla))
                    {
                        mensajes.AddRange(bll_dvv.DetectarCambios(tabla));
                    }
                }
            }
            catch (Exception)
            {
                lblMensaje.Text = "No se pudo verificar la integridad del sistema. Intentá nuevamente más tarde.";
                return false;
            }

            if (mensajes.Count > 0)
            {
                Session["MensajesIntegridad"] = mensajes;
                Response.Redirect("LoginWebmaster.aspx");
                return false;
            }

            return true;
        }
        private void MostrarToast(string mensaje)
        {
            string texto = System.Web.HttpUtility.JavaScriptStringEncode(mensaje, true);

            string script =
                "(function(){var t=document.createElement('div');t.className='toast toast-exito';" +
                "t.textContent=" + texto + ";document.body.appendChild(t);" +
                "setTimeout(function(){t.className+=' toast-hide';},4000);" +
                "setTimeout(function(){if(t.parentNode){t.parentNode.removeChild(t);}},4600);})();";

            ClientScript.RegisterStartupScript(GetType(), "toastLogout", script, true);
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            try
            {

                Obj_Usuario = bll_Usu.ObtenerUsuarioPorEmail(txtEmail.Text, txtPassword.Text);

                if (Obj_Usuario.Usuario == null)
                {
                    BE_LoginResultado emergencia = bll_Usu.LoginEmergencia(txtEmail.Text, txtPassword.Text);

                    if (emergencia.Usuario != null)
                    {
                        Obj_Usuario = emergencia;
                    }
                }

                if (Obj_Usuario.Usuario != null)
                {
                    Session["Usuario"] = Obj_Usuario.Usuario;


                    Response.Redirect("Menu.aspx");
                }
                else
                {
                    lblMensaje.Text = Obj_Usuario.Mensaje;
                    return;
                }
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("Login.btnLogin_Click", ex);
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