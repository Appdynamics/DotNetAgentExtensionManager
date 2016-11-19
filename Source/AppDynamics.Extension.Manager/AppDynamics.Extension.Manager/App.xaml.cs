using AppDynamics.Extension.Manager.View;
using AppDynamics.Extension.Manager.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace AppDynamics.Extension.Manager
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
            var mainWindow = new ExtensionManagerView();

            var viewModel = new ExtensionManagerVM();

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
