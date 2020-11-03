using System.Linq;
using System.Windows;
using System.Windows.Controls;
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
        }

        private void AddRandom_Click(object sender, RoutedEventArgs e) => portControl?.AddBoat(HarborHelper.GetRandomBoat().boat);

        private void LeftToday_Click(object sender, RoutedEventArgs e)
        {
            var wnd = new LeftToday(portControl);
            wnd.ShowDialog();
        }

        private void RemoveFirst_Click(object sender, RoutedEventArgs e)
        {
            Boat boat = portControl?.Boats.FirstOrDefault();
            portControl?.RemoveBoat(boat);
        }

        private void TickDay_Click(object sender, RoutedEventArgs e) => portControl?.IncrementTime();

        private void ShowLog_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ToggleAutomation_OnChecked(object sender, RoutedEventArgs e)
        {
            if (portControl is null) return;

            if ((AutoSwitchOn.IsChecked ?? false) && !portControl.IsSimulating)
            {
                RandomButton.IsEnabled = false;
                portControl.StartSimulation();
            }
            else if ((AutoSwitchOff.IsChecked ?? false) && portControl.IsSimulating)
            {
                portControl.StopSimulation();
                RandomButton.IsEnabled = true;
            }
        }

        private void ToggleLogChecker_OnChecked(object sender, RoutedEventArgs e)
        {
            if (portControl is null) return;
            if ((LogSwitchOn.IsChecked ?? false) && !portControl.IsLogCheckerRunning)
            {
                portControl.StartLogChecker();
            }
            else if ((LogSwitchOff.IsChecked ?? false) && portControl.IsLogCheckerRunning)
            {
                portControl.StopLogChecker();
            }
        }

        private void SaveDataButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (portControl.SavePortData())
                MessageBox.Show("Data was saved.", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
            else
                MessageBox.Show("Failed to save data, try again.", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void LoadDataButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (portControl.LoadPortData())
                MessageBox.Show("Data was loaded.", "Loaded", MessageBoxButton.OK, MessageBoxImage.Information);
            else
                MessageBox.Show("Could not load data.", "Failure", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }

        private void ResetHarborButton_OnClick(object sender, RoutedEventArgs e)
        {
            portControl.ResetPort();
            MessageBox.Show("The harbor was reset.", "Reset", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}