using CustomCalibrationLibrary.Commands;
using CustomCalibrationLibrary.Models;
using System.Windows.Input;

namespace CustomCalibrationLibrary.ViewModels
{
    public class DisconnectViewModel : ColoredViewModel
    {
        private ICommand _calibrationAbortCommand;
        /// <summary>
        /// Command to abort the calibration
        /// </summary>
        public ICommand CalibrationAbortCommand { get { return _calibrationAbortCommand; } }

        public DisconnectViewModel(CalibrationModel model) : base(model.BackgroundColor, model.FrameColor)
        {
            _calibrationAbortCommand = new CalibrationCommand(model, CalibrationEventType.Abort);
        }
    }
}
