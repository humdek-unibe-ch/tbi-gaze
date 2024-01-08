/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
﻿using CustomCalibrationLibrary.Models;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace CustomCalibrationLibrary.Views
{
    /// <summary>
    /// Interaction logic for CalibrationCollection.xaml
    /// </summary>
    public partial class CalibrationFrame : Frame
    {
        private CalibrationModel _model;
        private Window _window;
        private Spinner _computingView;

        /// <summary>
        /// Initializes a new instance of the <see cref="CalibrationFrame"/> class.
        /// </summary>
        /// <param name="model">The calibration model.</param>
        /// <param name="window">The target window.</param>
        public CalibrationFrame(CalibrationModel model, Window window)
        {
            _window = window;
            _computingView = new Spinner();
            InitializeComponent();
            _model = model;
            _model.PropertyChanged += OnPropertyChanged;
            this.Content = _computingView;
        }

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            UserControl newControl = _computingView;
            if (sender == null || e.PropertyName != "Status")
            {
                return;
            }
            switch (((CalibrationModel)sender).Status)
            {
                case CalibrationStatus.ScreenSelection:
                    newControl = new ScreenSelection(_model, _window);
                    break;
                case CalibrationStatus.HeadPosition:
                    newControl = new UserPositionGuide(_model);
                    break;
                case CalibrationStatus.DataCollection:
                    newControl = new Calibration(_model);
                    break;
                case CalibrationStatus.Computing:
                    newControl = _computingView;
                    break;
                case CalibrationStatus.CalibrationResult:
                    newControl = new CalibrationResult(_model);
                    break;
                case CalibrationStatus.ValidationResult:
                    newControl = new ValidationResult(_model);
                    break;
                case CalibrationStatus.Error:
                    newControl = new CalibrationFailed(_model);
                    break;
                case CalibrationStatus.Disconnect:
                    newControl = new Disconnect(_model);
                    break;
            }
            this.Content = newControl;
        }
    }

}
