using BE;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    public class DAL_AgendaMedico
    {
        DAL_Conexion conex = new DAL_Conexion();

        public List<BE_MedicoHorario> ObtenerHorarios(int idMedico)
        {
            List<BE_MedicoHorario> lista = new List<BE_MedicoHorario>();

            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand(@"Select IdHorario, IdMedico, DiaSemana, HoraDesde, HoraHasta, DuracionTurno, Activo, DVH
                                                From MedicoHorario
                                                Where IdMedico=@idMedico
                                                Order By DiaSemana, HoraDesde", cn);

                cmd.Parameters.Add("@idMedico", SqlDbType.Int).Value = idMedico;

                using (SqlDataReader lector = cmd.ExecuteReader())
                {
                    while (lector.Read())
                    {
                        lista.Add(new BE_MedicoHorario
                        {
                            IdHorario = lector.GetInt32(0),
                            IdMedico = lector.GetInt32(1),
                            DiaSemana = lector.GetByte(2),
                            HoraDesde = lector.GetTimeSpan(3),
                            HoraHasta = lector.GetTimeSpan(4),
                            DuracionTurno = lector.GetInt32(5),
                            Activo = lector.GetBoolean(6),
                            DVH = lector.IsDBNull(7) ? null : lector.GetString(7)
                        });
                    }
                }
            }

            return lista;
        }

        public void AgregarHorario(BE_MedicoHorario horario)
        {
            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand(@"Insert into MedicoHorario (IdMedico, DiaSemana, HoraDesde, HoraHasta, DuracionTurno, Activo)
                                                VALUES (@idMedico, @dia, @desde, @hasta, @duracion, @activo);
                                                SELECT CAST(SCOPE_IDENTITY() AS INT);", cn);

                cmd.Parameters.Add("@idMedico", SqlDbType.Int).Value = horario.IdMedico;
                cmd.Parameters.Add("@dia", SqlDbType.TinyInt).Value = horario.DiaSemana;
                cmd.Parameters.Add("@desde", SqlDbType.Time).Value = horario.HoraDesde;
                cmd.Parameters.Add("@hasta", SqlDbType.Time).Value = horario.HoraHasta;
                cmd.Parameters.Add("@duracion", SqlDbType.Int).Value = horario.DuracionTurno;
                cmd.Parameters.Add("@activo", SqlDbType.Bit).Value = horario.Activo;

                horario.IdHorario = (int)cmd.ExecuteScalar();
            }
        }

        public void ActualizarEstadoHorario(int idHorario, bool activo, string dvh)
        {
            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("Update MedicoHorario Set Activo=@activo, DVH=@dvh Where IdHorario=@id", cn);

                cmd.Parameters.Add("@activo", SqlDbType.Bit).Value = activo;
                cmd.Parameters.Add("@dvh", SqlDbType.VarChar, 255).Value = (object)dvh ?? DBNull.Value;
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idHorario;

                cmd.ExecuteNonQuery();
            }
        }

        public void ActualizarDVHHorario(int idHorario, string dvh)
        {
            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("Update MedicoHorario Set DVH=@dvh Where IdHorario=@id", cn);

                cmd.Parameters.Add("@dvh", SqlDbType.VarChar, 255).Value = dvh;
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idHorario;

                cmd.ExecuteNonQuery();
            }
        }

        public void EliminarHorario(int idHorario)
        {
            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("Delete From MedicoHorario Where IdHorario=@id", cn);
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idHorario;

                cmd.ExecuteNonQuery();
            }
        }

        public List<BE_MedicoBloqueo> ObtenerBloqueos(int idMedico, DateTime desdeFecha)
        {
            List<BE_MedicoBloqueo> lista = new List<BE_MedicoBloqueo>();

            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand(@"Select IdBloqueo, IdMedico, Fecha, HoraDesde, HoraHasta, Motivo, DVH
                                                From MedicoBloqueo
                                                Where IdMedico=@idMedico And Fecha >= @desde
                                                Order By Fecha, HoraDesde", cn);

                cmd.Parameters.Add("@idMedico", SqlDbType.Int).Value = idMedico;
                cmd.Parameters.Add("@desde", SqlDbType.Date).Value = desdeFecha.Date;

                using (SqlDataReader lector = cmd.ExecuteReader())
                {
                    while (lector.Read())
                    {
                        lista.Add(new BE_MedicoBloqueo
                        {
                            IdBloqueo = lector.GetInt32(0),
                            IdMedico = lector.GetInt32(1),
                            Fecha = lector.GetDateTime(2),
                            HoraDesde = lector.IsDBNull(3) ? (TimeSpan?)null : lector.GetTimeSpan(3),
                            HoraHasta = lector.IsDBNull(4) ? (TimeSpan?)null : lector.GetTimeSpan(4),
                            Motivo = lector.IsDBNull(5) ? null : lector.GetString(5),
                            DVH = lector.IsDBNull(6) ? null : lector.GetString(6)
                        });
                    }
                }
            }

            return lista;
        }

        public void AgregarBloqueo(BE_MedicoBloqueo bloqueo)
        {
            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand(@"Insert into MedicoBloqueo (IdMedico, Fecha, HoraDesde, HoraHasta, Motivo)
                                                VALUES (@idMedico, @fecha, @desde, @hasta, @motivo);
                                                SELECT CAST(SCOPE_IDENTITY() AS INT);", cn);

                cmd.Parameters.Add("@idMedico", SqlDbType.Int).Value = bloqueo.IdMedico;
                cmd.Parameters.Add("@fecha", SqlDbType.Date).Value = bloqueo.Fecha.Date;
                cmd.Parameters.Add("@desde", SqlDbType.Time).Value = (object)bloqueo.HoraDesde ?? DBNull.Value;
                cmd.Parameters.Add("@hasta", SqlDbType.Time).Value = (object)bloqueo.HoraHasta ?? DBNull.Value;
                cmd.Parameters.Add("@motivo", SqlDbType.VarChar, 100).Value = (object)bloqueo.Motivo ?? DBNull.Value;

                bloqueo.IdBloqueo = (int)cmd.ExecuteScalar();
            }
        }

        public void ActualizarDVHBloqueo(int idBloqueo, string dvh)
        {
            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("Update MedicoBloqueo Set DVH=@dvh Where IdBloqueo=@id", cn);

                cmd.Parameters.Add("@dvh", SqlDbType.VarChar, 255).Value = dvh;
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idBloqueo;

                cmd.ExecuteNonQuery();
            }
        }

        public void EliminarBloqueo(int idBloqueo)
        {
            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("Delete From MedicoBloqueo Where IdBloqueo=@id", cn);
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idBloqueo;

                cmd.ExecuteNonQuery();
            }
        }
    }
}
