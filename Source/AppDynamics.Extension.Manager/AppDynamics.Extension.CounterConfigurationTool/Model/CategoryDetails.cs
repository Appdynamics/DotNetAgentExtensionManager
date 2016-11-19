using AppDynamics.Extension.CCT.ViewModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace AppDynamics.Extension.CCT.Model
{
    public class CategoryDetails
    {

        public CategoryDetails(PerformanceCounterCategory category)
        {
            CategoryName = category.CategoryName;
            CategoryType = category.CategoryType;
            CategoryHelp = category.CategoryHelp;
            
            // Can not use cheaper ReadCategory, as it will not give CounterHelp text
            // LoadCounterandInstanceNames(category);

            ListCounterDetails = new List<PerformanceCounterDetail>();

            if (category.CategoryType.Equals(PerformanceCounterCategoryType.MultiInstance))
            {
                InstanceNames = category.GetInstanceNames().ToList<string>();

                foreach (string instanceName in InstanceNames)
                {
                    if (category.InstanceExists(instanceName))
                    {
                        PerformanceCounter[] arrayCounters = category.GetCounters(instanceName);
                        loadPerfCounterDetails(arrayCounters);
                    }
                }
            }
            else
            {
                PerformanceCounter[] arrayCounters = category.GetCounters();
                loadPerfCounterDetails(arrayCounters);
            }
            
        }

        private void LoadCounterandInstanceNames(PerformanceCounterCategory category)
        {
            // Not in use.. only because of counter help text :-(
            // Keeping it in case of perf optimization needed.

            //InstanceDataCollectionCollection idColCol = category.ReadCategory();
            //ICollection idColColKeys = idColCol.Keys;
            //string[] idCCKeysArray = new string[idColColKeys.Count];
            //idColColKeys.CopyTo(idCCKeysArray, 0);

            //CounterNames = idCCKeysArray.ToList<string>();
        }

        private void loadPerfCounterDetails(PerformanceCounter[] arrayCounters)
        {
            foreach (PerformanceCounter counter in arrayCounters)
            {
                PerformanceCounterDetail counterDetail = new PerformanceCounterDetail(counter);
                ListCounterDetails.Add(counterDetail);
            }
        }

        #region Public properties

        public string CategoryName { get; set; }

        public string CategoryHelp { get; set; }

        public PerformanceCounterCategoryType CategoryType { get; set; }

        public List<PerformanceCounterDetail> ListCounterDetails { get; set; }

        public List<PerformanceCounterDetail> ListCounterDetailWithoutInstances
        {
            get
            {
                if (ListCounterDetails != null && ListCounterDetails.Count > 0)
                {
                    string defaultInstanceName = this.CategoryType.Equals(PerformanceCounterCategoryType.SingleInstance) ?
                        "" : "*";
                    var results = from cdetail in ListCounterDetails
                                  group cdetail by new {cdetail.CounterName, cdetail.CounterHelp, cdetail.CategoryName}
                                  into groupCounters
                                      select new PerformanceCounterDetail()
                                      {
                                          CounterName = groupCounters.Key.CounterName,
                                          CounterHelp = groupCounters.Key.CounterHelp,
                                          CategoryName = groupCounters.Key.CategoryName,
                                          InstanceName = defaultInstanceName
                                      };

                    return results.ToList<PerformanceCounterDetail>();
                }
                else 
                    return null;
            }
        }

        public List<string> InstanceNames { get; set; }

        // commenting Not in use for now-- Anurag
        //public List<string> CounterNames { get; set; }

        #endregion

    }
}
