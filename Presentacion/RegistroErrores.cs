using System;
using System.IO;
using System.Web.Hosting;

namespace Presentacion
{
    public static class RegistroErrores
    {
        public static void Registrar(string origen, Exception ex)
        {
            try
            {
                string carpetaLogs = HostingEnvironment.MapPath("~/App_Data");
                Directory.CreateDirectory(carpetaLogs);
                File.AppendAllText(
                    Path.Combine(carpetaLogs, "errores.log"),
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {origen} - {ex}{Environment.NewLine}");
            }
            catch
            {
            }
        }
    }
}
