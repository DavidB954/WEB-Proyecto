using BE;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    public class DAL_Especialidad
    {
        DAL_Conexion conex = new DAL_Conexion();

        public List<BE_Especialidad> ObtenerEspecialidades()
        {
            List<BE_Especialidad> lista = new List<BE_Especialidad>();

            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("Select IdEspecialidad, NombreEspecialidad From Especialidad Order By NombreEspecialidad", cn);

                SqlDataReader lector = cmd.ExecuteReader();

                while (lector.Read())
                {
                    lista.Add(new BE_Especialidad
                    {
                        IdEspecialidad = lector.GetInt32(0),
                        Nombre = lector.GetString(1)
                    });
                }
            }

            return lista;
        }

        public void AgregarEspecialidad(BE_Especialidad especialidad)
        {
            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand(@"Insert into Especialidad (NombreEspecialidad)
                                                VALUES (@nombre);
                                                SELECT CAST(SCOPE_IDENTITY() AS INT);", cn);

                cmd.Parameters.Add("@nombre", SqlDbType.VarChar, 30).Value = especialidad.Nombre;

                especialidad.IdEspecialidad = (int)cmd.ExecuteScalar();
            }
        }

        public void EliminarEspecialidad(int idEspecialidad)
        {
            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("Delete From Especialidad Where IdEspecialidad=@id", cn);
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idEspecialidad;

                cmd.ExecuteNonQuery();
            }
        }
    }
}
