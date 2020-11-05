using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Harbor.Wpf
{
    /// <summary>
    /// Interaction logic for LogWindow.xaml
    /// </summary>
    public partial class LogWindow : Window
    {
        private string path;
        public LogWindow(string path)
        {
            InitializeComponent();
            this.path = path;
            if (File.Exists(path))
            {
                LogText.Text = File.ReadAllText(path);
            }
            Loaded += (e, o) => LogText.ScrollToEnd();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            File.Delete(path);
            LogText.Text = string.Empty;
        }
    }
}
