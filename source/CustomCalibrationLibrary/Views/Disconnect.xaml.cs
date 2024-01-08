/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
﻿using CustomCalibrationLibrary.Commands;
using CustomCalibrationLibrary.Models;
using System.Windows.Controls;
using System.Windows.Input;

namespace CustomCalibrationLibrary.Views
{
    /// <summary>
    /// Interaction logic for Disconnect.xaml
    /// </summary>
    public partial class Disconnect : UserControl
    {
        private ICommand _calibrationAbortCommand;
        /// <summary>
        /// Command to abort the calibration
        /// </summary>
        public ICommand CalibrationAbortCommand { get { return _calibrationAbortCommand; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="Disconnect"/> class.
        /// </summary>
        /// <param name="model">The calibration model</param>
        public Disconnect(CalibrationModel model)
        {
            InitializeComponent();
            _calibrationAbortCommand = new CalibrationCommand(model, CalibrationEventType.Abort);
            DataContext = this;
        }

        private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            Focus();
            Keyboard.Focus(this);
        }
    }
}
