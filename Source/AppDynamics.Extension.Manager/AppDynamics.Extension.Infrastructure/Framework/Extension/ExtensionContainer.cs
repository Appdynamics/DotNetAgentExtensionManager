using AppDynamics.Extension.SDK;
using AppDynamics.Extension.SDK.Model.Enumeration;
using AppDynamics.Extension.SDK.Handlers;
using AppDynamics.Infrastructure.Framework.Extension.Configuration;
using AppDynamics.Infrastructure.Framework.Extension.Handlers;
using AppDynamics.Infrastructure.Framework.Extension.Providers;
using System;
using System.Net;
using System.Text;
using System.Threading;

namespace AppDynamics.Infrastructure.Framework.Extension
{
    public class ExtensionContainer
    {
        public ExtensionContainer(IExtension extension)
        {
            _extension = extension;

            _logger.Info(String.Format("Validating extension {0}", _extension.ExtensionName));
            ValidateExtensionObject();

            // Initialize
            intializeExtension();
        }

        #region private properties
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private IExtension _extension;
        
        private Thread _workerThread = null;

        private bool _stopWorkerThread = true;

        private PerformanceCounterHandler _pcHandler = null;

        private AppDynamicsAgentType _agentConfig = new AppDynamicsAgentType();

        private IDataProvider _restApi = new ControllerRestApiProvider();

        private readonly object _workerThreadMonitor = new object();

        #endregion

        internal string extensionName { get { return _extension.ExtensionName; } }

        internal void Start()
        {
            if (_extension.Enabled)
            {
                // Create objects
                _extension.Initialize();

                #region SetUp
                switch (_extension.Type)
                {
                    case ExtensionType.EVENT:
                        // Subscribe to controller event
                        _extension.ControllerEventGenerated += PostEvent;
                        break;

                    case ExtensionType.METRIC:
                        // Create performance objects for metrics defiend
                        CreatePerformanceCounters();
                        break;

                    case ExtensionType.ACTION:
                        throw new ExtensionFrameworkException("custom Action extension not supported");

                    default:
                        throw new ExtensionFrameworkException("No defalut action defiend");
                }
                #endregion

                #region Execution
                switch (_extension.ExecutionMode)
                {
                    case ExecutionMode.CONTINUOUS:
                        // In case of Continuous execution, it needs to execute only once
                        _extension.Execute();
                        break;
                    case ExecutionMode.PERIODIC:
                        // Create a thread and call Execute as per frequency
                        schedulePeriodicExecution();
                        break;
                }
                #endregion
            }
            else
            {
                _logger.Debug(String.Format("Extension {0} is disabled. No action needed.", _extension.ExtensionName));
            }
        }

        internal void Stop()
        {
            _stopWorkerThread = true;



            // Abort if still alive.. 
            if (_workerThread != null)
            {
                lock (_workerThreadMonitor)
                {
                    Monitor.Pulse(_workerThreadMonitor);
                }
                
                // Giving 100 ms to terminate the thread.
                Thread.Sleep(150);

                if (_workerThread.IsAlive)
                {
                    _logger.Warn("Had to abort worker thread..");
                    // _workerThread.Abort();
                }else
                    _logger.Info("Stopped worker thread gracefully..");

            }

            DateTime startTime = DateTime.Now;

            if (_pcHandler != null)
            {
                _pcHandler.Dispose();
            }

            /*//--Old way of stopping worker thread- cause 1 min wait
             * Now using Monitor.Wait and Pulse- (Anurag 02/08/16)
            double msallowed = _extension.FrequencyInSec * 1000;
            
            while (_workerThread != null && _workerThread.IsAlive)
            {
                double mspassed = (DateTime.Now - startTime).TotalMilliseconds;

                Thread.Sleep(10);

                if (mspassed > msallowed)
                    break;
            }
            
            */

            if (_extension != null)
            {
                _extension.Stop();
            }
        }

        #region Private methods

        private void intializeExtension()
        {
            if (_extension.ControllerInfo != null)
            {
                if (_logger.IsDebugEnabled)
                    _logger.Debug(String.Format("Loading Agent configuration for {0}", _extension.ExtensionName));

                _agentConfig = AgentConfig.LoadAgentConfiguration();
            }

            switch (_extension.Type)
            {
                case ExtensionType.EVENT:

                    //Nothing to do as of Now, (tried initializing AgentConfig here)

                    break;
                case ExtensionType.METRIC:
                    // nothing to do 
                    break;
                case ExtensionType.ACTION:
                    throw new ExtensionFrameworkException("custom Action extension not supported");
            }
        }

        private void ValidateExtensionObject()
        {
            if (_extension == null)
                throw new ExtensionFrameworkException("Extension object is null");
            // any other validation needs to be added here.
        }

        private void schedulePeriodicExecution()
        {
            // creating new thread to start periodic metric collection
            _workerThread = new Thread(ExecuteWorkerThread);

            _stopWorkerThread = false;

            _workerThread.Start(_extension);
        }

        private void ExecuteWorkerThread(object obj)
        {
            if (_extension.ListExtensionInstance != null && _extension.Type == ExtensionType.METRIC)
            {
                // Before starting need to register category and counters in same thread 
                // to maintain the context and avoid corss refrencing.
                _pcHandler = new PerformanceCounterHandler(_extension.ExtensionName);
                try
                {
                    _pcHandler.RegisterCategoryandCounters(_extension.ListExtensionInstance);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error while registering category and counters");
                }
            }

            if (_logger.IsDebugEnabled)
                _logger.Debug("STarting worker thread for extension-{0}", extensionName);

            while (!_stopWorkerThread)
            {
                bool extensionExecuteStatus = false;

                try
                {
                    //TODO: impose timeout on execute method. #unsafe
                    extensionExecuteStatus = _extension.Execute();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error while executing {0}", _extension.ExtensionName);
                }

                SetPerfCounterValues(extensionExecuteStatus);

                // Now sleeping for given time
                // Old way of sleeping causing 1 min wait to shut down
                //Thread.Sleep(_extension.FrequencyInSec * 1000);

                lock (_workerThreadMonitor)
                {
                    Monitor.Wait(_workerThreadMonitor, 1000 * _extension.FrequencyInSec);
                }
            }
        }

        private void SetPerfCounterValues(bool extensionExecuteStatus)
        {
            if (extensionExecuteStatus)
            {
                // Fill values or register event via delegate in extension obj or action

                if (_extension.ListExtensionInstance != null && _extension.Type == ExtensionType.METRIC)
                {
                    // SetValue to performance counters and empty them
                    _pcHandler.SetValues(_extension.ListExtensionInstance);
                }
                else if (_extension.ListExtensionInstance != null && _extension.Type == ExtensionType.ACTION)
                {
                    throw new ExtensionFrameworkException("Action type extensions are not supported.");
                }
                else
                {
                    // do nothing for events, it is already registered using delegate.
                }
            }
            else
            {
                // TODO: error in execution, count to 10 and stop the thread #smartlogging
            }
        }

        private void CreatePerformanceCounters()
        {
            // nothing to do here for now, will remove later.
        }


        private void PostEvent(object sender, ControllerEventArgs e)
        {

            try
            {
                string url = GetURL(e);

                string _username = _extension.ControllerInfo.UserName + "@" + _extension.ControllerInfo.AccountName;

                if (_logger.IsTraceEnabled)
                    _logger.Trace("New event received: {0}", url);
                var result = _restApi.Request(new RestRequest
                {
                    Url = url,
                    Username = _username,
                    Password = _extension.ControllerInfo.Password,
                    Verb = WebRequestMethods.Http.Post
                });

                if (_logger.IsDebugEnabled)
                    _logger.Debug("Custom event post response: {0}", result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to post custom event.");
            }

        }

        private string GetURL(ControllerEventArgs e)
        {
            var url = new StringBuilder(string.Format("{0}:{1}/controller/rest/applications/{2}/events",
                                        _agentConfig.controller.host,
                                        _agentConfig.controller.port,
                                        _agentConfig.controller.application.name));
            url.Append("?eventtype=CUSTOM");

            if (_logger.IsTraceEnabled)
                _logger.Trace("URL forming-1: {0}", url);
 
            url.Append(e.GetInURL());

            if (_logger.IsTraceEnabled)
                _logger.Trace("URL forming-2: {0}", url);

            // For safe side, it url gets encoded from xml
            url.Replace("&amp;", "&");
            // To avoid url in message (need to make sure url is not starting with http)
            url.Replace("://", ": //");

            if (_logger.IsTraceEnabled)
                _logger.Trace("URL forming-3: {0}", url);

            return url.ToString();
        }

        #endregion
    }
}
