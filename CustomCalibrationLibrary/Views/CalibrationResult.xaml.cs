using CustomCalibrationLibrary.Models;
using CustomCalibrationLibrary.ViewModels;
using ModernWpf.Controls;
using System;
using System.Windows;

namespace CustomCalibrationLibrary.Views
{
    /// <summary>
    /// Interaction logic for CalibrationResult.xaml
    /// </summary>
    public partial class CalibrationResult : System.Windows.Controls.Page
    {
        private CalibrationModel _model;
        public CalibrationResult(CalibrationModel model)
        {
            InitializeComponent();
            _model = model;
            DataContext = new CalibrationResultViewModel(_model);
        }

        private void OnGazeToggle(object sender, RoutedEventArgs e)
        {
            Visibility visibility = Visibility.Collapsed;
            ToggleSwitch? toggleSwitch = sender as ToggleSwitch;
            if (toggleSwitch != null)
            {
                if (toggleSwitch.IsOn == true)
                {
                    visibility = Visibility.Visible;
                }
                else
                {
                    visibility = Visibility.Collapsed;
                }
            }

            _model.GazePoint = new GazePoint(_model.GazePoint.X, _model.GazePoint.Y, visibility);
        }

        private void OnCalibrationRestart(object sender, RoutedEventArgs e)
        {
            _model.OnCalibrationEvent(CalibrationEventType.Restart);
        }

        private void OnCalibrationAccept(object sender, RoutedEventArgs e)
        {
            _model.OnCalibrationEvent(CalibrationEventType.Accept);
        }
    }
}
