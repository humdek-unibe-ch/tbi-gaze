using CustomCalibrationLibrary.Models;
using CustomCalibrationLibrary.ViewModels;
using System.Windows.Controls;

namespace CustomCalibrationLibrary.Views
{
    /// <summary>
    /// Interaction logic for ValidationResult.xaml
    /// </summary>
    public partial class ValidationResult : Page
    {
        public ValidationResult(CalibrationModel model)
        {
            InitializeComponent();
            DataContext = new ValidationResultViewModel(model);
            Focus();
        }
    }
}
