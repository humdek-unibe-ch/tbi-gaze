using System;
using System.Linq;
using System.Windows.Input;
using WpfScreenHelper.Enum;
using WpfScreenHelper;
using System.Windows;

namespace CustomCalibrationLibrary.Commands
{
    internal class ScreenSwitchCommand : ICommand
    {
        private int _count;
        private Window _window;
        /// <summary>
        /// Event handler on can executed flag change.
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScreenSwitchCommand"/> class.
        /// </summary>
        /// <param name="window">The window to render on the screen.</param>
        public ScreenSwitchCommand(Window window, int count)
        {
            _window = window;
            _count = count;
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
        /// Switch window to screen of a given index.
        /// </summary>
        /// <param name="parameter">The command parameter</param>
        public void Execute(object? parameter)
        {
            int index = int.Parse(parameter?.ToString() ?? "0");
            if (index < _count && index >= 0)
            {
                _window.SetWindowPosition(WindowPositions.Center, Screen.AllScreens.ElementAt(index));
            }
        }
    }
}
