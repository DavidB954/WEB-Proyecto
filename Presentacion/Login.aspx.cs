using BE;
using BLL;
using System;
using System.Collections.Generic;

namespace Presentacion
{
    public partial class Login : PaginaLogin
    {
        BLL_Usuario bll_usuario = new BLL_Usuario();
        BLL_DVV bll_dvv = new BLL_DVV();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack)
            {
                return;
            }

            if (VerificarIntegridadOAbortar())
            {
                CargarSelectorIdioma(ddlIdioma);

                if (Session["MensajeLogout"] != null)
                {
                    string mensajeLogout = Session["MensajeLogout"] as string;
                    Session.Remove("MensajeLogout");
                    MostrarToast(mensajeLogout, true, 4000);
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

        protected void ddlIdioma_SelectedIndexChanged(object sender, EventArgs e)
        {
            CambiarIdioma(ddlIdioma, lblMensaje);
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            if (!VerificarIntegridadOAbortar())
            {
                return;
            }

            try
            {
                BE_LoginResultado resultado = ProbarLoginEmergencia(bll_usuario, bll_usuario.IniciarSesion(txtEmail.Text, txtPassword.Text), txtEmail.Text, txtPassword.Text);

                if (resultado.Usuario == null)
                {
                    lblMensaje.Text = resultado.Mensaje;
                    return;
                }

                AbrirSesion(resultado.Usuario, "Menu.aspx");
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("Login.btnLogin_Click", ex);
                lblMensaje.Text = T("No se pudo iniciar sesion. Intenta nuevamente mas tarde.");
            }
        }
    }
}