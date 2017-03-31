using AppDynamics.Extension.CCT.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace AppDynamics.Extension.CCT.Infrastructure
{
    public class PerformanceCounterHandler
    {
        public PerformanceCounterHandler()
        {
            
        }

        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public static string WorkStatus = "";

        public static int LoadedCategories = 0;

        public static void Initialize()
        {
            WorkStatus = ResourceStrings.StatusReading;

            LoadedCategories = 0;
            
            PerformanceCounterCategory[] arrayCategory = PerformanceCounterCategory.GetCategories();
            
            if (arrayCategory.Length > 0)
            {
                ListPerfCategories = new List<CategoryDetails>();

                WorkStatus = ResourceStrings.StatusLoading;

                _logger.Trace("Loading perf counter categories.");

                foreach (PerformanceCounterCategory cat in arrayCategory)
                {
                    try
                    {
                        ListPerfCategories.Add(new CategoryDetails(cat));
                        ++LoadedCategories;                    
                    }
                    catch (Exception ex)
                    {
                        // TODO:  create framework to display error messages.
                        string message = ex.Message;
                        WorkStatus = ResourceStrings.StatusFinished;

                    }
                }

                WorkStatus = ResourceStrings.StatusFinished;
            }
        }

        public static List<CategoryDetails> ListPerfCategories { get; set; }


    }
}
