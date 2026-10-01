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


        public void RegistrarEvento(BE_Bitacora Bitacora)
        {
            using (SqlConnection conexion = conex.ObtenerConexion())
            {
                conexion.Open();

                SqlCommand cmdBitacora = new SqlCommand("Insert into Bitacora (IdUsuario, FechaHora, Accion, Descripcion, DireccionIP, Modulo, NombreMaquina, Criticidad, DVH) VALUES (@idUsuario, @fechaHora, @accion, @descripcion, @ip, @modulo, @nombremaquina, @criticidad, @dvh)", conexion);



                cmdBitacora.Parameters.Add("@idUsuario", SqlDbType.Int).Value = (object)Bitacora.IdUsuario ?? DBNull.Value;
                cmdBitacora.Parameters.Add("@fechaHora", SqlDbType.DateTime).Value = Bitacora.FechaHora;
                cmdBitacora.Parameters.Add("@accion", SqlDbType.VarChar, 50).Value = Bitacora.Accion;
                cmdBitacora.Parameters.Add("@descripcion", SqlDbType.VarChar, 255).Value = Bitacora.Descripcion;
                cmdBitacora.Parameters.Add("@ip", SqlDbType.VarChar, 45).Value = Bitacora.IP;
                cmdBitacora.Parameters.Add("@modulo", SqlDbType.VarChar, 30).Value = Bitacora.Modulo;
                cmdBitacora.Parameters.Add("@nombremaquina", SqlDbType.VarChar, 100).Value = Bitacora.NombreMaquina;
                cmdBitacora.Parameters.Add("@criticidad", SqlDbType.VarChar, 10).Value = Bitacora.Criticidad;
                cmdBitacora.Parameters.Add("@dvh", SqlDbType.VarChar, 1000).Value = (object)Bitacora.DVH ?? DBNull.Value;

                cmdBitacora.ExecuteNonQuery();
            }

        }

        public void ActualizarDVH(int idBitacora, string dvh)
        {
            using (SqlConnection conexion = conex.ObtenerConexion())
            {
                conexion.Open();

                SqlCommand comando = new SqlCommand("Update Bitacora Set DVH=@dvh Where IdBitacora=@id", conexion);

                comando.Parameters.Add("@dvh", SqlDbType.VarChar, 1000).Value = dvh;
                comando.Parameters.Add("@id", SqlDbType.Int).Value = idBitacora;

                comando.ExecuteNonQuery();
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

                    SqlCommand cmdBitacora = new SqlCommand("Select IdBitacora, IdUsuario, FechaHora, Accion, Descripcion, DireccionIP, DVH, Modulo, NombreMaquina, Criticidad From Bitacora", conexion);

                    SqlDataReader Lector = cmdBitacora.ExecuteReader();

                    while (Lector.Read())
                    {
                        BE_Bitacora Bitacora = new BE_Bitacora();
                        Bitacora.IdBitacora = Lector.GetInt32(0);
                        Bitacora.IdUsuario = Lector.IsDBNull(1) ? (int?)null : Lector.GetInt32(1);
                        Bitacora.FechaHora = Lector.GetDateTime(2);

                        Bitacora.Accion = Enum.TryParse(Lector.GetString(3), out AccionBitacora accion)
                            ? accion
                            : AccionBitacora.ACCION_INVALIDA;

                        Bitacora.Descripcion = Lector.IsDBNull(4) ? null : Lector.GetString(4);
                        Bitacora.IP = Lector.IsDBNull(5) ? null : Lector.GetString(5);
                        Bitacora.DVH = Lector.IsDBNull(6) ? null : Lector.GetString(6);
                        Bitacora.Modulo = Lector.IsDBNull(7) ? null : Lector.GetString(7);
                        Bitacora.NombreMaquina = Lector.IsDBNull(8) ? null : Lector.GetString(8);
                        Bitacora.Criticidad = Lector.IsDBNull(9) ? null : Lector.GetString(9);

                        ListaBitacora.Add(Bitacora);
                    }
                }

                return ListaBitacora;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener bitacora: {ex.Message}", ex);
            }

        }

        public DataTable FiltrarBitacora(DateTime? Desde, DateTime? Hasta, int? idUsuario, string Modulo, string Ip, string Criticidad)
        {
            try
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
                    cmdFiltrado.Parameters.AddWithValue("@Criticidad", (object)Criticidad ?? DBNull.Value);

                    using (SqlDataAdapter adaptador = new SqlDataAdapter(cmdFiltrado))
                    {
                        DataTable dt = new DataTable();
                        adaptador.Fill(dt);

                        return dt;
                    }

                }
            }
            catch (Exception ex)
            {

                throw new Exception($"Error al filtrar bitacora: {ex.Message}", ex);
            }

        }
    }
}
