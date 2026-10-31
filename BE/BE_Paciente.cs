using System;

namespace BE
{
    // Un paciente es un Usuario con el rol PACIENTE. La fila en la tabla
    // Paciente se crea la primera vez que completa sus datos desde Turnos.aspx:
    // hasta entonces no puede solicitar turnos (Turno.IdPaciente apunta aca).
    public class BE_Paciente
    {
        public int IdPaciente { get; set; }

        public string NumeroHistoriaClinica { get; set; }

        public DateTime FechaNacimiento { get; set; }

        public string Telefono { get; set; }

        public string DVH { get; set; }
    }
}
