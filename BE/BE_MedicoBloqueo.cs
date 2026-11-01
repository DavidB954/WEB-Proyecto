using System;

namespace BE
{
    public class BE_MedicoBloqueo
    {
        public int IdBloqueo { get; set; }

        public int IdMedico { get; set; }

        public DateTime Fecha { get; set; }

        public TimeSpan? HoraDesde { get; set; }

        public TimeSpan? HoraHasta { get; set; }

        public string Motivo { get; set; }

        public string DVH { get; set; }

        public bool DiaCompleto
        {
            get
            {
                return !HoraDesde.HasValue || !HoraHasta.HasValue;
            }
        }

        public string FechaTexto
        {
            get
            {
                return BE_MedicoHorario.ObtenerNombreDia(BE_MedicoHorario.DiaSemanaDe(Fecha)) + " " + Fecha.ToString("dd/MM/yyyy");
            }
        }

        public string HorarioTexto
        {
            get
            {
                return DiaCompleto ? "Dia completo" : HoraDesde.Value.ToString(@"hh\:mm") + " - " + HoraHasta.Value.ToString(@"hh\:mm");
            }
        }

        public TimeSpan InicioEfectivo => HoraDesde ?? TimeSpan.Zero;

        public TimeSpan FinEfectivo => HoraHasta ?? TimeSpan.FromDays(1);
    }
}
