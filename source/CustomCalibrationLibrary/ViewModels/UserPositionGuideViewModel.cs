using CustomCalibrationLibrary.Models;
using System.Windows.Input;
using CustomCalibrationLibrary.Commands;
using GazeUtilityLibrary.DataStructs;

namespace CustomCalibrationLibrary.ViewModels
{
    class UserPositionGuideViewModel
    {
        private UserPositionData _userPosition;
        public UserPositionData UserPosition
        {
            get { return _userPosition; }
        }

        private ICommand _calibrationStartCommand;
        public ICommand CalibrationStartCommand { get { return _calibrationStartCommand; } }

        private ICommand _calibrationAbortCommand;
        public ICommand CalibrationAbortCommand { get { return _calibrationAbortCommand; } }

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
            _userPosition.XCoordLeft = position.XCoordLeft;
            _userPosition.YCoordLeft = position.YCoordLeft;
            _userPosition.ZCoordLeft = position.ZCoordLeft;
            _userPosition.XCoordRight = position.XCoordRight;
            _userPosition.YCoordRight = position.YCoordRight;
            _userPosition.ZCoordRight = position.ZCoordRight;
        }
    }
}
