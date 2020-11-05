using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    /// Interaction logic for AddWindow.xaml
    /// </summary>
    public partial class AddWindow : Window
    {
        private static readonly Random rng = new Random();
        private readonly PortControl control;
        private Boat template;
        public AddWindow(PortControl portControl)
        {
            InitializeComponent();
            control = portControl;
            DataContext = portControl;
            TypeCombo.SelectedIndex = 0;
        }

        private void TypeCombo_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var type = HarborHelper.RegisteredBoatTypes[(string) TypeCombo.SelectedValue];
            (_, template) = HarborHelper.GetRandomBoat(type);
            PrefixChar.Text = template.IdentityCode.First().ToString();
            PrefixLine.Visibility = Visibility.Visible;
            UniqueLabel.Text = template.Characteristic;
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e) => Close();

        private void AddButton_OnClick(object sender, RoutedEventArgs e)
        {
            string id = IdInput.Text.ToUpperInvariant();
            if (!int.TryParse(WeightInput.Text, out int weight)) DoError();
            if (!int.TryParse(SpeedInput.Text, out int speed)) DoError();
            if (!int.TryParse(UniqueInput.Text, out int unique)) DoError();
            var data = new BoatData
            {
                Type = HarborHelper.RegisteredBoatTypes[(string) TypeCombo.SelectedValue].FullName,
                Prefix = template.IdentityCode.First(),
                Code = id,
                TopSpeed = speed,
                Weight = weight,
                Characteristic = unique
            };

            try
            {
                var newBoat = Boat.FromData(data);
                control.AddBoat(newBoat);
                Close();
            }
            catch
            {
                DoError();
            }
        }

        private void DoError()
        {
            MessageBox.Show("Something went wrong. Please check your input.", "Failed to Add", MessageBoxButton.OK,
                            MessageBoxImage.Error);
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            const int lower = 'A';
            const int upper = 'Z' + 1;
            IdInput.Text = string.Create(3, rng, (chars, rand) =>
            {
                for (var i = 0; i < chars.Length; i++) chars[i] = (char) rand.Next(lower, upper);
            });
        }
    }
}
