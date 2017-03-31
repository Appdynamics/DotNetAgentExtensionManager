using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AppDynamics.Infrastructure.Helper
{
    public class RegistryHelper
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public static string GetorDefault(string registryKeyPath, string registryKeyName, string defaultValue)
        {
            _logger.Trace(String.Format("Reading registry value reg key={0}, path={1}, default={2}", registryKeyPath, registryKeyName, defaultValue));

            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registryKeyPath))
            {
                if (key != null)
                {
                    Object o = key.GetValue(registryKeyName, defaultValue);

                    if (o != null)
                        defaultValue = o.ToString();
                    else
                        _logger.Trace("key not found. name=" + registryKeyName);
                }
                else
                {
                    _logger.Trace("Key not found. path=" + registryKeyPath);
                }
            }
            return defaultValue;
        }
    }
}
