using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppDynamics.Infrastructure.Framework.Extension.Providers
{
    public static class CryptoProviderFactory
    {
        public static ICryptoProvider GetCryptoProvider()
        {
            ICryptoProvider cp = null;

            if (ConfigurationManager.AppSettings["Encryption"] == null)
                return new RijndaelCryptoProvider();

            switch (ConfigurationManager.AppSettings["Encryption"].ToLower())
            {
                case "basic":
                    {
                        cp = new BasicCryptoProvider();
                        break;
                    }
                case "windows":
                    {
                        cp = new CredManagerCryptoProvider();
                        break;
                    }
                case "environment":
                    {
                        cp = new EnvironmentVariableCryptoProvider();
                        break;
                    }
                default:
                    {
                        cp = new RijndaelCryptoProvider();
                        break;
                    }
            }
            return cp;
        }
    }
}
