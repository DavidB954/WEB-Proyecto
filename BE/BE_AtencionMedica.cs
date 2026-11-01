using System;
using System.Collections.Generic;
using System.Linq;

namespace BE
{
    public class BE_AtencionMedica
    {
        public int IdAtencion { get; set; }

        public int IdPaciente { get; set; }

        public int IdMedico { get; set; }

        public int IdTurno { get; set; }

        public DateTime FechaHora { get; set; }

        public string MotivoConsulta { get; set; }

        public string Diagnostico { get; set; }

        public string Observaciones { get; set; }

        public string DVH { get; set; }

        public string NombreMedico { get; set; }

        public List<BE_Receta> Recetas { get; set; } = new List<BE_Receta>();

        public string FechaTexto
        {
            get
            {
                return FechaHora.ToString("dd/MM/yyyy HH:mm");
            }
        }

        public string RecetasTexto
        {
            get
            {
                return Recetas.Count == 0 ? "-" : string.Join("; ", Recetas.Select(r => r.Texto));
            }
        }
    }
}
