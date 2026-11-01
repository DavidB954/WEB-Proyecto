using System;

namespace BE
{
    [Serializable]
    public class BE_Receta
    {
        public int IdReceta { get; set; }

        public int IdAtencion { get; set; }

        public string Medicamento { get; set; }

        public string Observaciones { get; set; }

        public string DVH { get; set; }

        public DateTime FechaEmision { get; set; }

        public string NombreMedico { get; set; }

        public string Especialidades { get; set; }

        public bool Vigente { get; set; }

        public string EstadoTexto
        {
            get
            {
                return TextoLocalizado.T(Vigente ? "Vigente" : "Vencida");
            }
        }

        public string EstadoCss
        {
            get
            {
                return Vigente ? "vigente" : "vencida";
            }
        }

        public string EspecialidadesTexto
        {
            get
            {
                return string.IsNullOrEmpty(Especialidades) ? "-" : Especialidades;
            }
        }

        public string Texto
        {
            get
            {
                return string.IsNullOrEmpty(Observaciones) ? Medicamento : Medicamento + " (" + Observaciones + ")";
            }
        }
    }
}
