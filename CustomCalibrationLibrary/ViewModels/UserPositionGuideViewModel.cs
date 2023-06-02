using CustomCalibrationLibrary.Models;
using System.ComponentModel;
using System.Windows;
using System.Runtime.CompilerServices;
using Tobii.Research;
using System.Windows.Input;
using CustomCalibrationLibrary.Commands;

namespace CustomCalibrationLibrary.ViewModels
{
    class UserPositionGuideViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private UserPositionGuide _left;
        public UserPositionGuide Left
        {
            get { return _left; }
            set { _left = value; OnPropertyChanged(); }
        }

        private UserPositionGuide _right;
        public UserPositionGuide Right
        {
            get { return _right; }
            set { _right = value; OnPropertyChanged(); }
        }

        private ICommand _calibrationStartCommand;
        public ICommand CalibrationStartCommand { get { return _calibrationStartCommand; } }

        private ICommand _calibrationAbortCommand;
        public ICommand CalibrationAbortCommand { get { return _calibrationAbortCommand; } }

        /// <summary>
        /// Called when when the state property of EyeTracker is changing.
        /// </summary>
        /// <param name="property_name">Name of the property in WPF.</param>
        private void OnPropertyChanged([CallerMemberName] string? property_name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property_name));
        }

        public UserPositionGuideViewModel(CalibrationModel model)
        {
            _left = new UserPositionGuide(new NormalizedPoint3D((float)0.5, (float)0.5, (float)0.5), Validity.Invalid);
            _right = new UserPositionGuide(new NormalizedPoint3D((float)0.5, (float)0.5, (float)0.5), Validity.Invalid);
            model.UserPositionGuideChanged += OnUserPositionGuideChanged;
            _calibrationStartCommand = new CalibrationCommand(model, CalibrationEventType.Start);
            _calibrationAbortCommand = new CalibrationCommand(model, CalibrationEventType.Abort);
        }

        private void OnUserPositionGuideChanged(object? sender, UserPositionGuideEventArgs position)
        {
            if (sender == null)
            {
                return;
            }
            NormalizedPoint3D left = new NormalizedPoint3D(
                1 - position.LeftEye.UserPosition.X,
                position.LeftEye.UserPosition.Y,
                1 - position.LeftEye.UserPosition.Z
            );
            NormalizedPoint3D right = new NormalizedPoint3D(
                1 - position.RightEye.UserPosition.X,
                position.RightEye.UserPosition.Y,
                1 - position.RightEye.UserPosition.Z
            );
            Left = new UserPositionGuide(left, position.LeftEye.Validity);
            Right = new UserPositionGuide(right, position.RightEye.Validity);
        }
    }
}
