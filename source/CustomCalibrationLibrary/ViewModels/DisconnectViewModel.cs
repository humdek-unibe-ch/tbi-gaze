/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
using CustomCalibrationLibrary.Commands;
using CustomCalibrationLibrary.Models;
using System.Windows.Input;

namespace CustomCalibrationLibrary.ViewModels
{
    /// <summary>
    /// The view model class of the diconnect view
    /// </summary>
    public class DisconnectViewModel : ColoredViewModel
    {
        private ICommand _calibrationAbortCommand;
        /// <summary>
        /// Command to abort the calibration
        /// </summary>
        public ICommand CalibrationAbortCommand { get { return _calibrationAbortCommand; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="model">The calibration model</param>
        public DisconnectViewModel(CalibrationModel model) : base(model.BackgroundColor, model.FrameColor)
        {
            _calibrationAbortCommand = new CalibrationCommand(model, CalibrationEventType.Abort);
        }
    }
}
