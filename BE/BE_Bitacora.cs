using System;
using System.Xml.Serialization;

namespace BE
{
    public class BE_Bitacora
    {
        public int IdBitacora
        {
            get; set;
        }

        public int? IdUsuario
        {
            get; set;
        }

        public DateTime FechaHora
        {
            get; set;
        }

        public AccionBitacora Accion
        {
            get; set;
        }

        public string Modulo
        {
            get; set;
        }

        public string Criticidad
        {
            get; set;
        }

        public string IP
        {
            get; set;
        }

        public string Descripcion
        {
            get; set;
        }

        public string NombreMaquina
        {
            get; set;
        }

        [XmlIgnore]
        public string DVH
        {
            get; set;
        }
    }
}
