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
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        BLL_Usuario bll_Usu = new BLL_Usuario();
        BE_LoginResultado Obj_Usuario = new BE_LoginResultado();
        protected void btnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                Obj_Usuario = bll_Usu.ObtenerUsuarioPorEmail(txtEmail.Text, txtPassword.Text);

                if (Obj_Usuario.Usuario != null)
                {
                    // Guardamos el usuario en sesión
                    Session["Usuario"] = Obj_Usuario.Usuario;


                    Response.Redirect("CRUD_Usuarios.aspx");
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