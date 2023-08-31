using System.Windows;
using System.Windows.Controls;
using CustomCalibrationLibrary.Models;
using CustomCalibrationLibrary.ViewModels;

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
    }
}