using AppDynamics.Extension.SDK;
using AppDynamics.Extension.SDK.Model;
using AppDynamics.Infrastructure.Framework.Extension;
using AppDynamics.Infrastructure.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AppDynamics.Infrastructure.Extensions
{
    public class WindowsServiceStatusMonitor: AExtensionBase
    {
        private string metricNameServiceStatus = "ServiceStatus";
        private string metricNameServiceUpTimeInMin = "UpTimeInSec";

        public override void Initialize()
        {
            // optional- using current class as logger
            logger = NLog.LogManager.GetLogger(ResourceStrings.ExtensionFrameworkNamespace + "." + ExtensionName);
        }

        public override void Stop()
        {
            logger.Info(String.Format("Stopped-{0}", this.ExtensionName));
        }

        public override bool Execute()
        {
            if (logger.IsTraceEnabled)
                logger.Trace(String.Format("Executing-{0}", this.ExtensionName));

            bool result = false;
            // execute and capture metric/instance values
            try
            {
                foreach (ExtensionInstance einstance in base.ListExtensionInstance)
                {
                    long status = 0;
                    status = getServiceStatus(einstance);

                    einstance.ListExtensionMetrics.
                    Find(m => m.MetricName.Equals
                        (metricNameServiceStatus, StringComparison.CurrentCultureIgnoreCase)).Value = status;

                    long uptime = 0;
                    if (status == 4)
                    {
                        uptime = getServiceUpTime(einstance);
                    }

                    einstance.ListExtensionMetrics.
                    Find(m => m.MetricName.Equals
                        (metricNameServiceUpTimeInMin, StringComparison.CurrentCultureIgnoreCase)).Value = status;

                }
                result = true;
            }
            catch (ExtensionFrameworkException ex)
            {
                logger.Error(String.Format("Error while executing {0}", ExtensionName), ex);
            }

            return result;
        }

        private long getServiceUpTime(ExtensionInstance einstance)
        {
            string serviceName = einstance.InstanceData;

            return (long)(ServiceHelper.GetServiceUpTime(serviceName));
        }

        private long getServiceStatus(ExtensionInstance einstance)
        {
            string serviceName = einstance.InstanceData;

            return ServiceHelper.GetServiceStatusInt(serviceName);
        }

    }
}
