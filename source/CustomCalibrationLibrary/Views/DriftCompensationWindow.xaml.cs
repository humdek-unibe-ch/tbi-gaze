using CustomCalibrationLibrary.ViewModels;
using System.Windows;

namespace CustomCalibrationLibrary.Views
{
    /// <summary>
    /// Interaction logic for DriftCompensation.xaml
    /// </summary>
    public partial class DriftCompensationWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DriftCompensationWindow"/> class.
        /// </summary>
        public DriftCompensationWindow()
        {
            InitializeComponent();
            DataContext = new DriftCompensationViewModel();
        }
    }
}
