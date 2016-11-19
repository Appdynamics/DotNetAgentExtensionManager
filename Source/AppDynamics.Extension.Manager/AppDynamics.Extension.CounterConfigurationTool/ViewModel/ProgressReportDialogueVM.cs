using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace AppDynamics.Extension.CCT.ViewModel
{
    public class ProgressReportDialogueVM : Presenter
    {

        public ProgressReportDialogueVM(string message)
        {
            ReportedProgress = 10;
            ProgressMessage = message;
            this.Visibility = System.Windows.Visibility.Visible;
        }

        public int ReportedProgress { get; set; }

        public string ProgressMessage { get; set; }

        public Visibility Visibility { get; set; }

    }

}

