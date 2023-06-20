using System.Windows;

namespace CustomCalibrationLibrary.ViewModels
{
    class DriftCompensationViewModel
    {
        public Models.CalibrationPoint FixationPoint { get; set; }
        public DriftCompensationViewModel()
        {
            FixationPoint = new Models.CalibrationPoint(new Point(0.5, 0.5), 0);
        }
    }
}
