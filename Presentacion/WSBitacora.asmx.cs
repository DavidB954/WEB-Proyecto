using BE;
using BE.Seguridad;
using BLL;
using SERVICIOS;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.Services;

namespace Presentacion
{
    
    /// WSDL: WSBitacora.asmx?WSDL. Usa la sesion del sitio (EnableSession) para
    /// exigir un usuario logueado con permisos sobre la bitacora: sin eso,
    /// cualquiera podria leer la auditoria del sistema desde afuera.
    ///
    [WebService(Namespace = "http://gestionmedica.local/ws/", Description = "Consulta y exportacion de la bitacora de eventos.")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    public class WSBitacora : WebService
    {
        private const string PermisoExportar = "BITACORA_EXPORTAR";

        private readonly BLL_Bitacora bll_bitacora = new BLL_Bitacora();

        [WebMethod(EnableSession = true, Description = "Devuelve los eventos de la bitacora que cumplen los filtros (vacio = sin filtro).")]
        public List<BE_Bitacora> ConsultarBitacora(string desde, string hasta, string idUsuario, string modulo, string ip, string criticidad)
        {
            ExigirAccesoBitacora();

            Filtros filtros = new Filtros(desde, hasta, idUsuario, modulo, ip, criticidad);

            return bll_bitacora.ConsultarBitacora(filtros.Desde, filtros.HastaFinDelDia, filtros.IdUsuario, filtros.Modulo, filtros.Ip, filtros.Criticidad);
        }

        [WebMethod(EnableSession = true, Description = "Devuelve un documento XML con los eventos que cumplen los filtros. Requiere el permiso BITACORA_EXPORTAR.")]
        public string ExportarBitacoraXml(string desde, string hasta, string idUsuario, string modulo, string ip, string criticidad)
        {
            BE_Usuario usuario = ExigirAccesoBitacora();

            if (!SeguridadHelper.TieneAccesoAccion(usuario, Session["Permisos"] as UsuarioComponente, PermisoExportar))
            {
                throw new UnauthorizedAccessException("El usuario no tiene permiso para exportar la bitacora.");
            }

            Filtros filtros = new Filtros(desde, hasta, idUsuario, modulo, ip, criticidad);

            List<BE_Bitacora> eventos = bll_bitacora.ConsultarBitacora(
                filtros.Desde, filtros.HastaFinDelDia, filtros.IdUsuario, filtros.Modulo, filtros.Ip, filtros.Criticidad);

            string xml = bll_bitacora.GenerarXml(
                eventos, filtros.Desde, filtros.Hasta, filtros.IdUsuario, filtros.Modulo, filtros.Ip, filtros.Criticidad, usuario.NombreApellido);

            bll_bitacora.RegistrarEvento(usuario.IdUsuario, AccionBitacora.BITACORA_EXPORTADA, "SEGURIDAD",
                $"{usuario.NombreApellido} exporto {eventos.Count} eventos de la bitacora a XML ({filtros.Descripcion()}).");

            return xml;
        }

        private BE_Usuario ExigirAccesoBitacora()
        {
            BE_Usuario usuario = Session["Usuario"] as BE_Usuario;
            UsuarioComponente permisos = Session["Permisos"] as UsuarioComponente;

            if (!SeguridadHelper.TieneAccesoPagina(usuario, permisos, "Bitacora.aspx"))
            {
                throw new UnauthorizedAccessException("Debe iniciar sesion con un usuario que tenga acceso a la bitacora.");
            }

            return usuario;
        }

        private class Filtros
        {
            public Filtros(string desde, string hasta, string idUsuario, string modulo, string ip, string criticidad)
            {
                Desde = ParsearFecha(desde, nameof(desde));
                Hasta = ParsearFecha(hasta, nameof(hasta));
                IdUsuario = ParsearEntero(idUsuario, nameof(idUsuario));
                Modulo = Normalizar(modulo);
                Ip = Normalizar(ip);
                Criticidad = Normalizar(criticidad);
            }

            public DateTime? Desde { get; }
            public DateTime? Hasta { get; }
            public int? IdUsuario { get; }
            public string Modulo { get; }
            public string Ip { get; }
            public string Criticidad { get; }

            // "hasta" es un dia completo: incluye los eventos hasta las 23:59:59.
            public DateTime? HastaFinDelDia => Hasta?.AddDays(1).AddSeconds(-1);

            public string Descripcion()
            {
                List<string> partes = new List<string>();

                if (Desde != null) partes.Add($"desde {Desde:yyyy-MM-dd}");
                if (Hasta != null) partes.Add($"hasta {Hasta:yyyy-MM-dd}");
                if (IdUsuario != null) partes.Add($"usuario {IdUsuario}");
                if (Modulo != null) partes.Add($"modulo {Modulo}");
                if (Ip != null) partes.Add($"IP {Ip}");
                if (Criticidad != null) partes.Add($"criticidad {Criticidad}");

                return partes.Count == 0 ? "sin filtros" : string.Join(", ", partes);
            }

            private static DateTime? ParsearFecha(string valor, string parametro)
            {
                if (string.IsNullOrWhiteSpace(valor))
                {
                    return null;
                }

                if (!DateTime.TryParseExact(valor.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fecha))
                {
                    throw new ArgumentException($"El parametro '{parametro}' debe tener el formato yyyy-MM-dd.");
                }

                return fecha.Date;
            }

            private static int? ParsearEntero(string valor, string parametro)
            {
                if (string.IsNullOrWhiteSpace(valor))
                {
                    return null;
                }

                if (!int.TryParse(valor.Trim(), out int numero))
                {
                    throw new ArgumentException($"El parametro '{parametro}' debe ser un numero entero.");
                }

                return numero;
            }

            private static string Normalizar(string valor)
            {
                return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
            }
        }
    }
}
