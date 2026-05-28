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
            //SHA256.Create() Crea un objeto que implementa el algoritmo de HASH256. 
            using (SHA256 sha256 = SHA256.Create())
            {

                //Convierte el string en bytes. Como resultado se obtiene un array de bytes.
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(Password));

                StringBuilder constructor = new StringBuilder();

                //Por cada byte del array, convierte a string en formato hexadecimal (x) y con dos digitos (2). Y lo agrega al constructor.
                foreach (byte b in bytes)
                {
                    constructor.Append(b.ToString("x2"));
                }


                //Devuelve el hash como string.
                return constructor.ToString();
            }
        }
    }
}
