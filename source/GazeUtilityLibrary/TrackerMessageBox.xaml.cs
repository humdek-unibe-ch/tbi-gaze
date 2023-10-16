/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
﻿using System.Windows;

namespace GazeUtilityLibrary
{
    /// <summary>
    /// Interaction logic for TrackerMessageBox.xaml
    /// </summary>
    public partial class TrackerMessageBox : Window
    {
        public TrackerMessageBox()
        {
            InitializeComponent();
        }

        private void Abort_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
