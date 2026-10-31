using BE;
using DAL;
using SERVICIOS;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BLL
{
    public class BLL_AgendaMedico
    {
        // Duraciones de turno que se ofrecen en pantalla (minutos).
        public static readonly int[] DuracionesPermitidas = { 10, 15, 20, 30, 40, 45, 60 };

        DAL_AgendaMedico dal_agenda = new DAL_AgendaMedico();
        DAL_Medico dal_medico = new DAL_Medico();
        DAL_Turno dal_turno = new DAL_Turno();
        BLL_Bitacora bll_bitacora = new BLL_Bitacora();

        // ---- Franjas semanales ----

        public List<BE_MedicoHorario> ObtenerHorarios(int idMedico)
        {
            try
            {
                return dal_agenda.ObtenerHorarios(idMedico);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener los horarios del medico: {ex.Message}", ex);
            }
        }

        // Agrega la misma franja (desde/hasta/duracion) en cada uno de los dias
        // indicados. Se valida todo antes de insertar: si algun dia choca con una
        // franja existente no se agrega ninguno.
        public void AgregarHorarios(BE_Usuario usuarioLogueado, int idMedico, List<int> dias, TimeSpan desde, TimeSpan hasta, int duracionTurno)
        {
            try
            {
                if (!dal_medico.ExisteMedico(idMedico))
                {
                    throw new InvalidOperationException(TextoLocalizado.T("Primero guarda la matricula y especialidades del medico; despues podes cargarle horarios."));
                }

                dias = (dias ?? new List<int>()).Distinct().OrderBy(d => d).ToList();

                if (dias.Count == 0)
                {
                    throw new InvalidOperationException(TextoLocalizado.T("Selecciona al menos un dia de la semana."));
                }

                if (dias.Any(d => d < 1 || d > 7))
                {
                    throw new InvalidOperationException(TextoLocalizado.T("Dia de la semana invalido."));
                }

                if (!DuracionesPermitidas.Contains(duracionTurno))
                {
                    throw new InvalidOperationException(TextoLocalizado.T("Duracion de turno invalida."));
                }

                if (hasta <= desde)
                {
                    throw new InvalidOperationException(TextoLocalizado.T("La hora de fin tiene que ser posterior a la hora de inicio."));
                }

                int minutosFranja = (int)(hasta - desde).TotalMinutes;

                // Si el rango no es multiplo de la duracion, el ultimo turno quedaria
                // cortado: se pide ajustar el horario en lugar de perder minutos en silencio.
                if (minutosFranja % duracionTurno != 0)
                {
                    throw new InvalidOperationException(TextoLocalizado.T("El rango {0:hh\\:mm} - {1:hh\\:mm} ({2} min) no se puede dividir en turnos de {3} min. Ajusta el horario o la duracion.", desde, hasta, minutosFranja, duracionTurno));
                }

                List<BE_MedicoHorario> existentes = dal_agenda.ObtenerHorarios(idMedico);

                List<string> choques = new List<string>();

                foreach (int dia in dias)
                {
                    BE_MedicoHorario choque = existentes.FirstOrDefault(h => h.DiaSemana == dia && desde < h.HoraHasta && h.HoraDesde < hasta);

                    if (choque != null)
                    {
                        choques.Add($"{choque.NombreDia} {choque.HorarioTexto}");
                    }
                }

                if (choques.Count > 0)
                {
                    throw new InvalidOperationException(TextoLocalizado.T("La franja se superpone con horarios ya cargados: {0}.", string.Join("; ", choques)));
                }

                foreach (int dia in dias)
                {
                    BE_MedicoHorario horario = new BE_MedicoHorario
                    {
                        IdMedico = idMedico,
                        DiaSemana = dia,
                        HoraDesde = desde,
                        HoraHasta = hasta,
                        DuracionTurno = duracionTurno,
                        Activo = true
                    };

                    dal_agenda.AgregarHorario(horario);

                    horario.DVH = HashHelper.GenerarHash(CadenaHorario(horario));
                    dal_agenda.ActualizarDVHHorario(horario.IdHorario, horario.DVH);
                }

                dal_medico.ActualizarRangoHorario(idMedico);

                string nombresDias = string.Join(", ", dias.Select(BE_MedicoHorario.ObtenerNombreDia));

                bll_bitacora.RegistrarEvento(usuarioLogueado.IdUsuario, AccionBitacora.HORARIO_ALTA, "AGENDA",
                    $"Se agrega al medico ID {idMedico} la franja {desde:hh\\:mm} - {hasta:hh\\:mm} (turnos de {duracionTurno} min) los dias: {nombresDias}. Realizado por {usuarioLogueado.NombreApellido}.");
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al agregar el horario: {ex.Message}", ex);
            }
        }

        // Al desactivar una franja se cancelan los turnos reservados que caen en
        // ella. Devuelve cuantos turnos se cancelaron.
        public int CambiarEstadoHorario(BE_Usuario usuarioLogueado, int idMedico, int idHorario, bool activo)
        {
            try
            {
                BE_MedicoHorario horario = dal_agenda.ObtenerHorarios(idMedico).FirstOrDefault(h => h.IdHorario == idHorario);

                if (horario == null)
                {
                    throw new InvalidOperationException(TextoLocalizado.T("La franja seleccionada ya no existe."));
                }

                if (horario.Activo == activo)
                {
                    return 0;
                }

                horario.Activo = activo;
                horario.DVH = HashHelper.GenerarHash(CadenaHorario(horario));

                dal_agenda.ActualizarEstadoHorario(idHorario, activo, horario.DVH);
                dal_medico.ActualizarRangoHorario(idMedico);

                bll_bitacora.RegistrarEvento(usuarioLogueado.IdUsuario, activo ? AccionBitacora.HORARIO_ACTIVADO : AccionBitacora.HORARIO_DESACTIVADO, "AGENDA",
                    $"Se {(activo ? "activa" : "desactiva")} la franja {horario.NombreDia} {horario.HorarioTexto} del medico ID {idMedico}. Realizado por {usuarioLogueado.NombreApellido}.");

                if (activo)
                {
                    return 0;
                }

                return CancelarTurnosDeFranja(usuarioLogueado, horario, $"se desactivo la franja {horario.NombreDia} {horario.HorarioTexto}");
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al cambiar el estado del horario: {ex.Message}", ex);
            }
        }

        // Cancela los turnos reservados que caian en la franja eliminada.
        // Devuelve cuantos turnos se cancelaron.
        public int EliminarHorario(BE_Usuario usuarioLogueado, int idMedico, int idHorario)
        {
            try
            {
                BE_MedicoHorario horario = dal_agenda.ObtenerHorarios(idMedico).FirstOrDefault(h => h.IdHorario == idHorario);

                if (horario == null)
                {
                    throw new InvalidOperationException(TextoLocalizado.T("La franja seleccionada ya no existe."));
                }

                dal_agenda.EliminarHorario(idHorario);
                dal_medico.ActualizarRangoHorario(idMedico);

                bll_bitacora.RegistrarEvento(usuarioLogueado.IdUsuario, AccionBitacora.HORARIO_BAJA, "AGENDA",
                    $"Se elimina la franja {horario.NombreDia} {horario.HorarioTexto} del medico ID {idMedico}. Realizado por {usuarioLogueado.NombreApellido}.");

                return CancelarTurnosDeFranja(usuarioLogueado, horario, $"se elimino la franja {horario.NombreDia} {horario.HorarioTexto}");
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al eliminar el horario: {ex.Message}", ex);
            }
        }

        // Las franjas de un medico no se superponen (lo valida AgregarHorarios),
        // asi que todo turno que empieza dentro de esta franja salio de ella.
        private int CancelarTurnosDeFranja(BE_Usuario usuarioLogueado, BE_MedicoHorario horario, string motivo)
        {
            List<BE_Turno> afectados = dal_turno.ObtenerTurnosReservadosMedico(horario.IdMedico, DateTime.Now)
                .Where(t => BE_MedicoHorario.DiaSemanaDe(t.FechaHora) == horario.DiaSemana
                         && t.FechaHora.TimeOfDay >= horario.HoraDesde
                         && t.FechaHora.TimeOfDay < horario.HoraHasta)
                .ToList();

            return CancelarTurnos(usuarioLogueado, afectados, motivo);
        }

        // Pasa los turnos a Cancelado y deja un evento por turno en la bitacora
        // (el paciente lo ve cancelado en "Mis turnos").
        private int CancelarTurnos(BE_Usuario usuarioLogueado, List<BE_Turno> turnos, string motivo)
        {
            int cancelados = 0;

            foreach (BE_Turno turno in turnos)
            {
                turno.Estado = EstadoTurno.Cancelado;
                turno.DVH = HashHelper.GenerarHash(BLL_Turno.CadenaTurno(turno));

                // false: el paciente lo cancelo o el medico lo atendio en el medio.
                if (!dal_turno.CambiarEstado(turno.IdTurno, EstadoTurno.Reservado, EstadoTurno.Cancelado, turno.DVH))
                {
                    continue;
                }

                cancelados++;

                bll_bitacora.RegistrarEvento(usuarioLogueado.IdUsuario, AccionBitacora.TURNO_CANCELADO, "TURNOS",
                    $"Se cancela el turno ID {turno.IdTurno} de {turno.NombrePaciente} con {turno.NombreMedico} del {turno.FechaTexto} a las {turno.HoraTexto} porque {motivo}. Realizado por {usuarioLogueado.NombreApellido}.");
            }

            return cancelados;
        }

        private string CadenaHorario(BE_MedicoHorario h)
        {
            string activo = h.Activo ? "1" : "0";
            return $"{h.IdHorario}|{h.IdMedico}|{h.DiaSemana}|{h.HoraDesde:hh\\:mm}|{h.HoraHasta:hh\\:mm}|{h.DuracionTurno}|{activo}";
        }

        // ---- Bloqueos puntuales ----

        // Solo los bloqueos de hoy en adelante: los pasados ya no afectan la agenda.
        public List<BE_MedicoBloqueo> ObtenerBloqueosVigentes(int idMedico)
        {
            try
            {
                return dal_agenda.ObtenerBloqueos(idMedico, DateTime.Today);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener los bloqueos del medico: {ex.Message}", ex);
            }
        }

        // desde/hasta null = se bloquea el dia completo. Cancela los turnos
        // reservados que se superponen con el bloqueo y devuelve cuantos fueron.
        public int AgregarBloqueo(BE_Usuario usuarioLogueado, int idMedico, DateTime fecha, TimeSpan? desde, TimeSpan? hasta, string motivo)
        {
            try
            {
                if (!dal_medico.ExisteMedico(idMedico))
                {
                    throw new InvalidOperationException(TextoLocalizado.T("Primero guarda la matricula y especialidades del medico."));
                }

                fecha = fecha.Date;

                if (fecha < DateTime.Today)
                {
                    throw new InvalidOperationException(TextoLocalizado.T("No se puede bloquear una fecha pasada."));
                }

                if (desde.HasValue != hasta.HasValue)
                {
                    throw new InvalidOperationException(TextoLocalizado.T("Indica hora de inicio y de fin, o marca el dia completo."));
                }

                if (desde.HasValue && hasta.Value <= desde.Value)
                {
                    throw new InvalidOperationException(TextoLocalizado.T("La hora de fin del bloqueo tiene que ser posterior a la de inicio."));
                }

                motivo = string.IsNullOrWhiteSpace(motivo) ? null : motivo.Trim();

                if (motivo != null && motivo.Length > 100)
                {
                    throw new InvalidOperationException(TextoLocalizado.T("El motivo no puede superar los 100 caracteres (tiene {0}).", motivo.Length));
                }

                BE_MedicoBloqueo nuevo = new BE_MedicoBloqueo
                {
                    IdMedico = idMedico,
                    Fecha = fecha,
                    HoraDesde = desde,
                    HoraHasta = hasta,
                    Motivo = motivo
                };

                BE_MedicoBloqueo choque = dal_agenda.ObtenerBloqueos(idMedico, fecha)
                    .FirstOrDefault(b => b.Fecha.Date == fecha && nuevo.InicioEfectivo < b.FinEfectivo && b.InicioEfectivo < nuevo.FinEfectivo);

                if (choque != null)
                {
                    throw new InvalidOperationException(TextoLocalizado.T("Ya hay un bloqueo ese dia que se superpone: {0} ({1}).", choque.FechaTexto, choque.HorarioTexto));
                }

                dal_agenda.AgregarBloqueo(nuevo);

                nuevo.DVH = HashHelper.GenerarHash(CadenaBloqueo(nuevo));
                dal_agenda.ActualizarDVHBloqueo(nuevo.IdBloqueo, nuevo.DVH);

                bll_bitacora.RegistrarEvento(usuarioLogueado.IdUsuario, AccionBitacora.BLOQUEO_AGENDA_ALTA, "AGENDA",
                    $"Se bloquea la agenda del medico ID {idMedico}: {nuevo.FechaTexto} ({nuevo.HorarioTexto}){(motivo == null ? "" : " - " + motivo)}. Realizado por {usuarioLogueado.NombreApellido}.");

                // Misma regla que BLL_Turno al ofrecer turnos: un turno queda
                // bloqueado si su rango (inicio + duracion de su franja) se superpone.
                List<BE_MedicoHorario> franjas = dal_agenda.ObtenerHorarios(idMedico);

                List<BE_Turno> afectados = dal_turno.ObtenerTurnosReservadosMedico(idMedico, DateTime.Now)
                    .Where(t => t.FechaHora.Date == fecha)
                    .Where(t =>
                    {
                        TimeSpan inicio = t.FechaHora.TimeOfDay;
                        BE_MedicoHorario franja = franjas.FirstOrDefault(f => f.DiaSemana == BE_MedicoHorario.DiaSemanaDe(fecha) && inicio >= f.HoraDesde && inicio < f.HoraHasta);
                        TimeSpan fin = inicio + TimeSpan.FromMinutes(franja?.DuracionTurno ?? 1);

                        return inicio < nuevo.FinEfectivo && nuevo.InicioEfectivo < fin;
                    })
                    .ToList();

                return CancelarTurnos(usuarioLogueado, afectados, $"se bloqueo la agenda ({nuevo.FechaTexto} {nuevo.HorarioTexto})");
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al bloquear la agenda: {ex.Message}", ex);
            }
        }

        public void EliminarBloqueo(BE_Usuario usuarioLogueado, int idMedico, int idBloqueo)
        {
            try
            {
                BE_MedicoBloqueo bloqueo = dal_agenda.ObtenerBloqueos(idMedico, DateTime.MinValue).FirstOrDefault(b => b.IdBloqueo == idBloqueo);

                if (bloqueo == null)
                {
                    throw new InvalidOperationException(TextoLocalizado.T("El bloqueo seleccionado ya no existe."));
                }

                dal_agenda.EliminarBloqueo(idBloqueo);

                bll_bitacora.RegistrarEvento(usuarioLogueado.IdUsuario, AccionBitacora.BLOQUEO_AGENDA_BAJA, "AGENDA",
                    $"Se quita el bloqueo del medico ID {idMedico}: {bloqueo.FechaTexto} ({bloqueo.HorarioTexto}). Realizado por {usuarioLogueado.NombreApellido}.");
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al quitar el bloqueo: {ex.Message}", ex);
            }
        }

        private string CadenaBloqueo(BE_MedicoBloqueo b)
        {
            return $"{b.IdBloqueo}|{b.IdMedico}|{b.Fecha:yyyy-MM-dd}|{b.HorarioTexto}|{b.Motivo}";
        }
    }
}
