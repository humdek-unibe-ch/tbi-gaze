/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
﻿using CustomCalibrationLibrary.Commands;
using CustomCalibrationLibrary.Models;
using GazeUtilityLibrary.DataStructs;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Windows;
using System.Windows.Input;

namespace CustomCalibrationLibrary.ViewModels
{
    /// <summary>
    /// View model class of the gaze validation result.
    /// </summary>
    class ValidationResultViewModel : ColoredViewModel, INotifyPropertyChanged
    {
        private CalibrationModel _model;

        private ICommand _validationRestartCommand;
        /// <summary>
        /// Command to restart the validation
        /// </summary>
        public ICommand ValidationRestartCommand { get { return _validationRestartCommand; } }

        private ICommand _validationCloseCommand;
        /// <summary>
        /// Command to close the validation window
        /// </summary>
        public ICommand ValidationCloseCommand { get { return _validationCloseCommand; } }

        private GazeValidationData _validationData;
        /// <summary>
        /// The validation result
        /// </summary>
        public GazeValidationData ValidationData
        {
            get { return _validationData; }
        }

        private Visibility _successVisibility;
        /// <summary>
        /// The visibility flag for all items if the accuracy is acceptable.
        /// </summary>
        public Visibility SuccessVisibility { get { return _successVisibility; } }

        private Visibility _alertVisibility;
        /// <summary>
        /// The visibility flag for all items if the accuracy is too low.
        /// </summary>
        public Visibility AlertVisibility { get { return _alertVisibility; } }

        private Visibility _redoTimerVisibility;
        /// <summary>
        /// The visibility flag for all items if the accuracy is too low.
        /// </summary>
        public Visibility RedoTimerVisibility { get { return _redoTimerVisibility; } }

        private int _remainingSec;
        /// <summary>
        /// The number or remaining seconds before an automatic calibration restart.
        /// </summary>
        public int RemainingSec
        {
            get { return _remainingSec; }
            set
            {
                _remainingSec = value;
                OnPropertyChanged();
            }
        }

        private Timer _timer;

        /// <summary>
        /// The protperty changed handler.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="model">The claibration model</param>
        public ValidationResultViewModel(CalibrationModel model) : base(model.BackgroundColor, model.FrameColor, model.ForegroundColor)
        {
            _model = model;
            _validationRestartCommand = new CalibrationCommand(_model, CalibrationEventType.Restart);
            _validationCloseCommand = new CalibrationCommand(_model, CalibrationEventType.Accept);
            _validationData = _model.ValidationData;

            bool isSuccess = _validationData.AccuracyLeft < _model.AccuracyThreshold && _validationData.AccuracyRight < _model.AccuracyThreshold
                && _validationData.PrecisionLeft < _model.PrecisionThreshold && _validationData.PrecisionRight < _model.PrecisionThreshold;
            bool redo = _model.RetryCount < _model.Retries;
            if (!redo)
            {
                _model.RetryCount = 0;
            }

            _successVisibility = isSuccess ? Visibility.Visible : Visibility.Collapsed;
            _alertVisibility = !isSuccess && !redo ? Visibility.Visible : Visibility.Collapsed;
            _redoTimerVisibility = !isSuccess && redo ? Visibility.Visible : Visibility.Collapsed;
            _timer = new Timer(1000);
            _remainingSec = 5;
            _timer.Elapsed += OnTimerElapsed;
            _timer.AutoReset = true;
            _timer.Enabled = !isSuccess && redo;
        }

        private void OnPropertyChanged([CallerMemberName] string? property_name = null)
        {
            Application.Current.Dispatcher.Invoke(() => {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property_name));
            });
        }

        private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            RemainingSec--;
            if (RemainingSec == 0)
            {
                _timer.Stop();
                if (_validationRestartCommand.CanExecute(null))
                {
                    _validationRestartCommand.Execute(null);
                    _model.RetryCount++;
                }
            }
        }
    }
}
