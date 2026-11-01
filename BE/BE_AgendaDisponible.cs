using System;
using System.Collections.Generic;

namespace BE
{
    public class BE_AgendaDisponible
    {
        public List<DateTime> TurnosLibres { get; set; }

        public HashSet<DateTime> DiasDeAtencion { get; set; }
    }
}
