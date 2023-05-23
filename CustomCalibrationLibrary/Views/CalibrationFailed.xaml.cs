using CustomCalibrationLibrary.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace CustomCalibrationLibrary.Views
{
    /// <summary>
    /// Interaction logic for CalibrationFailed.xaml
    /// </summary>
    public partial class CalibrationFailed : Page, INotifyPropertyChanged
    {
        private CalibrationModel _model;

        public event PropertyChangedEventHandler? PropertyChanged;
        private string _error;
        public string Error { get { return _error; } set { _error = value; OnPropertyChanged(); } }

        private void OnPropertyChanged([CallerMemberName] string? property_name = null)
        {
            Application.Current.Dispatcher.Invoke(() => {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property_name));
            });
        }

        public CalibrationFailed(CalibrationModel model)
        {
            _error = model.Error;
            _model = model;
            _model.PropertyChanged += OnPropertyChanged;
            InitializeComponent();
            DataContext = this;
        }

        private void OnCalibrationRestart(object sender, RoutedEventArgs e)
        {
            _model.OnCalibrationEvent(CalibrationEventType.Restart);
        }

        private void OnCalibrationAbort(object sender, RoutedEventArgs e)
        {
            _model.OnCalibrationEvent(CalibrationEventType.Abort);
        }

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender == null || e.PropertyName != "Error")
            {
                return;
            }
            Error = ((CalibrationModel)sender).Error;
        }
    }
}
