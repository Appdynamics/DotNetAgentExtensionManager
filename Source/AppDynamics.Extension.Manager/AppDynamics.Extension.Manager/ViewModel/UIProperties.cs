using AppDynamics.Infrastructure;
using AppDynamics.Infrastructure.Framework.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AppDynamics.Extension.Manager.ViewModel
{
    public class UIProperties : Presenter
    {
        public string HeadingExtensionService
        {
            get { return ResourceStrings.HeadingExtensionService; }
        }

        public string ButtonTextRestartCoordinator
        {
            get { return "Apply Changes"; }
        }

        public string ButtonTextOpenCCT
        {
            get { return "Configure Windows Performance Counters"; }
        }

        

        internal static string executionTypeForDLL = "DLL";
        // For future internal extensions based on type 
        // #inbuiltextension & #internalextension
        internal static string executionTypeForEvents = String.Format("{0},{1},{2}"
            , executionTypeForDLL
            , ResourceStrings.InternalExtensionFriendlyNameWindowsEventLogMonitor
            , ResourceStrings.InternalExtensionFriendlyNameWindowsRebootMonitor);

        internal static string executionTypeForMetrics = String.Format("{0},Script,{1}",
            executionTypeForDLL ,
            ResourceStrings.InternalExtensionFriendlyNameWindowsServiceStatusMonitor);

        public static string HelpTextExecutionPathforDLL { get; set; }

        public static string HelpTextExecutionPathforScript { get; set; }
    }
}
