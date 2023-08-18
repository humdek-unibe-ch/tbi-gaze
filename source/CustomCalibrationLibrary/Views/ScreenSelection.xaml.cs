using System;
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