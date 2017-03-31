using AppDynamics.Infrastructure;
using AppDynamics.Extension.SDK;
using AppDynamics.Extension.SDK.Model.Enumeration;
using AppDynamics.Infrastructure.Framework.Extension.Handlers;
using AppDynamics.Infrastructure.Framework.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;

namespace AppDynamics.Extension.Manager.ViewModel
{
    public sealed class ExtensionVM : Presenter
    {
        public ExtensionVM(IExtension exten)
        {
            this.Visibility = Visibility.Visible;
            
            if (exten != null)
            {
                this.Extension = exten;
                LoadXMLContent();
            }
            else
            {
                // creating new extension, empty
                this.Extension = new AppDynamics.Infrastructure.Extensions.EmptyExtension();
                Extension.ExtensionName = "Name of Extension";
                Extension.Type = ExtensionType.METRIC;
                Extension.ExecutionMode = ExecutionMode.PERIODIC;
                SelectedExecutionType = UIProperties.executionTypeForDLL;
                ExecutionPathEnabled = true;
                ExecutionPath = "";
            }
        }

        private void LoadXMLContent()
        {

            string xml = "";
            try
            {
                if (Extension != null)
                {
                    string xmlPath = Extension.Parameters[ResourceStrings.XMLParamName];

                    if (File.Exists(xmlPath))
                    {
                        XDocument xDoc = XDocument.Load(xmlPath, LoadOptions.None);

                        xml = xDoc.ToString();
                    }
                }
                else
                {
                    setmessage("Could not load extension details.");

                }
            }
            catch (Exception ex)
            {
                setmessage(ex.Message);
            }

            ExtensionXML = xml;
            this.OnPropertyChanged("ExtensionXML");
        }

        #region Properties

        public IExtension Extension {  get;  set; }

        public static IExtension SelectedExtension { get; set; }

        public Visibility Visibility { get; set; }

        public string Message { get; set; }

        public string ExtensionXML { get; set; }

        public string Name { get; set; }

        public ExtensionType SelectedExtensionType
        {
            get
            {
                return this.Extension.Type;
            }
            set
            {

                if (value == ExtensionType.METRIC)
                {
                    Extension.ExecutionMode = ExecutionMode.PERIODIC;
                }
                else if (value == ExtensionType.EVENT)
                {
                    Extension.ExecutionMode = ExecutionMode.CONTINUOUS;
                }
                else
                {
                    //do nothing, should never come here
                    //TODO: Add logger 
                    setmessage("Error: not supported");
                }
                this.Extension.Type = value;
                this.OnPropertyChanged("Extension");
                this.OnPropertyChanged("ExecutionTypeOptions");
                this.OnPropertyChanged("ControllerInfoVisibility");
            }
        }

        public string[] ExecutionTypeOptions
        {
            get
            {
                if (Extension.Type == ExtensionType.METRIC)
                    return UIProperties.executionTypeForMetrics.Split(',');
                else if (Extension.Type == ExtensionType.EVENT)
                    return UIProperties.executionTypeForEvents.Split(',');
                else return "DLL".Split(',');
            }
        }

        public string SelectedExecutionType {
            get { return this.Extension.ExecutionType.ToString(); }
            set
            {
                Extension.ExecutionType = ExecutionType.DLL;
                ExecutionPathEnabled = true;
                ExecutionPath = "";
                ExecutionPathHelpText = "";

                if (ExecutionType.SCRIPT.ToString().Equals(value, StringComparison.CurrentCultureIgnoreCase))
                {
                    Extension.ExecutionType = ExecutionType.SCRIPT;
                    ExecutionPathHelpText = UIProperties.HelpTextExecutionPathforScript;
                }
                else if (UIProperties.executionTypeForDLL.Equals(value, StringComparison.CurrentCultureIgnoreCase))
                {
                    ExecutionPathHelpText = UIProperties.HelpTextExecutionPathforDLL;
                }
                else if (ResourceStrings.InternalExtensionFriendlyNameWindowsEventLogMonitor
                    .Equals(value, StringComparison.CurrentCultureIgnoreCase))
                {
                    /// for WindowsEventLogMonitor or any other future inbuilt extension #internalextension #inbuiltextension 
                    Extension.ExecutionType = ExecutionType.DLL;
                    ExecutionPathEnabled = false;
                    ExecutionPath = ResourceStrings.InternalExtensionFriendlyNameWindowsEventLogMonitor;
                }
                this.OnPropertyChanged("ExecutionPath");
                this.OnPropertyChanged("ExecutionPathEnabled");
            }
        }

        public string ExecutionPath { get; set; }

        public bool ExecutionPathEnabled { get; set; }

        public string ExecutionPathHelpText { get; set; }

        public Visibility ControllerInfoVisibility
        {
            get
            {
                return ((Extension != null) && (Extension.Type == ExtensionType.EVENT))
                    ? Visibility.Visible : Visibility.Hidden;
            }
        }

        #endregion

        #region command and methods

        #region Creating New extension

        public ICommand ValidateNewExtension
        {
            get
            {
                return new DelegateCommand(
                    param => this.ValidateExtension(),
                    param => true);
            }
        }

        public ICommand SaveExtension
        {
            get
            {
                return new DelegateCommand(
                    param => this.SaveNewExtension(),
                    param => true);
            }
        }

        private object ValidateExtension()
        {
            throw new NotImplementedException();
        }

        private object SaveNewExtension()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Manage extension
        public ICommand UpdateExtensionXML
        {
            get
            {
                return new DelegateCommand(
                    param => this.SaveXML(),
                    param => true);
            }
        }

        public ICommand ValidateExtensionXML
        {
            get
            {
                return new DelegateCommand(
                    param => this.validateXML(),
                    param => true);
            }
        }

        public ICommand IgnoreChanges
        {
            get
            {
                return new DelegateCommand(
                    param => this.LoadXMLContent(),
                    param => true);
            }
        }

        private void SaveXML()
        {
            if (validateXML())
            {
                if (Extension != null)
                {
                    string xmlPath = Extension.Parameters[ResourceStrings.XMLParamName];

                    if (File.Exists(xmlPath))
                    {
                        try
                        {
                            StringReader reader = new StringReader(ExtensionXML);

                            XDocument xDoc = XDocument.Load(reader);

                            xDoc.Save(xmlPath);

                            setmessage("XML changes are saved successfully.");
                        }
                        catch (Exception ex)
                        {
                            // display error message to user on screen
                            setmessage(ex.Message);
                        }
                    }
                }
            }
        }

        private bool validateXML()
        {
            bool isValid = false;

            try
            {
                StringReader reader = new StringReader(ExtensionXML);

                XDocument xDoc = XDocument.Load(reader);

                isValid = ExtensionXMLHandler.ValidateExtensionXML(xDoc);

                setmessage("Success: XML is validated.");

            }
            catch (Exception ex)
            {
                // display error message to user on screen
                setmessage(ex.Message);
            }

            return isValid;
        }

        private void setmessage(string m)
        {
            Message = m;

            this.OnPropertyChanged("Message");
        }
        #endregion

        #endregion
    }
}
