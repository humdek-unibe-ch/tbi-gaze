using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace CustomCalibrate.ViewModels
{
    class CalibrationPointViewModel : Models.CalibrationPoint, INotifyPropertyChanged
    {
        public CalibrationPointViewModel(Point point, int index) : base(point, index) { }
        public CalibrationPointViewModel(Models.CalibrationPoint model) : base(model.Position, model.Index) {
            GazePositionAverage = model.GazePositionAverage;
            GazePositionLeft = model.GazePositionLeft;
            GazePositionRight = model.GazePositionRight;
        }
    }
}
