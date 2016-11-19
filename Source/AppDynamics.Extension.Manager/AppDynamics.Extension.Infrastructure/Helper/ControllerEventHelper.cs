using AppDynamics.Extension.SDK.Model.Enumeration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace AppDynamics.Infrastructure.Helper
{
    public class ControllerEventHelper
    {
        public static ControllerEventSeverity GetControllerEventSeverity(EventLogEntryType t)
        {
            ControllerEventSeverity severity = ControllerEventSeverity.INFO;

            switch (t)
            {
                case EventLogEntryType.Error:
                    severity = ControllerEventSeverity.ERROR;
                    break;
                case EventLogEntryType.Warning:
                    severity = ControllerEventSeverity.WARN;
                    break;
                case EventLogEntryType.Information:
                    severity = ControllerEventSeverity.INFO;
                    break;
                default:
                    severity = ControllerEventSeverity.INFO;
                    break;
            }
            return severity;
        }
    }
}
