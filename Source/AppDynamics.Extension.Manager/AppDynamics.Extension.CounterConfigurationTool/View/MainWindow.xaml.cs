using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AppDynamics.Extension.CounterConfigurationTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void textCategoryFilter_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textCategoryFilter = sender as TextBox;
            if (textCategoryFilter.Text.Equals(AppDynamics.Extension.CCT.Infrastructure.ResourceStrings.FilterDefaultText))
                textCategoryFilter.Text = "";
        }


        private void textCategoryFilter_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textCategoryFilter = sender as TextBox;
            if (textCategoryFilter.Text.Equals(""))
            {
                textCategoryFilter.Text = AppDynamics.Extension.CCT.Infrastructure.ResourceStrings.FilterDefaultText;
            }
        }
    }
}
