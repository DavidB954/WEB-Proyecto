using BE;
using DAL;
using SERVICIOS;
using System;
using System.Linq;

namespace BLL
{
    public class BLL_Paciente
    {
        DAL_Paciente dal_paciente = new DAL_Paciente();
        BLL_Bitacora bll_bitacora = new BLL_Bitacora();

        public bool EsPaciente(int idUsuario)
        {
            try
            {
                return dal_paciente.EsUsuarioPaciente(idUsuario);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al verificar el rol del usuario: {ex.Message}", ex);
            }
        }

        // null si todavia no completo sus datos.
        public BE_Paciente ObtenerPaciente(int idUsuario)
        {
            try
            {
                return dal_paciente.ObtenerPaciente(idUsuario);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener los datos del paciente: {ex.Message}", ex);
            }
        }

        // El propio paciente carga o corrige su fecha de nacimiento y telefono.
        // La historia clinica se asigna sola la primera vez y no cambia.
        public void GuardarDatos(BE_Usuario usuarioLogueado, DateTime fechaNacimiento, string telefono)
        {
            try
            {
                if (!dal_paciente.EsUsuarioPaciente(usuarioLogueado.IdUsuario))
                {
                    throw new InvalidOperationException(TextoLocalizado.T("Tu usuario no tiene el rol PACIENTE."));
                }

                fechaNacimiento = fechaNacimiento.Date;

                if (fechaNacimiento > DateTime.Today)
                {
                    throw new InvalidOperationException(TextoLocalizado.T("La fecha de nacimiento no puede ser futura."));
                }

                if (fechaNacimiento < DateTime.Today.AddYears(-120))
                {
                    throw new InvalidOperationException(TextoLocalizado.T("Revisa la fecha de nacimiento."));
                }

                telefono = string.IsNullOrWhiteSpace(telefono) ? null : telefono.Trim();

                // Paciente.Telefono es VARCHAR(20).
                if (telefono != null && (telefono.Length > 20 || !telefono.All(c => char.IsDigit(c) || " +-()".IndexOf(c) >= 0)))
                {
                    throw new InvalidOperationException(TextoLocalizado.T("El telefono solo puede tener numeros, espacios, +, - y parentesis (hasta 20 caracteres)."));
                }

                BE_Paciente existente = dal_paciente.ObtenerPaciente(usuarioLogueado.IdUsuario);

                BE_Paciente paciente = new BE_Paciente
                {
                    IdPaciente = usuarioLogueado.IdUsuario,
                    NumeroHistoriaClinica = existente != null ? existente.NumeroHistoriaClinica : "HC-" + usuarioLogueado.IdUsuario.ToString("D6"),
                    FechaNacimiento = fechaNacimiento,
                    Telefono = telefono
                };

                paciente.DVH = HashHelper.GenerarHash(CadenaPaciente(paciente));

                dal_paciente.GuardarPaciente(paciente);

                bll_bitacora.RegistrarEvento(usuarioLogueado.IdUsuario, AccionBitacora.PACIENTE_DATOS_MODIFICACION, "TURNOS",
                    $"{(existente == null ? "Se registran" : "Se actualizan")} los datos del paciente {usuarioLogueado.NombreApellido} (HC {paciente.NumeroHistoriaClinica}).");
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al guardar los datos del paciente: {ex.Message}", ex);
            }
        }

        private string CadenaPaciente(BE_Paciente p)
        {
            return $"{p.IdPaciente}|{p.NumeroHistoriaClinica}|{p.FechaNacimiento:yyyy-MM-dd}|{p.Telefono}";
        }
    }
}
