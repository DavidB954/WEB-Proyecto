using BE;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class DAL_Usuario
    {
        DAL_Conexion conex = new DAL_Conexion();


        public List<BE_Usuario> Usuarios()
        {
            List<BE_Usuario> ListaUsuarios = new List<BE_Usuario>();

            using (SqlConnection conexion = conex.ObtenerConexion())
            {
                conexion.Open();

                SqlCommand cmdUsuarios = new SqlCommand("Select * from Usuario", conexion);

                SqlDataReader Lector = cmdUsuarios.ExecuteReader();

                while (Lector.Read())
                {
                    BE_Usuario Usuario = new BE_Usuario();
                    Usuario.IdUsuario = Convert.ToInt32(Lector["IdUsuario"]);
                    Usuario.Nombre = Lector["Nombre"].ToString();
                    Usuario.Apellido = Lector["Apellido"].ToString();
                    Usuario.DNI = Lector["DNI"].ToString();
                    Usuario.Email = Lector["Email"].ToString();
                    Usuario.DVH = Lector["DVH"].ToString();
                    Usuario.HashPassword = Lector["HashPassword"].ToString();
                    Usuario.IntentosFallidos = Convert.ToInt32(Lector["IntentosFallidos"]);
                    Usuario.Activo = Convert.ToBoolean(Lector["Activo"]);
                    ListaUsuarios.Add(Usuario);
                }
            }
            return ListaUsuarios;
        }

        public List<BE_Usuario> UsuariosConRol()
        {
            List<BE_Usuario> ListaUsuarios = new List<BE_Usuario>();

            using (SqlConnection conexion = conex.ObtenerConexion())
            {
                conexion.Open();

                SqlCommand cmd = new SqlCommand(@"Select u.IdUsuario, u.Nombre, u.Apellido, u.DNI, u.Email, u.IntentosFallidos, u.Activo,
                                                       STRING_AGG(r.Nombre, ', ') WITHIN GROUP (ORDER BY r.Nombre) As NombreRol
                                                From Usuario u
                                                Left Join UsuarioRol ur On ur.IdUsuario = u.IdUsuario
                                                Left Join Rol r On r.IdRol = ur.IdRol
                                                Group By u.IdUsuario, u.Nombre, u.Apellido, u.DNI, u.Email, u.IntentosFallidos, u.Activo", conexion);

                SqlDataReader Lector = cmd.ExecuteReader();

                while (Lector.Read())
                {
                    BE_Usuario Usuario = new BE_Usuario();
                    Usuario.IdUsuario = Convert.ToInt32(Lector["IdUsuario"]);
                    Usuario.Nombre = Lector["Nombre"].ToString();
                    Usuario.Apellido = Lector["Apellido"].ToString();
                    Usuario.DNI = Lector["DNI"].ToString();
                    Usuario.Email = Lector["Email"].ToString();
                    Usuario.IntentosFallidos = Convert.ToInt32(Lector["IntentosFallidos"]);
                    Usuario.Activo = Convert.ToBoolean(Lector["Activo"]);
                    Usuario.NombreRol = Lector["NombreRol"] == DBNull.Value ? "(sin rol)" : Lector["NombreRol"].ToString();
                    ListaUsuarios.Add(Usuario);
                }
            }
            return ListaUsuarios;
        }

        public BE_Usuario ObtenerUsuarioPorEmail(string email)
        {

            using (SqlConnection conexion = conex.ObtenerConexion())
            {
                conexion.Open();

                SqlCommand cmdUsuEmail = new SqlCommand(@"Select u.IdUsuario, u.Nombre, u.Apellido, u.Email, u.HashPassword, u.DNI, u.DVH, u.IntentosFallidos, u.Activo, r.IdRol, r.Nombre As NombreRol, u.IdIdiomaPreferido
                                                        From Usuario u
                                                        Left Join UsuarioRol ur On ur.IdUsuario = u.IdUsuario
                                                        Left Join Rol r On r.IdRol = ur.IdRol
                                                        Where u.Email=@email", conexion);

                cmdUsuEmail.Parameters.Add("@email", SqlDbType.VarChar, 50).Value = email;

                SqlDataReader Lector = cmdUsuEmail.ExecuteReader();


                if (!Lector.Read())
                {
                    return null;
                }

                else
                    return new BE_Usuario
                    {
                        IdUsuario = Lector.GetInt32(0),
                        Nombre = Lector.GetString(1),
                        Apellido = Lector.GetString(2),
                        Email = Lector.GetString(3),
                        HashPassword = Lector.GetString(4),
                        DNI = Lector.GetString(5),
                        DVH = Lector.GetString(6),
                        IntentosFallidos = Lector.GetInt32(7),
                        Activo = Lector.GetBoolean(8),
                        IdRol = Lector.IsDBNull(9) ? 0 : Lector.GetInt32(9),
                        NombreRol = Lector.IsDBNull(10) ? null : Lector.GetString(10),
                        IdIdiomaPreferido = Lector.IsDBNull(11) ? (int?)null : Lector.GetInt32(11)
                    };
            }
        }

        public void ActualizarIntentosFallidos(int intentos, int IdUsuario)
        {
            using (SqlConnection conexion = conex.ObtenerConexion())
            {
                conexion.Open();

                SqlCommand cmdActualizarIntentos = new SqlCommand("Update Usuario Set IntentosFallidos = @intentos Where IdUsuario=@id", conexion);

                cmdActualizarIntentos.Parameters.Add("@intentos", SqlDbType.Int).Value = intentos;
                cmdActualizarIntentos.Parameters.Add("@id", SqlDbType.Int).Value = IdUsuario;

                cmdActualizarIntentos.ExecuteNonQuery();
            }
        }
        public void BloquearUsuario(int IdUsuario)
        {
            using (SqlConnection conexion = conex.ObtenerConexion())
            {
                conexion.Open();

                SqlCommand cmdBloquearUsuario = new SqlCommand("Update Usuario SET Activo=0 Where IdUsuario = @id", conexion);

                cmdBloquearUsuario.Parameters.Add("@id", SqlDbType.Int).Value = IdUsuario;

                cmdBloquearUsuario.ExecuteNonQuery();
            }
        }

        public void AgregarUsuario(BE_Usuario Usuario)
        {
            using (SqlConnection conexion = conex.ObtenerConexion())
            {
                conexion.Open();

                SqlCommand cmdUsuario = new SqlCommand(@"Insert into Usuario (Nombre, Apellido, Email, HashPassword, DNI, DVH, IntentosFallidos, Activo)
                                                    VALUES (@nombre, @apellido, @email, @password, @dni, @dvh, @intentos, @activo);
                                                    SELECT CAST(SCOPE_IDENTITY() AS INT);", conexion);

                cmdUsuario.Parameters.Add("@nombre", SqlDbType.VarChar, 50).Value = Usuario.Nombre;
                cmdUsuario.Parameters.Add("@apellido", SqlDbType.VarChar, 50).Value = Usuario.Apellido;
                cmdUsuario.Parameters.Add("@dni", SqlDbType.VarChar, 8).Value = Usuario.DNI;
                cmdUsuario.Parameters.Add("@email", SqlDbType.VarChar, 50).Value = Usuario.Email;
                cmdUsuario.Parameters.Add("@password", SqlDbType.VarChar, 255).Value = Usuario.HashPassword;
                cmdUsuario.Parameters.Add("@dvh", SqlDbType.VarChar, 255).Value = Usuario.DVH;
                cmdUsuario.Parameters.Add("@intentos", SqlDbType.Int).Value = Usuario.IntentosFallidos;
                cmdUsuario.Parameters.Add("@activo", SqlDbType.Bit).Value = Usuario.Activo;

                Usuario.IdUsuario = (int)cmdUsuario.ExecuteScalar();
            }
        }


        public void ModificarUsuario(BE_Usuario Usuario)
        {
            using (SqlConnection conexion = conex.ObtenerConexion())
            {
                conexion.Open();

                SqlCommand comando = new SqlCommand("Update Usuario SET Nombre=@nombre, Apellido=@apellido, Email=@email, HashPassword=@password, DNI=@dni, DVH=@dvh, IntentosFallidos=@intentos, Activo=@activo WHERE IdUsuario=@id", conexion);

                comando.Parameters.AddWithValue("@id", Usuario.IdUsuario);
                comando.Parameters.AddWithValue("@nombre", Usuario.Nombre);
                comando.Parameters.AddWithValue("@apellido", Usuario.Apellido);
                comando.Parameters.AddWithValue("@email", Usuario.Email);
                comando.Parameters.AddWithValue("@password", Usuario.HashPassword);
                comando.Parameters.AddWithValue("@dni", Usuario.DNI);
                comando.Parameters.AddWithValue("@dvh", Usuario.DVH);
                comando.Parameters.AddWithValue("@intentos", Usuario.IntentosFallidos);
                comando.Parameters.AddWithValue("@activo", Usuario.Activo);

                comando.ExecuteNonQuery();
            }
        }

        // Error de SQL Server cuando un DELETE choca con una clave foranea.
        private const int ErrorClaveForanea = 547;

        // Borra el usuario junto con lo que depende de el, todo en una transaccion:
        // - Como paciente: sus turnos, atenciones y recetas.
        // - Como medico: su agenda (franjas, bloqueos, especialidades). Sus turnos
        //   reservados pasan a Cancelado (el DVH lo arma la BLL con dvhTurnoCancelado)
        //   y los atendidos se conservan porque son la historia clinica de otros
        //   pacientes; quedan con el IdMedico del usuario borrado.
        // La Bitacora no se toca: sus filas conservan el IdUsuario del eliminado.
        // Devuelve false (sin borrar nada) si alguna otra tabla todavia lo referencia.
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
                        // ---- Datos como paciente ----
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

                        // ---- Datos como medico ----
                        // UPDLOCK/HOLDLOCK: nadie puede reservarle un turno nuevo
                        // mientras se cancelan los que tiene.
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

                        // ---- Seguridad y usuario ----
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
            catch (SqlException ex) when (ex.Number == ErrorClaveForanea)
            {
                // La transaccion se deshace sola al cerrar la conexion sin Commit.
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

        public void ResetearContrasena(int id, string nuevaPass)
        {
            using (SqlConnection conexion = conex.ObtenerConexion())
            {
                conexion.Open();
                SqlCommand comando = new SqlCommand("Update Usuario SET HashPassword=@nuevoHash, IntentosFallidos = 0, Activo=1 WHERE IdUsuario=@id", conexion);

                comando.Parameters.AddWithValue("@id", id);
                comando.Parameters.AddWithValue("@nuevoHash", nuevaPass);

                comando.ExecuteNonQuery();
            }
        }

    }
}
