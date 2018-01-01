using AppDynamics.Extension.CCT.Model;
using Microsoft.Win32;
using System;
using System.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace AppDynamics.Extension.CCT.Infrastructure
{
    public class ConfigHelper
    {
        
        /// IMP: Removing static constructor and adding initialize method
        

        public void Initialize()
        {
            // Verify if config.xml is present
            GetConfigFileLocation();
        }

        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();


        public bool isConfigFileLocated { get; private set; }

        public string ConfigFilelocation { get; private set; }

        public string LastException { get; private set; }

        public List<PerformanceCounterDetail> ReadAppDynamicsConfigFile()
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
            else
            {
                _logger.Info("Could not locate Config file location at-" + ConfigFilelocation);
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

        private void GetConfigFileLocation()
        {
            isConfigFileLocated = true;

            ConfigFilelocation = "";
            // HACK for #86008 
            if (String.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["ConfigFilePath"]))
            {
                ConfigFilelocation = AppDynamics.Infrastructure.ResourceStrings.AgentConfigFullPath;
            }
            else
            {
                ConfigFilelocation = ConfigurationManager.AppSettings["ConfigFilePath"].Trim();
                _logger.Trace("checking app.config for Config File Path @" + ConfigFilelocation);
            }

            isConfigFileLocated = File.Exists(ConfigFilelocation);

            _logger.Info(String.Format("Located-{0} Config file at - {1}", isConfigFileLocated, ConfigFilelocation));

        }

        private bool AddPerformanceCounterstoConfig(XElement xmlCounters)
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
                        _logger.Error(ex, "Error while adding perf counters");
                    }
                }
            }
            return success;
        }

        public bool AddPerformanceCounterstoConfig(List<PerformanceCounterDetail> listXMLCountersInConfig)
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
