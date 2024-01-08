/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CustomCalibrationLibrary.Models;
using CustomCalibrationLibrary.ViewModels;

namespace CustomCalibrationLibrary.Views
{
    /// <summary>
    /// Interaction logic for ScreenSelection.xaml
    /// </summary>
    public partial class ScreenSelection : UserControl
    {
        private ScreenSelectionViewModel _viewModel;
        /// <summary>
        /// Initializes a new instance of the <see cref="ScreenSelection"/> class.
        /// </summary>
        /// <param name="model">The calibration model.</param>
        /// <param name="window">The target window.</param>
        public ScreenSelection(CalibrationModel model, Window window)
        {
            InitializeComponent();
            _viewModel = new ScreenSelectionViewModel(model, window);
            DataContext = _viewModel;
        }
        private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            Focus();
            Keyboard.Focus(this);
        }
    }
}