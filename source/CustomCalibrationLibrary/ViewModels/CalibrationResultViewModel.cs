/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
﻿using CustomCalibrationLibrary.Commands;
using CustomCalibrationLibrary.Models;
using GazeUtilityLibrary.DataStructs;
using System.Windows;
using System.Windows.Input;

namespace CustomCalibrationLibrary.ViewModels
{
    /// <summary>
    /// View model class of the gaze calibration result.
    /// </summary>
    class CalibrationResultViewModel : CalibrationViewModel
    {
        private ICommand _calibrationRestartCommand;
        /// <summary>
        /// Command to restart the calibration
        /// </summary>
        public ICommand CalibrationRestartCommand { get { return _calibrationRestartCommand; } }

        private ICommand _calibrationAcceptCommand;
        /// <summary>
        /// Command to accept the calibration
        /// </summary>
        public ICommand CalibrationAcceptCommand { get { return _calibrationAcceptCommand; } }

        private ICommand _gazeVisibilityCommand;
        /// <summary>
        /// Command to toggle the visibility of the live gaze point
        /// </summary>
        public ICommand GazeVisibilityCommand { get { return _gazeVisibilityCommand; } }

        private LiveGazePoint _gazePoint;
        /// <summary>
        /// The position of the live gaze point
        /// </summary>
        public LiveGazePoint GazePoint
        {
            get { return _gazePoint; }
        }

        private double _accuracyLeft;
        /// <summary>
        /// The calibration accuracy of the left eye.
        /// </summary>
        public double AccuracyLeft { get { return _accuracyLeft; } }

        private double _accuracyRight;
        /// <summary>
        /// The calibration accuracy of the right eye.
        /// </summary>
        public double AccuracyRight { get { return _accuracyRight; } }

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

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="model">The claibration model</param>
        public CalibrationResultViewModel(CalibrationModel model) : base(model)
        {
            _gazePoint = new LiveGazePoint();
            _model.GazePointChanged += OnGazePointChanged;
            _calibrationRestartCommand = new CalibrationCommand(model, CalibrationEventType.Restart);
            _calibrationAcceptCommand = new CalibrationCommand(model, CalibrationEventType.Accept);
            _gazeVisibilityCommand = new GazeVisibilityCommand(this);
            _accuracyLeft = model.CalibrationAccuracyLeft;
            _accuracyRight = model.CalibrationAccuracyRight;
            bool isSuccess = _accuracyLeft < model.AccuracyThreshold || _accuracyRight < model.AccuracyThreshold;
            _alertVisibility = isSuccess ? Visibility.Collapsed : Visibility.Visible;
            _successVisibility = isSuccess ? Visibility.Visible : Visibility.Collapsed;
        }

        private void OnGazePointChanged(object? sender, Point point)
        {
            if (sender == null)
            {
                return;
            }
            _gazePoint.X = point.X;
            _gazePoint.Y = point.Y;
        }

        /// <summary>
        /// Toggle the visibility of the live gaze point.
        /// </summary>
        public void OnGazeToggle()
        {
            _gazePoint.Visibility = !_gazePoint.Visibility;
        }
    }
}
