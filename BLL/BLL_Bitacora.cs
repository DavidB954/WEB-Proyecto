using BE;
using DAL;
using SERVICIOS;
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
        DAL_DVV dal_dvv = new DAL_DVV();

        public void RegistrarEvento(int? IdUsuario, AccionBitacora accion, string modulo, string descripcion)
        {
            
            if (IdUsuario == 0)
            {
                IdUsuario = null;
            }

            BE_Bitacora objBitacora = new BE_Bitacora()
            {
                IdUsuario = IdUsuario,
                FechaHora = DateTime.Now,
                Accion = accion,
                Modulo = modulo,
                IP = ObtenerIP(),
                NombreMaquina = Dns.GetHostName(),
                Descripcion = descripcion,
                Criticidad = CriticidadBitacora.Obtener(accion)
            };

            //DVH con encriptación reversible: permite, ante una corrupción, desencriptar y comparar contra los valores actuales de la fila.
            objBitacora.DVH = EncryptionHelper.Encriptar(CadenaBitacora(objBitacora));

            dal_bitacora.RegistrarEvento(objBitacora);

            //Bitacora es una tabla que crece con cada evento (incluido este mismo insert, y el propio log de error de integridad): hay que refrescar su DVV en cada alta, si no cualquier acción legítima (o el propio chequeo fallido de otra tabla) haría que el próximo chequeo de Bitacora fallara igual.
            RefrescarDVVBitacora();
        }

        private void RefrescarDVVBitacora()
        {
            List<string> hashesFila = dal_bitacora.ObtenerBitacora().Select(b => EncryptionHelper.Encriptar(CadenaBitacora(b))).ToList();

            StringBuilder concatenacion = new StringBuilder();
            foreach (var hash in hashesFila)
            {
                concatenacion.Append(hash);
            }

            dal_dvv.ActualizarDVV(HashHelper.GenerarHash(concatenacion.ToString()), "Bitacora");
        }

        //Tiene que ser idéntica a BLL_DVV.CadenaBitacora.
        private string CadenaBitacora(BE_Bitacora bitacora)
        {
            return $"{bitacora.IdUsuario}|{bitacora.FechaHora}|{bitacora.Accion}|{bitacora.Modulo}|{bitacora.IP}|{bitacora.Descripcion}|{bitacora.NombreMaquina}|{bitacora.Criticidad}";
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

        public DataTable FiltrarBitacora(DateTime? desde, DateTime? hasta, int? idUsuario, string modulo, string ip, string criticidad)
        {
            return dal_bitacora.FiltrarBitacora(desde, hasta, idUsuario, modulo, ip, criticidad);
        }
    }
}