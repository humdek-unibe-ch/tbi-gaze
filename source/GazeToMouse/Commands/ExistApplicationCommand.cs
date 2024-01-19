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
    /// Command class to exit the application.
    /// </summary>
    public class ExitApplicationCommand : ICommand
    {
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
        /// Initializes a new instance of the <see cref="ExitApplicationCommand"/> class.
        /// </summary>
        /// <param name="app">The main application</param>
        public ExitApplicationCommand(App app)
        {
            _app = app;
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
        /// Exit the application.
        /// </summary>
        /// <param name="parameter">The command parameter</param>
        public void Execute(object? parameter)
        {
            _app.CustomDispatcher.InvokeShutdown();
        }
    }
}
