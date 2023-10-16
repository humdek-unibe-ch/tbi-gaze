/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
﻿using CustomCalibrationLibrary.Models;
using System.Windows.Input;
using CustomCalibrationLibrary.Commands;
using GazeUtilityLibrary.DataStructs;

namespace CustomCalibrationLibrary.ViewModels
{
    /// <summary>
    /// The view model class for the user position guide view.
    /// </summary>
    class UserPositionGuideViewModel
    {
        private UserPositionData _userPosition;
        /// <summary>
        /// The user position to be represented on the view
        /// </summary>
        public UserPositionData UserPosition
        {
            get { return _userPosition; }
        }

        private ICommand _calibrationStartCommand;
        /// <summary>
        /// Command to start the calibration
        /// </summary>
        public ICommand CalibrationStartCommand { get { return _calibrationStartCommand; } }

        private ICommand _calibrationAbortCommand;
        /// <summary>
        /// Command to abort the calibration
        /// </summary>
        public ICommand CalibrationAbortCommand { get { return _calibrationAbortCommand; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="model">The calibartion model</param>
        public UserPositionGuideViewModel(CalibrationModel model)
        {
            _userPosition = new UserPositionData();
            model.UserPositionGuideChanged += OnUserPositionGuideChanged;
            _calibrationStartCommand = new CalibrationCommand(model, CalibrationEventType.Start);
            _calibrationAbortCommand = new CalibrationCommand(model, CalibrationEventType.Abort);
        }

        private void OnUserPositionGuideChanged(object? sender, UserPositionData position)
        {
            if (sender == null)
            {
                return;
            }
            _userPosition.XCoordLeft = position.XCoordLeft;
            _userPosition.YCoordLeft = position.YCoordLeft;
            _userPosition.ZCoordLeft = position.ZCoordLeft;
            _userPosition.XCoordRight = position.XCoordRight;
            _userPosition.YCoordRight = position.YCoordRight;
            _userPosition.ZCoordRight = position.ZCoordRight;
        }
    }
}
