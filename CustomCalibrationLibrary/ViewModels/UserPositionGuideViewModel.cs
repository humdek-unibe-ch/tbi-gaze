using CustomCalibrationLibrary.Models;
using System.ComponentModel;
using System.Windows;
using System.Runtime.CompilerServices;
using Tobii.Research;
using System.Windows.Input;
using CustomCalibrationLibrary.Commands;
using GazeUtilityLibrary;

namespace CustomCalibrationLibrary.ViewModels
{
    class UserPositionGuideViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private UserPositionDataArgs _userPosition;
        public UserPositionDataArgs UserPosition
        {
            get { return _userPosition; }
            set { _userPosition = value; OnPropertyChanged(); }
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
            _userPosition = new UserPositionDataArgs();
            model.UserPositionGuideChanged += OnUserPositionGuideChanged;
            _calibrationStartCommand = new CalibrationCommand(model, CalibrationEventType.Start);
            _calibrationAbortCommand = new CalibrationCommand(model, CalibrationEventType.Abort);
        }

        private void OnUserPositionGuideChanged(object? sender, UserPositionDataArgs position)
        {
            if (sender == null)
            {
                return;
            }
            UserPosition = position;
        }
    }
}
