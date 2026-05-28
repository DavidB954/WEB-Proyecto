using BE;
using DAL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BLL
{
    public class BLL_Bitacora
    {
        DAL_Bitacora dal_bitacora = new DAL_Bitacora();

        public void RegistrarEvento(int? IdUsuario, AccionBitacora accion, string modulo, string descripcion)
        {
            BE_Bitacora objBitacora = new BE_Bitacora()
            {
                IdUsuario = IdUsuario,
                FechaHora = DateTime.Now,
                Accion = accion,
                Modulo = modulo,
                IP = ObtenerIP(),
                NombreMaquina = Dns.GetHostName(),
                Descripcion = descripcion
            };

            dal_bitacora.RegistrarEvento(objBitacora);
        }

        private string ObtenerIP()
        {
            //Obtener el nombre del equipo local
            string nombreHost = Dns.GetHostName();

            IPAddress[] direccionesIP = Dns.GetHostAddresses(nombreHost);

            string ipSeleccionada = "";

            foreach (IPAddress ip in direccionesIP)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    ipSeleccionada = ip.ToString();
                    break;
                }
            }

            return ipSeleccionada;
        }

        public List<BE_Bitacora> ObtenerBitacora()
        {
            return dal_bitacora.ObtenerBitacora();
        }

        public DataTable FiltrarBitacora(DateTime? desde, DateTime? hasta, int? idUsuario, string modulo, string ip)
        {
            return dal_bitacora.FiltrarBitacora(desde, hasta, idUsuario, modulo, ip);
        }
    }
}