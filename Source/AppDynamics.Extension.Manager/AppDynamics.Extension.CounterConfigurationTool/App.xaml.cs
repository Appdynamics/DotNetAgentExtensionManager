using AppDynamics.Extension.CCT.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace AppDynamics.Extension.CounterConfigurationTool
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initialize main window and view model
            var mainWindow = new MainWindow();

            var viewModel = new MainWindowVM();
            
            // When the ViewModel asks to be closed, close the window.
            EventHandler handler = null;

            handler = delegate
            {
                viewModel.RequestClose -= handler;
                mainWindow.Close();
            };

            viewModel.RequestClose += handler;

            mainWindow.DataContext = viewModel;

            mainWindow.Show();
        }
    }
}
