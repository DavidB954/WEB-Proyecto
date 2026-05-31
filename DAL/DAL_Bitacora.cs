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
    public class DAL_Bitacora
    {
        DAL_Conexion conex = new DAL_Conexion();

        //Registrar Eventos

        public void RegistrarEvento(BE_Bitacora Bitacora)
        {
            using (SqlConnection conexion = conex.ObtenerConexion())
            {
                conexion.Open();

                SqlCommand cmdBitacora = new SqlCommand("Insert into Bitacora (IdUsuario, FechaHora, Accion, Descripcion, DireccionIP, Modulo, NombreMaquina) VALUES (@idUsuario, @fechaHora, @accion, @descripcion, @ip, @modulo, @nombremaquina)", conexion);


                //Se utiliza el ADD con el SqlDbType en vez de AddWithValue ya que AddWithValue lo que hace es adivinar el tipo de parametro previsto. Esto puede generar probelmas de rendimiento cuando se debe convertir el valor de la columna a un tipo de dato diferente. Al especificar el tipo de dato con SqlDbType, se evita esta conversión y se mejora el rendimiento de la consulta.

                cmdBitacora.Parameters.Add("@idUsuario", SqlDbType.Int).Value = (object)Bitacora.IdUsuario ?? DBNull.Value;
                cmdBitacora.Parameters.Add("@fechaHora", SqlDbType.DateTime).Value = Bitacora.FechaHora;
                cmdBitacora.Parameters.Add("@accion", SqlDbType.VarChar, 50).Value = Bitacora.Accion;
                cmdBitacora.Parameters.Add("@descripcion", SqlDbType.VarChar, 255).Value = Bitacora.Descripcion;
                cmdBitacora.Parameters.Add("@ip", SqlDbType.VarChar, 45).Value = Bitacora.IP;
                cmdBitacora.Parameters.Add("@modulo", SqlDbType.VarChar, 30).Value = Bitacora.Modulo;
                cmdBitacora.Parameters.Add("@nombremaquina", SqlDbType.VarChar, 100).Value = Bitacora.NombreMaquina;

                cmdBitacora.ExecuteNonQuery();
            }

        }

        public List<BE_Bitacora> ObtenerBitacora()
        {
            try
            {
                List<BE_Bitacora> ListaBitacora = new List<BE_Bitacora>();

                using (SqlConnection conexion = conex.ObtenerConexion())
                {

                    conexion.Open();

                    SqlCommand cmdBitacora = new SqlCommand("Select * From Bitacora", conexion);

                    SqlDataReader Lector = cmdBitacora.ExecuteReader();

                    while (Lector.Read())
                    {
                        BE_Bitacora Bitacora = new BE_Bitacora();
                        Bitacora.IdBitacora = Lector.GetInt32(0);
                        Bitacora.IdUsuario = Lector.IsDBNull(1) ? (int?)null : Lector.GetInt32(1);
                        Bitacora.FechaHora = Lector.GetDateTime(2);
                        Bitacora.Accion = (AccionBitacora)Enum.Parse(typeof(AccionBitacora), Lector.GetString(3));
                        Bitacora.Descripcion = Lector.GetString(4);
                        Bitacora.IP = Lector.GetString(5);
                        Bitacora.DVH = Lector.IsDBNull(6) ? null : Lector.GetString(6);
                        Bitacora.Modulo = Lector.GetString(7);
                        Bitacora.NombreMaquina = Lector.GetString(8);

                        ListaBitacora.Add(Bitacora);
                    }
                }

                return ListaBitacora;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
           
        }

        public DataTable FiltrarBitacora(DateTime? Desde, DateTime? Hasta, int? idUsuario, string Modulo, string Ip)
        {
            using (SqlConnection conexion = conex.ObtenerConexion())
            {
                conexion.Open();
                SqlCommand cmdFiltrado = new SqlCommand("dbo.FiltrarBitacora", conexion);

                cmdFiltrado.CommandType = CommandType.StoredProcedure;

                cmdFiltrado.Parameters.AddWithValue("@Desde", (object)Desde ?? DBNull.Value);
                cmdFiltrado.Parameters.AddWithValue("@Hasta", (object)Hasta ?? DBNull.Value);
                cmdFiltrado.Parameters.AddWithValue("@IdUsuario", (object)idUsuario ?? DBNull.Value);
                cmdFiltrado.Parameters.AddWithValue("@Modulo", (object)Modulo ?? DBNull.Value);
                cmdFiltrado.Parameters.AddWithValue("@IP", (object)Ip ?? DBNull.Value);

                using (SqlDataAdapter adaptador = new SqlDataAdapter(cmdFiltrado))
                {
                    DataTable dt = new DataTable();
                    adaptador.Fill(dt);

                    return dt;
                }

            }
        }
    }
}
