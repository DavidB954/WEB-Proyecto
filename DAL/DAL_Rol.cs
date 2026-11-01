using BE;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    public class DAL_Rol
    {
        DAL_Conexion conex = new DAL_Conexion();

        public List<BE_Rol> ObtenerRoles()
        {
            List<BE_Rol> lista = new List<BE_Rol>();

            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("Select IdRol, Nombre, DVH From Rol", cn);

                using (SqlDataReader lector = cmd.ExecuteReader())
                {
                    while (lector.Read())
                    {
                        lista.Add(new BE_Rol
                        {
                            IdRol = lector.GetInt32(0),
                            Nombre = lector.GetString(1),
                            DVH = lector.IsDBNull(2) ? null : lector.GetString(2)
                        });
                    }
                }
            }

            return lista;
        }

        public void AgregarRol(BE_Rol rol)
        {
            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand(@"Insert into Rol (Nombre, DVH)
                                                VALUES (@nombre, @dvh);
                                                SELECT CAST(SCOPE_IDENTITY() AS INT);", cn);

                cmd.Parameters.Add("@nombre", SqlDbType.VarChar, 30).Value = rol.Nombre;
                cmd.Parameters.Add("@dvh", SqlDbType.VarChar, 255).Value = (object)rol.DVH ?? DBNull.Value;

                rol.IdRol = (int)cmd.ExecuteScalar();
            }
        }

        public void ModificarRol(BE_Rol rol)
        {
            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("Update Rol Set Nombre=@nombre, DVH=@dvh Where IdRol=@id", cn);

                cmd.Parameters.Add("@nombre", SqlDbType.VarChar, 30).Value = rol.Nombre;
                cmd.Parameters.Add("@dvh", SqlDbType.VarChar, 255).Value = rol.DVH;
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = rol.IdRol;

                cmd.ExecuteNonQuery();
            }
        }

        public void ActualizarDVH(int idRol, string dvh)
        {
            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("Update Rol Set DVH = @dvh Where IdRol = @idRol", cn);

                cmd.Parameters.Add("@dvh", SqlDbType.VarChar, 255).Value = dvh;
                cmd.Parameters.Add("@idRol", SqlDbType.Int).Value = idRol;

                cmd.ExecuteNonQuery();
            }
        }

        public void EliminarRolPorId(int idRol)
        {
            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                using (SqlTransaction transaccion = cn.BeginTransaction())
                {
                    SqlCommand cmd = new SqlCommand(
                        @"Delete From Rol_Permiso Where IdRol=@id;
                          Delete From Rol_Rol Where idPadre=@id;
                          Delete From Rol Where IdRol=@id;", cn, transaccion);
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = idRol;

                    cmd.ExecuteNonQuery();

                    transaccion.Commit();
                }
            }
        }

        public List<string> ObtenerNombresUsuariosConRol(int idRol)
        {
            return ObtenerNombres(
                @"Select u.Nombre + ' ' + u.Apellido
                  From UsuarioRol ur
                  Join Usuario u On u.IdUsuario = ur.IdUsuario
                  Where ur.IdRol = @id
                  Order By u.Apellido, u.Nombre", idRol);
        }

        public List<string> ObtenerNombresRolesPadre(int idRol)
        {
            return ObtenerNombres(
                @"Select r.Nombre
                  From Rol_Rol rr
                  Join Rol r On r.IdRol = rr.idPadre
                  Where rr.idHijo = @id
                  Order By r.Nombre", idRol);
        }

        private List<string> ObtenerNombres(string consulta, int idRol)
        {
            List<string> nombres = new List<string>();

            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand(consulta, cn);
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idRol;

                using (SqlDataReader lector = cmd.ExecuteReader())
                {
                    while (lector.Read())
                    {
                        nombres.Add(lector.IsDBNull(0) ? string.Empty : lector.GetString(0));
                    }
                }
            }

            return nombres;
        }

        public List<int> ObtenerIdsUsuariosConRol(int idRol)
        {
            List<int> ids = new List<int>();

            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("Select IdUsuario From UsuarioRol Where IdRol = @idRol", cn);
                cmd.Parameters.Add("@idRol", SqlDbType.Int).Value = idRol;

                using (SqlDataReader lector = cmd.ExecuteReader())
                {
                    while (lector.Read())
                    {
                        ids.Add(lector.GetInt32(0));
                    }
                }
            }

            return ids;
        }

        public List<BE_Rol> ObtenerRolesDeUsuario(int idUsuario)
        {
            List<BE_Rol> lista = new List<BE_Rol>();

            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand(@"Select r.IdRol, r.Nombre, r.DVH
                                                From UsuarioRol ur
                                                Join Rol r On r.IdRol = ur.IdRol
                                                Where ur.IdUsuario = @idUsuario", cn);

                cmd.Parameters.Add("@idUsuario", SqlDbType.Int).Value = idUsuario;

                using (SqlDataReader lector = cmd.ExecuteReader())
                {
                    while (lector.Read())
                    {
                        lista.Add(new BE_Rol
                        {
                            IdRol = lector.GetInt32(0),
                            Nombre = lector.GetString(1),
                            DVH = lector.IsDBNull(2) ? null : lector.GetString(2)
                        });
                    }
                }
            }

            return lista;
        }

        public void AgregarRolAUsuario(int idUsuario, int idRol)
        {
            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("Insert into UsuarioRol (IdUsuario, IdRol) Values (@idUsuario, @idRol)", cn);
                cmd.Parameters.Add("@idUsuario", SqlDbType.Int).Value = idUsuario;
                cmd.Parameters.Add("@idRol", SqlDbType.Int).Value = idRol;

                cmd.ExecuteNonQuery();
            }
        }

        public void QuitarRolDeUsuario(int idUsuario, int idRol)
        {
            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("Delete From UsuarioRol Where IdUsuario=@idUsuario And IdRol=@idRol", cn);
                cmd.Parameters.Add("@idUsuario", SqlDbType.Int).Value = idUsuario;
                cmd.Parameters.Add("@idRol", SqlDbType.Int).Value = idRol;

                cmd.ExecuteNonQuery();
            }
        }

        public List<int> ObtenerIdsSubRol(int idRolPadre)
        {
            List<int> lista = new List<int>();

            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("Select idHijo From Rol_Rol Where idPadre=@idPadre", cn);
                cmd.Parameters.Add("@idPadre", SqlDbType.Int).Value = idRolPadre;

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

        public void AgregarSubRol(int idRolPadre, int idRolHijo)
        {
            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("Insert into Rol_Rol (idPadre, idHijo) Values (@idPadre, @idHijo)", cn);
                cmd.Parameters.Add("@idPadre", SqlDbType.Int).Value = idRolPadre;
                cmd.Parameters.Add("@idHijo", SqlDbType.Int).Value = idRolHijo;

                cmd.ExecuteNonQuery();
            }
        }

        public void QuitarSubRol(int idRolPadre, int idRolHijo)
        {
            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("Delete From Rol_Rol Where idPadre=@idPadre And idHijo=@idHijo", cn);
                cmd.Parameters.Add("@idPadre", SqlDbType.Int).Value = idRolPadre;
                cmd.Parameters.Add("@idHijo", SqlDbType.Int).Value = idRolHijo;

                cmd.ExecuteNonQuery();
            }
        }
    }
}
