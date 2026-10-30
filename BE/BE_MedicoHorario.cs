using System;

namespace BE
{
    // Franja semanal de atencion: un dia de la semana, un rango horario y cada
    // cuantos minutos se da un turno dentro de ese rango.
    public class BE_MedicoHorario
    {
        private static readonly string[] NombresDias = { "Lunes", "Martes", "Miercoles", "Jueves", "Viernes", "Sabado", "Domingo" };

        public int IdHorario { get; set; }

        public int IdMedico { get; set; }

        // 1 = Lunes ... 7 = Domingo
        public int DiaSemana { get; set; }

        public TimeSpan HoraDesde { get; set; }

        public TimeSpan HoraHasta { get; set; }

        public int DuracionTurno { get; set; }

        public bool Activo { get; set; }

        public string DVH { get; set; }

        public string NombreDia
        {
            get
            {
                return ObtenerNombreDia(DiaSemana);
            }
        }

        public string HorarioTexto
        {
            get
            {
                return HoraDesde.ToString(@"hh\:mm") + " - " + HoraHasta.ToString(@"hh\:mm");
            }
        }

        public int CantidadTurnos
        {
            get
            {
                return DuracionTurno <= 0 ? 0 : (int)(HoraHasta - HoraDesde).TotalMinutes / DuracionTurno;
            }
        }

        public string Estado
        {
            get
            {
                return TextoLocalizado.T(Activo ? "Activa" : "Inactiva");
            }
        }

        // Clase CSS del badge: no depende del idioma, a diferencia de Estado.
        public string EstadoCss
        {
            get
            {
                return Activo ? "activa" : "inactiva";
            }
        }

        public static string ObtenerNombreDia(int diaSemana)
        {
            return diaSemana >= 1 && diaSemana <= 7 ? TextoLocalizado.T(NombresDias[diaSemana - 1]) : "?";
        }

        // DayOfWeek arranca en Domingo = 0; la agenda usa Lunes = 1 ... Domingo = 7.
        public static int DiaSemanaDe(DateTime fecha)
        {
            return ((int)fecha.DayOfWeek + 6) % 7 + 1;
        }
    }
}
