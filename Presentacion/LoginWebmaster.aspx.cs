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
                BE_LoginResultado resultado = bll_Usu.ObtenerUsuarioPorEmail(txtEmail.Text, txtPassword.Text);

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
                lblMensaje.Text = ex.Message;
            }
        }
    }
}
