using AppDynamics.Extension.SDK;
using AppDynamics.Infrastructure.Framework.Extension;
using AppDynamics.Infrastructure.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AppDynamics.Infrastructure.Extensions
{
    public class PerformanceCounterExtension: AExtensionBase
    {

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

            // execute and capture metric/instance values
            try
            {
                executeScript();

                return true;
            }
            catch (ExtensionFrameworkException ex)
            {
                logger.Error(String.Format("Error while executing {0}", ExtensionName), ex);
                return false;
            }
        }

        private void executeScript()
        {

        }

    }
}
