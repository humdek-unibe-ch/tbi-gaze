using CustomCalibrationLibrary.Commands;
using CustomCalibrationLibrary.Models;
using System.Windows.Input;

namespace CustomCalibrationLibrary.ViewModels
{
    class ValidationResultViewModel
    {
        private ICommand _calibrationRestartCommand;
        /// <summary>
        /// Command to restart the calibration
        /// </summary>
        public ICommand CalibrationRestartCommand { get { return _calibrationRestartCommand; } }

        private ICommand _calibrationAcceptCommand;
        /// <summary>
        /// Command to accept the calibration
        /// </summary>
        public ICommand CalibrationAcceptCommand { get { return _calibrationAcceptCommand; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="model">The claibration model</param>
        public ValidationResultViewModel(CalibrationModel model)
        {
            _calibrationRestartCommand = new CalibrationCommand(model, CalibrationEventType.Restart);
            _calibrationAcceptCommand = new CalibrationCommand(model, CalibrationEventType.Accept);
        }
    }
}
