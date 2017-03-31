using System;
using System.Collections.Generic;
using System.Configuration.Install;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;

namespace ExtensionService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        ///     
        [DllImport("kernel32.dll")]
        static extern bool AttachConsole(int dwProcessId);
        private const int ATTACH_PARENT_PROCESS = -1;
        static void Main(string[] args)
        {
            //string [] args1 = {"-encrypt", "MyPassword"};

            if (args.Length > 0)
            {
                AttachConsole(ATTACH_PARENT_PROCESS);
                AppDynamics.Infrastructure.Framework.Extension.Providers.CommandListner.HandleArgs(args);
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[] 
                { 
                    new AppDynamics_Extension_Service() 
                };
                ServiceBase.Run(ServicesToRun);
            }
        }

    }
}
