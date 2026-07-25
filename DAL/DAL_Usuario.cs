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

        //Trae todos los usuarios junto con el nombre de su rol (LEFT JOIN para que aparezcan también los que todavía no tienen rol asignado).
        public List<BE_Usuario> UsuariosConRol()
        {
            List<BE_Usuario> ListaUsuarios = new List<BE_Usuario>();

            using (SqlConnection conexion = conex.ObtenerConexion())
            {
                conexion.Open();

                SqlCommand cmd = new SqlCommand(@"Select u.IdUsuario, u.Nombre, u.Apellido, u.DNI, u.Email, u.IntentosFallidos, u.Activo, r.Nombre As NombreRol
                                                From Usuario u
                                                Left Join UsuarioRol ur On ur.IdUsuario = u.IdUsuario
                                                Left Join Rol r On r.IdRol = ur.IdRol", conexion);

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

                //Columnas explícitas (en vez de u.*) para que los índices del lector no dependan del orden físico
                //de la tabla: agregar una columna a Usuario en cualquier posición que no sea el final ya no rompe esto.
                SqlCommand cmdUsuEmail = new SqlCommand(@"Select u.IdUsuario, u.Nombre, u.Apellido, u.Email, u.HashPassword, u.DNI, u.DVH, u.IntentosFallidos, u.Activo, r.IdRol, r.Nombre As NombreRol
                                                        From Usuario u
                                                        Left Join UsuarioRol ur On ur.IdUsuario = u.IdUsuario
                                                        Left Join Rol r On r.IdRol = ur.IdRol
                                                        Where u.Email=@email", conexion);

                //Mismo tamaño que la columna real (ver AgregarUsuario/ModificarUsuario): si acá se usa un Size
                //menor, SqlParameter trunca el valor en silencio y un email legítimo de más de 30 caracteres
                //jamás matchea en el WHERE, dejando a ese usuario sin poder loguearse nunca.
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
                        NombreRol = Lector.IsDBNull(10) ? null : Lector.GetString(10)
                    };
            }
        }

        //Updateamos los intentos fallidos en caso de que el login sea incorrecto, y bloqueamos el usuario si supera los 3 intentos fallidos.
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

                //Recuperamos el ID generado para poder loguear en bitácora a qué usuario corresponde el alta.
                //Si el Insert falla (ej. DNI/Email duplicado), la excepción debe propagarse: si no, BLL_Usuario
                //sigue de largo y registra en bitácora un alta que en realidad nunca sucedió.
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

        public void EliminarUsuario(int id)
        {
            using (SqlConnection conexion = conex.ObtenerConexion())
            {
                conexion.Open();

                //Se borra primero la asignación de rol del usuario: si la FK de UsuarioRol hacia Usuario no tiene
                //ON DELETE CASCADE definido en la base, el DELETE de más abajo fallaría por violación de FK
                //mientras el usuario tenga un rol asignado (el caso normal). Es un no-op si no tenía rol.
                SqlCommand comandoRol = new SqlCommand("Delete from UsuarioRol WHERE IdUsuario = @id", conexion);
                comandoRol.Parameters.AddWithValue("@id", id);
                comandoRol.ExecuteNonQuery();

                //Ya no se borra a mano lo del usuario en Bitacora: el FK (FK_Bitacora_Usuario) hace SET NULL
                //en cascada, así el historial de auditoría del usuario borrado se conserva (solo pierde la referencia).
                SqlCommand comando = new SqlCommand("Delete from Usuario WHERE IdUsuario = @id", conexion);

                comando.Parameters.AddWithValue("@id", id);

                comando.ExecuteNonQuery();
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
