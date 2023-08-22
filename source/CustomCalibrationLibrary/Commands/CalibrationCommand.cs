using CustomCalibrationLibrary.Models;
using System;
using System.Windows.Input;

namespace CustomCalibrationLibrary.Commands
{
    /// <summary>
    /// Comand class to trigger calibration events.
    /// </summary>
    public class CalibrationCommand : ICommand
    {
        private CalibrationEventType _eventType;
        private CalibrationModel _model;
        /// <summary>
        /// Event handler on can executed flag change.
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CalibrationCommand"/> class.
        /// </summary>
        /// <param name="model">The calibration model</param>
        /// <param name="eventType">The type of the calibration event.</param>
        public CalibrationCommand(CalibrationModel model, CalibrationEventType eventType)
        {
            _model = model;
            _eventType = eventType;
        }

        /// <summary>
        /// Returns whether command can be executed or not.
        /// </summary>
        /// <param name="parameter">The command parameter</param>
        /// <returns>True</returns>
        public bool CanExecute(object? parameter)
        {
            return true;
        }

        /// <summary>
        /// Send calibration event.
        /// </summary>
        /// <param name="parameter">The command parameter</param>
        public void Execute(object? parameter)
        {
            _model.OnCalibrationEvent(_eventType);
        }
    }
}
