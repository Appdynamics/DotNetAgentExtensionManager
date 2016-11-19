using AppDynamics.Extension.CCT.Infrastructure;
using AppDynamics.Extension.CCT.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AppDynamics.Extension.CCT.ViewModel
{
    public class UIProperties: Presenter
    {
        public UIProperties()
        {
            // TODO: Load resourceFile based on culture..
        }
        
        public string HeadingConFigCounters
        {
            get { return ResourceStrings.HeadingConFigCounters; }
        }

        public string HeadingPerfCounters
        {
            get
            {
                return ResourceStrings.HeadingPerfCounters;
            }
        }

        public string HeadingDescriptionCounter
        {
            get { return ResourceStrings.HeadingDescriptionCounter; }
        }

        public string HelptextDoubleClickToAdd
        {
            get { return ResourceStrings.HelptextDoubleClickToAdd; }
        }

        public string CheckBoxText
        {
            get { return ResourceStrings.CheckBoxText; }
        }

        public string ButtonSaveConfigText
        {
            get { return ResourceStrings.ButtonSaveConfigText; }
        }

        public string ButtonReloadAllText
        {
            get { return ResourceStrings.ButtonReloadAllText; }
        }

        public string ButtonAddnewExtensionText
        {
            get { return ResourceStrings.ButtonAddnewExtensionText; }
        }

        public string HelptextRemoveDuplicates
        {
            get { return ResourceStrings.HelptextRemoveDuplicates; }
        }
    }
}
