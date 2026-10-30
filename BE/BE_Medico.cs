namespace BE
{
    // Un medico es un Usuario con el rol MEDICO. La fila en la tabla Medico
    // (matricula) recien existe cuando el administrativo la registra: hasta
    // entonces Matricula es null y no se le pueden cargar horarios.
    public class BE_Medico
    {
        public int IdMedico { get; set; }

        public string Nombre { get; set; }

        public string Apellido { get; set; }

        public string NombreApellido
        {
            get
            {
                return Apellido + ", " + Nombre;
            }
        }

        public bool UsuarioActivo { get; set; }

        public int? Matricula { get; set; }

        public string MatriculaTexto
        {
            get
            {
                return Matricula.HasValue ? Matricula.Value.ToString() : "-";
            }
        }

        public string Especialidades { get; set; }

        public string EspecialidadesTexto
        {
            get
            {
                return string.IsNullOrEmpty(Especialidades) ? "(sin especialidad)" : Especialidades;
            }
        }

        public int FranjasActivas { get; set; }

        public string DVH { get; set; }
    }
}
