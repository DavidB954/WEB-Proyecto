using BE;
using DAL;
using SERVICIOS;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BLL
{
    public class BLL_Turno
    {
        // Hasta cuantos dias hacia adelante se ofrecen turnos.
        public const int DiasAnticipacionMaxima = 60;

        // Un turno se puede cancelar hasta esta cantidad de horas antes.
        public const int HorasMinimasCancelacion = 2;

        // Turnos reservados a futuro que puede tener un paciente al mismo tiempo.
        public const int MaximoTurnosReservados = 5;

        DAL_Turno dal_turno = new DAL_Turno();
        DAL_Paciente dal_paciente = new DAL_Paciente();
        DAL_Medico dal_medico = new DAL_Medico();
        DAL_AgendaMedico dal_agenda = new DAL_AgendaMedico();
        BLL_Bitacora bll_bitacora = new BLL_Bitacora();

        // Solo medicos que el paciente puede elegir: usuario activo, con matricula
        // y al menos una franja activa. Se excluye al propio usuario (un medico
        // que tambien es paciente no se saca turno a si mismo).
        public List<BE_Medico> BuscarMedicosConAgenda(BE_Usuario usuarioLogueado, string nombre, int? idEspecialidad)
        {
            try
            {
                return dal_medico.BuscarMedicos(nombre, idEspecialidad)
                    .Where(m => m.UsuarioActivo && m.Matricula.HasValue && m.FranjasActivas > 0 && m.IdMedico != usuarioLogueado.IdUsuario)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al buscar medicos: {ex.Message}", ex);
            }
        }

        // Todos los horarios libres del medico desde ahora hasta
        // DiasAnticipacionMaxima, ya descontados los que no le sirven a este
        // paciente (ver CalcularTurnosLibres).
        public List<DateTime> ObtenerTurnosLibres(BE_Usuario usuarioLogueado, int idMedico)
        {
            try
            {
                return CalcularTurnosLibres(usuarioLogueado.IdUsuario, idMedico, DateTime.Today, DateTime.Today.AddDays(DiasAnticipacionMaxima));
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener los turnos disponibles: {ex.Message}", ex);
            }
        }

        public List<BE_Turno> ObtenerTurnosPaciente(BE_Usuario usuarioLogueado)
        {
            try
            {
                List<BE_Turno> turnos = dal_turno.ObtenerTurnosPaciente(usuarioLogueado.IdUsuario);

                foreach (BE_Turno turno in turnos)
                {
                    turno.PuedeCancelar = SePuedeCancelar(turno);
                }

                return turnos;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener los turnos: {ex.Message}", ex);
            }
        }

        public void SolicitarTurno(BE_Usuario usuarioLogueado, int idMedico, DateTime fechaHora)
        {
            try
            {
                int idPaciente = usuarioLogueado.IdUsuario;

                if (!dal_paciente.EsUsuarioPaciente(idPaciente))
                {
                    throw new InvalidOperationException(TextoLocalizado.T("Tu usuario no tiene el rol PACIENTE."));
                }

                if (dal_paciente.ObtenerPaciente(idPaciente) == null)
                {
                    throw new InvalidOperationException(TextoLocalizado.T("Antes de pedir un turno completa tus datos de paciente."));
                }

                BE_Medico medico = BuscarMedicosConAgenda(usuarioLogueado, null, null).FirstOrDefault(m => m.IdMedico == idMedico);

                if (medico == null)
                {
                    throw new InvalidOperationException(TextoLocalizado.T("El medico seleccionado no esta tomando turnos."));
                }

                List<BE_Turno> turnosPaciente = dal_turno.ObtenerTurnosPaciente(idPaciente);

                int reservados = turnosPaciente.Count(t => t.Estado == EstadoTurno.Reservado && t.FechaHora > DateTime.Now);

                if (reservados >= MaximoTurnosReservados)
                {
                    throw new InvalidOperationException(TextoLocalizado.T("Ya tenes {0} turnos reservados, que es el maximo permitido. Cancela alguno para pedir otro.", reservados));
                }

                // Vuelve a calcular la agenda de ese dia: entre que se mostro el
                // horario y se confirmo, el medico pudo bloquearlo o lo pudo tomar
                // otro paciente.
                if (!CalcularTurnosLibres(idPaciente, idMedico, fechaHora.Date, fechaHora.Date).Contains(fechaHora))
                {
                    throw new InvalidOperationException(TextoLocalizado.T("Ese horario ya no esta disponible. Elegi otro."));
                }

                BE_Turno turno = new BE_Turno
                {
                    IdPaciente = idPaciente,
                    IdMedico = idMedico,
                    FechaHora = fechaHora,
                    Estado = EstadoTurno.Reservado
                };

                if (!dal_turno.AgregarTurno(turno))
                {
                    throw new InvalidOperationException(TextoLocalizado.T("Otro paciente acaba de reservar ese horario. Elegi otro."));
                }

                turno.DVH = HashHelper.GenerarHash(CadenaTurno(turno));
                dal_turno.ActualizarDVH(turno.IdTurno, turno.DVH);

                bll_bitacora.RegistrarEvento(usuarioLogueado.IdUsuario, AccionBitacora.TURNO_ALTA, "TURNOS",
                    $"{usuarioLogueado.NombreApellido} reserva el turno ID {turno.IdTurno} con {medico.NombreApellido} para el {turno.FechaTexto} a las {turno.HoraTexto}.");
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al reservar el turno: {ex.Message}", ex);
            }
        }

        public void CancelarTurno(BE_Usuario usuarioLogueado, int idTurno)
        {
            try
            {
                // Se busca entre los turnos del paciente logueado: asi no puede
                // cancelar uno ajeno aunque fuerce el IdTurno en el postback.
                BE_Turno turno = dal_turno.ObtenerTurnosPaciente(usuarioLogueado.IdUsuario).FirstOrDefault(t => t.IdTurno == idTurno);

                if (turno == null)
                {
                    throw new InvalidOperationException(TextoLocalizado.T("El turno seleccionado ya no existe."));
                }

                if (turno.Estado != EstadoTurno.Reservado)
                {
                    throw new InvalidOperationException(TextoLocalizado.T("El turno no se puede cancelar porque esta {0}.", turno.EstadoTexto.ToLower()));
                }

                if (!SePuedeCancelar(turno))
                {
                    throw new InvalidOperationException(TextoLocalizado.T("Los turnos se pueden cancelar hasta {0} horas antes. Comunicate con el centro medico.", HorasMinimasCancelacion));
                }

                turno.Estado = EstadoTurno.Cancelado;
                turno.DVH = HashHelper.GenerarHash(CadenaTurno(turno));

                if (!dal_turno.CambiarEstado(idTurno, EstadoTurno.Reservado, EstadoTurno.Cancelado, turno.DVH))
                {
                    throw new InvalidOperationException(TextoLocalizado.T("El turno cambio de estado mientras lo cancelabas. Revisa tus turnos."));
                }

                bll_bitacora.RegistrarEvento(usuarioLogueado.IdUsuario, AccionBitacora.TURNO_CANCELADO, "TURNOS",
                    $"{usuarioLogueado.NombreApellido} cancela el turno ID {idTurno} con {turno.NombreMedico} del {turno.FechaTexto} a las {turno.HoraTexto}.");
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al cancelar el turno: {ex.Message}", ex);
            }
        }

        // Arma los turnos a partir de las franjas activas del medico y descuenta:
        // horarios ya pasados, bloqueos, turnos tomados por otros pacientes,
        // horarios en los que el paciente ya tiene turno con otro medico y dias
        // en los que ya tiene turno con este mismo medico.
        private List<DateTime> CalcularTurnosLibres(int idPaciente, int idMedico, DateTime desdeFecha, DateTime hastaFecha)
        {
            List<DateTime> libres = new List<DateTime>();

            desdeFecha = desdeFecha.Date < DateTime.Today ? DateTime.Today : desdeFecha.Date;
            hastaFecha = hastaFecha.Date > DateTime.Today.AddDays(DiasAnticipacionMaxima) ? DateTime.Today.AddDays(DiasAnticipacionMaxima) : hastaFecha.Date;

            if (hastaFecha < desdeFecha)
            {
                return libres;
            }

            List<BE_MedicoHorario> franjas = dal_agenda.ObtenerHorarios(idMedico).Where(h => h.Activo).ToList();

            if (franjas.Count == 0)
            {
                return libres;
            }

            List<BE_MedicoBloqueo> bloqueos = dal_agenda.ObtenerBloqueos(idMedico, desdeFecha);
            HashSet<DateTime> ocupados = dal_turno.ObtenerHorariosOcupadosMedico(idMedico, desdeFecha, hastaFecha.AddDays(1));

            List<BE_Turno> vigentesPaciente = dal_turno.ObtenerTurnosPaciente(idPaciente).Where(t => t.Estado != EstadoTurno.Cancelado).ToList();
            HashSet<DateTime> horasPaciente = new HashSet<DateTime>(vigentesPaciente.Select(t => t.FechaHora));
            HashSet<DateTime> diasConEsteMedico = new HashSet<DateTime>(vigentesPaciente.Where(t => t.IdMedico == idMedico).Select(t => t.FechaHora.Date));

            DateTime ahora = DateTime.Now;

            for (DateTime fecha = desdeFecha; fecha <= hastaFecha; fecha = fecha.AddDays(1))
            {
                if (diasConEsteMedico.Contains(fecha))
                {
                    continue;
                }

                int diaSemana = BE_MedicoHorario.DiaSemanaDe(fecha);
                List<BE_MedicoBloqueo> bloqueosDelDia = bloqueos.Where(b => b.Fecha.Date == fecha).ToList();

                foreach (BE_MedicoHorario franja in franjas.Where(f => f.DiaSemana == diaSemana))
                {
                    TimeSpan duracion = TimeSpan.FromMinutes(franja.DuracionTurno);

                    for (TimeSpan inicio = franja.HoraDesde; inicio + duracion <= franja.HoraHasta; inicio += duracion)
                    {
                        DateTime turno = fecha + inicio;
                        TimeSpan fin = inicio + duracion;

                        if (turno <= ahora
                            || ocupados.Contains(turno)
                            || horasPaciente.Contains(turno)
                            || bloqueosDelDia.Any(b => inicio < b.FinEfectivo && b.InicioEfectivo < fin))
                        {
                            continue;
                        }

                        libres.Add(turno);
                    }
                }
            }

            libres.Sort();
            return libres;
        }

        private bool SePuedeCancelar(BE_Turno turno)
        {
            return turno.Estado == EstadoTurno.Reservado && turno.FechaHora >= DateTime.Now.AddHours(HorasMinimasCancelacion);
        }

        // Tambien la usa BLL_AtencionMedica al pasar el turno a Atendido o Ausente.
        internal static string CadenaTurno(BE_Turno t)
        {
            return $"{t.IdTurno}|{t.IdPaciente}|{t.IdMedico}|{t.FechaHora:yyyy-MM-dd HH:mm}|{(int)t.Estado}";
        }
    }
}
