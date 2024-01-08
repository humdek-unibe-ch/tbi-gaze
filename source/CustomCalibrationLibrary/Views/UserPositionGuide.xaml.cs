/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
﻿using CustomCalibrationLibrary.Models;
using CustomCalibrationLibrary.ViewModels;
using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace CustomCalibrationLibrary.Views
{
    /// <summary>
    /// Interaction logic for UserPositionGuide.xaml
    /// </summary>
    public partial class UserPositionGuide : UserControl
    {
        private CalibrationModel _model;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserPositionGuide"/> class.
        /// </summary>
        /// <param name="model">The calibration model.</param>
        public UserPositionGuide(CalibrationModel model)
        {
            InitializeComponent();
            _model = model;
            DataContext = new UserPositionGuideViewModel(_model);
        }

        private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            Focus();
            Keyboard.Focus(this);
        }
    }
}
