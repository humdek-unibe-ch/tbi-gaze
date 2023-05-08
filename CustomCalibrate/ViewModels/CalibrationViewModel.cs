using System.Collections.ObjectModel;
using System.Windows;
using CustomCalibrate.Models;

namespace CustomCalibrate.ViewModels
{
    class CalibrationViewModel
    {
        private int Index = 0;
        private CalibrationModel _model;

        private ObservableCollection<CalibrationPointViewModel> _calibrationPoints = new ObservableCollection<CalibrationPointViewModel>();
        public ObservableCollection<CalibrationPointViewModel> CalibrationPoints
        {
            get { return _calibrationPoints; }
        }

        public CalibrationViewModel(CalibrationModel calibrationModel)
        {
            _model = calibrationModel;
        }

        private CalibrationPointViewModel CreateCalibrationPoint(Point position)
        {
            return new CalibrationPointViewModel(position);
        }

        public void NextCalibrationPoint()
        {
            if (_model.Positions == null)
            {
                return;
            }
            CalibrationPoints.Insert(Index, CreateCalibrationPoint(_model.Positions[Index]));
            Index++;
            if (Index == _model.Positions.Length)
            {
                Index = 0;
            }
        }

        public void GazeDataCollected()
        {
            CalibrationPoints[Index - 1].HasData = true;
            CalibrationPoints[Index - 1].GazePositionAverage = new Point(CalibrationPoints[Index - 1].Position.X, CalibrationPoints[Index - 1].Position.Y + 5);
            CalibrationPoints[Index - 1].GazePositionLeft = new Point(CalibrationPoints[Index - 1].Position.X - 10, CalibrationPoints[Index - 1].Position.Y + 5);
            CalibrationPoints[Index - 1].GazePositionRight = new Point(CalibrationPoints[Index - 1].Position.X + 10, CalibrationPoints[Index - 1].Position.Y + 5);
        }
    }
}
