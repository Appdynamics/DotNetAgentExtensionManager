using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AppDynamics.Infrastructure.Framework.Extension.Providers
{
    public interface ICryptoProvider : IDisposable
    {
        string EncryptString(string plainText);

        string DecryptString(string encryptedData);
    }
}
