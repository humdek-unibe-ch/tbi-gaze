/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
﻿using CustomCalibrationLibrary.Commands;
using CustomCalibrationLibrary.Models;
using CustomCalibrationLibrary.ViewModels;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CustomCalibrationLibrary.Views
{
    /// <summary>
    /// Interaction logic for CalibrationFailed.xaml
    /// </summary>
    public partial class CalibrationFailed : UserControl
    {

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="model">The claibration model</param>
        public CalibrationFailed(CalibrationModel model)
        {
            InitializeComponent();
            DataContext = new CalibrationFailedViewModel(model);
        }

        private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            Focus();
            Keyboard.Focus(this);
        }
    }
}
