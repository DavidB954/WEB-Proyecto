using BE;
using DAL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BLL
{
    public class BLL_Especialidad
    {
        DAL_Especialidad dal_especialidad = new DAL_Especialidad();
        DAL_Medico dal_medico = new DAL_Medico();
        BLL_Bitacora bll_bitacora = new BLL_Bitacora();

        public List<BE_Especialidad> ObtenerEspecialidades()
        {
            try
            {
                return dal_especialidad.ObtenerEspecialidades();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener especialidades: {ex.Message}", ex);
            }
        }

        public void AgregarEspecialidad(BE_Usuario usuarioLogueado, string nombre)
        {
            try
            {
                nombre = (nombre ?? string.Empty).Trim();

                if (nombre.Length == 0)
                {
                    throw new InvalidOperationException(TextoLocalizado.T("Ingresa el nombre de la especialidad."));
                }

                if (nombre.Length > 30)
                {
                    throw new InvalidOperationException(TextoLocalizado.T("El nombre de la especialidad no puede superar los 30 caracteres (tiene {0}).", nombre.Length));
                }

                if (dal_especialidad.ObtenerEspecialidades().Any(e => string.Equals(e.Nombre, nombre, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new InvalidOperationException(TextoLocalizado.T("Ya existe la especialidad '{0}'.", nombre));
                }

                BE_Especialidad especialidad = new BE_Especialidad { Nombre = nombre };
                dal_especialidad.AgregarEspecialidad(especialidad);

                bll_bitacora.RegistrarEvento(usuarioLogueado.IdUsuario, AccionBitacora.ESPECIALIDAD_ALTA, "MEDICO",
                    $"Se crea la especialidad '{especialidad.Nombre}' (ID {especialidad.IdEspecialidad}). Realizado por {usuarioLogueado.NombreApellido}.");
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al crear la especialidad: {ex.Message}", ex);
            }
        }

        public void EliminarEspecialidad(BE_Usuario usuarioLogueado, int idEspecialidad)
        {
            try
            {
                BE_Especialidad especialidad = dal_especialidad.ObtenerEspecialidades().FirstOrDefault(e => e.IdEspecialidad == idEspecialidad);

                if (especialidad == null)
                {
                    throw new InvalidOperationException(TextoLocalizado.T("La especialidad seleccionada ya no existe."));
                }

                List<BE_Medico> medicos = dal_medico.BuscarMedicos(null, idEspecialidad);

                if (medicos.Count > 0)
                {
                    throw new InvalidOperationException(TextoLocalizado.T(
                        "No se puede eliminar '{0}' porque la tienen asignada: {1}. Para quitarsela a un medico, seleccionalo, destilda la especialidad y presiona 'Guardar datos'.",
                        especialidad.Nombre, string.Join("; ", medicos.Select(m => m.NombreApellido))));
                }

                dal_especialidad.EliminarEspecialidad(idEspecialidad);

                bll_bitacora.RegistrarEvento(usuarioLogueado.IdUsuario, AccionBitacora.ESPECIALIDAD_BAJA, "MEDICO",
                    $"Se elimina la especialidad '{especialidad.Nombre}' (ID {idEspecialidad}). Realizado por {usuarioLogueado.NombreApellido}.");
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al eliminar la especialidad: {ex.Message}", ex);
            }
        }
    }
}
