using GazeUtilityLibrary.DataStructs;
using System.Windows;

namespace CustomCalibrationLibrary.ViewModels
{
    /// <summary>
    /// The view model class of the drift compensation view.
    /// </summary>
    class DriftCompensationViewModel
    {
        /// <summary>
        /// The point on the screen which the participant is supposed to fixate.
        /// </summary>
        public CalibrationPoint FixationPoint { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public DriftCompensationViewModel()
        {
            FixationPoint = new CalibrationPoint(new Point(0.5, 0.5), 0);
        }
    }
}
