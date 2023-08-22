﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WpfScreenHelper.Enum;
using WpfScreenHelper;
using System.Windows.Input;
using CustomCalibrationLibrary.Commands;
using CustomCalibrationLibrary.Models;
using CustomCalibrationLibrary.ViewModels;
using System.Collections.ObjectModel;

namespace CustomCalibrationLibrary.Views
{
    /// <summary>
    /// Interaction logic for ScreenSelection.xaml
    /// </summary>
    public partial class ScreenSelection : Page
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
            Focus();
        }

        private void OnMonitorButtonClick(object sender, RoutedEventArgs e)
        {
            Button? button = sender as Button;
            _viewModel.SwitchScreen(int.Parse(button?.Tag.ToString() ?? "0"));
        }
    }
}