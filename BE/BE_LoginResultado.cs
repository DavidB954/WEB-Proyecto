using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE
{
    public class BE_LoginResultado
    {
        public bool ExitoLogin
        {
            get; set;
        }

        public int IntentosFallidos
        {
            get; set;
        }

        public string Mensaje
        {
            get; set;
        }

        public bool Activo
        {
            get; set;
        }

        public BE_Usuario Usuario
        {
            get; set;
        }
    }
}
