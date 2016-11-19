using AppDynamics.Infrastructure.Framework.Extension;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;

namespace ExtensionService
{
    partial class AppDynamics_Extension_Service : ServiceBase
    {

        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        ExtensionLoader exeLoader = null;

        public AppDynamics_Extension_Service()
        {
            InitializeComponent();

            exeLoader = new ExtensionLoader();

            _logger.Info("Extensions Initialized- now starting.");

        }

        protected override void OnStart(string[] args)
        {
            _logger.Info("Extensions service - starting. Version"+AppDynamics.Infrastructure.ResourceStrings.CurrentVersion);

            try
            {
                exeLoader.CreateExtensions();

                _logger.Info("Extensions created now starting.");

                exeLoader.StartExtensions();

                _logger.Info("Extensions service running.");
            }
            catch (ExtensionFrameworkException ex)
            {
                _logger.Error("Could not start extensions.", ex);
            }
            catch (Exception ex)
            {
                // Unexpected exception, needs to be thrown
                _logger.Error(ex, "Could not start extensions.");
                throw ex;
            }
        }

        protected override void OnStop()
        {
            // Deleting category to avoid false values.. 
            // IMP- deleting category can cause issues with agent.
            exeLoader.StopExtensions();

            _logger.Debug("Stopped service..");
        }
    }
}

