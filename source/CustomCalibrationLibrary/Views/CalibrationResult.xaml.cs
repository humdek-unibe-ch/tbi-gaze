/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
﻿using CustomCalibrationLibrary.Models;
using CustomCalibrationLibrary.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace CustomCalibrationLibrary.Views
{
    /// <summary>
    /// Interaction logic for CalibrationResult.xaml
    /// </summary>
    public partial class CalibrationResult : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CalibrationResult"/> class.
        /// </summary>
        /// <param name="model">The calibration model.</param>
        public CalibrationResult(CalibrationModel model)
        {
            InitializeComponent();
            DataContext = new CalibrationResultViewModel(model);
        }

        private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            Focus();
            Keyboard.Focus(this);
        }
    }
}
