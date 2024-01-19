/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
using System.Windows.Input;
using GazeToMouse.Commands;

namespace GazeToMouse;

/// <summary>
/// Provides bindable properties and commands for the NotifyIcon.
/// </summary>
public partial class NotifyIconViewModel
{
    private ICommand _exitApplicationCommand;
    /// <summary>
    /// Command to exit the application
    /// </summary>
    public ICommand ExitApplicationCommand { get { return _exitApplicationCommand; } }

    private ICommand _startCalibrationCommand;
    /// <summary>
    /// Command to start the calibration
    /// </summary>
    public ICommand StartCalibrationCommand { get { return _startCalibrationCommand; } }

    private ICommand _startValidationCommand;
    /// <summary>
    /// Command to start the validation
    /// </summary>
    public ICommand StartValidationCommand { get { return _startValidationCommand; } }

    private ICommand _startDriftCompensationCommand;
    /// <summary>
    /// Command to start the drift compensation
    /// </summary>
    public ICommand StartDriftCompensationCommand { get { return _startDriftCompensationCommand; } }

    public NotifyIconViewModel(App app)
    {
        _exitApplicationCommand = new ExitApplicationCommand(app);
        _startCalibrationCommand = new StartCalibrationCommand(app);
        _startValidationCommand = new StartValidationCommand(app);
        _startDriftCompensationCommand = new StartDriftCompensationCommand(app);
    }
}
