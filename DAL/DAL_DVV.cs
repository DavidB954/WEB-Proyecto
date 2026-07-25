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
            //Sin catch: un fallo de conexión debe propagarse como error real (BLL_DVV ya lo envuelve),
            //no enmascararse como "null" y hacerse pasar por una violación de integridad.
            using (SqlConnection conexion = conex.ObtenerConexion())
            {
                conexion.Open();

                SqlCommand cmdObtenerDVV = new SqlCommand("Select DVV from DigitoVerificadorVertical Where NombreTabla=@tabla", conexion);

                cmdObtenerDVV.Parameters.Add("@tabla", SqlDbType.VarChar, 50).Value = NombreTabla;

                object result = cmdObtenerDVV.ExecuteScalar();

                return result?.ToString();
            }
        }

        public void ActualizarDVV(string DVV, string NombreTabla)
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

        //Lista de IDs aceptados como válidos para una tabla (hoy solo se usa para Bitacora), guardada la última vez
        //que se ejecutó "Recalcular DV". Sirve de base para detectar borrados puntuales sin re-marcar para siempre
        //huecos que el webmaster ya aceptó.
        public string ObtenerIdsVigentes(string nombreTabla)
        {
            using (SqlConnection conexion = conex.ObtenerConexion())
            {
                conexion.Open();

                SqlCommand cmd = new SqlCommand("Select IdsVigentes from DigitoVerificadorVertical Where NombreTabla=@tabla", conexion);
                cmd.Parameters.Add("@tabla", SqlDbType.VarChar, 50).Value = nombreTabla;

                object result = cmd.ExecuteScalar();

                return result == null || result == DBNull.Value ? null : result.ToString();
            }
        }

        public void ActualizarIdsVigentes(string nombreTabla, string idsVigentesCsv)
        {
            using (SqlConnection conexion = conex.ObtenerConexion())
            {
                conexion.Open();

                SqlCommand cmd = new SqlCommand("UPDATE DigitoVerificadorVertical SET IdsVigentes=@ids WHERE NombreTabla=@tabla", conexion);
                cmd.Parameters.Add("@ids", SqlDbType.NVarChar, -1).Value = (object)idsVigentesCsv ?? DBNull.Value;
                cmd.Parameters.Add("@tabla", SqlDbType.VarChar, 50).Value = nombreTabla;
                cmd.ExecuteNonQuery();
            }
        }

        //Carpeta de backups propia del motor de SQL Server (ej. ...\MSSQL\Backup): es la única que se garantiza
        //accesible para la cuenta de servicio de SQL Server (NT Service\MSSQLSERVER), sin depender de permisos
        //de carpetas arbitrarias que el usuario podría escribir a mano.
        public string ObtenerCarpetaBackupPorDefecto()
        {
            using (SqlConnection conexion = conex.ObtenerConexion())
            {
                conexion.Open();

                SqlCommand cmd = new SqlCommand("SELECT CAST(SERVERPROPERTY('InstanceDefaultBackupPath') AS NVARCHAR(500))", conexion);

                return (string)cmd.ExecuteScalar();
            }
        }

        //Lista los .bak de la carpeta de backups usando xp_dirtree: corre del lado del motor de SQL Server,
        //así la app no necesita permisos de archivo sobre esa carpeta para poder listarla.
        public List<string> ListarBackups()
        {
            string carpeta = ObtenerCarpetaBackupPorDefecto();

            using (SqlConnection conexion = conex.ObtenerConexion())
            {
                conexion.Open();

                SqlCommand cmd = new SqlCommand("EXEC master.sys.xp_dirtree @carpeta, 1, 1", conexion);
                cmd.Parameters.Add("@carpeta", SqlDbType.NVarChar, 500).Value = carpeta;

                var archivos = new List<string>();

                using (SqlDataReader lector = cmd.ExecuteReader())
                {
                    while (lector.Read())
                    {
                        string nombre = lector.GetString(0);
                        bool esArchivo = lector.GetInt32(2) == 1;

                        if (esArchivo && nombre.EndsWith(".bak", StringComparison.OrdinalIgnoreCase))
                        {
                            archivos.Add(nombre);
                        }
                    }
                }

                return archivos;
            }
        }

        public void GenerarBackUp(string rutaBackup)
        {

            using (SqlConnection conexion = conex.ObtenerConexion())
            {
                conexion.Open();

                //Ruta parametrizada (evita inyección) y timeout amplio: un backup puede tardar.
                SqlCommand cmd = new SqlCommand("BACKUP DATABASE GestionWEB TO DISK = @ruta", conexion);
                cmd.Parameters.AddWithValue("@ruta", rutaBackup);
                cmd.CommandTimeout = 120;

                cmd.ExecuteNonQuery();
            }
        }

        public void RestaurarBackup(string rutaBackup)
        {
            SqlConnection.ClearAllPools();

            string masterConnectionString = "Data Source=.;Initial Catalog=master;Integrated Security=True";

            using (SqlConnection conexion = new SqlConnection(masterConnectionString))
            {
                conexion.Open();

                //Cerramos las conexiones existentes
                SqlCommand cmdSingleUser = new SqlCommand("ALTER DATABASE GestionWEB SET SINGLE_USER WITH ROLLBACK IMMEDIATE", conexion);
                cmdSingleUser.ExecuteNonQuery();

                try
                {
                    //Restauramos con REPLACE (ruta parametrizada)
                    SqlCommand cmdRestauracion = new SqlCommand("RESTORE DATABASE GestionWEB FROM DISK = @ruta WITH REPLACE", conexion);
                    cmdRestauracion.Parameters.AddWithValue("@ruta", rutaBackup);
                    cmdRestauracion.CommandTimeout = 120;
                    cmdRestauracion.ExecuteNonQuery();
                }
                finally
                {
                    //Pase lo que pase (incluso si el RESTORE falla), devolvemos la base a multiusuario para no dejarla bloqueada.
                    SqlCommand cmdMultiUser = new SqlCommand("ALTER DATABASE GestionWEB SET MULTI_USER", conexion);
                    cmdMultiUser.ExecuteNonQuery();
                }
            }
        }
    }
}
