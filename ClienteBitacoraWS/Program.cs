using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Services.Protocols;

namespace ClienteBitacoraWS
{
    // Consume WSBitacora.asmx por SOAP real desde otro proceso, con sesion propia.
    //
    // Uso: ClienteBitacoraWS <url del .asmx> <email> [archivo.xml]
    // La contraseña se pide por consola (o se toma de WS_PASSWORD) para que no quede
    // en el historial. Con archivo.xml, ademas exporta la bitacora a ese archivo.
    internal static class Program
    {
        private const int EventosAMostrar = 10;

        private static int Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Uso: ClienteBitacoraWS <url del .asmx> <email> [archivo.xml]");
                return 2;
            }

            string password = Environment.GetEnvironmentVariable("WS_PASSWORD") ?? PedirPassword();

            try
            {
                using (ProxyBitacora servicio = new ProxyBitacora(args[0]))
                {
                    servicio.IniciarSesion(args[1], password);
                    Console.WriteLine("Sesion iniciada como " + args[1]);

                    EventoBitacora[] eventos = servicio.ConsultarBitacora("", "", "", "", "", "");
                    Console.WriteLine($"La bitacora tiene {eventos.Length} eventos. Ultimos {EventosAMostrar}:");

                    foreach (EventoBitacora evento in eventos.OrderByDescending(e => e.IdBitacora).Take(EventosAMostrar))
                    {
                        Console.WriteLine($"  {evento.FechaHora:yyyy-MM-dd HH:mm} [{evento.Criticidad}] {evento.Accion}: {evento.Descripcion}");
                    }

                    if (args.Length > 2)
                    {
                        string xml = servicio.ExportarBitacoraXml("", "", "", "", "", "");
                        File.WriteAllText(args[2], xml, Encoding.UTF8);
                        Console.WriteLine("Bitacora exportada a " + args[2]);
                    }

                    servicio.CerrarSesion();
                }

                return 0;
            }
            catch (SoapException ex)
            {
                Console.Error.WriteLine("El servicio rechazo la operacion: " + MotivoDelRechazo(ex));
                return 1;
            }
            catch (System.Net.WebException ex)
            {
                Console.Error.WriteLine("No se pudo conectar con el servicio: " + ex.Message);
                return 1;
            }
            catch (IOException ex)
            {
                Console.Error.WriteLine("No se pudo guardar el archivo: " + ex.Message);
                return 1;
            }
        }

        // ex.Message trae "...SoapException: ... ---> Tipo: motivo" seguido de la traza
        // del servidor: al usuario solo le sirve el motivo.
        private static string MotivoDelRechazo(SoapException ex)
        {
            Match motivo = Regex.Match(ex.Message, @"--->\s*[\w.]+Exception:\s*(.+)");

            return motivo.Success ? motivo.Groups[1].Value.Trim() : ex.Message;
        }

        private static string PedirPassword()
        {
            Console.Write("Contraseña: ");

            StringBuilder password = new StringBuilder();

            for (ConsoleKeyInfo tecla = Console.ReadKey(true); tecla.Key != ConsoleKey.Enter; tecla = Console.ReadKey(true))
            {
                if (tecla.Key == ConsoleKey.Backspace)
                {
                    password.Length = Math.Max(0, password.Length - 1);
                }
                else
                {
                    password.Append(tecla.KeyChar);
                }
            }

            Console.WriteLine();

            return password.ToString();
        }
    }
}
