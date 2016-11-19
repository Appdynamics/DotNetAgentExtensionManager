using AppDynamics.Extension.CCT.Infrastructure;
using AppDynamics.Extension.CCT.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace AppDynamics.Extension.CCT.Model
{
    public class PerformanceCounterDetail : IEquatable<PerformanceCounterDetail>
    {
        //private System.Diagnostics.PerformanceCounter counter;
        public PerformanceCounterDetail()
        {
            IsAdded = false;
            InstanceName = "";
        }

        public PerformanceCounterDetail(System.Diagnostics.PerformanceCounter counter)
        {
            this.CategoryName = counter.CategoryName;

            this.CounterName = counter.CounterName;

            this.InstanceName = counter.InstanceName;

            this.CounterHelp = counter.CounterHelp;

            IsAdded = false;

            counter.Dispose();
        }

        public PerformanceCounterDetail(XElement element)
        {
            if (element != null && element.HasAttributes)
            {
                this.CounterName = getAttributeValue(element, "name");

                this.CategoryName = getAttributeValue(element, "cat");

                this.InstanceName = getAttributeValue(element, "instance");

                this.IsAdded = true;
            }
            else
            {
                //TODO: It should never come here..
                IsAdded = false;
                InstanceName = "";
            }
        }

        public string CategoryName { get; set; }

        public string CounterName { get; set; }

        public string InstanceName { get; set; }

        public string CounterHelp { get; set; }

        public bool IsAdded { get; set; }

        public string ToXml()
        {
            return String.Format(ResourceStrings.PerfCounterXmlTemplate, CategoryName,
                CounterName, InstanceName);
        }

        public override string ToString()
        {
            return GetXElement().ToString();
        }

        public XElement GetXElement()
        {
            XElement xe = new XElement(ResourceStrings.PerfCounterElementName);
            xe.SetAttributeValue("cat", CategoryName);
            xe.SetAttributeValue("name", CounterName);
            xe.SetAttributeValue("instance", InstanceName);
            return xe;
        }

        private string getAttributeValue(XElement element, string attName)
        {
            return (element.Attribute(attName) != null) ?
                element.Attribute(attName).Value : "";
        }

        public bool Equals(PerformanceCounterDetail other)
        {
            return (this.CounterName == other.CounterName &&
                this.CategoryName == other.CategoryName &&
                this.InstanceName == other.InstanceName);
        }
    }
}
