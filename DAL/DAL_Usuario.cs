using BE;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    public class DAL_Usuario
    {
        DAL_Conexion conex = new DAL_Conexion();

        public List<BE_Usuario> Usuarios()
        {
            List<BE_Usuario> listaUsuarios = new List<BE_Usuario>();

            using (SqlConnection conexion = conex.ObtenerConexion())
            {
                conexion.Open();

                SqlCommand cmdUsuarios = new SqlCommand("Select IdUsuario, Nombre, Apellido, DNI, Email, DVH, HashPassword, IntentosFallidos, Activo From Usuario", conexion);

                using (SqlDataReader lector = cmdUsuarios.ExecuteReader())
                {
                    while (lector.Read())
                    {
                        BE_Usuario usuario = new BE_Usuario();
                        usuario.IdUsuario = Convert.ToInt32(lector["IdUsuario"]);
                        usuario.Nombre = lector["Nombre"].ToString();
                        usuario.Apellido = lector["Apellido"].ToString();
                        usuario.DNI = lector["DNI"].ToString();
                        usuario.Email = lector["Email"].ToString();
                        usuario.DVH = lector["DVH"].ToString();
                        usuario.HashPassword = lector["HashPassword"].ToString();
                        usuario.IntentosFallidos = Convert.ToInt32(lector["IntentosFallidos"]);
                        usuario.Activo = Convert.ToBoolean(lector["Activo"]);
                        listaUsuarios.Add(usuario);
                    }
                }
            }
            return listaUsuarios;
        }

        public List<BE_Usuario> UsuariosConRol()
        {
            List<BE_Usuario> listaUsuarios = new List<BE_Usuario>();

            using (SqlConnection conexion = conex.ObtenerConexion())
            {
                conexion.Open();

                SqlCommand cmd = new SqlCommand(@"Select u.IdUsuario, u.Nombre, u.Apellido, u.DNI, u.Email, u.IntentosFallidos, u.Activo,
                                                       STRING_AGG(r.Nombre, ', ') WITHIN GROUP (ORDER BY r.Nombre) As NombreRol
                                                From Usuario u
                                                Left Join UsuarioRol ur On ur.IdUsuario = u.IdUsuario
                                                Left Join Rol r On r.IdRol = ur.IdRol
                                                Group By u.IdUsuario, u.Nombre, u.Apellido, u.DNI, u.Email, u.IntentosFallidos, u.Activo", conexion);

                using (SqlDataReader lector = cmd.ExecuteReader())
                {
                    while (lector.Read())
                    {
                        BE_Usuario usuario = new BE_Usuario();
                        usuario.IdUsuario = Convert.ToInt32(lector["IdUsuario"]);
                        usuario.Nombre = lector["Nombre"].ToString();
                        usuario.Apellido = lector["Apellido"].ToString();
                        usuario.DNI = lector["DNI"].ToString();
                        usuario.Email = lector["Email"].ToString();
                        usuario.IntentosFallidos = Convert.ToInt32(lector["IntentosFallidos"]);
                        usuario.Activo = Convert.ToBoolean(lector["Activo"]);
                        usuario.NombreRol = lector["NombreRol"] == DBNull.Value ? "(sin rol)" : lector["NombreRol"].ToString();
                        listaUsuarios.Add(usuario);
                    }
                }
            }
            return listaUsuarios;
        }

        public BE_Usuario ObtenerUsuarioPorEmail(string email)
        {
            using (SqlConnection conexion = conex.ObtenerConexion())
            {
                conexion.Open();

                SqlCommand cmdUsuEmail = new SqlCommand(@"Select u.IdUsuario, u.Nombre, u.Apellido, u.Email, u.HashPassword, u.DNI, u.DVH, u.IntentosFallidos, u.Activo, r.Nombre As NombreRol, u.IdIdiomaPreferido
                                                        From Usuario u
                                                        Left Join UsuarioRol ur On ur.IdUsuario = u.IdUsuario
                                                        Left Join Rol r On r.IdRol = ur.IdRol
                                                        Where u.Email=@email", conexion);

                cmdUsuEmail.Parameters.Add("@email", SqlDbType.VarChar, 50).Value = email;

                using (SqlDataReader lector = cmdUsuEmail.ExecuteReader())
                {
                    if (!lector.Read())
                    {
                        return null;
                    }

                    else
                        return new BE_Usuario
                        {
                            IdUsuario = lector.GetInt32(0),
                            Nombre = lector.GetString(1),
                            Apellido = lector.GetString(2),
                            Email = lector.GetString(3),
                            HashPassword = lector.GetString(4),
                            DNI = lector.GetString(5),
                            DVH = lector.GetString(6),
                            IntentosFallidos = lector.GetInt32(7),
                            Activo = lector.GetBoolean(8),
                            NombreRol = lector.IsDBNull(9) ? null : lector.GetString(9),
                            IdIdiomaPreferido = lector.IsDBNull(10) ? (int?)null : lector.GetInt32(10)
                        };
                }
            }
        }

        public void ActualizarIntentosFallidos(int intentos, int idUsuario)
        {
            using (SqlConnection conexion = conex.ObtenerConexion())
            {
                conexion.Open();

                SqlCommand cmdActualizarIntentos = new SqlCommand("Update Usuario Set IntentosFallidos = @intentos Where IdUsuario=@id", conexion);

                cmdActualizarIntentos.Parameters.Add("@intentos", SqlDbType.Int).Value = intentos;
                cmdActualizarIntentos.Parameters.Add("@id", SqlDbType.Int).Value = idUsuario;

                cmdActualizarIntentos.ExecuteNonQuery();
            }
        }

        public void BloquearUsuario(int idUsuario)
        {
            using (SqlConnection conexion = conex.ObtenerConexion())
            {
                conexion.Open();

                SqlCommand cmdBloquearUsuario = new SqlCommand("Update Usuario SET Activo=0 Where IdUsuario = @id", conexion);

                cmdBloquearUsuario.Parameters.Add("@id", SqlDbType.Int).Value = idUsuario;

                cmdBloquearUsuario.ExecuteNonQuery();
            }
        }

        public void AgregarUsuario(BE_Usuario usuario)
        {
            using (SqlConnection conexion = conex.ObtenerConexion())
            {
                conexion.Open();

                SqlCommand cmdUsuario = new SqlCommand(@"Insert into Usuario (Nombre, Apellido, Email, HashPassword, DNI, DVH, IntentosFallidos, Activo)
                                                    VALUES (@nombre, @apellido, @email, @password, @dni, @dvh, @intentos, @activo);
                                                    SELECT CAST(SCOPE_IDENTITY() AS INT);", conexion);

                cmdUsuario.Parameters.Add("@nombre", SqlDbType.VarChar, 50).Value = usuario.Nombre;
                cmdUsuario.Parameters.Add("@apellido", SqlDbType.VarChar, 50).Value = usuario.Apellido;
                cmdUsuario.Parameters.Add("@dni", SqlDbType.VarChar, 8).Value = usuario.DNI;
                cmdUsuario.Parameters.Add("@email", SqlDbType.VarChar, 50).Value = usuario.Email;
                cmdUsuario.Parameters.Add("@password", SqlDbType.VarChar, 255).Value = usuario.HashPassword;
                cmdUsuario.Parameters.Add("@dvh", SqlDbType.VarChar, 255).Value = usuario.DVH;
                cmdUsuario.Parameters.Add("@intentos", SqlDbType.Int).Value = usuario.IntentosFallidos;
                cmdUsuario.Parameters.Add("@activo", SqlDbType.Bit).Value = usuario.Activo;

                usuario.IdUsuario = (int)cmdUsuario.ExecuteScalar();
            }
        }

        public void ModificarUsuario(BE_Usuario usuario)
        {
            using (SqlConnection conexion = conex.ObtenerConexion())
            {
                conexion.Open();

                SqlCommand comando = new SqlCommand("Update Usuario SET Nombre=@nombre, Apellido=@apellido, Email=@email, HashPassword=@password, DNI=@dni, DVH=@dvh, IntentosFallidos=@intentos, Activo=@activo WHERE IdUsuario=@id", conexion);

                comando.Parameters.AddWithValue("@id", usuario.IdUsuario);
                comando.Parameters.AddWithValue("@nombre", usuario.Nombre);
                comando.Parameters.AddWithValue("@apellido", usuario.Apellido);
                comando.Parameters.AddWithValue("@email", usuario.Email);
                comando.Parameters.AddWithValue("@password", usuario.HashPassword);
                comando.Parameters.AddWithValue("@dni", usuario.DNI);
                comando.Parameters.AddWithValue("@dvh", usuario.DVH);
                comando.Parameters.AddWithValue("@intentos", usuario.IntentosFallidos);
                comando.Parameters.AddWithValue("@activo", usuario.Activo);

                comando.ExecuteNonQuery();
            }
        }

        public bool EliminarUsuario(int id, Func<BE_Turno, string> dvhTurnoCancelado, out int turnosCancelados, out int turnosPacienteEliminados)
        {
            turnosCancelados = 0;
            turnosPacienteEliminados = 0;

            try
            {
                using (SqlConnection conexion = conex.ObtenerConexion())
                {
                    conexion.Open();

                    using (SqlTransaction tx = conexion.BeginTransaction())
                    {
                        SqlCommand comandoRecetas = new SqlCommand(@"Delete r From Receta r
                                                                   Join AtencionMedica a On a.IdAtencion = r.IdAtencion
                                                                   Where a.IdPaciente = @id", conexion, tx);
                        comandoRecetas.Parameters.AddWithValue("@id", id);
                        comandoRecetas.ExecuteNonQuery();

                        SqlCommand comandoAtenciones = new SqlCommand("Delete from AtencionMedica WHERE IdPaciente = @id", conexion, tx);
                        comandoAtenciones.Parameters.AddWithValue("@id", id);
                        comandoAtenciones.ExecuteNonQuery();

                        SqlCommand comandoTurnosPaciente = new SqlCommand("Delete from Turno WHERE IdPaciente = @id", conexion, tx);
                        comandoTurnosPaciente.Parameters.AddWithValue("@id", id);
                        turnosPacienteEliminados = comandoTurnosPaciente.ExecuteNonQuery();

                        SqlCommand comandoPaciente = new SqlCommand("Delete from Paciente WHERE IdPaciente = @id", conexion, tx);
                        comandoPaciente.Parameters.AddWithValue("@id", id);
                        comandoPaciente.ExecuteNonQuery();

                        List<BE_Turno> reservados = new List<BE_Turno>();

                        SqlCommand comandoReservados = new SqlCommand(@"Select IdTurno, IdPaciente, IdMedico, FechaHora From Turno With (UPDLOCK, HOLDLOCK)
                                                                      Where IdMedico = @id And IdEstadoTurno = @reservado", conexion, tx);
                        comandoReservados.Parameters.AddWithValue("@id", id);
                        comandoReservados.Parameters.AddWithValue("@reservado", (int)EstadoTurno.Reservado);

                        using (SqlDataReader lector = comandoReservados.ExecuteReader())
                        {
                            while (lector.Read())
                            {
                                reservados.Add(new BE_Turno
                                {
                                    IdTurno = lector.GetInt32(0),
                                    IdPaciente = lector.GetInt32(1),
                                    IdMedico = lector.GetInt32(2),
                                    FechaHora = lector.GetDateTime(3),
                                    Estado = EstadoTurno.Cancelado
                                });
                            }
                        }

                        foreach (BE_Turno turno in reservados)
                        {
                            SqlCommand comandoCancelar = new SqlCommand("Update Turno Set IdEstadoTurno = @cancelado, DVH = @dvh Where IdTurno = @idTurno", conexion, tx);
                            comandoCancelar.Parameters.AddWithValue("@cancelado", (int)EstadoTurno.Cancelado);
                            comandoCancelar.Parameters.Add("@dvh", SqlDbType.VarChar, 255).Value = (object)dvhTurnoCancelado(turno) ?? DBNull.Value;
                            comandoCancelar.Parameters.AddWithValue("@idTurno", turno.IdTurno);
                            comandoCancelar.ExecuteNonQuery();
                        }

                        turnosCancelados = reservados.Count;

                        foreach (string tabla in new[] { "MedicoBloqueo", "MedicoHorario", "MedicoEspecialidad", "Medico" })
                        {
                            SqlCommand comandoMedico = new SqlCommand($"Delete from {tabla} WHERE IdMedico = @id", conexion, tx);
                            comandoMedico.Parameters.AddWithValue("@id", id);
                            comandoMedico.ExecuteNonQuery();
                        }

                        SqlCommand comandoPermiso = new SqlCommand("Delete from UsuarioPermiso WHERE IdUsuario = @id", conexion, tx);
                        comandoPermiso.Parameters.AddWithValue("@id", id);
                        comandoPermiso.ExecuteNonQuery();

                        SqlCommand comandoRol = new SqlCommand("Delete from UsuarioRol WHERE IdUsuario = @id", conexion, tx);
                        comandoRol.Parameters.AddWithValue("@id", id);
                        comandoRol.ExecuteNonQuery();

                        SqlCommand comando = new SqlCommand("Delete from Usuario WHERE IdUsuario = @id", conexion, tx);

                        comando.Parameters.AddWithValue("@id", id);

                        comando.ExecuteNonQuery();

                        tx.Commit();
                        return true;
                    }
                }
            }
            catch (SqlException ex) when (ErroresSql.EsClaveForanea(ex))
            {
                turnosCancelados = 0;
                turnosPacienteEliminados = 0;
                return false;
            }
        }

        public void ActualizarDVH(int idUsuario, string dvh)
        {
            using (SqlConnection conexion = conex.ObtenerConexion())
            {
                conexion.Open();

                SqlCommand comando = new SqlCommand("Update Usuario Set DVH=@dvh Where IdUsuario=@id", conexion);

                comando.Parameters.Add("@dvh", SqlDbType.VarChar, 255).Value = dvh;
                comando.Parameters.Add("@id", SqlDbType.Int).Value = idUsuario;

                comando.ExecuteNonQuery();
            }
        }

        public void ActualizarIdiomaPreferido(int idUsuario, int idIdioma)
        {
            using (SqlConnection conexion = conex.ObtenerConexion())
            {
                conexion.Open();

                SqlCommand comando = new SqlCommand("Update Usuario Set IdIdiomaPreferido=@idIdioma Where IdUsuario=@id", conexion);

                comando.Parameters.Add("@idIdioma", SqlDbType.Int).Value = idIdioma;
                comando.Parameters.Add("@id", SqlDbType.Int).Value = idUsuario;

                comando.ExecuteNonQuery();
            }
        }
    }
}
