using System.Windows;
using Harbor.Model;

namespace Harbor.Wpf
{
    /// <summary>
    /// Interaction logic for LeftToday.xaml
    /// </summary>
    public partial class LeftToday : Window
    {
        public LeftToday(PortControl control)
        {
            InitializeComponent();
            DataContext = control;
        }

        private void Button_Click(object sender, RoutedEventArgs e) => Close();
    }
}