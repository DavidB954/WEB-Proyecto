using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class DAL_Conexion
    {
        private static string connetionString = " Data Source=.;Initial Catalog=GestionWEB ;Integrated Security= True;";
    

    public SqlConnection ObtenerConexion()
        {
            return new SqlConnection(connetionString);
        }
    }
}