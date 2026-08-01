using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SERVICIOS
{
    public class HashHelper
    {
        public static string GenerarHash(string Password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {

                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(Password));

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
