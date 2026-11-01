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

        public string DVH { get; set; }

        public string NombreRol { get; set; }

        public int? IdIdiomaPreferido { get; set; }

        public bool EsEmergencia { get; private set; }

        public static BE_Usuario CrearEmergencia(string nombre, string email, string nombreRol)
        {
            return new BE_Usuario
            {
                Nombre = nombre,
                Apellido = "de Emergencia",
                Email = email,
                NombreRol = nombreRol,
                Activo = true,
                EsEmergencia = true
            };
        }
    }
}
