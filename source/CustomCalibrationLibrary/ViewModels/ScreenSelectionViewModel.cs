/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
﻿using CustomCalibrationLibrary.Commands;
using CustomCalibrationLibrary.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows;
using WpfScreenHelper;

namespace CustomCalibrationLibrary.ViewModels
{
    /// <summary>
    /// A representation of the screen.
    /// </summary>
    class Monitor
    {
        private string _name;
        /// <summary>
        /// The name of the screen.
        /// </summary>
        public string Name { get { return _name;  } }
        private int _index;
        /// <summary>
        /// The screen index.
        /// </summary>
        public int Index { get { return _index; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="Monitor"/> class.
        /// </summary>
        /// <param name="index">The screen index.</param>
        /// <param name="name">The name of the screen.</param>
        public Monitor(int index, string name)
        {
            _index = index;
            _name = name;
        }
    }

    /// <summary>
    /// The view model class for the screen selection view.
    /// </summary>
    class ScreenSelectionViewModel : ColoredViewModel
    {
        private ObservableCollection<Monitor> _monitors = new ObservableCollection<Monitor>();
        /// <summary>
        /// The observable lidt of monitors to select from.
        /// </summary>
        public ObservableCollection<Monitor> Monitors
        {
            get { return _monitors; }
        }

        private ICommand _calibrationStartCommand;
        /// <summary>
        /// Command to start the calibration
        /// </summary>
        public ICommand CalibrationStartCommand { get { return _calibrationStartCommand; } }

        private ICommand _calibrationAbortCommand;
        /// <summary>
        /// Command to abort the calibration
        /// </summary>
        public ICommand CalibrationAbortCommand { get { return _calibrationAbortCommand; } }

        private ICommand _screenSwitchCommand;
        /// <summary>
        /// Command to switch the screen
        /// </summary>
        public ICommand ScreenSwitchCommand { get { return _screenSwitchCommand; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScreenSelectionViewModel"/> class.
        /// </summary>
        /// <param name="model">The calibration model</param>
        /// <param name="window">The target window of the screen selection</param>
        public ScreenSelectionViewModel(CalibrationModel model, Window window) : base(model.BackgroundColor, model.FrameColor)
        {
            foreach (var screen in Screen.AllScreens)
            {
                _monitors.Add(new Monitor(_monitors.Count, screen.DeviceName));
            }

            _calibrationStartCommand = new CalibrationCommand(model, CalibrationEventType.Init);
            _calibrationAbortCommand = new CalibrationCommand(model, CalibrationEventType.Abort);
            _screenSwitchCommand = new ScreenSwitchCommand(window, _monitors.Count);
        }
    }
}
