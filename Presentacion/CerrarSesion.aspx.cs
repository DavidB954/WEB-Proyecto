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
    public partial class CerrarSesion : System.Web.UI.Page
    {
        BLL_Bitacora bll_bitacora = new BLL_Bitacora();

        protected void Page_Load(object sender, EventArgs e)
        {

            BE_Usuario usuarioLogueado = Session["Usuario"] as BE_Usuario;

            if (usuarioLogueado != null)
            {
                try
                {
                    bll_bitacora.RegistrarEvento(usuarioLogueado.IdUsuario, AccionBitacora.LOGOUT, "LOGIN", $"Cierre de sesión del usuario: {usuarioLogueado.NombreApellido}, ID: {usuarioLogueado.IdUsuario}");
                }
                catch (Exception ex)
                {
                    try
                    {
                        string carpetaLogs = Server.MapPath("~/App_Data");
                        System.IO.Directory.CreateDirectory(carpetaLogs);
                        System.IO.File.AppendAllText(
                            System.IO.Path.Combine(carpetaLogs, "errores.log"),
                            $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - CerrarSesion - Fallo al registrar bitácora: {ex}{Environment.NewLine}");
                    }
                    catch
                    {
                    }
                }
            }

            Session.Clear();
            Session.Abandon();
            Response.Redirect("Login.aspx");
        }
    }
}