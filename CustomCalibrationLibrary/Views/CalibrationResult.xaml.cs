using CustomCalibrationLibrary.Models;
using CustomCalibrationLibrary.ViewModels;
using ModernWpf.Controls;
using System;
using System.Windows;

namespace CustomCalibrationLibrary.Views
{
    /// <summary>
    /// Interaction logic for CalibrationResult.xaml
    /// </summary>
    public partial class CalibrationResult : System.Windows.Controls.Page
    {
        private CalibrationModel _model;
        public CalibrationResult(CalibrationModel model)
        {
            InitializeComponent();
            _model = model;
            DataContext = new CalibrationResultViewModel(_model);
            Focus();
        }
    }
}
