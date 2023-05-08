using CustomCalibrate.Models;
using CustomCalibrate.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace CustomCalibrate
{
    /// <summary>
    /// Interaction logic for Calibration.xaml
    /// </summary>
    public partial class Calibration : Page
    {
        private CalibrationViewModel _viewModel;
        private CalibrationModel _model;

        public Calibration(CalibrationModel model)
        {
            InitializeComponent();
            _model = model;
            _viewModel = new CalibrationViewModel(_model);
            DataContext = _viewModel;
        }

        public void OnLoad(object sender, RoutedEventArgs e)
        {
            _model.Init(ActualWidth, ActualHeight);
        }

        public void NextCalibrationPoint(object sender, RoutedEventArgs e)
        {
            _viewModel.NextCalibrationPoint();
        }

        public void GazeDataCollected(object sender, RoutedEventArgs e)
        {
            _viewModel.GazeDataCollected();
        }
    }
}
