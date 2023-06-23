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
        /// <summary>
        /// Command to restart the calibration
        /// </summary>
        public ICommand CalibrationRestartCommand { get { return _calibrationRestartCommand; } }

        private ICommand _calibrationAbortCommand;
        /// <summary>
        /// Command to abort the calibration
        /// </summary>
        public ICommand CalibrationAbortCommand { get { return _calibrationAbortCommand; } }

        /// <summary>
        /// The property change event to update the view.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        private string _error;
        /// <summary>
        /// The error message to be updated on the view.
        /// </summary>
        public string Error { get { return _error; } set { _error = value; OnPropertyChanged(); } }

        /// <summary>
        /// The property change handler to update the view.
        /// </summary>
        /// <param name="property_name">The name of the property to update</param>
        private void OnPropertyChanged([CallerMemberName] string? property_name = null)
        {
            Application.Current.Dispatcher.Invoke(() => {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property_name));
            });
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="model">The claibration model</param>
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
