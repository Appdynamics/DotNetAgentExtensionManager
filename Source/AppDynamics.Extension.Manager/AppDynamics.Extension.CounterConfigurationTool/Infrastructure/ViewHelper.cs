using AppDynamics.Extension.CCT.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace AppDynamics.Extension.CCT.Infrastructure
{
    class ViewHelper
    {

        internal static string GenerateDetails(PerformanceCounterDetail counter, bool? showInstances,
             CategoryDetails selectedCategory)
        {
            PerformanceCounterCategoryType type = PerformanceCounterCategoryType.MultiInstance;

            StringBuilder sb = new StringBuilder();

            if (selectedCategory != null)
            {
                type = selectedCategory.CategoryType;
            }
            if (counter != null)
            {
                string instanceName = GetInstanceNameMessage(counter, showInstances, type);
                sb.Append("Category: ").Append(counter.CategoryName).AppendLine()
                    .Append("Counter: ").Append(counter.CategoryName).Append("\t")
                    .Append("Instance: ").Append(instanceName).AppendLine();
            sb.AppendLine(counter.ToXml());

            sb.AppendLine(counter.CounterHelp);

            }
            else if (selectedCategory != null)
            {
                sb.Append("Category: ").Append(selectedCategory.CategoryName).AppendLine()
                    .Append("Type: ").Append(selectedCategory.CategoryType).AppendLine()
                    .AppendLine(selectedCategory.CategoryHelp);
            }
            else
            {
                sb.Append("Please select performance counter or category.");
            }


            return sb.ToString();
        }

        public static string GetInstanceName(PerformanceCounterDetail counter, bool? showInstances,
             PerformanceCounterCategoryType categoryType)
        {
            string instanceName = "";

            if (categoryType.Equals(PerformanceCounterCategoryType.MultiInstance))
            {
                instanceName = true.Equals(showInstances) ? counter.InstanceName : ResourceStrings.AllInstanceName;
            }

            return instanceName;
        }

        private static string GetInstanceNameMessage(PerformanceCounterDetail counter, bool? showInstances,
             PerformanceCounterCategoryType categoryType)
        {
            string instanceName = GetInstanceName(counter, showInstances, categoryType);

            return instanceName.Equals("") ? ResourceStrings.NoInstanceMessage : instanceName;
        }

        public static List<PerformanceCounterDetail> RemoveDuplicate(List<PerformanceCounterDetail> list)
        {
            //Method 1-- issue @loading
            //var noDuplicates = new HashSet<PerformanceCounterDetail>(list);

            //List<PerformanceCounterDetail> distinctList = noDuplicates.ToList<PerformanceCounterDetail>();

            #region Odd tries
            // Method 2-- needIequatable
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
