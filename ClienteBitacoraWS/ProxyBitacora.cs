using System;
using System.Net;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml.Serialization;

namespace ClienteBitacoraWS
{
    // Proxy SOAP de WSBitacora.asmx. No referencia ningun proyecto de la solucion:
    // lo unico que comparte con el servicio es el contrato publicado en el WSDL.
    [WebServiceBinding(Name = "WSBitacoraSoap", Namespace = Espacio)]
    public class ProxyBitacora : SoapHttpClientProtocol
    {
        public const string Espacio = "http://gestionmedica.local/ws/";

        public ProxyBitacora(string url)
        {
            Url = url;

            // El servicio guarda el usuario en la sesion: sin conservar la cookie,
            // cada llamada seria un visitante nuevo.
            CookieContainer = new CookieContainer();
        }

        [SoapDocumentMethod(Espacio + "IniciarSesion")]
        public void IniciarSesion(string email, string password)
        {
            Invoke("IniciarSesion", new object[] { email, password });
        }

        [SoapDocumentMethod(Espacio + "CerrarSesion")]
        public void CerrarSesion()
        {
            Invoke("CerrarSesion", new object[0]);
        }

        [SoapDocumentMethod(Espacio + "ConsultarBitacora")]
        public EventoBitacora[] ConsultarBitacora(string desde, string hasta, string idUsuario, string modulo, string ip, string criticidad)
        {
            return (EventoBitacora[])Invoke("ConsultarBitacora", new object[] { desde, hasta, idUsuario, modulo, ip, criticidad })[0];
        }

        [SoapDocumentMethod(Espacio + "ExportarBitacoraXml")]
        public string ExportarBitacoraXml(string desde, string hasta, string idUsuario, string modulo, string ip, string criticidad)
        {
            return (string)Invoke("ExportarBitacoraXml", new object[] { desde, hasta, idUsuario, modulo, ip, criticidad })[0];
        }
    }

    // Solo los campos que usa el cliente: los demas del WSDL se ignoran al deserializar.
    [XmlType("BE_Bitacora", Namespace = ProxyBitacora.Espacio)]
    public class EventoBitacora
    {
        public int IdBitacora { get; set; }

        public DateTime FechaHora { get; set; }

        public string Accion { get; set; }

        public string Modulo { get; set; }

        public string Criticidad { get; set; }

        public string Descripcion { get; set; }
    }
}
