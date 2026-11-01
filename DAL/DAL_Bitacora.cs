using BE;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    public class DAL_Bitacora
    {
        DAL_Conexion conex = new DAL_Conexion();

        public void RegistrarEvento(BE_Bitacora bitacora)
        {
            using (SqlConnection conexion = conex.ObtenerConexion())
            {
                conexion.Open();

                SqlCommand cmdBitacora = new SqlCommand("Insert into Bitacora (IdUsuario, FechaHora, Accion, Descripcion, DireccionIP, Modulo, NombreMaquina, Criticidad, DVH) VALUES (@idUsuario, @fechaHora, @accion, @descripcion, @ip, @modulo, @nombremaquina, @criticidad, @dvh)", conexion);

                cmdBitacora.Parameters.Add("@idUsuario", SqlDbType.Int).Value = (object)bitacora.IdUsuario ?? DBNull.Value;
                cmdBitacora.Parameters.Add("@fechaHora", SqlDbType.DateTime).Value = bitacora.FechaHora;
                cmdBitacora.Parameters.Add("@accion", SqlDbType.VarChar, 50).Value = bitacora.Accion;
                cmdBitacora.Parameters.Add("@descripcion", SqlDbType.VarChar, 255).Value = bitacora.Descripcion;
                cmdBitacora.Parameters.Add("@ip", SqlDbType.VarChar, 45).Value = bitacora.IP;
                cmdBitacora.Parameters.Add("@modulo", SqlDbType.VarChar, 30).Value = bitacora.Modulo;
                cmdBitacora.Parameters.Add("@nombremaquina", SqlDbType.VarChar, 100).Value = bitacora.NombreMaquina;
                cmdBitacora.Parameters.Add("@criticidad", SqlDbType.VarChar, 10).Value = bitacora.Criticidad;
                cmdBitacora.Parameters.Add("@dvh", SqlDbType.VarChar, 1000).Value = (object)bitacora.DVH ?? DBNull.Value;

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
                List<BE_Bitacora> listaBitacora = new List<BE_Bitacora>();

                using (SqlConnection conexion = conex.ObtenerConexion())
                {
                    conexion.Open();

                    SqlCommand cmdBitacora = new SqlCommand("Select IdBitacora, IdUsuario, FechaHora, Accion, Descripcion, DireccionIP, DVH, Modulo, NombreMaquina, Criticidad From Bitacora", conexion);

                    using (SqlDataReader lector = cmdBitacora.ExecuteReader())
                    {
                        while (lector.Read())
                        {
                            BE_Bitacora bitacora = new BE_Bitacora();
                            bitacora.IdBitacora = lector.GetInt32(0);
                            bitacora.IdUsuario = lector.IsDBNull(1) ? (int?)null : lector.GetInt32(1);
                            bitacora.FechaHora = lector.GetDateTime(2);

                            bitacora.Accion = Enum.TryParse(lector.GetString(3), out AccionBitacora accion)
                                ? accion
                                : AccionBitacora.ACCION_INVALIDA;

                            bitacora.Descripcion = lector.IsDBNull(4) ? null : lector.GetString(4);
                            bitacora.IP = lector.IsDBNull(5) ? null : lector.GetString(5);
                            bitacora.DVH = lector.IsDBNull(6) ? null : lector.GetString(6);
                            bitacora.Modulo = lector.IsDBNull(7) ? null : lector.GetString(7);
                            bitacora.NombreMaquina = lector.IsDBNull(8) ? null : lector.GetString(8);
                            bitacora.Criticidad = lector.IsDBNull(9) ? null : lector.GetString(9);

                            listaBitacora.Add(bitacora);
                        }
                    }
                }

                return listaBitacora;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener bitacora: {ex.Message}", ex);
            }
        }

        public DataTable FiltrarBitacora(DateTime? desde, DateTime? hasta, int? idUsuario, string modulo, string ip, string criticidad)
        {
            try
            {
                using (SqlConnection conexion = conex.ObtenerConexion())
                {
                    conexion.Open();
                    SqlCommand cmdFiltrado = new SqlCommand("dbo.FiltrarBitacora", conexion);

                    cmdFiltrado.CommandType = CommandType.StoredProcedure;

                    cmdFiltrado.Parameters.AddWithValue("@Desde", (object)desde ?? DBNull.Value);
                    cmdFiltrado.Parameters.AddWithValue("@Hasta", (object)hasta ?? DBNull.Value);
                    cmdFiltrado.Parameters.AddWithValue("@IdUsuario", (object)idUsuario ?? DBNull.Value);
                    cmdFiltrado.Parameters.AddWithValue("@Modulo", (object)modulo ?? DBNull.Value);
                    cmdFiltrado.Parameters.AddWithValue("@IP", (object)ip ?? DBNull.Value);
                    cmdFiltrado.Parameters.AddWithValue("@Criticidad", (object)criticidad ?? DBNull.Value);

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
