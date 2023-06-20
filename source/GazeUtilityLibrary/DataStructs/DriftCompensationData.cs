using GazeUtilityLibrary.Tracker;
using System.Numerics;

namespace GazeUtilityLibrary.DataStructs
{

    /// <summary>
    /// 
    /// </summary>
    public class DriftCompensationData
    {
        private Vector2 _gazePosition2d;
        public Vector2 GazePosition2d { get { return _gazePosition2d; } }

        private Vector3 _gazePosition3d;
        public Vector3 GazePosition3d { get { return _gazePosition3d; } }

        private Quaternion _compensation;
        public Quaternion Compensation { get { return _compensation; } }

        public DriftCompensationData(ScreenArea screen, Quaternion driftCompensation, GazeData3d gazeData)
        {
            _compensation = driftCompensation;
            Vector3 newGazeDirection = Vector3.Transform(gazeData.GazeDirection, _compensation);
            _gazePosition3d = screen.GetIntersectionPoint(gazeData.GazeOrigin, newGazeDirection) ?? Vector3.Zero;
            _gazePosition2d = screen.GetPoint2dNormalized(_gazePosition3d);
        }
    }
}
