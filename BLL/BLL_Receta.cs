using BE;
using DAL;
using System;
using System.Collections.Generic;

namespace BLL
{
    public class BLL_Receta
    {
        // Una receta vale esta cantidad de dias desde que se emite.
        public const int DiasVigencia = 30;

        DAL_AtencionMedica dal_atencion = new DAL_AtencionMedica();
        DAL_Paciente dal_paciente = new DAL_Paciente();

        // Las recetas del paciente logueado: no recibe un IdPaciente, asi nadie
        // puede ver las de otro forzando el postback.
        public List<BE_Receta> ObtenerRecetasPaciente(BE_Usuario usuarioLogueado)
        {
            try
            {
                if (!dal_paciente.EsUsuarioPaciente(usuarioLogueado.IdUsuario))
                {
                    throw new InvalidOperationException(TextoLocalizado.T("Tu usuario no tiene el rol PACIENTE."));
                }

                List<BE_Receta> recetas = dal_atencion.ObtenerRecetasPaciente(usuarioLogueado.IdUsuario);

                foreach (BE_Receta receta in recetas)
                {
                    receta.Vigente = receta.FechaEmision.Date.AddDays(DiasVigencia) >= DateTime.Today;
                }

                return recetas;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener las recetas: {ex.Message}", ex);
            }
        }
    }
}
