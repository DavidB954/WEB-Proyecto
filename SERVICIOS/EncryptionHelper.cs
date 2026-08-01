using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SERVICIOS
{
    public class EncryptionHelper
    {
        private static readonly byte[] Clave = Encoding.UTF8.GetBytes("GestionWeb_DVH_Key_32Bytes!!!!!!");
        private static readonly byte[] VI = Encoding.UTF8.GetBytes("GestionWebIV1234");

        public static string Encriptar(string texto)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Clave;
                aes.IV = VI;

                using (ICryptoTransform encriptador = aes.CreateEncryptor(aes.Key, aes.IV))
                using (MemoryStream memoria = new MemoryStream())
                {
                    using (CryptoStream cripto = new CryptoStream(memoria, encriptador, CryptoStreamMode.Write))
                    using (StreamWriter escritor = new StreamWriter(cripto))
                    {
                        escritor.Write(texto);
                    }

                    return Convert.ToBase64String(memoria.ToArray());
                }
            }
        }

        public static string Desencriptar(string textoCifrado)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Clave;
                aes.IV = VI;

                using (ICryptoTransform desencriptador = aes.CreateDecryptor(aes.Key, aes.IV))
                using (MemoryStream memoria = new MemoryStream(Convert.FromBase64String(textoCifrado)))
                using (CryptoStream cripto = new CryptoStream(memoria, desencriptador, CryptoStreamMode.Read))
                using (StreamReader lector = new StreamReader(cripto))
                {
                    return lector.ReadToEnd();
                }
            }
        }
    }
}
