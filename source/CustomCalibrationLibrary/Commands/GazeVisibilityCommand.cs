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
        /// <param name="model">The calibration result view model</param>
        public GazeVisibilityCommand(CalibrationResultViewModel model)
        {
            _model = model;
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
        /// Toggle the gaze visibility.
        /// </summary>
        /// <param name="parameter">The command parameter</param>
        public void Execute(object? parameter)
        {
            _model.OnGazeToggle();
        }
    }
}
