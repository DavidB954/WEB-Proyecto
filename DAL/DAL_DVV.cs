using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class DAL_DVV
    {
        DAL_Conexion conex = new DAL_Conexion();

        public string ObtenerDVV(string NombreTabla)
        {
            try
            {
                using (SqlConnection conexion = conex.ObtenerConexion())
                {
                    conexion.Open();

                    SqlCommand cmdObtenerDVV = new SqlCommand("Select DVV from DigitoVerificadorVertical Where NombreTabla=@tabla", conexion);

                    cmdObtenerDVV.Parameters.Add("@tabla", SqlDbType.VarChar, 50).Value = NombreTabla;

                    object result = cmdObtenerDVV.ExecuteScalar();

                    return result?.ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
         
        }

        public void ActualizarDVV(string DVV, string NombreTabla)
        {
            try
            {
                using (SqlConnection conexion = conex.ObtenerConexion())
                {
                    conexion.Open();

                    SqlCommand cmdDVV = new SqlCommand("UPDATE DigitoVerificadorVertical SET DVV=@DVV, FechaActualizacion=GETDATE() WHERE NombreTabla=@tabla", conexion);

                    cmdDVV.Parameters.Add("@DVV", SqlDbType.VarChar, 255).Value = DVV;
                    cmdDVV.Parameters.Add("@tabla", SqlDbType.VarChar, 50).Value = NombreTabla;
                    cmdDVV.ExecuteNonQuery();

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
           
        }

        public void GenerarBackUp(string rutaBackup)
        {
            try
            {
                using (SqlConnection conexion = conex.ObtenerConexion())
                {
                    conexion.Open();

                    SqlCommand cmd = new SqlCommand($"BACKUP DATABASE GestionWEB TO DISK = '{rutaBackup}'", conexion);

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
          
        }

        public void RestaurarBackup(string rutaBackup)
        {
            try
            {
                SqlConnection.ClearAllPools();

                string masterConnectionString = "Data Source=.;Initial Catalog=master;Integrated Security=True";

                using (SqlConnection conexion = new SqlConnection(masterConnectionString))
                {
                    conexion.Open();

                    //Cerramos las conexiones existentes
                    SqlCommand cmdSingleUser = new SqlCommand("ALTER DATABASE GestionWEB SET SINGLE_USER WITH ROLLBACK IMMEDIATE", conexion);

                    cmdSingleUser.ExecuteNonQuery();

                    //Restauramos con REPLACE

                    SqlCommand cmdRestauracion = new SqlCommand($"RESTORE DATABASE GestionWEB FROM DISK = '{rutaBackup}' WITH REPLACE", conexion);

                    cmdRestauracion.ExecuteNonQuery();

                    //Volvemos a multiuser
                    SqlCommand cmdMultiUser = new SqlCommand("ALTER DATABASE GestionWEB SET MULTI_USER", conexion);

                    cmdMultiUser.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
           
        }
    }
}
