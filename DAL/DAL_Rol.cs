using BE;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class DAL_Rol
    {
        DAL_Conexion conex = new DAL_Conexion();

        //Un usuario tiene un solo rol activo a la vez: borramos el anterior antes de insertar el nuevo.
        public void AsignarRol(int IdUsuario, int idRol)
        {
            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmdBorrar = new SqlCommand("Delete From UsuarioRol Where IdUsuario = @idUsuario", cn);
                cmdBorrar.Parameters.Add("@idUsuario", SqlDbType.Int).Value = IdUsuario;
                cmdBorrar.ExecuteNonQuery();

                SqlCommand cmd = new SqlCommand("Insert into UsuarioRol (IdUsuario, IdRol) values (@idUsuario, @idrol)", cn);

                cmd.Parameters.Add("@idUsuario", SqlDbType.Int).Value = IdUsuario;
                cmd.Parameters.Add("@idrol", SqlDbType.Int).Value = idRol;

                cmd.ExecuteNonQuery();
            }
        }

        public List<BE_Rol> ObtenerRoles()
        {
            List<BE_Rol> lista = new List<BE_Rol>();

            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("Select IdRol, Nombre, DVH From Rol", cn);

                SqlDataReader lector = cmd.ExecuteReader();

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

            return lista;
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

        public BE_Rol ObtenerRolPorUsuario(int idUsuario)
        {
            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand(@"Select Top 1 r.IdRol, r.Nombre
                                                From UsuarioRol ur
                                                Join Rol r On r.IdRol = ur.IdRol
                                                Where ur.IdUsuario = @idUsuario", cn);

                cmd.Parameters.Add("@idUsuario", SqlDbType.Int).Value = idUsuario;

                SqlDataReader lector = cmd.ExecuteReader();

                if (!lector.Read())
                {
                    return null;
                }

                return new BE_Rol
                {
                    IdRol = lector.GetInt32(0),
                    Nombre = lector.GetString(1)
                };
            }
        }

        public void EliminarRol(int IdUsuario, int idRol)
        {
            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("DELETE FROM UsuarioRol WHERE IdUsuario = @idUsuario AND IdRol = @idRol", cn);

                cmd.Parameters.Add("@idUsuario", SqlDbType.Int).Value = IdUsuario;
                cmd.Parameters.Add("@idrol", SqlDbType.Int).Value = idRol;

                cmd.ExecuteNonQuery();
            }
        }


    }
}
