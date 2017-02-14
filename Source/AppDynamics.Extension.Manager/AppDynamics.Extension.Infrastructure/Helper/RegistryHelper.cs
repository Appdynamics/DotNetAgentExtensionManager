using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AppDynamics.Infrastructure.Helper
{
    public class RegistryHelper
    {

        public static string GetorDefault(string registryKeyPath, string registryKeyName, string defaultValue)
        {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registryKeyPath))
            {
                if (key != null)
                {
                    Object o = key.GetValue(registryKeyName, defaultValue);

                    if (o != null)
                        defaultValue = o.ToString();
                }
            }
            return defaultValue;
        }
    }
}
