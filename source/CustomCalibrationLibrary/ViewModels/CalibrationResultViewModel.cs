using CustomCalibrationLibrary.Commands;
using CustomCalibrationLibrary.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace CustomCalibrationLibrary.ViewModels
{
    public class GazePoint
    {
        public double X { get; set; } = 0;
        public double Y { get; set; } = 0;
        public bool Visibility { get; set; } = false;
    }

    class CalibrationResultViewModel : CalibrationViewModel, INotifyPropertyChanged
    {
        private ICommand _calibrationRestartCommand;
        public ICommand CalibrationRestartCommand { get { return _calibrationRestartCommand; } }

        private ICommand _calibrationAcceptCommand;
        public ICommand CalibrationAcceptCommand { get { return _calibrationAcceptCommand; } }

        private ICommand _gazeVisibilityCommand;
        public ICommand GazeVisibilityCommand { get { return _gazeVisibilityCommand; } }

        public event PropertyChangedEventHandler? PropertyChanged;
        private GazePoint _gazePoint;
        public GazePoint GazePoint
        {
            get { return _gazePoint; }
        }
        /// <summary>
        /// Called when when the state property of EyeTracker is changing.
        /// </summary>
        /// <param name="property_name">Name of the property in WPF.</param>
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public CalibrationResultViewModel(CalibrationModel model) : base(model)
        {
            _gazePoint = new GazePoint();
            _model.GazePointChanged += OnGazePointChanged;
            _calibrationRestartCommand = new CalibrationCommand(model, CalibrationEventType.Restart);
            _calibrationAcceptCommand = new CalibrationCommand(model, CalibrationEventType.Accept);
            _gazeVisibilityCommand = new GazeVisibilityCommand(this);
        }

        private void OnGazePointChanged(object? sender, Point point)
        {
            if (sender == null)
            {
                return;
            }
            _gazePoint.X = point.X;
            _gazePoint.Y = point.Y;
            OnPropertyChanged("GazePoint");
        }

        public void OnGazeToggle()
        {
            _gazePoint.Visibility = !_gazePoint.Visibility;
            OnPropertyChanged("GazePoint");
        }
    }
}
