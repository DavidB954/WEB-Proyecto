using System;

namespace BE
{
    // Fecha (o rango de horas de una fecha) en la que el medico no atiende,
    // aunque caiga dentro de una de sus franjas semanales.
    public class BE_MedicoBloqueo
    {
        public int IdBloqueo { get; set; }

        public int IdMedico { get; set; }

        public DateTime Fecha { get; set; }

        // Ambos null = dia completo.
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

        // Para comparar superposiciones: un dia completo ocupa 00:00 - 24:00.
        public TimeSpan InicioEfectivo => HoraDesde ?? TimeSpan.Zero;

        public TimeSpan FinEfectivo => HoraHasta ?? TimeSpan.FromDays(1);
    }
}
