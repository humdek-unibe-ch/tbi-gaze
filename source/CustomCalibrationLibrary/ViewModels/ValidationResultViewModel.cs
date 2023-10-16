/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
﻿using CustomCalibrationLibrary.Commands;
using CustomCalibrationLibrary.Models;
using GazeUtilityLibrary.DataStructs;
using System.Windows.Input;

namespace CustomCalibrationLibrary.ViewModels
{
    /// <summary>
    /// View model class of the gaze validation result.
    /// </summary>
    class ValidationResultViewModel
    {
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

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="model">The claibration model</param>
        public ValidationResultViewModel(CalibrationModel model)
        {
            _validationRestartCommand = new CalibrationCommand(model, CalibrationEventType.Restart);
            _validationCloseCommand = new CalibrationCommand(model, CalibrationEventType.Accept);
            _validationData = model.ValidationData;
        }
    }
}
