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
            if (!IsPostBack)
            {
                var tablasProtegidas = new[] { "Usuario", "Rol", "Bitacora" };
                var mensajes = new List<string>();

                foreach (var tabla in tablasProtegidas)
                {
                    if (!bll_dvv.VerificarIntegridad(tabla))
                    {
                        mensajes.AddRange(bll_dvv.DetectarCambios(tabla));
                    }
                }

                if (mensajes.Count > 0)
                {
                    Session["MensajesIntegridad"] = mensajes;
                    Response.Redirect("LoginWebmaster.aspx");
                }

                //Si la integridad está OK, mostramos el aviso de "volvé a loguearte" que dejó Seguridad.aspx tras Recalcular/BackUp/Restore.
                //Se muestra como toast flotante (no como texto fijo en el formulario de login).
                if (Session["MensajeLogout"] != null)
                {
                    string mensajeLogout = Session["MensajeLogout"] as string;
                    Session.Remove("MensajeLogout");
                    MostrarToast(mensajeLogout);
                }
            }
        }
        //Muestra un mensaje flotante (toast) que se cierra solo
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

                //Si el login normal falla, probamos las credenciales de emergencia (por si se eliminó al único ADMINISTRADOR o WEBMASTER del sistema).
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
                    // Guardamos el usuario en sesión
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
                lblMensaje.Text = ex.Message; 

            }

        }
    }
}