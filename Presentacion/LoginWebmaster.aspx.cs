using BE;
using BLL;
using System;

namespace Presentacion
{
    public partial class LoginWebmaster : PaginaLogin
    {
        BLL_Usuario bll_usuario = new BLL_Usuario();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarSelectorIdioma(ddlIdioma);
            }
        }

        protected void ddlIdioma_SelectedIndexChanged(object sender, EventArgs e)
        {
            CambiarIdioma(ddlIdioma, lblMensaje);
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                BE_LoginResultado resultado = bll_usuario.ValidarWebmaster(txtEmail.Text, txtPassword.Text);

                if (resultado.Usuario == null)
                {
                    lblMensaje.Text = resultado.Mensaje;
                    return;
                }

                if (resultado.Usuario.NombreRol != RolesSistema.Webmaster)
                {
                    lblMensaje.Text = T("La base de datos tiene un problema de integridad. Solo Webmaster puede ingresar en este momento.");
                    return;
                }

                AbrirSesion(resultado.Usuario, "Seguridad.aspx");
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("LoginWebmaster.btnLogin_Click", ex);
                lblMensaje.Text = T("No se pudo iniciar sesion. Intenta nuevamente mas tarde.");
            }
        }
    }
}
