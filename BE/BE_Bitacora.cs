using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        // No se expone en el Web Service ni en las exportaciones: es el
        // digito verificador interno de la fila.
        [XmlIgnore]
        public string DVH
        {
            get; set;
        }
    }
}
