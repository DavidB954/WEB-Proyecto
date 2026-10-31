using BE;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    public class DAL_Turno
    {
        // Errores de SQL Server por clave/indice unico duplicado.
        private const int ErrorIndiceUnico = 2601;
        private const int ErrorClaveUnica = 2627;

        DAL_Conexion conex = new DAL_Conexion();

        // Misma consulta para las dos vistas: el paciente ve sus turnos y el
        // medico los de su agenda; cambia solo el filtro. Left Join al medico:
        // si se elimino su usuario, el turno sigue en el historial del paciente.
        private const string SelectTurnos = @"Select t.IdTurno, t.IdPaciente, t.IdMedico, t.FechaHora, t.IdEstadoTurno, t.DVH,
                                                   IsNull(um.Apellido + ', ' + um.Nombre, '(Medico eliminado)') As NombreMedico,
                                                   (Select STRING_AGG(e.NombreEspecialidad, ', ') WITHIN GROUP (ORDER BY e.NombreEspecialidad)
                                                    From MedicoEspecialidad me
                                                    Join Especialidad e On e.IdEspecialidad = me.IdEspecialidad
                                                    Where me.IdMedico = t.IdMedico) As Especialidades,
                                                   up.Apellido + ', ' + up.Nombre As NombrePaciente,
                                                   p.NumeroHistoriaClinica, p.FechaNacimiento
                                            From Turno t
                                            Left Join Usuario um On um.IdUsuario = t.IdMedico
                                            Join Paciente p On p.IdPaciente = t.IdPaciente
                                            Join Usuario up On up.IdUsuario = t.IdPaciente ";

        public List<BE_Turno> ObtenerTurnosPaciente(int idPaciente)
        {
            SqlCommand cmd = new SqlCommand(SelectTurnos + "Where t.IdPaciente=@idPaciente Order By t.FechaHora");
            cmd.Parameters.Add("@idPaciente", SqlDbType.Int).Value = idPaciente;

            return LeerTurnos(cmd);
        }

        // Turnos de un dia de la agenda del medico (incluye los cancelados, que
        // la pantalla muestra igual para que el medico sepa que ese hueco se libero).
        public List<BE_Turno> ObtenerTurnosMedico(int idMedico, DateTime fecha)
        {
            SqlCommand cmd = new SqlCommand(SelectTurnos + @"Where t.IdMedico=@idMedico And t.FechaHora >= @desde And t.FechaHora < @hasta
                                                           Order By t.FechaHora, t.IdEstadoTurno");
            cmd.Parameters.Add("@idMedico", SqlDbType.Int).Value = idMedico;
            cmd.Parameters.Add("@desde", SqlDbType.DateTime).Value = fecha.Date;
            cmd.Parameters.Add("@hasta", SqlDbType.DateTime).Value = fecha.Date.AddDays(1);

            return LeerTurnos(cmd);
        }

        // Turnos reservados del medico posteriores a "desde": los que puede
        // afectar un cambio de agenda (bloqueo o baja de franja).
        public List<BE_Turno> ObtenerTurnosReservadosMedico(int idMedico, DateTime desde)
        {
            SqlCommand cmd = new SqlCommand(SelectTurnos + @"Where t.IdMedico=@idMedico And t.IdEstadoTurno=@reservado And t.FechaHora > @desde
                                                           Order By t.FechaHora");
            cmd.Parameters.Add("@idMedico", SqlDbType.Int).Value = idMedico;
            cmd.Parameters.Add("@reservado", SqlDbType.Int).Value = (int)EstadoTurno.Reservado;
            cmd.Parameters.Add("@desde", SqlDbType.DateTime).Value = desde;

            return LeerTurnos(cmd);
        }

        // null si no existe.
        public BE_Turno ObtenerTurno(int idTurno)
        {
            SqlCommand cmd = new SqlCommand(SelectTurnos + "Where t.IdTurno=@idTurno");
            cmd.Parameters.Add("@idTurno", SqlDbType.Int).Value = idTurno;

            List<BE_Turno> turnos = LeerTurnos(cmd);
            return turnos.Count == 0 ? null : turnos[0];
        }

        private List<BE_Turno> LeerTurnos(SqlCommand cmd)
        {
            List<BE_Turno> lista = new List<BE_Turno>();

            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();
                cmd.Connection = cn;

                SqlDataReader lector = cmd.ExecuteReader();

                while (lector.Read())
                {
                    lista.Add(new BE_Turno
                    {
                        IdTurno = lector.GetInt32(0),
                        IdPaciente = lector.GetInt32(1),
                        IdMedico = lector.GetInt32(2),
                        FechaHora = lector.GetDateTime(3),
                        Estado = (EstadoTurno)lector.GetInt32(4),
                        DVH = lector.IsDBNull(5) ? null : lector.GetString(5),
                        NombreMedico = lector.GetString(6),
                        Especialidades = lector.IsDBNull(7) ? null : lector.GetString(7),
                        NombrePaciente = lector.GetString(8),
                        HistoriaClinica = lector.GetString(9),
                        FechaNacimientoPaciente = lector.GetDateTime(10)
                    });
                }
            }

            return lista;
        }

        // Horarios del medico ya tomados (no cancelados) entre dos fechas.
        public HashSet<DateTime> ObtenerHorariosOcupadosMedico(int idMedico, DateTime desde, DateTime hasta)
        {
            HashSet<DateTime> ocupados = new HashSet<DateTime>();

            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand(@"Select FechaHora From Turno
                                                Where IdMedico=@idMedico And IdEstadoTurno<>@cancelado
                                                  And FechaHora >= @desde And FechaHora < @hasta", cn);

                cmd.Parameters.Add("@idMedico", SqlDbType.Int).Value = idMedico;
                cmd.Parameters.Add("@cancelado", SqlDbType.Int).Value = (int)EstadoTurno.Cancelado;
                cmd.Parameters.Add("@desde", SqlDbType.DateTime).Value = desde;
                cmd.Parameters.Add("@hasta", SqlDbType.DateTime).Value = hasta;

                SqlDataReader lector = cmd.ExecuteReader();

                while (lector.Read())
                {
                    ocupados.Add(lector.GetDateTime(0));
                }
            }

            return ocupados;
        }

        // Devuelve false si el horario lo tomo otro paciente entre que se mostro
        // y se confirmo (lo frenan los indices unicos filtrados de Turno).
        public bool AgregarTurno(BE_Turno turno)
        {
            try
            {
                using (SqlConnection cn = conex.ObtenerConexion())
                {
                    cn.Open();

                    SqlCommand cmd = new SqlCommand(@"Insert into Turno (IdPaciente, IdMedico, FechaHora, IdEstadoTurno)
                                                    VALUES (@idPaciente, @idMedico, @fechaHora, @estado);
                                                    SELECT CAST(SCOPE_IDENTITY() AS INT);", cn);

                    cmd.Parameters.Add("@idPaciente", SqlDbType.Int).Value = turno.IdPaciente;
                    cmd.Parameters.Add("@idMedico", SqlDbType.Int).Value = turno.IdMedico;
                    cmd.Parameters.Add("@fechaHora", SqlDbType.DateTime).Value = turno.FechaHora;
                    cmd.Parameters.Add("@estado", SqlDbType.Int).Value = (int)turno.Estado;

                    turno.IdTurno = (int)cmd.ExecuteScalar();
                    return true;
                }
            }
            catch (SqlException ex) when (ex.Number == ErrorIndiceUnico || ex.Number == ErrorClaveUnica)
            {
                return false;
            }
        }

        public void ActualizarDVH(int idTurno, string dvh)
        {
            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("Update Turno Set DVH=@dvh Where IdTurno=@id", cn);

                cmd.Parameters.Add("@dvh", SqlDbType.VarChar, 255).Value = dvh;
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idTurno;

                cmd.ExecuteNonQuery();
            }
        }

        // Solo cambia el estado si sigue en el esperado: si dos postbacks cancelan
        // el mismo turno a la vez, el segundo no pisa nada y devuelve false.
        public bool CambiarEstado(int idTurno, EstadoTurno estadoEsperado, EstadoTurno estadoNuevo, string dvh)
        {
            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand(@"Update Turno Set IdEstadoTurno=@nuevo, DVH=@dvh
                                                Where IdTurno=@id And IdEstadoTurno=@esperado", cn);

                cmd.Parameters.Add("@nuevo", SqlDbType.Int).Value = (int)estadoNuevo;
                cmd.Parameters.Add("@dvh", SqlDbType.VarChar, 255).Value = (object)dvh ?? DBNull.Value;
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idTurno;
                cmd.Parameters.Add("@esperado", SqlDbType.Int).Value = (int)estadoEsperado;

                return cmd.ExecuteNonQuery() > 0;
            }
        }
    }
}
