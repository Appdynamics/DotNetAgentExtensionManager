using AppDynamics.Extension.SDK;
using AppDynamics.Extension.SDK.Model.Enumeration;
using AppDynamics.Extension.SDK.Handlers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace AppDynamics.Infrastructure.Extensions
{
    public class WindowsEventLogMonitor : AExtensionBase
    {

        private EventLog _eventLog;

        private string _logName = "";

        private List<string> _allowedSources = null;
        private List<string> _allowedEventType = null;
        private List<string> _filters = null;
        private List<string> _allowedEventIds = null;

        //TODO: Make this optionally readable from config file
        private string _summaryFormat = "Type:{0},%0A Source:{1},%0A EventID:{2},%0A Instance ID:{3},%0A Machine:{4},%0A Message:{5}";

        public override void Initialize()
        {
            // optional- using current class as logger
            logger = NLog.LogManager.GetLogger(ResourceStrings.ExtensionFrameworkNamespace + "." + ExtensionName);

            // get event log name from parametrs
            var value = Parameters["EventLogPath"];

            // Set event log name if configured otherwise set a default
            _logName = string.IsNullOrEmpty(value) ? "System" : value;

            // Set sources if configured otherwise set a default
            setPropertyFromConfig("EventSources", "WAS", out _allowedSources);

            //set filter if configured
            setPropertyFromConfig("EventLogMessageContains", "", out _filters);

            //set event ID if configured, default -1 to make sure it never matches
            setPropertyFromConfig("EventID", "", out _allowedEventIds);

            //set error level
            setPropertyFromConfig("EventLogEntryType", EventLogEntryType.Error.ToString(), out _allowedEventType);
        }

        private void setPropertyFromConfig(string propertyNameinConfig, string defaultStringValue, out List<string> _liststr)
        {
            string value = "";

            Parameters.TryGetValue(propertyNameinConfig, out value);

            _liststr = new List<string>();

            if (string.IsNullOrEmpty(value))
            {
                // adding default value
                _liststr.Add(defaultStringValue.ToLowerInvariant());
            }
            else
            {
                value = value.ToLowerInvariant();

                _liststr.AddRange(value.Split(','));
            }

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
            _eventLog = new EventLog(_logName)
            {
                EnableRaisingEvents = true,
            };

            _eventLog.EntryWritten += ProcessEventEntry;
            
            if (logger.IsDebugEnabled)
                logger.Debug(String.Format("Created Event Log watch for {0}", _logName));

            return true;
        }

        private void ProcessEventEntry(object sender, EntryWrittenEventArgs e)
        {
            // Ensure the event is of interest
            if (isValidEvent(e.Entry))
            {
                string message = e.Entry.Message;
                string summary = string.Format(_summaryFormat, 
                    e.Entry.EntryType, 
                    e.Entry.Source,
                    e.Entry.EventID,
                    e.Entry.InstanceId, 
                    e.Entry.MachineName, 
                    e.Entry.Message);

                summary = summary.Replace("&", " and ");

                string comment = "Event originated from the Windows event log and provided by the .Net extension service";

                ControllerEventArgs args = new ControllerEventArgs(
                    summary,
                    comment,
                    getSeverity(e.Entry.EntryType),
                    ExtensionName,
                    EventProperties);

                if (logger.IsTraceEnabled)
                    logger.Trace(String.Format("posting Event Log for {0}-{1}=>{2}", _logName, e.Entry.Source, summary));

                PostEventToController(args);
            }
            else
            {
                if (logger.IsTraceEnabled)
                    logger.Trace(String.Format("Not posting Event Log for {0}-{1}", _logName, e.Entry.Source));

            }
        }

        private bool isValidEvent(EventLogEntry entry)
        {
            bool isValid = false;

            bool validType = containsItemInList(entry.EntryType.ToString().ToLowerInvariant(), _allowedEventType);

            bool validSource = containsItemInList(entry.Source.ToLowerInvariant(), _allowedSources);

            bool validEventId = containsItemInList(entry.EventID.ToString(), _allowedEventIds);

            if (validType && validSource && validEventId)
            {
                foreach (string filter in _filters)
                {
                    // Comparing this way make it culture insensitive
                    if (entry.Message.ToLowerInvariant().Contains(filter.ToLowerInvariant()))
                    {
                        isValid = true;
                        break;
                    }
                }
            }
            
            return isValid;
        }

        /// <summary>
        /// returning true if list is empty or contains item with same value
        /// </summary>
        private bool containsItemInList(string strValue, List<string> listValues)
        {
            // Need to compare with empty string to make sure in case of default it passes. 
            return
                listValues == null ||
                listValues.Count == 0 ||
                listValues.Contains("") ||
            listValues.Contains(strValue);
        }

        private ControllerEventSeverity getSeverity(EventLogEntryType t)
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

        private string replaceSplChars(string s)
        {

            return s;
        }

    }
}
