using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE
{
    public class BE_Idioma
    {
        public int IdIdioma { get; set; }

        public string Codigo { get; set; }

        public string Nombre { get; set; }

        public bool Activo { get; set; }

        public bool PorDefecto { get; set; }
    }
}
