using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace AppDynamics.Infrastructure.Framework.Extension.Providers
{
    public class BasicCryptoProvider : ICryptoProvider
    {

        int offset = 20;

        public string EncryptString(string plainText)
        {
            byte[] textBytes = Encoding.ASCII.GetBytes(plainText);

            textBytes = textBytes.Reverse().ToArray();

            for (int i=0; i<textBytes.Length;i++)
            {
                int b = (textBytes[i] < 250) ? textBytes[i] + 3 : textBytes[i];

                textBytes[i] = byte.Parse(b.ToString());
            }

            return Convert.ToBase64String(textBytes);
        }

        public string DecryptString(string encryptedData)
        {
            byte[] encryptedBytes = Convert.FromBase64String(encryptedData);

            byte[] textBytes = new byte[encryptedBytes.Length];

            for (int i = encryptedBytes.Length - 1; i >= 0; i--)
            {
                int b = (encryptedBytes[i] < 250) ? encryptedBytes[i] - 3 : encryptedBytes[i];

                textBytes[encryptedBytes.Length - i - 1] = byte.Parse(b.ToString());
            }

            return Encoding.ASCII.GetString(textBytes);
        }

        public void Dispose()
        {
            
        }
    }
}
