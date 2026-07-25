using System;
using System.IO;
using System.Web;

namespace Presentacion
{
    public class Global : HttpApplication
    {
        //Red de seguridad final: si una excepción se escapa de cualquier página sin ser atrapada,
        //esto evita que el usuario vea la pantalla amarilla de ASP.NET (y, con ella, el stack trace).
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
                //Si ni siquiera se puede escribir el log, no hay nada más para hacer acá: seguimos con la redirección.
            }

            Server.ClearError();
            Response.Redirect("~/ErrorGeneral.aspx");
        }
    }
}
