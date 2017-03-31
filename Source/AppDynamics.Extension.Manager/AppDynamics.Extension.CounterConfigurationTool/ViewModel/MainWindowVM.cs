using AppDynamics.Extension.CCT.Infrastructure;
using AppDynamics.Extension.CCT.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace AppDynamics.Extension.CCT.ViewModel
{
    public sealed class MainWindowVM : Presenter
    {

        public MainWindowVM()
        {
            _showInstances = false;
            _filterText = ResourceStrings.FilterDefaultText;

            CanSaveConfig = false;
            UIDetails = new UIProperties();
            ConfigFileLocation = ResourceStrings.DefaultConfigFileText;

            cHelper = new ConfigHelper();
            cHelper.Initialize();

            PrepareBackGroundWorker();

            InitializeAsynchronously();
        }

        #region presentation properties

        public bool ShowInstances { 
            get { return _showInstances; }
            set 
            {
                _showInstances = value;
                ToggleInstanceVisibility();
                this.OnPropertyChanged("ShowInstances");
            }
        }

        public string Filtertext
        {
            get { return _filterText; }
            set
            {
                this._filterText = value;
                FilterTextChange();
                this.OnPropertyChanged("Filtertext");
            }
        }

        public bool CanSaveConfig { get; set; }

        public UIProperties UIDetails { get; set; }

        public string ConfigFileLocation { get; set; }

        public IEnumerable<CategoryDetails> ListCategories { get; set; }

        public IEnumerable<PerformanceCounterDetail> ListCategoryCounters
        {
            get
            {
                CategoryDetails category = getSelectedCategory();
                if (category != null)
                {
                    var counters = _showInstances ? category.ListCounterDetails : 
                        category.ListCounterDetailWithoutInstances;
                    return counters;
                }
                else
                {
                    return null;
                }
            }
        }

        public ObservableCollection<PerformanceCounterDetail> ListConfigCounter 
        {
            get
            {
                if (_listConfigCounter == null)
                {
                    _listConfigCounter = new List<PerformanceCounterDetail>();
                }
                return new ObservableCollection<PerformanceCounterDetail>(_listConfigCounter);
            }
            set
            {
                _listConfigCounter = value.ToList<PerformanceCounterDetail>();
            }
        }

        public int SelectedCategoryIndex
        {
            get { return _selectedCategoryindex; }
            set
            {
                this._selectedCategoryindex = value;
                CategorySelectionChanged();
                this.OnPropertyChanged("SelectedCategoryIndex");
                this.OnPropertyChanged("ListCategoryCounters");
                this.OnPropertyChanged("CounterDetailWithXML");
            }
        }

        public int SelectedCounterIndex
        {
            get
            {
                if (_selectedCounterIndex < 0 && _listConfigCounter.Count > 0)
                {
                    _selectedCounterIndex = 0;
                }
                return _selectedCounterIndex;
            }
            set
            {
                this._selectedCounterIndex = value;
                this.OnPropertyChanged("CounterDetailWithXML");
            }
        }

        public PerformanceCounterDetail SelectedCounter { get; set; }

        public MessageDialogueVM MessageBoxDialogue { get; set; }

        public ProgressReportDialogueVM ProgressBar { get; set; }

        public string CounterDetailWithXML {
            get 
            {
                PerformanceCounterDetail cd = ListCategoryCounters.ToList<PerformanceCounterDetail>()[_selectedCounterIndex];

                string details =ViewHelper.GenerateDetails(cd, ShowInstances, getSelectedCategory());
                return details;
            } 
        }

        public string ListNoCountersText
        {
            get
            {
                string noCountertext =
                    (_listConfigCounter != null && _listConfigCounter.Count > 0) ?
                    "" : ResourceStrings.TextNoCounters;

                return noCountertext;
            }
        }

        #endregion

        #region private properties and delegates

        private bool _showInstances;

        private string _filterText;

        private DelegateCommand _saveCountersInConfig;

        private System.ComponentModel.BackgroundWorker backGroundWorker;

        private static List<CategoryDetails> _listCategories;

        private List<PerformanceCounterDetail> _listConfigCounter;

        private int _selectedCategoryindex = 1;

        private int _selectedCounterIndex = 0;

        //TODO: Hack to avoid hang in 2003 server while reload.
        private bool _is2003Reload = false;

        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        #endregion

        #region Async operations

        private void PrepareBackGroundWorker()
        {
            backGroundWorker = new System.ComponentModel.BackgroundWorker();

            backGroundWorker.DoWork += backGroundWorker_DoWork;

            backGroundWorker.WorkerReportsProgress = true;


            backGroundWorker.RunWorkerCompleted += backGroundWorker_RunWorkerCompleted;

            backGroundWorker.ProgressChanged += backGroundWorker_Progress;
        }

        ConfigHelper cHelper = null;

        private void InitializeAsynchronously()
        {
            #region Synchronous

            VerifyAndReadConfig(null);

            _listConfigCounter = ViewHelper.RemoveDuplicate(_listConfigCounter);

            OnPropertyChanged("ListConfigCounter");

            OnPropertyChanged("ListNoCountersText");

            #endregion

            ProgressBar = new ProgressReportDialogueVM("Loading Categories.");

            OnPropertyChanged("ProgressBar");
            try
            {
                backGroundWorker.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                DisplayMessage(ex.Message, ex.StackTrace, true);
            }
            ThreadPool.QueueUserWorkItem(reportProgress);

        }

        private void backGroundWorker_DoWork(object Sender, System.ComponentModel.DoWorkEventArgs e)
        {
            InitializePerformanceCounters(null);
        }


        private void backGroundWorker_RunWorkerCompleted(object Sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                // ToDO: hide progress animated..
                if (_listCategories != null && _listCategories.Count > 0)
                {
                    ListCategories = _listCategories;
                    this.OnPropertyChanged("ListCategories");
                    CanSaveConfig = true;
                    this.OnPropertyChanged("CanSaveConfig");

                }
                else
                {
                    DisplayMessage("Can not load Performance categories", cHelper.LastException, true);
                }
                
            }
            else
            {
                // in case of exception
            }

            FinishProgressBar();
        }

        private void backGroundWorker_Progress(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            //textProgressReport.Text = e.UserState.ToString();
            string message = (e != null && e.UserState != null) ? e.UserState.ToString() : "Reading All Available Performance Counter Categories.";

            if (ProgressBar == null)
            {
                ProgressBar = new ProgressReportDialogueVM(message);
            }

            ProgressBar.ReportedProgress = e.ProgressPercentage;
            
            ProgressBar.ProgressMessage = message;

            OnPropertyChanged("ProgressBar");
        }

        private void reportProgress(object state)
        {
            while (true)
            {
                if (PerformanceCounterHandler.WorkStatus.Equals(ResourceStrings.StatusLoading))
                {
                    string status = "Loading Performance Counter Categories " +
                        PerformanceCounterHandler.LoadedCategories;

                    backGroundWorker.ReportProgress(PerformanceCounterHandler.LoadedCategories, status);

                    Thread.Sleep(5);
                }
                else if (PerformanceCounterHandler.WorkStatus.Equals(ResourceStrings.StatusReading))
                {
                    string status = "Reading All Available Performance Counter Categories.";

                    backGroundWorker.ReportProgress(10, status);

                    Thread.Sleep(10);
                }
                else
                {
                    break;
                }
            }
        }

        private void InitializePerformanceCounters(object state)
        {
            //Initialize PerformanceCounter Objects
            try
            {
                if (!_is2003Reload)
                    PerformanceCounterHandler.Initialize();
            }
            catch (Exception ex)
            {
                // TODO:
                DisplayMessage("Can not read performance counters", ex.Message, true);
            }
            var results = PerformanceCounterHandler.ListPerfCategories;

            _listCategories = results.OrderBy(categories => categories.CategoryName).ToList<CategoryDetails>();
        }

        private void VerifyAndReadConfig(object state)
        {

            if (cHelper.isConfigFileLocated)
            {
                _listConfigCounter = cHelper.ReadAppDynamicsConfigFile();

                ConfigFileLocation = cHelper.ConfigFilelocation;
            }
            else
            {
                _listConfigCounter = new List<PerformanceCounterDetail>();

                ConfigFileLocation = ResourceStrings.ErrorConfigFileNotFound;
            }
        }

        #endregion

        #region Commands

        public ICommand SaveCounters
        {
            get
            {
                if (_saveCountersInConfig == null)
                {
                    _saveCountersInConfig = new DelegateCommand(
                        param => this.SaveCountersInConfig(),
                        param => this.CanSaveConfig);
                }
                return _saveCountersInConfig;
            }
        }

        public ICommand ReloadAll
        {
            get
            {
                return new DelegateCommand(
                    param => this.ReloadAllCounters(),
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

        public ICommand CopyToClip
        {
            get
            {
                return new DelegateCommand(
                    param => this.CopyXMLToClipBoard(),
                    param => true);
            }
        }
        #endregion

        #region EventHandler methods

        private void DisplayMessage(string message, string details, bool isError)
        {
            MessageBoxDialogue = null;

            MessageBoxDialogue = new MessageDialogueVM(message, details, isError);

            OnPropertyChanged("MessageBoxDialogue");
        }

        public void ToggleInstanceVisibility()
        {
            if (_showInstances)
            {

            }
            else
            {

            }
            this.OnPropertyChanged("ListCategoryCounters");
        }

        private void CategorySelectionChanged()
        {

        }

        private void CounterSelectionChanged()
        {

        }

        public void SaveCountersInConfig()
        {
            if (cHelper.isConfigFileLocated)
            {
                bool isSaved = cHelper.AddPerformanceCounterstoConfig(_listConfigCounter);

                if (isSaved)
                {
                    DisplayMessage(ResourceStrings.MessageConfigSaved,
                        String.Format(ResourceStrings.MessageConfigSavedDetail, ConfigFileLocation),
                        false);
                }
                else
                {
                    string error = cHelper.LastException;

                    CopyXMLToClipBoard();

                    DisplayMessage(ResourceStrings.ErrorConfigSaveOperation, error, false);
                }
            }
            else
            {
                CopyXMLToClipBoard();

                DisplayMessage(ResourceStrings.ErrorConfigSaveOperation,
                    ResourceStrings.ErrorConfigFileNotFound, false);

            }

        }

        public void ReloadAllCounters()
        {
            _listConfigCounter = null;

            OnPropertyChanged("ListConfigCounter");

            _is2003Reload = (Environment.OSVersion.Version.Minor == 2);

            ListCategories = null;
            
            OnPropertyChanged("ListCategories");

            OnPropertyChanged("ListCategoryCounters");

            InitializeAsynchronously();
        }

        public void CounterConfigDoubleClick()
        {
            if (SelectedCounter != null)
            {
                SelectedCounter.IsAdded = true;

                if (_listConfigCounter == null)
                {
                    _listConfigCounter = new List<PerformanceCounterDetail>();
                }

                _listConfigCounter.Add(SelectedCounter);

                _listConfigCounter = ViewHelper.RemoveDuplicate(_listConfigCounter);

                OnPropertyChanged("ListConfigCounter");

                OnPropertyChanged("ListNoCountersText");
            }

        }

        public void HideMessage()
        {
            MessageBoxDialogue = null;
            OnPropertyChanged("MessageBoxDialogue");
        }

        public void FinishProgressBar()
        {

            ProgressBar.ReportedProgress = 100;

            ProgressBar.ProgressMessage = "Loading completed..";

            OnPropertyChanged("ProgressBar");

            Thread.Sleep(50);

            ProgressBar = null;

            OnPropertyChanged("ProgressBar");

        }

        public void CopyXMLToClipBoard()
        {
            try
            {
                string text = ConfigHelper.GetXMLFromXMLCounterList(_listConfigCounter);

                Clipboard.SetText(text);
                
                DisplayMessage(ResourceStrings.ClipBoardCopyMessage,
                    text, false);
            }
            catch (Exception ex)
            {
                DisplayMessage("Can not copy to clipboard",
                    ex.Message, true);
            }
        }

        #endregion

        #region internal Methods

        public void FilterTextChange()
        {
            string text = _filterText.Trim().ToLower();
            if (_listCategories != null && _listCategories.Count > 0)
            {
                if (!text.Equals(ResourceStrings.FilterDefaultText,StringComparison.CurrentCultureIgnoreCase) &&
                        text.Length > 0)
                {
                    var filteredCategories = _listCategories.Where(c => c.CategoryName.ToLower().Contains(text));
                    ListCategories = filteredCategories;
                    this.OnPropertyChanged("ListCategories");

                }
                else
                {
                    if (ListCategories == null || _listCategories.Count != ListCategories.Count())
                    {
                        ListCategories = _listCategories;
                        this.OnPropertyChanged("ListCategories");
                    }
                }
            }
        }

        private CategoryDetails getSelectedCategory()
        {
            CategoryDetails cd = null;
            if (ListCategories != null && ListCategories.Count() > _selectedCategoryindex && _selectedCategoryindex >= 0)
            {
                cd = ListCategories.ElementAt(_selectedCategoryindex);
            }
            return cd;
        }

        #endregion

    }
}
