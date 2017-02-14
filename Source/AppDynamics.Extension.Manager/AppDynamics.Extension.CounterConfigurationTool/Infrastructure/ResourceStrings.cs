using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AppDynamics.Extension.CCT.Infrastructure
{
    class ResourceStrings
    {
        internal const string PerfCounterXmlTemplate = "<perf-counter cat=\"{0}\" name=\"{1}\" instance=\"{2}\"/>";

        internal const string PerfCounterRootName = "perf-counters";
        internal const string PerfCounterElementName = "perf-counter";

        // TODO: change to status enum
        public const string StatusFinished = "Finished";
        public const string StatusLoading = "Loading";
        public const string StatusReading = "Reading";

        public const string NoInstanceMessage ="No Instances- Single instance category";

        public const string AllInstanceName = "all";


        public const string HeadingConFigCounters = "Performance Counters Defined in config.xml";
        public const string HeadingPerfCounters = "Available Performance Category and Counters";
        public const string HeadingDescriptionCounter = "Description of selected category and counter";
        public const string HelptextDoubleClickToAdd = "* Double click on any counter (instance) to add it to config file counter list.";
        public const string HelptextRemoveDuplicates = "* Removes duplicate counters while adding here or reading from config.xml";
        public const string CheckBoxText = "Show Instances (check to add specific instance)";


        public const string FilterDefaultText = "-Type Category to Filter-";
        public const string DefaultConfigFileText = "Loading config.xml";
        public const string ButtonSaveConfigText = "Save Configuration";
        public const string ButtonAddnewExtensionText = "Add New Extension";
        public const string ButtonReloadAllText ="Reload All";

        public const string MessageConfigSavedDetail = "Performance Counters updated to: \n{0}";
        public const string MessageConfigSaved = "Config file saved successfully!";

        public const string ErrorConfigSaveOperation = "Sorry! Could not save config file.\n Please copy the xml manually. XML is copied to clipboard";
        public const string ErrorConfigFileNotFound = "Sorry! could not locate config.xml";

        public const string TextNoCounters = "No Counters defined in config.xml. \nPlease double click on performance counters to add here.";


        public const string ClipBoardCopyMessage = "Successfully copied XML for listed counters to clipboard.";

    }
}
