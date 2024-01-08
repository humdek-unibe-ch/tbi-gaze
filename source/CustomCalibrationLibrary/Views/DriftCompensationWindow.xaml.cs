/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
﻿using CustomCalibrationLibrary.ViewModels;
using System.Windows;
using System.Windows.Media;

namespace CustomCalibrationLibrary.Views
{
    /// <summary>
    /// Interaction logic for DriftCompensation.xaml
    /// </summary>
    public partial class DriftCompensationWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DriftCompensationWindow"/> class.
        /// </summary>
        public DriftCompensationWindow(Color backgroundColor)
        {
            InitializeComponent();
            DataContext = new DriftCompensationViewModel(backgroundColor);
        }
    }
}
