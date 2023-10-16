/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
﻿using CustomCalibrationLibrary.Models;
using CustomCalibrationLibrary.ViewModels;
using System.Windows.Controls;

namespace CustomCalibrationLibrary.Views
{
    /// <summary>
    /// Interaction logic for Calibration.xaml
    /// </summary>
    public partial class Calibration : Page
    {
        private CalibrationModel _model;

        /// <summary>
        /// Initializes a new instance of the <see cref="Calibration"/> class.
        /// </summary>
        /// <param name="model">The calibration model</param>
        public Calibration(CalibrationModel model)
        {
            InitializeComponent();
            _model = model;
            DataContext = new CalibrationViewModel(_model);
        }
    }
}
