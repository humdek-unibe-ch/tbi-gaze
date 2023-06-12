using CustomCalibrationLibrary.ViewModels;
using System.Windows;

namespace CustomCalibrationLibrary.Views
{
    /// <summary>
    /// Interaction logic for DriftCompensation.xaml
    /// </summary>
    public partial class DriftCompensationWindow : Window
    {
        public DriftCompensationWindow()
        {
            InitializeComponent();
            DataContext = new DriftCompensationViewModel();
        }
    }
}
