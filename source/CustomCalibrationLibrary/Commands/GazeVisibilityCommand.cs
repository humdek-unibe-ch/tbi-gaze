using CustomCalibrationLibrary.ViewModels;
using System;
using System.Windows.Input;

namespace CustomCalibrationLibrary.Commands
{
    /// <summary>
    /// Command class to change the gaze visibility
    /// </summary>
    internal class GazeVisibilityCommand : ICommand
    {
        private CalibrationResultViewModel _model;
        public event EventHandler? CanExecuteChanged;

        public GazeVisibilityCommand(CalibrationResultViewModel model)
        {
            _model = model;
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            _model.OnGazeToggle();
        }
    }
}
