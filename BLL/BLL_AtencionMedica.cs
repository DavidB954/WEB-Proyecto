using BE;
using DAL;
using SERVICIOS;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BLL
{
    public class BLL_AtencionMedica
    {
        // Medicamentos que se pueden recetar en una misma atencion.
        public const int MaximoMedicamentos = 10;

        DAL_AtencionMedica dal_atencion = new DAL_AtencionMedica();
        DAL_Turno dal_turno = new DAL_Turno();
        DAL_Medico dal_medico = new DAL_Medico();
        BLL_Bitacora bll_bitacora = new BLL_Bitacora();

        // Solo atiende quien tiene fila en Medico (matricula cargada desde
        // AgendaMedicos.aspx): Turno.IdMedico y AtencionMedica.IdMedico la referencian.
        public bool EsMedicoRegistrado(int idUsuario)
        {
            try
            {
                return dal_medico.ExisteMedico(idUsuario);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al verificar los datos del medico: {ex.Message}", ex);
            }
        }

        public List<BE_Turno> ObtenerTurnosDelDia(BE_Usuario usuarioLogueado, DateTime fecha)
        {
            try
            {
                List<BE_Turno> turnos = dal_turno.ObtenerTurnosMedico(usuarioLogueado.IdUsuario, fecha);

                foreach (BE_Turno turno in turnos)
                {
                    turno.PuedeAtender = SePuedeAtender(turno);
                    turno.PuedeMarcarAusente = SePuedeMarcarAusente(turno);
                }

                return turnos;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener los turnos del dia: {ex.Message}", ex);
            }
        }

        // El turno que el medico eligio para atender, ya validado.
        public BE_Turno ObtenerTurnoParaAtender(BE_Usuario usuarioLogueado, int idTurno)
        {
            try
            {
                return ValidarTurnoDelMedico(usuarioLogueado, idTurno, true);
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener el turno: {ex.Message}", ex);
            }
        }

        // El medico solo ve la historia clinica de pacientes que tienen o
        // tuvieron turno con el.
        public List<BE_AtencionMedica> ObtenerHistoriaClinica(BE_Usuario usuarioLogueado, int idPaciente)
        {
            try
            {
                if (!dal_turno.ObtenerTurnosPaciente(idPaciente).Any(t => t.IdMedico == usuarioLogueado.IdUsuario))
                {
                    throw new InvalidOperationException(TextoLocalizado.T("Solo podes ver la historia clinica de tus pacientes."));
                }

                return dal_atencion.ObtenerAtencionesPaciente(idPaciente);
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener la historia clinica: {ex.Message}", ex);
            }
        }

        // Valida un medicamento antes de sumarlo a la receta en pantalla; se
        // vuelve a validar todo al guardar la atencion.
        public BE_Receta ArmarMedicamento(string medicamento, string indicaciones)
        {
            medicamento = (medicamento ?? string.Empty).Trim();
            indicaciones = string.IsNullOrWhiteSpace(indicaciones) ? null : indicaciones.Trim();

            if (medicamento.Length == 0)
            {
                throw new InvalidOperationException(TextoLocalizado.T("Ingresa el nombre del medicamento."));
            }

            // Receta.Medicamento es VARCHAR(30) y Receta.Observaciones VARCHAR(255).
            if (medicamento.Length > 30)
            {
                throw new InvalidOperationException(TextoLocalizado.T("El medicamento no puede superar los 30 caracteres (tiene {0}).", medicamento.Length));
            }

            if (indicaciones != null && indicaciones.Length > 255)
            {
                throw new InvalidOperationException(TextoLocalizado.T("Las indicaciones no pueden superar los 255 caracteres (tienen {0}).", indicaciones.Length));
            }

            return new BE_Receta { Medicamento = medicamento, Observaciones = indicaciones };
        }

        public void RegistrarAtencion(BE_Usuario usuarioLogueado, int idTurno, string motivo, string diagnostico, string observaciones, List<BE_Receta> recetas)
        {
            try
            {
                BE_Turno turno = ValidarTurnoDelMedico(usuarioLogueado, idTurno, true);

                motivo = Normalizar(motivo);
                diagnostico = Normalizar(diagnostico);
                observaciones = Normalizar(observaciones);

                if (motivo == null || diagnostico == null)
                {
                    throw new InvalidOperationException(TextoLocalizado.T("Completa al menos el motivo de la consulta y el diagnostico."));
                }

                // Largos de AtencionMedica: Motivo 255, Diagnostico 500, Observaciones 1000.
                ValidarLargo("El motivo de la consulta", motivo, 255);
                ValidarLargo("El diagnostico", diagnostico, 500);
                ValidarLargo("Las observaciones", observaciones, 1000);

                recetas = (recetas ?? new List<BE_Receta>()).Select(r => ArmarMedicamento(r.Medicamento, r.Observaciones)).ToList();

                if (recetas.Count > MaximoMedicamentos)
                {
                    throw new InvalidOperationException(TextoLocalizado.T("Una receta puede tener hasta {0} medicamentos.", MaximoMedicamentos));
                }

                // Sin milisegundos: el datetime de SQL los redondea y la firma
                // (DVH) dejaria de coincidir con lo guardado.
                DateTime ahora = DateTime.Now;

                BE_AtencionMedica atencion = new BE_AtencionMedica
                {
                    IdPaciente = turno.IdPaciente,
                    IdMedico = turno.IdMedico,
                    IdTurno = turno.IdTurno,
                    FechaHora = new DateTime(ahora.Ticks - ahora.Ticks % TimeSpan.TicksPerSecond),
                    MotivoConsulta = motivo,
                    Diagnostico = diagnostico,
                    Observaciones = observaciones,
                    Recetas = recetas
                };

                if (!dal_atencion.RegistrarAtencion(atencion))
                {
                    throw new InvalidOperationException(TextoLocalizado.T("El turno ya fue atendido o cambio de estado. Revisa la lista de turnos."));
                }

                atencion.DVH = HashHelper.GenerarHash(CadenaAtencion(atencion));
                dal_atencion.ActualizarDVHAtencion(atencion.IdAtencion, atencion.DVH);

                foreach (BE_Receta receta in atencion.Recetas)
                {
                    receta.DVH = HashHelper.GenerarHash(CadenaReceta(receta));
                    dal_atencion.ActualizarDVHReceta(receta.IdReceta, receta.DVH);
                }

                turno.Estado = EstadoTurno.Atendido;
                dal_turno.ActualizarDVH(turno.IdTurno, HashHelper.GenerarHash(BLL_Turno.CadenaTurno(turno)));

                bll_bitacora.RegistrarEvento(usuarioLogueado.IdUsuario, AccionBitacora.ATENCION_REGISTRADA, "ATENCION",
                    $"{usuarioLogueado.NombreApellido} registra la atencion ID {atencion.IdAtencion} del turno ID {turno.IdTurno} ({turno.NombrePaciente}, HC {turno.HistoriaClinica})" +
                    (recetas.Count == 0 ? " sin receta." : $" con receta de {recetas.Count} medicamento(s)."));
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al registrar la atencion: {ex.Message}", ex);
            }
        }

        public void MarcarAusente(BE_Usuario usuarioLogueado, int idTurno)
        {
            try
            {
                BE_Turno turno = ValidarTurnoDelMedico(usuarioLogueado, idTurno, false);

                if (!SePuedeMarcarAusente(turno))
                {
                    throw new InvalidOperationException(TextoLocalizado.T("Solo se puede marcar ausente un turno cuyo horario ya paso."));
                }

                turno.Estado = EstadoTurno.Ausente;
                string dvh = HashHelper.GenerarHash(BLL_Turno.CadenaTurno(turno));

                if (!dal_turno.CambiarEstado(idTurno, EstadoTurno.Reservado, EstadoTurno.Ausente, dvh))
                {
                    throw new InvalidOperationException(TextoLocalizado.T("El turno cambio de estado. Revisa la lista de turnos."));
                }

                bll_bitacora.RegistrarEvento(usuarioLogueado.IdUsuario, AccionBitacora.TURNO_AUSENTE, "ATENCION",
                    $"{usuarioLogueado.NombreApellido} marca ausente a {turno.NombrePaciente} (HC {turno.HistoriaClinica}) en el turno ID {idTurno} del {turno.FechaTexto} a las {turno.HoraTexto}.");
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al marcar el turno como ausente: {ex.Message}", ex);
            }
        }

        // Se busca el turno por Id y se exige que sea de este medico: asi no se
        // puede atender un turno ajeno forzando el IdTurno en el postback.
        private BE_Turno ValidarTurnoDelMedico(BE_Usuario usuarioLogueado, int idTurno, bool paraAtender)
        {
            if (!dal_medico.ExisteMedico(usuarioLogueado.IdUsuario))
            {
                throw new InvalidOperationException(TextoLocalizado.T("Tu usuario no esta registrado como medico (falta la matricula)."));
            }

            BE_Turno turno = dal_turno.ObtenerTurno(idTurno);

            if (turno == null || turno.IdMedico != usuarioLogueado.IdUsuario)
            {
                throw new InvalidOperationException(TextoLocalizado.T("El turno seleccionado ya no existe."));
            }

            if (turno.Estado != EstadoTurno.Reservado)
            {
                throw new InvalidOperationException(TextoLocalizado.T("El turno de {0} esta {1}.", turno.NombrePaciente, turno.EstadoTexto.ToLower()));
            }

            if (paraAtender && !SePuedeAtender(turno))
            {
                throw new InvalidOperationException(TextoLocalizado.T("Todavia no se puede atender este turno: es de otro dia."));
            }

            return turno;
        }

        // Se puede atender durante el dia del turno (aunque el paciente llegue
        // antes de hora) o despues, para cargar una atencion atrasada.
        private bool SePuedeAtender(BE_Turno turno)
        {
            return turno.Estado == EstadoTurno.Reservado && turno.FechaHora.Date <= DateTime.Today;
        }

        private bool SePuedeMarcarAusente(BE_Turno turno)
        {
            return turno.Estado == EstadoTurno.Reservado && turno.FechaHora <= DateTime.Now;
        }

        private static string Normalizar(string texto)
        {
            return string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
        }

        private static void ValidarLargo(string campo, string texto, int maximo)
        {
            if (texto != null && texto.Length > maximo)
            {
                throw new InvalidOperationException(TextoLocalizado.T("{0} no puede superar los {1} caracteres (tiene {2}).", TextoLocalizado.T(campo), maximo, texto.Length));
            }
        }

        private string CadenaAtencion(BE_AtencionMedica a)
        {
            return $"{a.IdAtencion}|{a.IdPaciente}|{a.IdMedico}|{a.IdTurno}|{a.FechaHora:yyyy-MM-dd HH:mm:ss}|{a.MotivoConsulta}|{a.Diagnostico}|{a.Observaciones}";
        }

        private string CadenaReceta(BE_Receta r)
        {
            return $"{r.IdReceta}|{r.IdAtencion}|{r.Medicamento}|{r.Observaciones}";
        }
    }
}
