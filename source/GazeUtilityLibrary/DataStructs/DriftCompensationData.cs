using GazeUtilityLibrary.Tracker;
using System.Numerics;

namespace GazeUtilityLibrary.DataStructs
{

    /// <summary>
    /// The drift compensation data structure
    /// </summary>
    public class DriftCompensationData
    {
        private Vector2 _gazePosition2d;
        /// <summary>
        /// The drift compensated 2d gaze position
        /// </summary>
        public Vector2 GazePosition2d { get { return _gazePosition2d; } }

        private Vector3 _gazePosition3d;
        /// <summary>
        /// The drift compensated 3d gaze position
        /// </summary>
        public Vector3 GazePosition3d { get { return _gazePosition3d; } }

        private Quaternion _compensation;
        /// <summary>
        /// The drift compensation quaternion
        /// </summary>
        public Quaternion Compensation { get { return _compensation; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="screen">The screen area</param>
        /// <param name="driftCompensation">The drift compensation quaternion</param>
        /// <param name="gazeData">The 3d gaze data structure</param>
        public DriftCompensationData(ScreenArea screen, Quaternion driftCompensation, GazeData3d gazeData)
        {
            _compensation = driftCompensation;
            Vector3 newGazeDirection = Vector3.Transform(gazeData.GazeDirection, _compensation);
            _gazePosition3d = screen.GetIntersectionPoint(gazeData.GazeOrigin, newGazeDirection) ?? Vector3.Zero;
            _gazePosition2d = screen.GetPoint2dNormalized(_gazePosition3d);
        }
    }
}
