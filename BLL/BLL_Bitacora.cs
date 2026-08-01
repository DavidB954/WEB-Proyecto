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

            objBitacora.DVH = EncryptionHelper.Encriptar(CadenaBitacora(objBitacora));

            dal_bitacora.RegistrarEvento(objBitacora);

            RefrescarDVVBitacora();
        }

        public void RefrescarDVHDeFilas(IEnumerable<int> idsBitacora)
        {
            if (idsBitacora == null)
            {
                return;
            }

            var eventosPorId = dal_bitacora.ObtenerBitacora().ToDictionary(b => b.IdBitacora);

            bool huboCambios = false;

            foreach (var idBitacora in idsBitacora)
            {
                if (eventosPorId.TryGetValue(idBitacora, out var evento))
                {
                    dal_bitacora.ActualizarDVH(evento.IdBitacora, EncryptionHelper.Encriptar(CadenaBitacora(evento)));
                    huboCambios = true;
                }
            }

            if (huboCambios)
            {
                RefrescarDVVBitacora();
            }
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

        private string CadenaBitacora(BE_Bitacora bitacora)
        {
            return $"{bitacora.IdUsuario}|{bitacora.FechaHora}|{bitacora.Accion}|{bitacora.Modulo}|{bitacora.IP}|{bitacora.Descripcion}|{bitacora.NombreMaquina}|{bitacora.Criticidad}";
        }

        private string ObtenerIP()
        {
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