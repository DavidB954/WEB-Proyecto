using System;
using System.IO;
using System.Web;

namespace Presentacion
{
    public class Global : HttpApplication
    {
        protected void Application_Error(object sender, EventArgs e)
        {
            Exception ex = Server.GetLastError();

            try
            {
                string carpetaLogs = Server.MapPath("~/App_Data");
                Directory.CreateDirectory(carpetaLogs);

                string linea = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {Request?.Path} - {ex}{Environment.NewLine}";
                File.AppendAllText(Path.Combine(carpetaLogs, "errores.log"), linea);
            }
            catch
            {
            }

            Server.ClearError();
            Response.Redirect("~/ErrorGeneral.aspx");
        }
    }
}
