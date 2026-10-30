using BE;
using System;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    public class DAL_Paciente
    {
        // Nombre del rol (tabla Rol) que identifica a un usuario como paciente.
        private const string RolPaciente = "PACIENTE";

        DAL_Conexion conex = new DAL_Conexion();

        public bool EsUsuarioPaciente(int idUsuario)
        {
            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand(@"Select Count(*) From UsuarioRol ur
                                                Join Rol r On r.IdRol = ur.IdRol
                                                Where ur.IdUsuario = @idUsuario And r.Nombre = @rolPaciente", cn);

                cmd.Parameters.Add("@idUsuario", SqlDbType.Int).Value = idUsuario;
                cmd.Parameters.Add("@rolPaciente", SqlDbType.VarChar, 30).Value = RolPaciente;

                return (int)cmd.ExecuteScalar() > 0;
            }
        }

        // null si el usuario todavia no completo sus datos de paciente.
        public BE_Paciente ObtenerPaciente(int idPaciente)
        {
            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand(@"Select IdPaciente, NumeroHistoriaClinica, FechaNacimiento, Telefono, DVH
                                                From Paciente
                                                Where IdPaciente=@id", cn);

                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idPaciente;

                SqlDataReader lector = cmd.ExecuteReader();

                if (!lector.Read())
                {
                    return null;
                }

                return new BE_Paciente
                {
                    IdPaciente = lector.GetInt32(0),
                    NumeroHistoriaClinica = lector.GetString(1),
                    FechaNacimiento = lector.GetDateTime(2),
                    Telefono = lector.IsDBNull(3) ? null : lector.GetString(3),
                    DVH = lector.IsDBNull(4) ? null : lector.GetString(4)
                };
            }
        }

        public void GuardarPaciente(BE_Paciente paciente)
        {
            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand(@"If Exists (Select 1 From Paciente Where IdPaciente=@id)
                                                    Update Paciente Set FechaNacimiento=@nacimiento, Telefono=@telefono, DVH=@dvh Where IdPaciente=@id
                                                Else
                                                    Insert into Paciente (IdPaciente, NumeroHistoriaClinica, FechaNacimiento, Telefono, DVH)
                                                    Values (@id, @historia, @nacimiento, @telefono, @dvh)", cn);

                cmd.Parameters.Add("@id", SqlDbType.Int).Value = paciente.IdPaciente;
                cmd.Parameters.Add("@historia", SqlDbType.VarChar, 20).Value = paciente.NumeroHistoriaClinica;
                cmd.Parameters.Add("@nacimiento", SqlDbType.Date).Value = paciente.FechaNacimiento.Date;
                cmd.Parameters.Add("@telefono", SqlDbType.VarChar, 20).Value = (object)paciente.Telefono ?? DBNull.Value;
                cmd.Parameters.Add("@dvh", SqlDbType.VarChar, 255).Value = (object)paciente.DVH ?? DBNull.Value;

                cmd.ExecuteNonQuery();
            }
        }
    }
}
