using GazeUtilityLibrary.DataStructs;
using System.Windows;

namespace CustomCalibrationLibrary.ViewModels
{
    /// <summary>
    /// The view model for a calibration point.
    /// </summary>
    class CalibrationPointViewModel : CalibrationPoint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CalibrationPointViewModel"/> class.
        /// </summary>
        /// <param name="point">The position of the calibration point.</param>
        /// <param name="index">The index of the calibration point.</param>
        public CalibrationPointViewModel(Point point, int index) : base(point, index) { }
    }
}
