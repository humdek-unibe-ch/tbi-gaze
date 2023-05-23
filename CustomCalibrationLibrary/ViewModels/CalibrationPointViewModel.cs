using System.ComponentModel;
using System.Windows;

namespace CustomCalibrationLibrary.ViewModels
{
    class CalibrationPointViewModel : Models.CalibrationPoint
    {
        public CalibrationPointViewModel(Point point, int index) : base(point, index) { }
        public CalibrationPointViewModel(Models.CalibrationPoint model) : base(model.Position, model.Index) {
            GazePositionAverage = model.GazePositionAverage;
            GazePositionLeft = model.GazePositionLeft;
            GazePositionRight = model.GazePositionRight;
        }
    }
}
