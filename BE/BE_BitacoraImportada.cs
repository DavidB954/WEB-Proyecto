using System;
using System.Collections.Generic;

namespace BE
{
    // Contenido de un XML de bitacora subido para consultarlo: los eventos del archivo
    // y el filtro con el que se exporto. No se guarda en la base.
    public class BE_BitacoraImportada
    {
        public string NombreArchivo { get; set; }

        public string Generado { get; set; }

        public string GeneradoPor { get; set; }

        public DateTime? Desde { get; set; }

        public DateTime? Hasta { get; set; }

        public int? IdUsuario { get; set; }

        public string Modulo { get; set; }

        public string Ip { get; set; }

        public string Criticidad { get; set; }

        public List<BE_Bitacora> Eventos { get; set; }
    }
}
