using System.Numerics;

namespace GazeUtilityLibrary.DataStructs
{
    /// <summary>
    /// The 3d gaze data set.
    /// </summary>
    public class GazeData3d
    {
        private Vector3 _gazePoint;
        public Vector3 GazePoint { get { return _gazePoint; } }

        private bool _isGazePointValid;
        public bool IsGazePointValid { get { return _isGazePointValid; } }

        private Vector3 _gazeOrigin;
        public Vector3 GazeOrigin { get { return _gazeOrigin; } }

        private Vector3 _gazeDirection;
        public Vector3 GazeDirection { get { return _gazeDirection; } }

        private float _gazeDistance;
        public float GazeDistance { get { return _gazeDistance; } }

        private bool _isGazeOriginValid;
        public bool IsGazeOriginValid { get { return _isGazeOriginValid; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="GazeData3d"/> class.
        /// </summary>
        /// <param name="gazePoint">The 3d coordinates of the gaze point.</param>
        /// <param name="isGazePointValid">The validity of the 3d gaze point.</param>
        /// <param name="gazeOrigin">The 3d coordinates of the gaze origin.</param>
        /// <param name="isGazeOriginValid">The validity of the 3d gaze origin.</param>
        public GazeData3d(Vector3 gazePoint, bool isGazePointValid, Vector3 gazeOrigin, bool isGazeOriginValid)
        {
            _gazePoint = gazePoint;
            _isGazePointValid = isGazePointValid;
            _gazeOrigin = gazeOrigin;
            _gazeDirection = gazePoint - gazeOrigin;
            _gazeDistance = _gazeDirection.Length();
            _isGazeOriginValid = isGazeOriginValid;
        }
    }
}
