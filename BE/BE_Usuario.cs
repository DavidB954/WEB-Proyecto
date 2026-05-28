using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE
{
    public class BE_Usuario
    {
        public int IdUsuario { get; set; }

        public string Nombre { get; set; }

        public string Apellido { get; set; }

        public string NombreApellido {
            get
            {
                return Nombre + " " + Apellido;
            }
        }

        public string DNI { get; set; }

        public string Email { get; set; }

        public string HashPassword { get; set; }

        public int IntentosFallidos { get; set; }

        public bool Activo { get; set; }
    }
}
