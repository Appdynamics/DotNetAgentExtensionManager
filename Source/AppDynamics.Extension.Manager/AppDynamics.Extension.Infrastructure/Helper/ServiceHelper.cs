using AppDynamics.Infrastructure.Framework.Extension;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration.Install;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.ServiceProcess;
using System.Text;

namespace AppDynamics.Infrastructure.Helper
{
    public class ServiceHelper
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public static ExtensionFrameworkException ManageExtensionService(string serviceCommand)
        {
            ServiceController sc = new ServiceController(ResourceStrings.ExtensionServiceName);

            ExtensionFrameworkException error = null;
            try
            {
                switch (serviceCommand.ToLower())
                {
                    case "install":
                        InstallorUninstallExtensionService(true);
                        break;

                    case "uninstall":
                        InstallorUninstallExtensionService(false);
                        break;

                    case "start":
                        StartService(sc);
                        break;

                    case "stop":
                        StopService(sc);
                        break;

                    case "restart":
                        if (sc.CanStop)
                        {
                            StopService(sc);
                            StartService(sc);
                        }
                        break;
                }
            }
            catch (System.ServiceProcess.TimeoutException ex)
            {
                error = new ExtensionFrameworkException(
                    String.Format("Failed to {0} extension service..", serviceCommand), ex);
            }
            catch (ExtensionFrameworkException ex)
            {
                error = ex;
            }
            catch (InvalidOperationException ex)
            {
                error = new ExtensionFrameworkException(
                    String.Format("Invalid operation- {0}> {1}", ResourceStrings.ExtensionServiceName, ex.Message));
            }
            catch (Exception ex)
            {
                error = new ExtensionFrameworkException(
                    String.Format("Failed to {0} extension service..", serviceCommand), ex);
            }

            if (error != null)
            {
                _logger.Error(error.Message, error);
            }

            return error;
        }

        public static bool IsExtensionServiceInstalled()
        {
            return IsServiceInstalled(ResourceStrings.ExtensionServiceName);
        }

        public static string GetExtensionServiceStatus()
        {
            return GetServiceStatusString(ResourceStrings.ExtensionServiceName);
        }

        public static void RestartExtensionService()
        {
            ManageExtensionService("stop");
            //Hack: adding 200 ms sleep to make sure service is stopped before trying to start.
            System.Threading.Thread.Sleep(200);
            ManageExtensionService("start");
        }

        /// <summary>
        /// Throws exception 
        /// </summary>
        public static void RestartAppDynamicsAgentCoordinatorService()
        {

            ServiceController sc = new ServiceController(ResourceStrings.AppDynamicsAgentCoordinatorServiceName);

            if (sc.CanStop)
            {
                sc.Stop();
            }
            sc.WaitForStatus(ServiceControllerStatus.Stopped);

            sc.Start();
        }

        public static bool IsAgentServiceInstalled
        {
            get
            {
                return IsServiceInstalled(ResourceStrings.AppDynamicsAgentCoordinatorServiceName);
            }
        }

        #region Private methods

        private static void InstallorUninstallExtensionService(bool install)
        {
            string fileName = FileHelper.GetAbsolutePath(ResourceStrings.ExtensionServiceEXEName);

            if (FileHelper.FileExists(fileName))
            {
                if (install)
                    InstallService(fileName);
                else
                    UnInstallService(fileName);
            }
            else
            {
                throw new ExtensionFrameworkException("Could not locate extension service exe- " + fileName);
            }
        }

        private static void StartService(ServiceController sc)
        {

            if (sc.Status == ServiceControllerStatus.Stopped)
            {
                sc.Start();
                sc.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, 0, 10));
            }
            else
            {
                string m = "Can not start Extension service now. Extension Service is {0}. " +
                                    "Please wait for the service to Stop.";

                throw new ExtensionFrameworkException(String.Format(m, sc.Status));
            }
        }

        private static void StopService(ServiceController sc)
        {
            if (sc.CanStop)
            {
                sc.Stop();

                // Do not wait for status, it takes considerable amount of time.== frequency default 55 sec
                //sc.WaitForStatus(ServiceControllerStatus.Stopped);
            }
        }


        private static bool InstallService(string fullFileName)
        {
            IDictionary savedState = new Hashtable();

            using (var ai = new AssemblyInstaller(fullFileName, null))
            {
                savedState.Clear();

                ai.Install(null);
                ai.Commit(null);
                return true;
            }

        }

        private static bool UnInstallService(string fullFileName)
        {

            IDictionary savedState = new Hashtable();
            try
            {
                using (var ai = new AssemblyInstaller(fullFileName, null))
                {
                    ai.Uninstall(savedState);
                    ai.Commit(savedState);
                    return true;
                }
            }
            catch (System.IO.FileNotFoundException ex)
            {
                if (ex.Message.Contains(".InstallState"))
                {
                    // No need to throw the exception, it will not stop the uninstallation.
                    return true;
                }
                else
                {
                    throw;
                }
            }

        }

        public static bool IsServiceInstalled(string serviceName)
        {
            bool serviceExists = ServiceController.GetServices().Any(s => s.ServiceName == serviceName);

            return serviceExists;
        }

        public static string GetServiceStatusString(string serviceName)
        {
            string status = ResourceStrings.NotInstalledtext;

            if (IsServiceInstalled(serviceName))
            {
                status = getServiceStatus(serviceName).ToString();
            }
            return status;
        }

        public static int GetServiceStatusInt(string serviceName)
        {
            int status = -1;

            if (IsServiceInstalled(serviceName))
            {
                status = (int)getServiceStatus(serviceName);
            }

            return status;
        }

        private static ServiceControllerStatus getServiceStatus(string serviceName)
        {
            ServiceControllerStatus status = ServiceControllerStatus.Stopped;

            var service = ServiceController.GetServices().First(s => s.ServiceName == serviceName);

            if (service != null)
            {
                status = service.Status;
            }

            //if (_logger.IsTraceEnabled)
            //    _logger.Trace(String.Format("status for service {0} = {1}", serviceName, status.ToString()));

            return status;
        }

        #endregion



        public static double GetServiceUpTime(string serviceName)
        {
            double totalSecStartUp = 0;

            SelectQuery query = new SelectQuery("Select ProcessId from Win32_Service WHERE Name=\"" + serviceName + "\"");

            int pid = 0;

            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);

            foreach (ManagementObject o in searcher.Get())
            {
                pid = int.Parse(o["ProcessId"].ToString());
                break;
            }

            if (pid != 0)
            {
                Process p = Process.GetProcessById(pid);

                DateTime startTime = p.StartTime;

                totalSecStartUp = (DateTime.Now - startTime).TotalSeconds;

                p.Dispose();
                p.Close();
            }

            return totalSecStartUp;
        }
    }
}
