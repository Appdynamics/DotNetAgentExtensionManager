using AppDynamics.Extension.SDK;
using AppDynamics.Extension.SDK.Model.Enumeration;
using AppDynamics.Extension.SDK.Model;
using AppDynamics.Extension.SDK.Model.XML;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace AppDynamics.Infrastructure.Framework.Extension.Handlers
{
    public class ExtensionXMLHandler
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Validates xml with embeded xsd in sdk
        /// Throws exception if not valid.
        /// </summary>
        /// <param name="xmlFilePath">File path for extension.xml</param>
        public static bool ValidateExtensionXML(string xmlFilePath)
        {
            // TODO: make it ignore case 
            // by converting entire xml to lower case then validate-
            // update xsd accordingly, wherever needed (enums)

            if (_logger.IsTraceEnabled)
                _logger.Trace(String.Format("validating extension xml @ {0} ...", xmlFilePath));

            var xdoc = XDocument.Load(xmlFilePath);

            return ValidateExtensionXML(xdoc);
        }

        public static bool ValidateExtensionXML(XDocument xdoc)
        {

            System.Reflection.Assembly a = System.Reflection.Assembly.Load("AppDynamics.Extension.SDK");

            if (a != null)
            {
                string xsdName = "AppDynamics.Extension.SDK.Extension.xsd";

                var schemas = new XmlSchemaSet();

                using (Stream stream = a.GetManifestResourceStream(xsdName))
                {
                    using (XmlReader reader = XmlReader.Create(stream))
                    {
                        schemas.Add(ResourceStrings.TargetNamespaceInXSD, reader);

                        if (_logger.IsTraceEnabled)
                            _logger.Trace(String.Format("xsd found @ {0} .", xsdName));

                        xdoc.Validate(schemas, null);

                        if (_logger.IsTraceEnabled)
                            _logger.Trace(String.Format("extension xml validated {0} .", xdoc.ToString()));
                    }
                }
            }
            else
            {
                throw new ExtensionFrameworkException("Could not load extension framework dll.");
            }

            return true;
        }

        internal static IExtension GetExtObjectFromXML(string xmlFilePath)
        {
            if (_logger.IsTraceEnabled)
                _logger.Trace(String.Format("parsing extension xml @ {0} ...", xmlFilePath));

            XDocument xDoc = XDocument.Load(xmlFilePath);

            if (xDoc != null)
            {
                if (_logger.IsTraceEnabled)
                    _logger.Trace(String.Format("Extension XML loaded:{0}", xDoc.ToString(SaveOptions.None)));
            }
            else
            {
                throw new ExtensionFrameworkException(
                    String.Format("Could not load extension xml from path: {0}", xmlFilePath), false);
            }
            // getting required valus, failing would cause an exception

            string extensionType = xDoc.Root.Attribute("type").Value;

            XElement execution = xDoc.Root.Descendants().Where(d => d.Name.LocalName.Equals("execution")).Single();

            string executionType = execution.Attribute("type").Value;

            string executionMode = execution.Attribute("mode").Value;

            string executionPath = execution.Attribute("path").Value;

            IExtension extobj = ExtensionActivator.CreateExtensionInstance(
                executionPath, extensionType, executionType, executionMode, xmlFilePath);

            // Now capturing optional values for extension object
            int freqency = 50;// #defaultfrequency hardcoded to 50

            if (execution.Attribute("frequency-in-sec") != null)
            {
                string frequencyInSec = execution.Attribute("frequency-in-sec").Value;

                Int32.TryParse(frequencyInSec, out freqency);
            }

            // #minfrequency #hardcoded to 10
            extobj.FrequencyInSec = (freqency > 10) ? freqency : 10;

            ExtensionXMLHandler.FillExtensionObjectFromXML(xDoc, ref extobj);

            // Hack: add path to parameters[needed for script to run] #scriptextension
            addPathtoParameters(xmlFilePath, executionPath, ref extobj);

            if (_logger.IsTraceEnabled)
                _logger.Trace(String.Format("created object from extension xml @ {0} ...", xmlFilePath));

            if (_logger.IsTraceEnabled)
            {
                logTraceForCapturedObj(extobj);
            }

            return extobj;
        }

        private static void logTraceForCapturedObj(IExtension extobj)
        {
            if (extobj != null)
            {
                // TODO: add trace for captured object and props
            }
            else
            {
                throw new ExtensionFrameworkException("Could not activate extension object.");
            }
        }

        internal static ExtensionXML GetExtensionXMlObject(string xmlFilePath)
        {
            ExtensionXML extensionXMlObject;

            XmlSerializer mySerializer = new XmlSerializer(typeof(ExtensionXML));

            FileStream myFileStream =  new FileStream(xmlFilePath, FileMode.Open);

            extensionXMlObject = (ExtensionXML)mySerializer.Deserialize(myFileStream);

            return extensionXMlObject;
        }

        private static void addPathtoParameters(string xmlFilePath, string executionPath, ref IExtension extobj)
        {
            if (extobj.Parameters == null)
                extobj.Parameters = new Dictionary<string, string>();

            string pathToscript = xmlFilePath.Replace(ResourceStrings.ExtensionXmlName, executionPath);

            extobj.Parameters.Add("_path", pathToscript);
        }

        private static void FillExtensionObjectFromXML(XDocument xDoc, ref IExtension extobj)
        {
            // get Name
            if (xDoc.Root.Attribute("name") != null)
                extobj.ExtensionName = xDoc.Root.Attribute("name").Value;

            // get if not enabled 
            bool enabled = true;
            if (xDoc.Root.Attribute("enabled") != null)
            {
                enabled = false;
                Boolean.TryParse(xDoc.Root.Attribute("enabled").Value, out enabled);
            }
            extobj.Enabled = enabled;

            // get description
            extobj.Description = getTextOfElement(xDoc, "description", ResourceStrings.ExtensionDescription);

            // get controller-info
            var cinfo = xDoc.Root.Descendants().Where(d=>d.Name.LocalName.Equals("controller-info"));

            extobj.ControllerInfo = getControllerInfo(cinfo);

            // get metrics/instances
            var metrics = xDoc.Root.Descendants().Where(d=>d.Name.LocalName.Equals("metric"));

            var instances = xDoc.Root.Descendants().Where(d=>d.Name.LocalName.Equals("instance"));

            extobj.ListExtensionInstance = getInstanceList(instances, metrics);

            // get controller-event-properties
            var eventProps = xDoc.Root.Descendants().Where(d=>d.Name.LocalName.Equals("controller-event-properties"));

            extobj.EventProperties = getDictionaryValues(eventProps);

            // get parameters
            var parameters = xDoc.Root.Descendants().Where(d=>d.Name.LocalName.Equals("parameters"));

            extobj.Parameters = getDictionaryValues(parameters);
        }

        private static Dictionary<string, string> getDictionaryValues(IEnumerable<XElement> parameters)
        {
            Dictionary<string, string> values = new Dictionary<string, string>();

            if (parameters != null)
            {
                var adds = parameters.Descendants().Where(d=>d.Name.LocalName.Equals("add"));

                if (adds != null && adds.Count() > 0)
                {
                    foreach (XElement xe in adds)
                    {
                        string key = xe.Attribute("key").Value;

                        string val = xe.Attribute("value").Value;

                        values.Add(key, val);
                    }
                }
            }
            return values;
        }

        private static List<ExtensionMetric> getNewMetricList(IEnumerable<XElement> metrics)
        {
            List<ExtensionMetric> listExtMetrics = new List<ExtensionMetric>();

            foreach (XElement met in metrics)
            {
                ExtensionMetric extMetric = new ExtensionMetric();

                extMetric.MetricName = met.Attribute("name").Value;

                if (met.Attribute("description") != null)
                    extMetric.Description = met.Attribute("description").Value;
                else
                    extMetric.Description = extMetric.MetricName;

                if (met.Attribute("enabled") != null)
                    extMetric.Enabled = Boolean.Parse(met.Attribute("enabled").Value);
                else
                    extMetric.Enabled = true;


                listExtMetrics.Add(extMetric);
            }

            return listExtMetrics;
        }

        private static List<ExtensionInstance> getInstanceList(
            IEnumerable<XElement> instances, IEnumerable<XElement> metrics)
        {
            var listExtInstances = new List<ExtensionInstance>();

            if (metrics != null && instances != null)
            {
                foreach (XElement ins in instances)
                {
                    ExtensionInstance extInstance = new ExtensionInstance();

                    extInstance.InstanceName = ins.Attribute("name").Value;

                    if (ins.Attribute("data") != null)
                        extInstance.InstanceData = ins.Attribute("data").Value;
                    else
                        extInstance.InstanceData = extInstance.InstanceName;

                    if (ins.Attribute("enabled") != null)
                        extInstance.Enabled = Boolean.Parse(ins.Attribute("enabled").Value);
                    else
                        extInstance.Enabled = true;

                    extInstance.ListExtensionMetrics = getNewMetricList(metrics);

                    listExtInstances.Add(extInstance);
                }
            }
            else
            {
                throw new ExtensionFrameworkException("Empty metrics and instances element from config.");
            }
            return listExtInstances;
        }

        private static ControllerInformation getControllerInfo(IEnumerable<XElement> elements)
        {
            ControllerInformation controllerInfo = null;

            if (elements != null && elements.Count() > 0)
            {
                try
                {
                    controllerInfo = new ControllerInformation();

                    XElement cInfo = elements.Single();

                    controllerInfo.UserName = cInfo.Attribute("user").Value;

                    controllerInfo.AccountName = cInfo.Attribute("account").Value;

                    controllerInfo.Password = cInfo.Attribute("password").Value;

                    controllerInfo.Encrypted = (cInfo.Attribute("encrypted") == null) ?
                        false : Boolean.Parse(cInfo.Attribute("encrypted").Value);
                }
                catch (Exception)
                {
                    controllerInfo = null;
                }
            }

            return controllerInfo;
        }

        private static string getTextOfElement(XDocument xDoc, string elementName, string defaultValue)
        {
            try
            {
                XElement xE = xDoc.Root.Descendants().Where(d=>d.Name.LocalName.Equals(elementName)).Single();

                return xE.Value;
            }
            catch (Exception) 
            {
                return defaultValue;
            }
        }


    }
}
