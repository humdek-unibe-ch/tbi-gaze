using CustomCalibrate.Models;
using CustomCalibrate.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace CustomCalibrate.Views
{
    /// <summary>
    /// Interaction logic for Calibration.xaml
    /// </summary>
    public partial class Calibration : Page
    {
        private CalibrationModel _model;

        public Calibration(CalibrationModel model)
        {
            InitializeComponent();
            _model = model;
            DataContext = new CalibrationViewModel(_model);
        }

        public void NextCalibrationPoint(object sender, RoutedEventArgs e)
        {
            _model.NextCalibrationPoint();
        }

        public void GazeDataCollected(object sender, RoutedEventArgs e)
        {
            _model.GazeDataCollected();
        }

        public void CalibrationResult(object sender, RoutedEventArgs e)
        {
            _model.Status = CalibrationModel.CalibrationStatus.DataResult;
        }
    }
}
