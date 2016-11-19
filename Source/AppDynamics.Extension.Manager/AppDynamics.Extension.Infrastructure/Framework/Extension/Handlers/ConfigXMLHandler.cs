using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace AppDynamics.Infrastructure.Framework.Extension.Handlers
{

    public static class ConfigXMLHandler
    {
        static ConfigXMLHandler()
        {
            // Verify if config.xml is present
            ConfigFilelocation = ResourceStrings.AgentConfigFullPath;
        }

        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public static bool isConfigFileLocated { get; private set; }

        public static string ConfigFilelocation { get; private set; }

        public static string LastException { get; private set; }

        public static List<PerformanceCounterDetail> ReadAppDynamicsConfigFile()
        {
            XElement xmlData = null;

            if (File.Exists(ConfigFilelocation))
            {
                var doc = XDocument.Load(ConfigFilelocation);
                XElement xMachineAgent = GetChildElementbyName(doc.Root, "machine-agent");
                if (xMachineAgent.HasElements)
                {
                    XElement perfCounters = GetChildElementbyName(xMachineAgent,
                        ResourceStrings.PerfCounterRootName);
                    xmlData = perfCounters;
                }
            }

            List<PerformanceCounterDetail> counters = GetConterConfigViewFromXElement(xmlData);

            return counters;
        }

        private static List<PerformanceCounterDetail> GetConterConfigViewFromXElement(XElement xmlCounters)
        {
            List<PerformanceCounterDetail> list = new List<PerformanceCounterDetail>();

            if (xmlCounters != null && ResourceStrings.PerfCounterRootName.Equals(xmlCounters.Name.ToString()))
            {
                foreach (XElement element in xmlCounters.Elements(ResourceStrings.PerfCounterElementName))
                {
                    PerformanceCounterDetail counter = new PerformanceCounterDetail(element);
                    list.Add(counter);
                }
            }

            return list;
        }

        private static XElement GetChildElementbyName(XElement node, string childNodeName)
        {
            XElement machineAgent = null;
            if (node != null && node.HasElements)
            {
                // TODO: can use sarch using xpath
                foreach (XElement xe in node.Elements())
                {
                    string name = xe.Name.LocalName;
                    if (name.Equals(childNodeName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        machineAgent = xe;
                        break;
                    }
                }
            }
            return machineAgent;
        }

        private static bool AddPerformanceCounterstoConfig(XElement xmlCounters)
        {
            bool success = false;

            if (xmlCounters != null)
            {
                if (File.Exists(ConfigFilelocation))
                {
                    var doc = XDocument.Load(ConfigFilelocation);

                    XElement xMachineAgent = GetChildElementbyName(doc.Root, "machine-agent");

                    if (xMachineAgent.HasElements)
                    {
                        XElement perfCounters = GetChildElementbyName(xMachineAgent, "perf-counters");
                        if (perfCounters != null)
                            perfCounters.Remove();
                    }

                    if (xmlCounters != null && xmlCounters.HasElements)
                    {
                        xMachineAgent.Add(xmlCounters);
                    }

                    try
                    {
                        _logger.Info("Saving counters in config.xml");

                        doc.Save(ConfigFilelocation, SaveOptions.OmitDuplicateNamespaces);
                        
                        success = true;

                        _logger.Info("Successfully saved counters in config.xml");
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Could not add perf counters to config file.");
                        
                        success = false;
                        
                        LastException = ex.Message;
                    }
                }
            }
            return success;
        }

        public static bool AddPerformanceCounterstoConfig(List<PerformanceCounterDetail> listXMLCountersInConfig)
        {
            bool result = false;

            if (listXMLCountersInConfig != null)
            {
                XElement xelement = new XElement(ResourceStrings.PerfCounterRootName);

                foreach (PerformanceCounterDetail counter in listXMLCountersInConfig)
                {
                    if (counter.IsAdded)
                        xelement.Add(counter.GetXElement());
                }

                result = AddPerformanceCounterstoConfig(xelement);
            }
            else
            {
                LastException = "No Counters Could be added to the file.(CD156)";
            }
            return result;
        }

        internal static string GetXMLFromXMLCounterList(List<PerformanceCounterDetail> listXMLCountersInConfig)
        {
            string xmlStr = "";

            if (listXMLCountersInConfig != null)
            {
                XElement xelement = new XElement(ResourceStrings.PerfCounterRootName);

                foreach (PerformanceCounterDetail counter in listXMLCountersInConfig)
                {
                    if (counter.IsAdded)
                        xelement.Add(counter.GetXElement());
                }

                xmlStr = xelement.ToString();
            }
            return xmlStr;
        }


        public static List<PerformanceCounterDetail> RemoveDuplicate(List<PerformanceCounterDetail> list)
        {
            //Method 1-- issue @loading
            //var noDuplicates = new HashSet<PerformanceCounterDetail>(list);

            //List<PerformanceCounterDetail> distinctList = noDuplicates.ToList<PerformanceCounterDetail>();

            #region Odd tries
            // Method 2-- needs Iequatable
            //IEnumerable<CounterConfigView> l = list.Distinct();

            //TODO: Bad way.. need to check why above methods are not working
            List<PerformanceCounterDetail> distinctList = new List<PerformanceCounterDetail>();

            foreach (PerformanceCounterDetail c1 in list)
            {
                bool found = false;
                foreach (PerformanceCounterDetail c2 in distinctList)
                {
                    if (c1.Equals(c2))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                    distinctList.Add(c1);
            }
            #endregion

            return distinctList;
        }
    }

}
