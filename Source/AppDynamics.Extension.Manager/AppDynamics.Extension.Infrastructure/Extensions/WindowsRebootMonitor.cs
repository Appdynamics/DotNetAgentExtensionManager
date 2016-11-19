using AppDynamics.Extension.SDK;
using AppDynamics.Extension.SDK.Model.Enumeration;
using AppDynamics.Extension.SDK.Handlers;
using AppDynamics.Infrastructure.Helper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace AppDynamics.Infrastructure.Extensions
{
    public class WindowsRebootMonitor : AExtensionBase
    {

        private EventLog _eventLog;

        private string _logName = "";

        //TODO: Make this optionally readable from config file
        private string _summaryFormat = "Service Started at: {0} \n Last system reboot details -> \n"
            +" Time to initiate Reboot: {1}, \n Type:{2} Source:{3}, EventID:{4} Machine:{5} --> Message:{6}";

        public override void Initialize()
        {
            // optional- using current class as logger
            logger = NLog.LogManager.GetLogger(ResourceStrings.ExtensionFrameworkNamespace + "." + ExtensionName);
        }

        public override void Stop()
        {
            if (_eventLog != null)
            {
                _eventLog.Close();

                _eventLog.Dispose();
            }
        }

        public override bool Execute()
        {
            _eventLog = new EventLog()
            {
                Log = "System",
                EnableRaisingEvents = false,
                Source = "USER32",
            };

            if (_eventLog != null)
            {
                int count = _eventLog.Entries.Count;
                
                if (count > 0)
                {
                    for (var i = count; i-- > 0; )
                    {
                        var e = _eventLog.Entries[i];
                        if (e.Source.Equals("user32", StringComparison.CurrentCultureIgnoreCase))
                        {
                            ProcessEventEntry(e);
                            break;
                        }
                    }
                }
                else
                {
                    logger.Debug(String.Format("Zero Event Log entries found for USER32 {0}", count));
                }
            }
            else
            {
                logger.Warn("Didn't found event log for given source- USER32");
            }

            if (logger.IsDebugEnabled)
                logger.Debug(String.Format("Created Event Log watch for {0}", _logName));

            return true;
        }

        private void ProcessEventEntry(EventLogEntry e)
        {
            string summary = string.Format(_summaryFormat,
                DateTime.Now, e.TimeGenerated, e.EntryType, e.Source, e.EventID, e.MachineName, e.Message);

            summary = summary.Replace("&", " and ");

            string comment = "Event originated from the Windows event log and provided by the .Net extension service";

            ControllerEventArgs args = new ControllerEventArgs(
                summary,
                comment,
                ControllerEventHelper.GetControllerEventSeverity(e.EntryType),
                ExtensionName,
                EventProperties);

            if (logger.IsTraceEnabled)
                logger.Trace(String.Format("posting Event Log for {0}-{1}=>{2}", _logName, e.Source, summary));

            PostEventToController(args);

        }

    }
}
