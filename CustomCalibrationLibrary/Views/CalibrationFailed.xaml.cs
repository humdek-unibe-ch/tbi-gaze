using CustomCalibrationLibrary.Models;
using System.Windows;
using System.Windows.Controls;

namespace CustomCalibrationLibrary.Views
{
    /// <summary>
    /// Interaction logic for CalibrationFailed.xaml
    /// </summary>
    public partial class CalibrationFailed : Page
    {
        private CalibrationModel _model;
        private string _error;
        public string Error { get { return _error; } }

        public CalibrationFailed(CalibrationModel model)
        {
            _error = model.Error;
            _model = model;
            InitializeComponent();
        }

        private void OnCalibrationRestart(object sender, RoutedEventArgs e)
        {
            _model.OnCalibrationEvent(CalibrationEventType.Restart);
        }

        private void OnCalibrationAbort(object sender, RoutedEventArgs e)
        {
            _model.OnCalibrationEvent(CalibrationEventType.Abort);
        }
    }
}
