/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
﻿using System;
using System.Windows;
using System.Windows.Controls;

namespace CustomCalibrationLibrary.Views
{
    /// <summary>
    /// Interaction logic for Computing.xaml
    /// </summary>
    public partial class Spinner : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Computing"/> class.
        /// </summary>
        public Spinner()
        {
            InitializeComponent();

            this.Initialized += Computing_Initialized;
            this.Loaded += Computing_Loaded;
            this.Unloaded += Computing_Unloaded;
        }

        private void Computing_Unloaded(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Unloaded Computing");
        }

        private void Computing_Loaded(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Loaded Computing");
        }

        private void Computing_Initialized(object? sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Initialized Computing");
        }
    }
}
