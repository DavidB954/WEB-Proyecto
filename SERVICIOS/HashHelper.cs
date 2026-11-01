using System.Security.Cryptography;
using System.Text;

namespace SERVICIOS
{
    public class HashHelper
    {
        public static string GenerarHash(string texto)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(texto));

                StringBuilder constructor = new StringBuilder();

                foreach (byte b in bytes)
                {
                    constructor.Append(b.ToString("x2"));
                }

                return constructor.ToString();
            }
        }
    }
}
