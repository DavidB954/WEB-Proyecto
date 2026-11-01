using System;

namespace BE
{
    public class BE_Turno
    {
        public int IdTurno { get; set; }

        public int IdPaciente { get; set; }

        public int IdMedico { get; set; }

        public DateTime FechaHora { get; set; }

        public EstadoTurno Estado { get; set; }

        public string DVH { get; set; }

        public string NombreMedico { get; set; }

        public string Especialidades { get; set; }

        public string NombrePaciente { get; set; }

        public string HistoriaClinica { get; set; }

        public DateTime FechaNacimientoPaciente { get; set; }

        public bool PuedeCancelar { get; set; }

        public bool PuedeAtender { get; set; }

        public bool PuedeMarcarAusente { get; set; }

        public int EdadPaciente
        {
            get
            {
                int edad = FechaHora.Year - FechaNacimientoPaciente.Year;
                return FechaNacimientoPaciente.Date > FechaHora.Date.AddYears(-edad) ? edad - 1 : edad;
            }
        }

        public string FechaTexto
        {
            get
            {
                return BE_MedicoHorario.ObtenerNombreDia(BE_MedicoHorario.DiaSemanaDe(FechaHora)) + " " + FechaHora.ToString("dd/MM/yyyy");
            }
        }

        public string HoraTexto
        {
            get
            {
                return FechaHora.ToString("HH:mm");
            }
        }

        public string EspecialidadesTexto
        {
            get
            {
                return string.IsNullOrEmpty(Especialidades) ? "-" : Especialidades;
            }
        }

        public string EstadoTexto
        {
            get
            {
                return TextoLocalizado.T(Estado.ToString());
            }
        }

        public string EstadoCss
        {
            get
            {
                return Estado.ToString().ToLower();
            }
        }
    }
}
