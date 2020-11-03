using System.Windows;
using Harbor.Console;

namespace Harbor.Wpf
{
    /// <summary>
    /// Interaction logic for LeftToday.xaml
    /// </summary>
    public partial class LeftToday : Window
    {
        private readonly PortControl portControl;

        public LeftToday(PortControl control)
        {
            InitializeComponent();
            portControl = control;
            DataContext = portControl;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}