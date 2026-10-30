using BE;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace DAL
{
    public class DAL_AtencionMedica
    {
        // Errores de SQL Server por clave/indice unico duplicado.
        private const int ErrorIndiceUnico = 2601;
        private const int ErrorClaveUnica = 2627;

        DAL_Conexion conex = new DAL_Conexion();

        // Inserta la atencion con sus recetas y pasa el turno a Atendido, todo en
        // una transaccion. Devuelve false (sin guardar nada) si el turno ya no
        // estaba Reservado o si ya tenia una atencion cargada.
        public bool RegistrarAtencion(BE_AtencionMedica atencion)
        {
            try
            {
                using (SqlConnection cn = conex.ObtenerConexion())
                {
                    cn.Open();

                    using (SqlTransaction tx = cn.BeginTransaction())
                    {
                        SqlCommand cmdTurno = new SqlCommand(@"Update Turno Set IdEstadoTurno=@atendido
                                                             Where IdTurno=@idTurno And IdEstadoTurno=@reservado", cn, tx);

                        cmdTurno.Parameters.Add("@atendido", SqlDbType.Int).Value = (int)EstadoTurno.Atendido;
                        cmdTurno.Parameters.Add("@idTurno", SqlDbType.Int).Value = atencion.IdTurno;
                        cmdTurno.Parameters.Add("@reservado", SqlDbType.Int).Value = (int)EstadoTurno.Reservado;

                        if (cmdTurno.ExecuteNonQuery() == 0)
                        {
                            tx.Rollback();
                            return false;
                        }

                        SqlCommand cmdAtencion = new SqlCommand(@"Insert into AtencionMedica (IdPaciente, IdMedico, IdTurno, FechaHora, MotivoConsulta, Diagnostico, Observaciones)
                                                                VALUES (@idPaciente, @idMedico, @idTurno, @fechaHora, @motivo, @diagnostico, @observaciones);
                                                                SELECT CAST(SCOPE_IDENTITY() AS INT);", cn, tx);

                        cmdAtencion.Parameters.Add("@idPaciente", SqlDbType.Int).Value = atencion.IdPaciente;
                        cmdAtencion.Parameters.Add("@idMedico", SqlDbType.Int).Value = atencion.IdMedico;
                        cmdAtencion.Parameters.Add("@idTurno", SqlDbType.Int).Value = atencion.IdTurno;
                        cmdAtencion.Parameters.Add("@fechaHora", SqlDbType.DateTime).Value = atencion.FechaHora;
                        cmdAtencion.Parameters.Add("@motivo", SqlDbType.VarChar, 255).Value = (object)atencion.MotivoConsulta ?? DBNull.Value;
                        cmdAtencion.Parameters.Add("@diagnostico", SqlDbType.VarChar, 500).Value = (object)atencion.Diagnostico ?? DBNull.Value;
                        cmdAtencion.Parameters.Add("@observaciones", SqlDbType.VarChar, 1000).Value = (object)atencion.Observaciones ?? DBNull.Value;

                        atencion.IdAtencion = (int)cmdAtencion.ExecuteScalar();

                        foreach (BE_Receta receta in atencion.Recetas)
                        {
                            SqlCommand cmdReceta = new SqlCommand(@"Insert into Receta (IdAtencion, Medicamento, Observaciones)
                                                                  VALUES (@idAtencion, @medicamento, @observaciones);
                                                                  SELECT CAST(SCOPE_IDENTITY() AS INT);", cn, tx);

                            cmdReceta.Parameters.Add("@idAtencion", SqlDbType.Int).Value = atencion.IdAtencion;
                            cmdReceta.Parameters.Add("@medicamento", SqlDbType.VarChar, 30).Value = receta.Medicamento;
                            cmdReceta.Parameters.Add("@observaciones", SqlDbType.VarChar, 255).Value = (object)receta.Observaciones ?? DBNull.Value;

                            receta.IdAtencion = atencion.IdAtencion;
                            receta.IdReceta = (int)cmdReceta.ExecuteScalar();
                        }

                        tx.Commit();
                        return true;
                    }
                }
            }
            catch (SqlException ex) when (ex.Number == ErrorIndiceUnico || ex.Number == ErrorClaveUnica)
            {
                // La transaccion se deshace sola al cerrar la conexion sin Commit.
                return false;
            }
        }

        // Historia clinica: todas las atenciones del paciente, de la mas reciente
        // a la mas vieja, cada una con sus recetas.
        public List<BE_AtencionMedica> ObtenerAtencionesPaciente(int idPaciente)
        {
            List<BE_AtencionMedica> lista = new List<BE_AtencionMedica>();

            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand(@"Select a.IdAtencion, a.IdPaciente, a.IdMedico, a.IdTurno, a.FechaHora,
                                                       a.MotivoConsulta, a.Diagnostico, a.Observaciones, a.DVH,
                                                       IsNull(u.Apellido + ', ' + u.Nombre, '(Medico eliminado)') As NombreMedico
                                                From AtencionMedica a
                                                Left Join Usuario u On u.IdUsuario = a.IdMedico
                                                Where a.IdPaciente=@idPaciente
                                                Order By a.FechaHora Desc", cn);

                cmd.Parameters.Add("@idPaciente", SqlDbType.Int).Value = idPaciente;

                using (SqlDataReader lector = cmd.ExecuteReader())
                {
                    while (lector.Read())
                    {
                        lista.Add(new BE_AtencionMedica
                        {
                            IdAtencion = lector.GetInt32(0),
                            IdPaciente = lector.GetInt32(1),
                            IdMedico = lector.GetInt32(2),
                            IdTurno = lector.GetInt32(3),
                            FechaHora = lector.GetDateTime(4),
                            MotivoConsulta = lector.IsDBNull(5) ? null : lector.GetString(5),
                            Diagnostico = lector.IsDBNull(6) ? null : lector.GetString(6),
                            Observaciones = lector.IsDBNull(7) ? null : lector.GetString(7),
                            DVH = lector.IsDBNull(8) ? null : lector.GetString(8),
                            NombreMedico = lector.GetString(9)
                        });
                    }
                }

                SqlCommand cmdRecetas = new SqlCommand(@"Select r.IdReceta, r.IdAtencion, r.Medicamento, r.Observaciones, r.DVH
                                                       From Receta r
                                                       Join AtencionMedica a On a.IdAtencion = r.IdAtencion
                                                       Where a.IdPaciente=@idPaciente
                                                       Order By r.IdReceta", cn);

                cmdRecetas.Parameters.Add("@idPaciente", SqlDbType.Int).Value = idPaciente;

                Dictionary<int, BE_AtencionMedica> porId = lista.ToDictionary(a => a.IdAtencion);

                using (SqlDataReader lector = cmdRecetas.ExecuteReader())
                {
                    while (lector.Read())
                    {
                        BE_Receta receta = new BE_Receta
                        {
                            IdReceta = lector.GetInt32(0),
                            IdAtencion = lector.GetInt32(1),
                            Medicamento = lector.GetString(2),
                            Observaciones = lector.IsDBNull(3) ? null : lector.GetString(3),
                            DVH = lector.IsDBNull(4) ? null : lector.GetString(4)
                        };

                        if (porId.TryGetValue(receta.IdAtencion, out BE_AtencionMedica atencion))
                        {
                            atencion.Recetas.Add(receta);
                        }
                    }
                }
            }

            return lista;
        }

        // Recetas del paciente, de la mas reciente a la mas vieja, con el medico
        // y la fecha de la atencion que las emitio.
        public List<BE_Receta> ObtenerRecetasPaciente(int idPaciente)
        {
            List<BE_Receta> lista = new List<BE_Receta>();

            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand(@"Select r.IdReceta, r.IdAtencion, r.Medicamento, r.Observaciones, r.DVH,
                                                       a.FechaHora, IsNull(u.Apellido + ', ' + u.Nombre, '(Medico eliminado)') As NombreMedico,
                                                       (Select STRING_AGG(e.NombreEspecialidad, ', ') WITHIN GROUP (ORDER BY e.NombreEspecialidad)
                                                        From MedicoEspecialidad me
                                                        Join Especialidad e On e.IdEspecialidad = me.IdEspecialidad
                                                        Where me.IdMedico = a.IdMedico) As Especialidades
                                                From Receta r
                                                Join AtencionMedica a On a.IdAtencion = r.IdAtencion
                                                Left Join Usuario u On u.IdUsuario = a.IdMedico
                                                Where a.IdPaciente=@idPaciente
                                                Order By a.FechaHora Desc, r.IdReceta", cn);

                cmd.Parameters.Add("@idPaciente", SqlDbType.Int).Value = idPaciente;

                SqlDataReader lector = cmd.ExecuteReader();

                while (lector.Read())
                {
                    lista.Add(new BE_Receta
                    {
                        IdReceta = lector.GetInt32(0),
                        IdAtencion = lector.GetInt32(1),
                        Medicamento = lector.GetString(2),
                        Observaciones = lector.IsDBNull(3) ? null : lector.GetString(3),
                        DVH = lector.IsDBNull(4) ? null : lector.GetString(4),
                        FechaEmision = lector.GetDateTime(5),
                        NombreMedico = lector.GetString(6),
                        Especialidades = lector.IsDBNull(7) ? null : lector.GetString(7)
                    });
                }
            }

            return lista;
        }

        public void ActualizarDVHAtencion(int idAtencion, string dvh)
        {
            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("Update AtencionMedica Set DVH=@dvh Where IdAtencion=@id", cn);

                cmd.Parameters.Add("@dvh", SqlDbType.VarChar, 255).Value = dvh;
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idAtencion;

                cmd.ExecuteNonQuery();
            }
        }

        public void ActualizarDVHReceta(int idReceta, string dvh)
        {
            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("Update Receta Set DVH=@dvh Where IdReceta=@id", cn);

                cmd.Parameters.Add("@dvh", SqlDbType.VarChar, 255).Value = dvh;
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idReceta;

                cmd.ExecuteNonQuery();
            }
        }
    }
}
