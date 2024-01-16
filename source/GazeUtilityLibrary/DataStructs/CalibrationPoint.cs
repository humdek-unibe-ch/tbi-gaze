/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
﻿using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace GazeUtilityLibrary.DataStructs
{
    /// <summary>
    /// A calibration point class holding several metrics connected to a calibration point.
    /// </summary>
    public class CalibrationPoint : INotifyPropertyChanged
    {
        /// <summary>
        /// Event to trigger property changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        private int _index;
        /// <summary>
        /// The index of the calibration point.
        /// </summary>
        public int Index { get { return _index; } }

        private bool _hasData;
        /// <summary>
        /// Flag to indicate whether data has been collected for this calibration point.
        /// </summary>
        public bool HasData
        {
            get { return _hasData; }
            set { _hasData = value; OnPropertyChanged(); }
        }

        private bool _hasFailed;
        /// <summary>
        /// Flag to indicate whether data has been collected for this calibration point.
        /// </summary>
        public bool HasFailed
        {
            get { return _hasFailed; }
            set { _hasFailed = value; OnPropertyChanged(); }
        }

        private Point _position;
        /// <summary>
        /// The position of the calibration point.
        /// </summary>
        public Point Position
        {
            get { return _position; }
            set { _position = value; OnPropertyChanged(); }
        }

        private Point _gazePositionAverage;
        /// <summary>
        /// The average between the left and the right gaze point.
        /// </summary>
        public Point GazePositionAverage
        {
            get { return _gazePositionAverage; }
            set { _gazePositionAverage = value; OnPropertyChanged(); }
        }

        private Point _gazePositionLeft;
        /// <summary>
        /// The left gaze point.
        /// </summary>
        public Point GazePositionLeft
        {
            get { return _gazePositionLeft; }
            set { _gazePositionLeft = value; OnPropertyChanged(); }
        }

        private Point _gazePositionRight;
        /// <summary>
        /// The right gaze point.
        /// </summary>
        public Point GazePositionRight
        {
            get { return _gazePositionRight; }
            set { _gazePositionRight = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Called when when the state property of EyeTracker is changing.
        /// </summary>
        /// <param name="property_name">Name of the property in WPF.</param>
        private void OnPropertyChanged([CallerMemberName] string? property_name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property_name));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CalibrationPoint"/> class.
        /// </summary>
        /// <param name="position">The position of the calibration point.</param>
        /// <param name="index">The index of the calibration point.</param>
        public CalibrationPoint(Point position, int index)
        {
            HasData = false;
            HasFailed = false;
            Position = position;
            _index = index;
        }
    }
}
