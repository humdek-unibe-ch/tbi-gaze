/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
using CustomCalibrationLibrary.Models;
using CustomCalibrationLibrary.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace CustomCalibrationLibrary.Views
{
    /// <summary>
    /// Interaction logic for Disconnect.xaml
    /// </summary>
    public partial class Disconnect : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Disconnect"/> class.
        /// </summary>
        /// <param name="model">The calibration model</param>
        public Disconnect(CalibrationModel model)
        {
            InitializeComponent();
            DataContext = new DisconnectViewModel(model);
        }

        private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            Focus();
            Keyboard.Focus(this);
        }
    }
}
