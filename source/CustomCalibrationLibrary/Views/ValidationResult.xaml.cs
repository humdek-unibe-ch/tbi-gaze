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
    /// Interaction logic for ValidationResult.xaml
    /// </summary>
    public partial class ValidationResult : Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationResult"/> class.
        /// </summary>
        /// <param name="model">The calibration model.</param>
        public ValidationResult(CalibrationModel model)
        {
            InitializeComponent();
            DataContext = new ValidationResultViewModel(model);
            Focus();
        }
    }
}
