/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
using System;
using System.Windows.Input;

namespace GazeToMouse.Commands
{
    /// <summary>
    /// Command class to start the drift compensation.
    /// </summary>
    public class UpdateDriftDeviationAngleCommand : ICommand
    {
        private Func<double, double> _lambda;
        private App _app;
        /// <summary>
        /// Event handler on can executed flag change.
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateDriftDeviationAngleCommand"/> class.
        /// </summary>
        /// <param name="app">The main application</param>
        public UpdateDriftDeviationAngleCommand(App app, Func<double, double> lambda)
        {
            _app = app;
            _lambda = lambda;
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
        /// Start the drift compensation.
        /// </summary>
        /// <param name="parameter">The command parameter</param>
        public void Execute(object? parameter)
        {
            _lambda(_app.GetDriftDeviationAngle());
        }
    }
}
