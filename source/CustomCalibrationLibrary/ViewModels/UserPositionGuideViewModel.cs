using CustomCalibrationLibrary.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CustomCalibrationLibrary.Commands;
using GazeUtilityLibrary.Tracker;
using GazeUtilityLibrary.DataStructs;

namespace CustomCalibrationLibrary.ViewModels
{
    class UserPositionGuideViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private UserPositionData _userPosition;
        public UserPositionData UserPosition
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
            _userPosition = new UserPositionData();
            model.UserPositionGuideChanged += OnUserPositionGuideChanged;
            _calibrationStartCommand = new CalibrationCommand(model, CalibrationEventType.Start);
            _calibrationAbortCommand = new CalibrationCommand(model, CalibrationEventType.Abort);
        }

        private void OnUserPositionGuideChanged(object? sender, UserPositionData position)
        {
            if (sender == null)
            {
                return;
            }
            UserPosition = position;
        }
    }
}
