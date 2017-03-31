using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace AppDynamics.Infrastructure.Framework.Extension.Providers
{
    public class RijndaelCryptoProvider : ICryptoProvider
    {
        //TODO: Need to improve this by adding dynamic salt. #NotImp
        private static byte[] _pepper = Encoding.ASCII.GetBytes("H@r h@R m@H@d3V~H@r h@R m@H@d3V$");
        private static string _name = "AppDynamics .NET Agent Extension";
        private static byte[] बेतरतीब = Encoding.ASCII.GetBytes("b~8e}6F|hvLw9U$#");

        public RijndaelCryptoProvider()
        {

        }



        public string EncryptString(string plainText)
        {
            byte[] cipherTextBytes;

            using (RijndaelManaged rijAlg = new RijndaelManaged())
            {
                rijAlg.Key = _pepper;
                rijAlg.IV = बेतरतीब;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);

                using (var memoryStream = new MemoryStream())
                {
                    using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(cryptoStream))
                        {
                            swEncrypt.Write(plainText);
                        }
                        cipherTextBytes = memoryStream.ToArray();
                    }
                }
            }
            return Convert.ToBase64String(cipherTextBytes);
        }

        public string DecryptString(string encryptedData)
        {
            byte[] encryptedBytes = Convert.FromBase64String(encryptedData);

            return DecryptStringFromBytes(encryptedBytes);
        }


        static string DecryptStringFromBytes(byte[] cipherText)
        {
            string plaintext = null;

            using (RijndaelManaged rijAlg = new RijndaelManaged())
            {
                rijAlg.Key = _pepper;
                rijAlg.IV = बेतरतीब;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }

            }

            return plaintext;

        }


        public void Dispose()
        {
            
        }
    }
}
