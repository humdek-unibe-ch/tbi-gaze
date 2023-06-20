using System;
using System.Windows;
using System.Windows.Controls;

namespace CustomCalibrationLibrary.Views
{
    /// <summary>
    /// Interaction logic for Computing.xaml
    /// </summary>
    public partial class Computing : Page
    {
        public Computing()
        {
            InitializeComponent();

            this.Initialized += Computing_Initialized;
            this.Loaded += Computing_Loaded;
            this.Unloaded += Computing_Unloaded;
        }

        private void Computing_Unloaded(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Unloaded Computing");
        }

        private void Computing_Loaded(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Loaded Computing");
        }

        private void Computing_Initialized(object? sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Initialized Computing");
        }
    }
}
