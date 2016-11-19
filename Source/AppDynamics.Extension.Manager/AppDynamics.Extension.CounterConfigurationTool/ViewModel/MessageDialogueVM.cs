using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace AppDynamics.Extension.CCT.ViewModel
{
    public class MessageDialogueVM : Presenter
    {

        public MessageDialogueVM(string message, string details, bool isError)
        {
            // TODO: Complete member initialization
            this.ErrorMessage = message;
            this.ErrorMessageDetail = details;
            this.IsError = isError;
            ErrorVisibility = (isError) ? Visibility.Visible : Visibility.Hidden;
            InfoVisibility = (isError) ? Visibility.Hidden : Visibility.Visible;
            this.Visibility = System.Windows.Visibility.Visible;
        }

        public string ErrorMessage { get; set; }

        public string ErrorMessageDetail { get; set; }

        public bool IsError { get; set; }

        public Visibility Visibility { get; set; }

        public ICommand HideMessageBox { get; set; }

        public Visibility ErrorVisibility { get; set; }
        public Visibility InfoVisibility { get; set; }
    }

}

