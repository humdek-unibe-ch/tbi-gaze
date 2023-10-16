/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
﻿using System.Windows;
using GazeUtilityLibrary;

namespace ShowMouse
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private TrackerLogger _logger;
        private GazeConfiguration _config;

        public App()
        {
            InitializeComponent();

            _logger = new TrackerLogger(null);
            _config = new GazeConfiguration(_logger);
        }

        private void OnApplicationStartup(object sender, StartupEventArgs e)
        {
            MouseHider hider = new MouseHider(_logger);
            hider.ShowCursor(_config.Config.MouseStandardIconPath);
            Shutdown();
        }
    }
}
