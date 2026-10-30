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
    public partial class Login : PaginaBase
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
                CargarSelectorIdioma();

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
                lblMensaje.Text = T("No se pudo verificar la integridad del sistema. Intenta nuevamente mas tarde.");
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
                RegistrarErrorInterno("Login.ddlIdioma_SelectedIndexChanged", ex);
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
                RegistrarErrorInterno("Login.CargarSelectorIdioma", ex);
                ddlIdioma.Visible = false;
            }
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

                    if (Obj_Usuario.Usuario.IdUsuario > 0)
                    {
                        Session["Permisos"] = new BLL_Permiso().ConstruirArbolUsuario(Obj_Usuario.Usuario);
                    }

                    if (Obj_Usuario.Usuario.IdIdiomaPreferido.HasValue)
                    {
                        GestorIdioma.CambiarIdioma(Obj_Usuario.Usuario.IdIdiomaPreferido.Value);
                    }

                    // Redirect(url, false) + CompleteRequest: sin cortar el hilo con
                    // ThreadAbortException, que el catch de abajo registraba como
                    // error en cada login exitoso.
                    Response.Redirect("Menu.aspx", false);
                    Context.ApplicationInstance.CompleteRequest();
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