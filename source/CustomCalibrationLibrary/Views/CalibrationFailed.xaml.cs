using CustomCalibrationLibrary.Commands;
using CustomCalibrationLibrary.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CustomCalibrationLibrary.Views
{
    /// <summary>
    /// Interaction logic for CalibrationFailed.xaml
    /// </summary>
    public partial class CalibrationFailed : Page, INotifyPropertyChanged
    {
        private CalibrationModel _model;

        private ICommand _calibrationRestartCommand;
        public ICommand CalibrationRestartCommand { get { return _calibrationRestartCommand; } }

        private ICommand _calibrationAbortCommand;
        public ICommand CalibrationAbortCommand { get { return _calibrationAbortCommand; } }

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
            Focus();
            _calibrationRestartCommand = new CalibrationCommand(model, CalibrationEventType.Restart);
            _calibrationAbortCommand = new CalibrationCommand(model, CalibrationEventType.Abort);
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
