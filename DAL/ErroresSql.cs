using System.Data.SqlClient;

namespace DAL
{
    public static class ErroresSql
    {
        private const int ClaveUnica = 2627;
        private const int IndiceUnico = 2601;
        private const int ClaveForanea = 547;

        public static bool EsClaveDuplicada(SqlException ex)
        {
            return ex.Number == ClaveUnica || ex.Number == IndiceUnico;
        }

        public static bool EsClaveForanea(SqlException ex)
        {
            return ex.Number == ClaveForanea;
        }
    }
}
