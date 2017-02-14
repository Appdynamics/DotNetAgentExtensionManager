using AppDynamics.Extension.CCT.Model;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace AppDynamics.Extension.CCT.Infrastructure
{
    public static class ConfigHelper
    {
        static ConfigHelper()
        {
            // Verify if config.xml is present
            ConfigFilelocation = GetConfigFileLocation();
        }

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
                    XElement perfCounters = GetChildElementbyName(xMachineAgent, "perf-counters");
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

        private static string GetConfigFileLocation()
        {
            isConfigFileLocated = true;

            string configFilePath = AppDynamics.Infrastructure.ResourceStrings.AgentConfigFullPath;

            if (File.Exists(configFilePath))
            {
                return configFilePath;
            }
            else
            {
                isConfigFileLocated = false;
                return "";
            }
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
                        doc.Save(ConfigFilelocation, SaveOptions.OmitDuplicateNamespaces);
                        success = true;
                    }
                    catch (Exception ex)
                    {
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
    }
}
