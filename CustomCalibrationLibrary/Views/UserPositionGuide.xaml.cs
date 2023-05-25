using CustomCalibrationLibrary.Models;
using CustomCalibrationLibrary.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace CustomCalibrationLibrary.Views
{
    /// <summary>
    /// Interaction logic for UserPositionGuide.xaml
    /// </summary>
    public partial class UserPositionGuide : Page
    {
        private CalibrationModel _model;
        public UserPositionGuide(CalibrationModel model)
        {
            InitializeComponent();
            _model = model;
            DataContext = new UserPositionGuideViewModel(_model);
        }
        private void OnCalibrationAbort(object sender, RoutedEventArgs e)
        {
            _model.OnCalibrationEvent(CalibrationEventType.Abort);
        }

        private void OnCalibrationStart(object sender, RoutedEventArgs e)
        {
            _model.OnCalibrationEvent(CalibrationEventType.Start);
        }
    }
}
