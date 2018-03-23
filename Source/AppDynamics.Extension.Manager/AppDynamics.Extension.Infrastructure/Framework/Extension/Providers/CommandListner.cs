using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace AppDynamics.Infrastructure.Framework.Extension.Providers
{
    public static class CommandListner
    {
        //TODO: Need to organize this class, if more functionality to be added
        public static void HandleArgs(string[] args)
        {
            if (args.Contains("-encrypt"))
            {
                HandleEncryption(args);
            }
        }

        private static void HandleEncryption(string[] args)
        {
            string pwd = "";

            for (int i = 0; i <= args.Length - 2; i++)
            {
                if ("-encrypt".Equals(args[i], StringComparison.CurrentCultureIgnoreCase))
                {
                    pwd = args[i + 1];
                }
            }
            if (String.IsNullOrWhiteSpace(pwd))
            {
                Console.WriteLine("Please provide text for encryption in following format-");
                Console.WriteLine("ExtensionService.exe -encrypt Myp@ssw0rd");
            }
            else
            {
                Console.WriteLine("Encrypting password -> " + pwd);
            }
            ICryptoProvider cprovider = CryptoProviderFactory.GetCryptoProvider();

            string encrypted = cprovider.EncryptString(pwd);

            Console.WriteLine(encrypted);
        }
        
    }
}
