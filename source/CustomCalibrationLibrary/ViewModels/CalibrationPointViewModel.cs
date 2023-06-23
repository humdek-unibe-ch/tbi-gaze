using GazeUtilityLibrary.DataStructs;
using System.Windows;

namespace CustomCalibrationLibrary.ViewModels
{
    /// <summary>
    /// The view model for a calibration point.
    /// </summary>
    class CalibrationPointViewModel : CalibrationPoint
    {
        public CalibrationPointViewModel(Point point, int index) : base(point, index) { }
        public CalibrationPointViewModel(CalibrationPoint model) : base(model.Position, model.Index) {
            GazePositionAverage = model.GazePositionAverage;
            GazePositionLeft = model.GazePositionLeft;
            GazePositionRight = model.GazePositionRight;
        }
    }
}
