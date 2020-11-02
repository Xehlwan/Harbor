using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Harbor.Console;
using Harbor.Model;

namespace Harbor.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly PortControl portControl;
        public MainWindow(PortControl control)
        {
            InitializeComponent();
            portControl = control;
            DataContext = portControl;
            portControl.StartLogChecker();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            portControl.AddBoat(HarborHelper.GetRandomBoat());
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var boat = portControl.Boats.First();
            portControl.RemoveBoat(boat);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            portControl.IncrementTime();
        }
    }
}
