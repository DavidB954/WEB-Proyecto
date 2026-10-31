using System;

namespace BE
{
    // Serializable: Medicos.aspx guarda en ViewState los medicamentos que el
    // medico va agregando antes de guardar la atencion.
    [Serializable]
    public class BE_Receta
    {
        public int IdReceta { get; set; }

        public int IdAtencion { get; set; }

        public string Medicamento { get; set; }

        // Indicaciones (dosis, frecuencia, duracion).
        public string Observaciones { get; set; }

        public string DVH { get; set; }

        // Datos de la atencion que la emitio, solo para mostrar (Recetas.aspx).
        public DateTime FechaEmision { get; set; }

        public string NombreMedico { get; set; }

        public string Especialidades { get; set; }

        // Lo calcula BLL_Receta segun los dias de vigencia.
        public bool Vigente { get; set; }

        public string EstadoTexto
        {
            get
            {
                return TextoLocalizado.T(Vigente ? "Vigente" : "Vencida");
            }
        }

        // Clase CSS del badge: no depende del idioma, a diferencia de EstadoTexto.
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
