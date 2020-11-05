using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Harbor.Model;

namespace Harbor.Wpf
{
    /// <summary>
    /// Interaction logic for TurnedAwayWindow.xaml
    /// </summary>
    public partial class TurnedAwayWindow : Window
    {
        public TurnedAwayWindow(PortControl control)
        {
            InitializeComponent();
            DataContext = control;
        }

        private void Button_Click(object sender, RoutedEventArgs e) => Close();
    }
}
