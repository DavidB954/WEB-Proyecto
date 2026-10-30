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

        // Datos del medico, solo para mostrar.
        public string NombreMedico { get; set; }

        public string Especialidades { get; set; }

        // Datos del paciente, solo para mostrar.
        public string NombrePaciente { get; set; }

        public string HistoriaClinica { get; set; }

        public DateTime FechaNacimientoPaciente { get; set; }

        // Lo calcula BLL_Turno segun la anticipacion minima para cancelar.
        public bool PuedeCancelar { get; set; }

        // Los calcula BLL_AtencionMedica para la grilla del medico.
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

        // Clase CSS del badge: no depende del idioma, a diferencia de EstadoTexto.
        public string EstadoCss
        {
            get
            {
                return Estado.ToString().ToLower();
            }
        }
    }
}
