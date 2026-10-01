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
    public class DAL_Permiso
    {
        DAL_Conexion conex = new DAL_Conexion();

        public List<BE_Permiso> ObtenerPermisos()
        {
            List<BE_Permiso> lista = new List<BE_Permiso>();

            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("Select IdPermiso, Nombre, Tipo, DVH From Permiso", cn);

                SqlDataReader lector = cmd.ExecuteReader();

                while (lector.Read())
                {
                    lista.Add(new BE_Permiso
                    {
                        IdPermiso = lector.GetInt32(0),
                        Nombre = lector.GetString(1),
                        Tipo = lector.GetString(2),
                        DVH = lector.IsDBNull(3) ? null : lector.GetString(3)
                    });
                }
            }

            return lista;
        }

        public void AgregarPermiso(BE_Permiso permiso)
        {
            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand(@"Insert into Permiso (Nombre, Tipo, DVH)
                                                VALUES (@nombre, @tipo, @dvh);
                                                SELECT CAST(SCOPE_IDENTITY() AS INT);", cn);

                cmd.Parameters.Add("@nombre", SqlDbType.VarChar, 30).Value = permiso.Nombre;
                cmd.Parameters.Add("@tipo", SqlDbType.VarChar, 20).Value = permiso.Tipo;
                cmd.Parameters.Add("@dvh", SqlDbType.VarChar, 255).Value = (object)permiso.DVH ?? DBNull.Value;

                permiso.IdPermiso = (int)cmd.ExecuteScalar();
            }
        }

        public void ModificarPermiso(BE_Permiso permiso)
        {
            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("Update Permiso Set Nombre=@nombre, Tipo=@tipo, DVH=@dvh Where IdPermiso=@id", cn);

                cmd.Parameters.Add("@nombre", SqlDbType.VarChar, 30).Value = permiso.Nombre;
                cmd.Parameters.Add("@tipo", SqlDbType.VarChar, 20).Value = permiso.Tipo;
                cmd.Parameters.Add("@dvh", SqlDbType.VarChar, 255).Value = permiso.DVH;
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = permiso.IdPermiso;

                cmd.ExecuteNonQuery();
            }
        }

        public void ActualizarDVH(int idPermiso, string dvh)
        {
            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("Update Permiso Set DVH=@dvh Where IdPermiso=@id", cn);

                cmd.Parameters.Add("@dvh", SqlDbType.VarChar, 255).Value = dvh;
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idPermiso;

                cmd.ExecuteNonQuery();
            }
        }

        public void EliminarPermiso(int idPermiso)
        {
            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("Delete From Permiso Where IdPermiso=@id", cn);

                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idPermiso;

                cmd.ExecuteNonQuery();
            }
        }

        // ---- Rol_Permiso: permisos que un Rol otorga directamente ----

        public List<int> ObtenerIdsPermisoDeRol(int idRol)
        {
            List<int> lista = new List<int>();

            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("Select IdPermiso From Rol_Permiso Where IdRol=@idRol", cn);
                cmd.Parameters.Add("@idRol", SqlDbType.Int).Value = idRol;

                SqlDataReader lector = cmd.ExecuteReader();

                while (lector.Read())
                {
                    lista.Add(lector.GetInt32(0));
                }
            }

            return lista;
        }

        public void AgregarPermisoARol(int idRol, int idPermiso)
        {
            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("Insert into Rol_Permiso (IdRol, IdPermiso) Values (@idRol, @idPermiso)", cn);
                cmd.Parameters.Add("@idRol", SqlDbType.Int).Value = idRol;
                cmd.Parameters.Add("@idPermiso", SqlDbType.Int).Value = idPermiso;

                cmd.ExecuteNonQuery();
            }
        }

        public void QuitarPermisoDeRol(int idRol, int idPermiso)
        {
            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("Delete From Rol_Permiso Where IdRol=@idRol And IdPermiso=@idPermiso", cn);
                cmd.Parameters.Add("@idRol", SqlDbType.Int).Value = idRol;
                cmd.Parameters.Add("@idPermiso", SqlDbType.Int).Value = idPermiso;

                cmd.ExecuteNonQuery();
            }
        }

        // ---- UsuarioPermiso: permisos asignados directo a un usuario ----

        public List<int> ObtenerIdsPermisoDeUsuario(int idUsuario)
        {
            List<int> lista = new List<int>();

            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("Select IdPermiso From UsuarioPermiso Where IdUsuario=@idUsuario", cn);
                cmd.Parameters.Add("@idUsuario", SqlDbType.Int).Value = idUsuario;

                SqlDataReader lector = cmd.ExecuteReader();

                while (lector.Read())
                {
                    lista.Add(lector.GetInt32(0));
                }
            }

            return lista;
        }

        public void AgregarPermisoAUsuario(int idUsuario, int idPermiso)
        {
            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("Insert into UsuarioPermiso (IdUsuario, IdPermiso) Values (@idUsuario, @idPermiso)", cn);
                cmd.Parameters.Add("@idUsuario", SqlDbType.Int).Value = idUsuario;
                cmd.Parameters.Add("@idPermiso", SqlDbType.Int).Value = idPermiso;

                cmd.ExecuteNonQuery();
            }
        }

        public void QuitarPermisoDeUsuario(int idUsuario, int idPermiso)
        {
            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("Delete From UsuarioPermiso Where IdUsuario=@idUsuario And IdPermiso=@idPermiso", cn);
                cmd.Parameters.Add("@idUsuario", SqlDbType.Int).Value = idUsuario;
                cmd.Parameters.Add("@idPermiso", SqlDbType.Int).Value = idPermiso;

                cmd.ExecuteNonQuery();
            }
        }
    }
}
