using AppDynamics.Infrastructure;
using AppDynamics.Infrastructure.Helper;
using AppDynamics.Infrastructure.Framework.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using AppDynamics.Extension.SDK.Model;
using AppDynamics.Infrastructure.Framework.Extension;
using AppDynamics.Extension.SDK;

namespace AppDynamics.Extension.Manager.ViewModel
{
    public sealed class ExtensionManagerVM : Presenter
    {

        public ExtensionManagerVM()
        {

            UIDetails = new UIProperties();

            // Launch thread to refresh Service status and command buttons.
            ThreadPool.QueueUserWorkItem(RefreshServiceStatus);
            
            LoadExtensionVMList();

        }

        private void LoadExtensionVMList()
        {
            string error = "";

            var list = ExtensionLoader.GetAllAvailableExtensions(out error);

            if (list != null && list.Count > 0)
            {
                ListExtensions = new List<ExtensionVM>();

                foreach (IExtension ext in list)
                {
                    ListExtensions.Add(new ExtensionVM(ext));
                }
            }   
            //IExtension obj = null; 
            //obj.ExecutionType
            
            if (!String.IsNullOrWhiteSpace(error))
            {
                // display message only
                //DisplayMessage("Could not found extensions", error, false);
            }
        }

        private void RefreshServiceStatus(object state)
        {
            while (true)
            {
                try
                {
                    OnPropertyChanged("ServiceStatus");

                    OnPropertyChanged("ButtonStartVisibility");

                    OnPropertyChanged("ButtonStartText");
                    
                    OnPropertyChanged("ButtonInstallText");

                }
                catch (Exception) { }

                Thread.Sleep(100);

            }
        }

        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        #region Presentation props

        public string AgentInstallationStatusMessage { get; set; }

        public string ExtensionServiceStatusMessage { get; set; }

        public bool IsAgentInstalled { get; set; }

        public bool IsExtensionServiceInstalled
        {
            get
            {
                return ServiceHelper.IsExtensionServiceInstalled();
            }
        }

        public UIProperties UIDetails { get; set; }

        public MessageDialogueVM MessageBoxDialogue { get; set; }

        public ExtensionVM ExtensionDetail { get; set; }

        public ExtensionVM NewExtension { get; set; }

        public string ServiceStatus 
        { 
            get 
            { 
                return ServiceHelper.GetExtensionServiceStatus(); 
            } 
        }

        public string ButtonInstallText
        {
            get
            {
                return (IsExtensionServiceInstalled) ? ResourceStrings.UninstallStr : ResourceStrings.InstallStr;
            }
        }

        public string ButtonStartText
        {
            get
            {
                string status = ServiceHelper.GetExtensionServiceStatus();

                return (status.Equals(ResourceStrings.ServiceStatusRunning, StringComparison.CurrentCultureIgnoreCase)) ? ResourceStrings.StopStr : ResourceStrings.StartStr;
            }
        }

        public Visibility ButtonStartVisibility
        {
            get
            {
                return (IsExtensionServiceInstalled) ? Visibility.Visible : Visibility.Hidden;
            }
        }

        public List<ExtensionVM> ListExtensions { get; set; }

        #endregion

        #region Commands and private methods

        public ICommand Install
        {
            get
            {
                return new DelegateCommand(
                    param => this.InstallUnInstallExtensionService(),
                    param => true);

            }
        }

        public ICommand StartStop
        {
            get
            {
                return new DelegateCommand(
                    param => this.StartStopExtensionService(),
                    param => true);

            }
        }

        public ICommand HideMessageBox
        {
            get
            {
                return new DelegateCommand(
                    param => this.HideMessage(),
                    param => true);
            }
        }

        public ICommand Refresh
        {
            get
            {
                return new DelegateCommand(
                    param => this.RefreshServiceStatus(),
                    param => true);
            }
        }

        public ICommand ApplyChanges
        {
            get
            {
                return new DelegateCommand(
                    param => this.RestartAgentCoordinatorService(),
                    param => true);
            }
        }

        public ICommand ConfigurePerfCounters
        {
            get
            {
                return new DelegateCommand(
                    param => this.OpenCCT(),
                    param => true);
            }
        }

        public ICommand AddnewExtension
        {
            get
            {
                return new DelegateCommand(
                    param => this.ShowCreateExtensionDialogue(),
                    param => true);
            }
        }
        public ICommand HideNewExtension
        {
            get
            {
                return new DelegateCommand(
                    param => this.HideCreateExtensionDialogue(),
                    param => true);
            }
        }
        public ICommand RefreshLoadedExtension
        {
            get
            {
                return new DelegateCommand(
                    param => this.RefreshExtensionList(),
                    param => true);
            }
        }

        private void RefreshExtensionList()
        {
            LoadExtensionVMList();
            OnPropertyChanged("ListExtensions");
        }
        
        private void ShowCreateExtensionDialogue()
        {
            NewExtension = new ExtensionVM(null);

            OnPropertyChanged("NewExtension");
        }

        private void HideCreateExtensionDialogue()
        {
            NewExtension = null;
            OnPropertyChanged("NewExtension");

            RefreshExtensionList();
        }

        private void OpenCCT()
        {
            // Open custom Configuration utility.
            // TODO: need to refernece the utility in same solution, still need to merge projects

            System.Diagnostics.Process cctprocess = new System.Diagnostics.Process();
            try
            {
                cctprocess.StartInfo.FileName = "AppDynamics.Extension.CCT.exe";
                cctprocess.Start();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Could not launch AppDynamics.Extension.CCT.exe");
                DisplayMessage("Could not launch AppDynamics.Extension.CCT.exe", ex.Message, true);
            }
        }

        private void RestartAgentCoordinatorService()
        {
            try
            {
                if (ServiceHelper.IsAgentServiceInstalled)
                {
                    try
                    {
                        ServiceHelper.RestartExtensionService(); 
                        
                        ServiceHelper.RestartAppDynamicsAgentCoordinatorService();

                        DisplayMessage("Operation completed successfully!", 
                            "AppDynamics Agent coordinator service and extension services restarted successfully.", false);

                    }
                    catch (Exception ex)
                    {
                        DisplayMessage("Operation not completed!", ex.Message, true);
                    }

                }
                else
                    DisplayMessage("AppDynamics Agent is not installed", 
                        "Please install appdynamics dot net agent for the extension to work correctly.", false);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Could not restart coordinator service.");
            }
        }

        private void RefreshServiceStatus()
        {
            ServiceHelper.RestartExtensionService();

            OnPropertyChanged("ServiceStatus");

            OnPropertyChanged("ButtonStartVisibility");

            OnPropertyChanged("ButtonStartText");

            OnPropertyChanged("ButtonInstallText");
        }

        public void StartStopExtensionService()
        {

            string status = ServiceHelper.GetExtensionServiceStatus();

            string command = "";
            if (status.Equals(ResourceStrings.ServiceStatusRunning, StringComparison.CurrentCultureIgnoreCase))
            {
                command = "stop";
            }
            else
            {
                command = "start";
            }

            manageExtensionService(command);

        }

        public void InstallUnInstallExtensionService()
        {
            
            string status = ServiceHelper.GetExtensionServiceStatus();

            string command = "";

            if (status.Equals(ResourceStrings.NotInstalledtext))
            {
               command = "install";
            }
            else
            {
                command = "uninstall";
            }

            manageExtensionService(command);
        }

        private void manageExtensionService(string command)
        {
            ExtensionFrameworkException error = ServiceHelper.ManageExtensionService(command);

            if (error == null)
            {
                string msg = "";

                if (command == "stop")
                    msg = ResourceStrings.ExtServiceStoppingMessage;
                else if (command == "start")
                    msg = ResourceStrings.ExtServiceStartedMessage;
                else if (command == "install")
                    msg = ResourceStrings.ExtServiceInstalledMessage;
                else if (command == "uninstall")
                    msg = ResourceStrings.ExtServiceUnInstalledMessage;
                

                DisplayMessage(ResourceStrings.SuccessMessage,
                    msg, false);

                //Uncommenting following line will cause auto start of service after install
                //RefreshServiceStatus();
            }
            else
            {
                DisplayMessage(String.Format(ResourceStrings.ServiceManageErrorMessage, command),
                    getCompleteErrorMessage(error), true);
            }
        }

        private string getCompleteErrorMessage(ExtensionFrameworkException error)
        {
            string message = error.Message;

            Exception inner = error.InnerException;
            while (inner != null)
            {
                message += "\n-- "+error.Message;
                inner = inner.InnerException;
            }

            if (error.NeedStackTrace)
            {
                message += "\n>> " + error.StackTrace;
            }

            return message;
        }

        private void DisplayMessage(string message, string details, bool isError)
        {
            MessageBoxDialogue = null;

            MessageBoxDialogue = new MessageDialogueVM(message, details, isError);

            OnPropertyChanged("MessageBoxDialogue");
        }

        public void HideMessage()
        {
            MessageBoxDialogue = null;
            OnPropertyChanged("MessageBoxDialogue");
        }


        public ICommand ShowExtensionDetail
        {
            get
            {
                return new DelegateCommand(
                    param => this.ShowExtensionDetailView(),
                    param => true);
            }
        }

        public ICommand HideExtensionDetail
        {
            get
            {
                return new DelegateCommand(
                    param => this.HideExtensionDetailView(),
                    param => true);
            }
        }

        public void ShowExtensionDetailView()
        {
            ExtensionDetail = SelectedExtension;
            
            OnPropertyChanged("ExtensionDetail");
        }

        public void HideExtensionDetailView()
        {
            ExtensionDetail = null;            

            OnPropertyChanged("ExtensionDetail");

            RefreshExtensionList();
        }

        public ExtensionVM SelectedExtension { get; set; }

        #endregion


    }
}
