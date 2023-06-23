using CustomCalibrationLibrary.Commands;
using CustomCalibrationLibrary.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace CustomCalibrationLibrary.ViewModels
{
    public class GazePoint : INotifyPropertyChanged
    {
        private double _x = 0;
        public double X
        {
            get { return _x; }
            set { _x = value; OnPropertyChanged(); }
        }

        private double _y = 0;
        public double Y
        {
            get { return _y; }
            set { _y = value; OnPropertyChanged(); }
        }

        private bool _visibility = false;
        public bool Visibility
        {
            get { return _visibility; }
            set { _visibility = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;


        /// <summary>
        /// Called when when the state property of EyeTracker is changing.
        /// </summary>
        /// <param name="property_name">Name of the property in WPF.</param>
        private void OnPropertyChanged([CallerMemberName] string? property_name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property_name));
        }
    }

    class CalibrationResultViewModel : CalibrationViewModel
    {
        private ICommand _calibrationRestartCommand;
        public ICommand CalibrationRestartCommand { get { return _calibrationRestartCommand; } }

        private ICommand _calibrationAcceptCommand;
        public ICommand CalibrationAcceptCommand { get { return _calibrationAcceptCommand; } }

        private ICommand _gazeVisibilityCommand;
        public ICommand GazeVisibilityCommand { get { return _gazeVisibilityCommand; } }

        private GazePoint _gazePoint;
        public GazePoint GazePoint
        {
            get { return _gazePoint; }
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
        }

        public void OnGazeToggle()
        {
            _gazePoint.Visibility = !_gazePoint.Visibility;
        }
    }
}
