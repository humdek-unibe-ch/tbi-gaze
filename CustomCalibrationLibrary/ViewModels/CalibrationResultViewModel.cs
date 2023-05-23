using CustomCalibrationLibrary.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CustomCalibrationLibrary.ViewModels
{
    class CalibrationResultViewModel : CalibrationViewModel, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private GazePoint _gazePoint;
        public GazePoint GazePoint
        {
            get { return _gazePoint; }
            set { _gazePoint = value; OnPropertyChanged(); }
        }
        /// <summary>
        /// Called when when the state property of EyeTracker is changing.
        /// </summary>
        /// <param name="property_name">Name of the property in WPF.</param>
        private void OnPropertyChanged([CallerMemberName] string? property_name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property_name));
        }

        public CalibrationResultViewModel(CalibrationModel model) : base(model)
        {
            _gazePoint = new GazePoint();
            _model.GazePointChanged += OnGazePointChanged;
        }

        private void OnGazePointChanged(object? sender, GazePoint gazePoint)
        {
            if (sender == null)
            {
                return;
            }
            GazePoint = new GazePoint(gazePoint.X, gazePoint.Y, gazePoint.Visibility);
        }
    }
}
