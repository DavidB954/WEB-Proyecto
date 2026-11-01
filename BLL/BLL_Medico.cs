using BE;
using DAL;
using SERVICIOS;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BLL
{
    public class BLL_Medico
    {
        DAL_Medico dal_medico = new DAL_Medico();
        DAL_Especialidad dal_especialidad = new DAL_Especialidad();
        BLL_Bitacora bll_bitacora = new BLL_Bitacora();

        public List<BE_Medico> BuscarMedicos(string nombre, int? idEspecialidad)
        {
            try
            {
                return dal_medico.BuscarMedicos(nombre, idEspecialidad);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al buscar medicos: {ex.Message}", ex);
            }
        }

        public List<int> ObtenerIdsEspecialidad(int idMedico)
        {
            try
            {
                return dal_medico.ObtenerIdsEspecialidad(idMedico);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener las especialidades del medico: {ex.Message}", ex);
            }
        }

        public void GuardarDatosMedico(BE_Usuario usuarioLogueado, int idMedico, int matricula, List<int> idsEspecialidad)
        {
            try
            {
                if (!dal_medico.EsUsuarioMedico(idMedico))
                {
                    throw new InvalidOperationException(TextoLocalizado.T("El usuario seleccionado no tiene el rol MEDICO."));
                }

                if (matricula <= 0)
                {
                    throw new InvalidOperationException(TextoLocalizado.T("La matricula debe ser un numero mayor a cero."));
                }

                if (dal_medico.ExisteMatricula(matricula, idMedico))
                {
                    throw new InvalidOperationException(TextoLocalizado.T("La matricula {0} ya esta registrada para otro medico.", matricula));
                }

                idsEspecialidad = (idsEspecialidad ?? new List<int>()).Distinct().ToList();

                if (idsEspecialidad.Count == 0)
                {
                    throw new InvalidOperationException(TextoLocalizado.T("Selecciona al menos una especialidad para el medico."));
                }

                List<BE_Especialidad> especialidades = dal_especialidad.ObtenerEspecialidades();

                if (idsEspecialidad.Any(id => especialidades.All(e => e.IdEspecialidad != id)))
                {
                    throw new InvalidOperationException(TextoLocalizado.T("Alguna de las especialidades seleccionadas ya no existe. Recarga la pagina."));
                }

                string dvh = HashHelper.GenerarHash($"{idMedico}|{matricula}");

                dal_medico.GuardarMedico(idMedico, matricula, dvh, idsEspecialidad);
                dal_medico.ActualizarRangoHorario(idMedico);

                string nombres = string.Join(", ", especialidades.Where(e => idsEspecialidad.Contains(e.IdEspecialidad)).Select(e => e.Nombre));

                bll_bitacora.RegistrarEvento(usuarioLogueado.IdUsuario, AccionBitacora.MEDICO_DATOS_MODIFICACION, "MEDICO",
                    $"Se actualizan los datos del medico ID {idMedico}: matricula {matricula}, especialidades: {nombres}. Realizado por {usuarioLogueado.NombreApellido}.");
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al guardar los datos del medico: {ex.Message}", ex);
            }
        }
    }
}
