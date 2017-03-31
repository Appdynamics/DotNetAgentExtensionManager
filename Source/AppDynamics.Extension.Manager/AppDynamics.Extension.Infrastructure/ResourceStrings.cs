using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AppDynamics.Infrastructure
{
    public class ResourceStrings
    {

        internal const string DotNetAgentRegistryKey = @"SOFTWARE\AppDynamics\dotNet Agent";
        internal const string InstallationDirRegistryName = "InstallationDir";
        internal const string DotNetAgentFolderRegistryName = "DotNetAgentFolder";

        internal const string DotNetAgentFolderLocation = @"AppDynamics\DotNetAgent";
        internal const string ConfigFileFolder = "config";
        internal const string ConfigFileName = "config.xml";

        internal const string ExtensionServiceEXEName = "ExtensionService.exe";
        internal const string ExtensionServiceName = "AppDynamics.Agent.Extension_Service";
        internal const string CustomCountercategoryDescription = "Performance counter category to store custom performance counters for appdynamics extensions.";


        public static string UninstallStr { get { return "UnInstall"; } }
        public static string InstallStr { get { return "Install"; } }
        public static string StartStr { get { return "Start"; } }
        public static string StopStr { get { return "Stop"; } }

        /// <summary>
        /// Contains full path to config.xml file. Supports custom locations.
        /// </summary>
        public static string AgentConfigFullPath
        {
            get
            {
                //TODO: Need to update to get correct file if config location is changed.

                string defaultPath= 
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) +
                    System.IO.Path.DirectorySeparatorChar +
                    ResourceStrings.DotNetAgentFolderLocation +
                    System.IO.Path.DirectorySeparatorChar;

                string path = AppDynamics.Infrastructure.Helper.RegistryHelper.
                    GetorDefault(DotNetAgentRegistryKey, DotNetAgentFolderRegistryName, defaultPath);

                return path +
                    ResourceStrings.ConfigFileFolder +
                    System.IO.Path.DirectorySeparatorChar +
                    ResourceStrings.ConfigFileName;
            }
        }

        public static string ExtensionDirPath
        {
            get
            {
                return AppDomain.CurrentDomain.BaseDirectory + System.IO.Path.DirectorySeparatorChar + "extensions";
            }
        }

        /// <summary>
        /// Contains friendly name and type string for internal extensions.
        /// </summary>
        public static IDictionary<string, string> InternalExtensions
        {
            get
            {
                IDictionary<string, string> exts = new Dictionary<string, string>();

                // For more internal or inbuilt extensions #internalextension #inbuiltextension 
                exts.Add(InternalExtensionFriendlyNameWindowsEventLogMonitor,
                    "AppDynamics.Infrastructure.Extensions.WindowsEventLogMonitor");
                exts.Add(InternalExtensionFriendlyNameWindowsRebootMonitor,
                    "AppDynamics.Infrastructure.Extensions.WindowsRebootMonitor");
                exts.Add("script",
                    "AppDynamics.Infrastructure.Extensions.PeriodicScriptMetricExtension");
                exts.Add("WindowsServiceStatusMonitor",
                    "AppDynamics.Infrastructure.Extensions.WindowsServiceStatusMonitor");

                return exts;
            }
        }

        public static string InternalExtensionFriendlyNameWindowsEventLogMonitor { get { return "WindowsEventLogMonitor"; } }
        public static string InternalExtensionFriendlyNameWindowsRebootMonitor { get { return "WindowsRebootMonitor"; } }
        public static string InternalExtensionFriendlyNameWindowsServiceStatusMonitor { get { return "WindowsServiceStatusMonitor"; } }


        public static string HeadingExtensionService { get { return "AppDynamics Extension Service for DotNet Agent"; } }

        public static string NotInstalledtext { get { return "Not Installed"; } }

        public static string ServiceinstallErrorMessage { get { return "Sorry, Could not install the service."; } }

        public static string ServiceinstalledMessage { get { return "AppDynamics Service installed successfully."; } }

        public static string ServiceUnInstalledMessage { get { return "AppDynamics Service Un-installed successfully."; } }

        public static string ExtServiceStoppingMessage { get { return "AppDynamics dot net agent extension service is stopping. Please wait for the service to Stop completely."; } }

        public static string ExtServiceStartedMessage { get { return "AppDynamics dot net agent extension service started successfully."; } }

        public static string ExtServiceInstalledMessage { get { return "AppDynamics dot net agent extension service installed successfully."; } }

        public static string ExtServiceUnInstalledMessage { get { return "AppDynamics dot net agent extension service uninstalled successfully."; } }

        public static string ServiceStatusRunning { get { return "Running"; } }

        public static string ServiceManageErrorMessage { get { return "Sorry, Could not {0} the extension service."; } }

        public static string SuccessMessage { get { return "Operation completed successfully."; } }

        public static string ExtensionXmlName { get { return "extension.xml"; } }

        public static string ExtensionDescription { get { return "Appdynamics Extension object"; } }

        public static string ExtensionMetricDescription { get { return "Appdynamics Extension Metric"; } }

        public static string TargetNamespaceInXSD { get { return "AppDynamics.DotNetAgent.Extension"; } }

        public static string ExtensionFrameworkNamespace { get { return "AppDynamics.Extension.SDK"; } }

        public static string AppDynamicsAgentCoordinatorServiceName { get { return "AppDynamics.Agent.Coordinator_service"; } }

        public static string XMLParamName { get { return "XMLPATH"; } }

        /// <summary>
        /// This is default line format for script- "metric1 | instance1, value = 32350"
        /// </summary>
        public static string LineFormatScriptExtension { get { return @"#MetricName# | #InstanceName#, value = #Value#"; } }


        public static string PerfCounterRootName { get { return "perf-counters"; } }
        public static string PerfCounterElementName { get { return "perf-counter"; } }
        public static string PerfCounterXmlTemplate { get { return "<perf-counter cat=\"{0}\" name=\"{1}\" instance=\"{2}\"/>"; } }


        public static string CurrentVersion
        {
            get
            {
                return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

    }
}
