using CustomCalibrate.Models;
using System.Windows;

namespace CustomCalibrate
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            CalibrationModel model = new CalibrationModel();
            Main.Content = new Calibration(model);
        }
    }
}
