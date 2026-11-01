using BE;
using BLL;
using System;

namespace Presentacion
{
    public partial class CerrarSesion : PaginaBase
    {
        BLL_Bitacora bll_bitacora = new BLL_Bitacora();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (UsuarioLogueado != null)
            {
                bll_bitacora.RegistrarEvento(UsuarioLogueado.IdUsuario, AccionBitacora.LOGOUT, "LOGIN", $"Cierre de sesion del usuario: {UsuarioLogueado.NombreApellido}, ID: {UsuarioLogueado.IdUsuario}");
            }

            Session.Clear();
            Session.Abandon();
            Response.Redirect("Login.aspx");
        }
    }
}