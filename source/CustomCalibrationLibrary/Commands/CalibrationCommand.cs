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
        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        public CalibrationCommand(CalibrationModel model, CalibrationEventType eventType)
        {
            _model = model;
            _eventType = eventType;
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            _model.OnCalibrationEvent(_eventType);
        }
    }
}
