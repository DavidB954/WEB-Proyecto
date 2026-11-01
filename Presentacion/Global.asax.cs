using BE;
using System;
using System.Web;

namespace Presentacion
{
    public class Global : HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            TextoLocalizado.Traductor = GestorIdioma.Traducir;
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            Exception ex = Server.GetLastError();

            RegistroErrores.Registrar(Request.Path, ex);

            Server.ClearError();
            Response.Redirect("~/ErrorGeneral.aspx");
        }
    }
}
