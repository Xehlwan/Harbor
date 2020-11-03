using System.Linq;
using System.Windows;
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

        private void AddRandom_Click(object sender, RoutedEventArgs e)
        {
            portControl.AddBoat(HarborHelper.GetRandomBoat());
        }

        private void LeftToday_Click(object sender, RoutedEventArgs e)
        {
            var wnd = new LeftToday(portControl);
            wnd.ShowDialog();
        }

        private void RemoveFirst_Click(object sender, RoutedEventArgs e)
        {
            Boat boat = portControl.Boats.FirstOrDefault();
            portControl.RemoveBoat(boat);
        }

        private void TickDay_Click(object sender, RoutedEventArgs e)
        {
            portControl.IncrementTime();
        }

        private void ShowLog_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}