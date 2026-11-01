using System.Data.SqlClient;

namespace DAL
{
    public class DAL_Conexion
    {
        private const string cadenaConexion = "Data Source=.;Initial Catalog=GestionWEB;Integrated Security=True;";

        public SqlConnection ObtenerConexion()
        {
            return new SqlConnection(cadenaConexion);
        }

        public SqlConnection ObtenerConexionMaster()
        {
            SqlConnectionStringBuilder cadena = new SqlConnectionStringBuilder(cadenaConexion) { InitialCatalog = "master" };

            return new SqlConnection(cadena.ConnectionString);
        }
    }
}
