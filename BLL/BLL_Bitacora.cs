using BE;
using DAL;
using SERVICIOS;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace BLL
{
    public class BLL_Bitacora
    {
        DAL_Bitacora dal_bitacora = new DAL_Bitacora();
        DAL_DVV dal_dvv = new DAL_DVV();

        public void RegistrarEvento(int? idUsuario, AccionBitacora accion, string modulo, string descripcion)
        {
            try
            {
                if (idUsuario == 0)
                {
                    idUsuario = null;
                }

                BE_Bitacora objBitacora = new BE_Bitacora()
                {
                    IdUsuario = idUsuario,
                    FechaHora = DateTime.Now,
                    Accion = accion,
                    Modulo = modulo,
                    IP = ObtenerIP(),
                    NombreMaquina = Dns.GetHostName(),
                    Descripcion = descripcion,
                    Criticidad = CriticidadBitacora.Obtener(accion)
                };

                objBitacora.DVH = EncryptionHelper.Encriptar(CadenasIntegridad.CadenaBitacora(objBitacora));

                dal_bitacora.RegistrarEvento(objBitacora);

                RefrescarDVVBitacora();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Fallo al registrar bitacora ({modulo}/{accion}): {ex}");
            }
        }

        private void RefrescarDVVBitacora()
        {
            IEnumerable<string> firmas = dal_bitacora.ObtenerBitacora().Select(b => EncryptionHelper.Encriptar(CadenasIntegridad.CadenaBitacora(b)));

            dal_dvv.ActualizarDVV(CadenasIntegridad.CalcularDVV(firmas), "Bitacora");
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

        public List<BE_Bitacora> ConsultarBitacora(BE_Usuario usuarioLogueado, DateTime? desde, DateTime? hasta, int? idUsuario, string modulo, string ip, string criticidad)
        {
            bool sinFiltros = desde == null && hasta == null && idUsuario == null && modulo == null && ip == null && criticidad == null;

            IEnumerable<BE_Bitacora> eventos = sinFiltros
                ? (IEnumerable<BE_Bitacora>)dal_bitacora.ObtenerBitacora()
                : dal_bitacora.FiltrarBitacora(desde, hasta, idUsuario, modulo, ip, criticidad).AsEnumerable().Select(MapearFila);

            return SoloEventosVisibles(usuarioLogueado, eventos).OrderByDescending(e => e.IdBitacora).ToList();
        }

        // BLL_Permiso se crea aca y no como campo porque BLL_Permiso ya tiene un campo BLL_Bitacora.
        private static IEnumerable<BE_Bitacora> SoloEventosVisibles(BE_Usuario usuarioLogueado, IEnumerable<BE_Bitacora> eventos)
        {
            BLL_Permiso bll_permiso = new BLL_Permiso();

            if (bll_permiso.EsWebmaster(usuarioLogueado))
            {
                return eventos;
            }

            HashSet<int> idsWebmaster = bll_permiso.ObtenerIdsUsuariosWebmaster();

            return eventos.Where(e => !(e.IdUsuario.HasValue && idsWebmaster.Contains(e.IdUsuario.Value)) && !MencionaWebmaster(e));
        }

        private static bool MencionaWebmaster(BE_Bitacora evento)
        {
            return evento.Descripcion != null && evento.Descripcion.IndexOf(RolesSistema.Webmaster, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public string GenerarXml(List<BE_Bitacora> eventos, DateTime? desde, DateTime? hasta, int? idUsuario,
                                 string modulo, string ip, string criticidad, string generadoPor)
        {
            XDocument documento = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XElement("Bitacora",
                    new XAttribute("generado", DateTime.Now.ToString("s")),
                    new XAttribute("generadoPor", generadoPor ?? string.Empty),
                    new XElement("Filtros",
                        AtributoOpcional("desde", desde?.ToString("yyyy-MM-dd")),
                        AtributoOpcional("hasta", hasta?.ToString("yyyy-MM-dd")),
                        AtributoOpcional("idUsuario", idUsuario?.ToString()),
                        AtributoOpcional("modulo", modulo),
                        AtributoOpcional("ip", ip),
                        AtributoOpcional("criticidad", criticidad)),
                    new XElement("Eventos",
                        new XAttribute("cantidad", eventos.Count),
                        eventos.Select(e => new XElement("Evento",
                            new XAttribute("id", e.IdBitacora),
                            AtributoOpcional("idUsuario", e.IdUsuario?.ToString()),
                            new XAttribute("fechaHora", e.FechaHora.ToString("s")),
                            AtributoOpcional("criticidad", e.Criticidad),
                            new XElement("Accion", e.Accion.ToString()),
                            new XElement("Modulo", e.Modulo ?? string.Empty),
                            new XElement("IP", e.IP ?? string.Empty),
                            new XElement("NombreMaquina", e.NombreMaquina ?? string.Empty),
                            new XElement("Descripcion", e.Descripcion ?? string.Empty))))));

            return documento.Declaration + Environment.NewLine + documento.ToString();
        }

        public BE_BitacoraImportada ImportarXml(BE_Usuario usuarioLogueado, string nombreArchivo, Stream contenidoXml)
        {
            XElement raiz = LeerXmlBitacora(contenidoXml);
            XElement filtros = raiz.Element("Filtros");
            XElement eventos = raiz.Element("Eventos");

            if (eventos == null)
            {
                throw new InvalidOperationException(TextoLocalizado.T("El archivo no tiene el formato de bitacora del sistema."));
            }

            BE_BitacoraImportada importada = new BE_BitacoraImportada
            {
                NombreArchivo = nombreArchivo,
                Generado = (string)raiz.Attribute("generado"),
                GeneradoPor = (string)raiz.Attribute("generadoPor"),
                Desde = LeerFechaFiltro(filtros, "desde"),
                Hasta = LeerFechaFiltro(filtros, "hasta"),
                IdUsuario = LeerEntero((string)filtros?.Attribute("idUsuario")),
                Modulo = (string)filtros?.Attribute("modulo"),
                Ip = (string)filtros?.Attribute("ip"),
                Criticidad = (string)filtros?.Attribute("criticidad"),
                Eventos = eventos.Elements("Evento").Select(LeerEvento).ToList()
            };

            if (LeerEntero((string)eventos.Attribute("cantidad")) != importada.Eventos.Count)
            {
                throw new InvalidOperationException(TextoLocalizado.T("El archivo esta incompleto: la cantidad de eventos no coincide con la declarada."));
            }

            RegistrarEvento(usuarioLogueado.IdUsuario, AccionBitacora.BITACORA_IMPORTADA, "SEGURIDAD",
                $"{usuarioLogueado.NombreApellido} consulto el archivo '{nombreArchivo}' con {importada.Eventos.Count} eventos de bitacora.");

            return importada;
        }

        public List<BE_Bitacora> FiltrarEventos(BE_Usuario usuarioLogueado, IEnumerable<BE_Bitacora> eventos, DateTime? desde, DateTime? hasta, int? idUsuario, string modulo, string ip, string criticidad)
        {
            return SoloEventosVisibles(usuarioLogueado, eventos)
                .Where(e => desde == null || e.FechaHora >= desde)
                .Where(e => hasta == null || e.FechaHora <= hasta)
                .Where(e => idUsuario == null || e.IdUsuario == idUsuario)
                .Where(e => modulo == null || string.Equals(e.Modulo, modulo, StringComparison.OrdinalIgnoreCase))
                .Where(e => ip == null || string.Equals(e.IP, ip, StringComparison.OrdinalIgnoreCase))
                .Where(e => criticidad == null || string.Equals(e.Criticidad, criticidad, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(e => e.IdBitacora)
                .ToList();
        }

        private static XElement LeerXmlBitacora(Stream contenidoXml)
        {
            XmlReaderSettings opciones = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null
            };

            XDocument documento;

            try
            {
                using (XmlReader lector = XmlReader.Create(contenidoXml, opciones))
                {
                    documento = XDocument.Load(lector);
                }
            }
            catch (XmlException ex)
            {
                throw new InvalidOperationException(TextoLocalizado.T("El archivo no es un XML valido."), ex);
            }

            if (documento.Root == null || documento.Root.Name != "Bitacora")
            {
                throw new InvalidOperationException(TextoLocalizado.T("El archivo no tiene el formato de bitacora del sistema."));
            }

            return documento.Root;
        }

        private static BE_Bitacora LeerEvento(XElement evento)
        {
            if (!int.TryParse((string)evento.Attribute("id"), out int id)
                || !DateTime.TryParseExact((string)evento.Attribute("fechaHora"), "s", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fechaHora))
            {
                throw new InvalidOperationException(TextoLocalizado.T("El archivo tiene un evento sin ID o sin fecha valida."));
            }

            return new BE_Bitacora
            {
                IdBitacora = id,
                IdUsuario = LeerEntero((string)evento.Attribute("idUsuario")),
                FechaHora = fechaHora,
                Criticidad = (string)evento.Attribute("criticidad"),
                Accion = Enum.TryParse((string)evento.Element("Accion"), out AccionBitacora accion) ? accion : AccionBitacora.ACCION_INVALIDA,
                Modulo = (string)evento.Element("Modulo"),
                IP = (string)evento.Element("IP"),
                NombreMaquina = (string)evento.Element("NombreMaquina"),
                Descripcion = (string)evento.Element("Descripcion")
            };
        }

        private static DateTime? LeerFechaFiltro(XElement filtros, string atributo)
        {
            string valor = (string)filtros?.Attribute(atributo);

            if (string.IsNullOrEmpty(valor))
            {
                return null;
            }

            if (!DateTime.TryParseExact(valor, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fecha))
            {
                throw new InvalidOperationException(TextoLocalizado.T("El archivo tiene un filtro de fecha invalido."));
            }

            return fecha;
        }

        private static int? LeerEntero(string valor)
        {
            return int.TryParse(valor, out int numero) ? numero : (int?)null;
        }

        private static XAttribute AtributoOpcional(string nombre, string valor)
        {
            return string.IsNullOrEmpty(valor) ? null : new XAttribute(nombre, valor);
        }

        private static BE_Bitacora MapearFila(DataRow fila)
        {
            return new BE_Bitacora
            {
                IdBitacora = fila.Field<int>("IdBitacora"),
                IdUsuario = fila.Field<int?>("IdUsuario"),
                FechaHora = fila.Field<DateTime>("FechaHora"),
                Accion = Enum.TryParse(fila.Field<string>("Accion"), out AccionBitacora accion) ? accion : AccionBitacora.ACCION_INVALIDA,
                Criticidad = fila.Field<string>("Criticidad"),
                Modulo = fila.Field<string>("Modulo"),
                IP = fila.Field<string>("IP"),
                Descripcion = fila.Field<string>("Descripcion"),
                NombreMaquina = fila.Field<string>("NombreMaquina")
            };
        }
    }
}