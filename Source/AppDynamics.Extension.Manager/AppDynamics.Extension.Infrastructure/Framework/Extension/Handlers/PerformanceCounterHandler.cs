using AppDynamics.Extension.SDK;
using AppDynamics.Extension.SDK.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace AppDynamics.Infrastructure.Framework.Extension.Handlers
{
    public class PerformanceCounterHandler
    {

        public PerformanceCounterHandler(string extensionName)
        {
            _categoryName = extensionName;
        }

        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private string _categoryName = "AppDynamics Extension Object";

        private string _categoryHelp = ResourceStrings.CustomCountercategoryDescription;

        // TODO: #optimization Use Dictionary-- done @6/3/16-Anurag
        private IDictionary<string, PerformanceCounter> _counters = null;

        internal void RegisterCategoryandCounters(List<ExtensionInstance> listExtensionInstance)
        {
            createPCCategory(listExtensionInstance);

            initializePerfCounters(listExtensionInstance);
        }

        internal void SetValues(List<ExtensionInstance> listExtensionInstance)
        {
            foreach (ExtensionInstance eInstance in listExtensionInstance)
            {
                foreach (ExtensionMetric emetric in eInstance.ListExtensionMetrics)
                {
                    var counter = _counters[emetric.MetricName + "-" + eInstance.InstanceName];

                    counter.ReadOnly = false;

                    counter.RawValue = emetric.Value;

                    _logger.Trace("Counter Name={0}, InstanceName={1}, NextValue={2}", counter.CounterName, counter.InstanceName, counter.RawValue);

                    //resetting value to zero in obj for next time
                    emetric.Value = 0;
                }
            }
        }

        public void Dispose()
        {
            if (_counters!= null)
            {
                foreach (PerformanceCounter pc in _counters.Values)
                {
                    // close and dispose performancecounter object
                    if (_logger.IsDebugEnabled)
                        _logger.Debug("Closing and disposing counter- {0}", pc.CounterName);

                    pc.Close();

                    try
                    {
                        pc.Dispose();
                    }
                    catch (ObjectDisposedException) { }
                }
            }
        }

        private void initializePerfCounters(List<ExtensionInstance> listExtensionInstance)
        {
            if (listExtensionInstance != null && listExtensionInstance.Count > 0)
            {
                _counters = new Dictionary<string, PerformanceCounter>();

                // #hack #lastminute to check if counter is already added in config.xml.
                // Following property is to add perf counter in config.xml during initialization
                List<PerformanceCounterDetail> listPerfCounterDetails = ConfigXMLHandler.ReadAppDynamicsConfigFile();

                foreach (ExtensionInstance eInstance in listExtensionInstance)
                {
                    foreach (ExtensionMetric emetric in eInstance.ListExtensionMetrics)
                    {
                        PerformanceCounter pc = new PerformanceCounter(_categoryName,
                            emetric.MetricName, eInstance.InstanceName);

                        //pc.ReadOnly = false;
                        // always initializing with 0
                        //pc.RawValue = 0;

                        _counters.Add(emetric.MetricName + "-" + eInstance.InstanceName, pc);

                        _logger.Info(String.Format("Adding Perf counter {0}-{1}-{2}"
                            , pc.CategoryName, pc.CounterName, pc.InstanceName));

                        // Adding to already added list of counters in config.xml
                        listPerfCounterDetails.Add(new PerformanceCounterDetail(pc));
                    }
                }

                //removing duplicates if added
                List<PerformanceCounterDetail> distinctList = ConfigXMLHandler.RemoveDuplicate(listPerfCounterDetails);

                _logger.Info(String.Format("Saving distinct list from {0} to {1}", listPerfCounterDetails.Count, distinctList.Count));

                bool isSaved = ConfigXMLHandler.AddPerformanceCounterstoConfig(distinctList);

            }

        }

        /// <summary>
        /// Create the performance counter category
        /// </summary>
        private void createPCCategory(List<ExtensionInstance> listExtensionInstance)
        {
            //// Must create a counter in a new category; adding a counter to an existing user-defined 
            //// category will raise an exception.
            deletePCCategory(_categoryName);

            //// Create the counters with metadata
            var counterData = GetCounterCreationDataCollection(listExtensionInstance);

            if (_logger.IsDebugEnabled)
                _logger.Debug("Creating new category with counters after deleting old one.");

            // creating new user defined performance counter category
            PerformanceCounterCategory.Create(
                _categoryName,
                _categoryHelp,
                PerformanceCounterCategoryType.MultiInstance,
                counterData);

            if (_logger.IsDebugEnabled)
                _logger.Debug("Created category with counters");
        }

        /// <summary>
        /// Delete the performance counter category if it exists
        /// </summary>
        /// <param name="categoryName">Category to delete</param>
        private void deletePCCategory(string categoryName)
        {
            if (PerformanceCounterCategory.Exists(categoryName))
            {
                if (_logger.IsDebugEnabled)
                    _logger.Debug(" category {0} already exists, Deleting..", categoryName);

                PerformanceCounterCategory.Delete(categoryName);

                if (_logger.IsDebugEnabled)
                    _logger.Debug("Category {0} deleted", categoryName);
            }
            else
            {
                if (_logger.IsDebugEnabled)
                    _logger.Debug("Category {0} doesn't exists, delete not needed.", categoryName);
            }
        }

        /// <summary>
        /// Get the performance counter collection
        /// </summary>
        /// <returns>Collection of perf counters</returns>
        private CounterCreationDataCollection GetCounterCreationDataCollection(List<ExtensionInstance> listExtensionInstance)
        {
            var counterData = new CounterCreationDataCollection();

            //Limitation: #limitation All instances contains same set of metrics(counters)
            foreach (ExtensionMetric em in listExtensionInstance[0].ListExtensionMetrics)
            {
                var counter = new CounterCreationData
                {
                    CounterName = em.MetricName,
                    CounterType = PerformanceCounterType.NumberOfItems64,
                    CounterHelp = em.Description
                };

                counterData.Add(counter);

                if (_logger.IsDebugEnabled)
                    _logger.Debug("Added counter {0}", counter.CounterName);
            }

            return counterData;
        }

    }
}
