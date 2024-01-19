/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using GazeToMouse.Commands;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GazeToMouse;

/// <summary>
/// Provides bindable properties and commands for the NotifyIcon.
/// </summary>
public partial class NotifyIconViewModel : INotifyPropertyChanged
{
    /// <summary>
    /// The protperty changed handler.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    private double _driftDeviationAngle = 0;
    /// <summary>
    /// The deviation angle of the currently active drift compensation.
    /// </summary>
    public double DriftDeviationAngle
    {
        get { return _driftDeviationAngle; }
        set
        {
            _driftDeviationAngle = value;
            OnPropertyChanged();
        }
    }

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

    private ICommand _resetDriftCompensationCommand;
    /// <summary>
    /// Command to reset the drift compensation
    /// </summary>
    public ICommand ResetDriftCompensationCommand { get { return _resetDriftCompensationCommand; } }

    private ICommand _updateDriftDeviationAngleCommand;
    /// <summary>
    /// Command to update the drift deviation angle value
    /// </summary>
    public ICommand UpdateDriftDeviationAngleCommand { get { return _updateDriftDeviationAngleCommand; } }

    /// <summary>
    /// The constructor.
    /// </summary>
    /// <param name="app">The main application.</param>
    public NotifyIconViewModel(App app)
    {
        _exitApplicationCommand = new ExitApplicationCommand(app);
        _startCalibrationCommand = new StartCalibrationCommand(app);
        _startValidationCommand = new StartValidationCommand(app);
        _startDriftCompensationCommand = new StartDriftCompensationCommand(app);
        _resetDriftCompensationCommand = new ResetDriftCompensationCommand(app);
        _updateDriftDeviationAngleCommand = new UpdateDriftDeviationAngleCommand(app, v => DriftDeviationAngle = v);
    }

    private void OnPropertyChanged([CallerMemberName] string? property_name = null)
    {
        Application.Current.Dispatcher.Invoke(() => {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property_name));
        });
    }
}
