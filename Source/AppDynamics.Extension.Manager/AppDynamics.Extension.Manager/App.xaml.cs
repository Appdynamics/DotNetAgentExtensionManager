/*
 *  AppDynamics Manager for .Net Agent Extension
 *  Author: Anurag Bajpai @ AppDynamics Inc. (abajpai@appdynamics.com)
 *    
 *  Copyright 2015 AppDynamics LLC
 *  
 *  Licensed under the Apache License, Version 2.0 (the "License");
 *  you may not use this file except in compliance with the License.
 *  You may obtain a copy of the License at
 *  
 *      http://www.apache.org/licenses/LICENSE-2.0
 *  
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 */
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
