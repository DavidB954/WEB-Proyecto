using BE;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    public class DAL_Medico
    {
        private const string RolMedico = RolesSistema.Medico;

        DAL_Conexion conex = new DAL_Conexion();

        public List<BE_Medico> BuscarMedicos(string nombre, int? idEspecialidad)
        {
            List<BE_Medico> lista = new List<BE_Medico>();

            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand(@"Select u.IdUsuario, u.Nombre, u.Apellido, u.Activo, m.Matricula, m.DVH,
                                                       (Select STRING_AGG(e.NombreEspecialidad, ', ') WITHIN GROUP (ORDER BY e.NombreEspecialidad)
                                                        From MedicoEspecialidad me
                                                        Join Especialidad e On e.IdEspecialidad = me.IdEspecialidad
                                                        Where me.IdMedico = u.IdUsuario) As Especialidades,
                                                       (Select Count(*) From MedicoHorario h
                                                        Where h.IdMedico = u.IdUsuario And h.Activo = 1) As FranjasActivas
                                                From Usuario u
                                                Left Join Medico m On m.IdMedico = u.IdUsuario
                                                Where Exists (Select 1 From UsuarioRol ur
                                                              Join Rol r On r.IdRol = ur.IdRol
                                                              Where ur.IdUsuario = u.IdUsuario And r.Nombre = @rolMedico)
                                                  And (@nombre Is Null
                                                       Or (u.Nombre + ' ' + u.Apellido) Like @nombre
                                                       Or (u.Apellido + ' ' + u.Nombre) Like @nombre)
                                                  And (@idEspecialidad Is Null
                                                       Or Exists (Select 1 From MedicoEspecialidad me
                                                                  Where me.IdMedico = u.IdUsuario And me.IdEspecialidad = @idEspecialidad))
                                                Order By u.Apellido, u.Nombre", cn);

                cmd.Parameters.Add("@rolMedico", SqlDbType.VarChar, 30).Value = RolMedico;
                cmd.Parameters.Add("@nombre", SqlDbType.VarChar, 200).Value =
                    string.IsNullOrWhiteSpace(nombre) ? (object)DBNull.Value : "%" + EscaparLike(nombre.Trim()) + "%";
                cmd.Parameters.Add("@idEspecialidad", SqlDbType.Int).Value = (object)idEspecialidad ?? DBNull.Value;

                using (SqlDataReader lector = cmd.ExecuteReader())
                {
                    while (lector.Read())
                    {
                        lista.Add(new BE_Medico
                        {
                            IdMedico = lector.GetInt32(0),
                            Nombre = lector.GetString(1),
                            Apellido = lector.GetString(2),
                            UsuarioActivo = lector.GetBoolean(3),
                            Matricula = lector.IsDBNull(4) ? (int?)null : lector.GetInt32(4),
                            DVH = lector.IsDBNull(5) ? null : lector.GetString(5),
                            Especialidades = lector.IsDBNull(6) ? null : lector.GetString(6),
                            FranjasActivas = lector.GetInt32(7)
                        });
                    }
                }
            }

            return lista;
        }

        public bool EsUsuarioMedico(int idUsuario)
        {
            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand(@"Select Count(*) From UsuarioRol ur
                                                Join Rol r On r.IdRol = ur.IdRol
                                                Where ur.IdUsuario = @idUsuario And r.Nombre = @rolMedico", cn);

                cmd.Parameters.Add("@idUsuario", SqlDbType.Int).Value = idUsuario;
                cmd.Parameters.Add("@rolMedico", SqlDbType.VarChar, 30).Value = RolMedico;

                return (int)cmd.ExecuteScalar() > 0;
            }
        }

        public bool ExisteMedico(int idMedico)
        {
            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("Select Count(*) From Medico Where IdMedico=@id", cn);
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idMedico;

                return (int)cmd.ExecuteScalar() > 0;
            }
        }

        public bool ExisteMatricula(int matricula, int idMedicoExcluido)
        {
            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("Select Count(*) From Medico Where Matricula=@matricula And IdMedico<>@id", cn);
                cmd.Parameters.Add("@matricula", SqlDbType.Int).Value = matricula;
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idMedicoExcluido;

                return (int)cmd.ExecuteScalar() > 0;
            }
        }

        public List<int> ObtenerIdsEspecialidad(int idMedico)
        {
            List<int> lista = new List<int>();

            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("Select IdEspecialidad From MedicoEspecialidad Where IdMedico=@id", cn);
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idMedico;

                using (SqlDataReader lector = cmd.ExecuteReader())
                {
                    while (lector.Read())
                    {
                        lista.Add(lector.GetInt32(0));
                    }
                }
            }

            return lista;
        }

        public void GuardarMedico(int idMedico, int matricula, string dvh, List<int> idsEspecialidad)
        {
            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                using (SqlTransaction tx = cn.BeginTransaction())
                {
                    SqlCommand cmdMedico = new SqlCommand(@"If Exists (Select 1 From Medico Where IdMedico=@id)
                                                              Update Medico Set Matricula=@matricula, DVH=@dvh Where IdMedico=@id
                                                          Else
                                                              Insert into Medico (IdMedico, Matricula, HorarioDesde, HorarioHasta, DVH)
                                                              Values (@id, @matricula, '00:00', '00:00', @dvh)", cn, tx);

                    cmdMedico.Parameters.Add("@id", SqlDbType.Int).Value = idMedico;
                    cmdMedico.Parameters.Add("@matricula", SqlDbType.Int).Value = matricula;
                    cmdMedico.Parameters.Add("@dvh", SqlDbType.VarChar, 255).Value = (object)dvh ?? DBNull.Value;
                    cmdMedico.ExecuteNonQuery();

                    SqlCommand cmdBorrar = new SqlCommand("Delete From MedicoEspecialidad Where IdMedico=@id", cn, tx);
                    cmdBorrar.Parameters.Add("@id", SqlDbType.Int).Value = idMedico;
                    cmdBorrar.ExecuteNonQuery();

                    foreach (int idEspecialidad in idsEspecialidad)
                    {
                        SqlCommand cmdAgregar = new SqlCommand("Insert into MedicoEspecialidad (IdMedico, IdEspecialidad) Values (@idMedico, @idEspecialidad)", cn, tx);
                        cmdAgregar.Parameters.Add("@idMedico", SqlDbType.Int).Value = idMedico;
                        cmdAgregar.Parameters.Add("@idEspecialidad", SqlDbType.Int).Value = idEspecialidad;
                        cmdAgregar.ExecuteNonQuery();
                    }

                    tx.Commit();
                }
            }
        }

        public void ActualizarRangoHorario(int idMedico)
        {
            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand(@"Update Medico Set
                                                    HorarioDesde = IsNull((Select Min(HoraDesde) From MedicoHorario Where IdMedico=@id And Activo=1), '00:00'),
                                                    HorarioHasta = IsNull((Select Max(HoraHasta) From MedicoHorario Where IdMedico=@id And Activo=1), '00:00')
                                                Where IdMedico=@id", cn);

                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idMedico;

                cmd.ExecuteNonQuery();
            }
        }

        private static string EscaparLike(string texto)
        {
            return texto.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]");
        }
    }
}
