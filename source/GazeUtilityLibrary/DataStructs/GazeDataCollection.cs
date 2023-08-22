using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GazeUtilityLibrary.DataStructs
{

    /// <summary>
    /// The gaze data set, including 2d and (optionally) 3d gaze data as well as optional eye data.
    /// </summary>
    public class GazeDataCollection
    {
        private GazeData2d _gazeData2d;
        /// <summary>
        /// The 2d gaze data.
        /// </summary>
        public GazeData2d GazeData2d { get { return _gazeData2d; } }

        /// <summary>
        /// The 3d gaze data.
        /// </summary>
        private GazeData3d? _gazeData3d = null;
        public GazeData3d? GazeData3d { get { return _gazeData3d; } }

        private EyeData? _eyeData = null;
        /// <summary>
        /// Pupil data of the eye.
        /// </summary>
        public EyeData? EyeData { get { return _eyeData; } }


        /// <summary>
        /// Initializes a new instance of the <see cref="GazeDataItem"/> class.
        /// </summary>
        /// <param name="gazePoint2d">The 2d coordinates of the gaze point.</param>
        /// <param name="isGazePoint2dValid">The validity of the 2d gaze point.</param>
        public GazeDataCollection(Vector2 gazePoint2d, bool isGazePoint2dValid)
        {
            _gazeData2d = new GazeData2d(gazePoint2d, isGazePoint2dValid);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GazeDataItem"/> class.
        /// </summary>
        /// <param name="gazePoint2d">The 2d coordinates of the gaze point.</param>
        /// <param name="isGazePoint2dValid">The validity of the 2d gaze point.</param>
        /// <param name="gazePoint3d">The 3d coordinates of the gaze point.</param>
        /// <param name="isGazePoint3dValid">The validity of the 3d gaze point.</param>
        /// <param name="gazeOrigin3d">The 3d coordinates of the gaze origin.</param>
        /// <param name="isGazeOrigin3dValid">The validity of the 3d gaze origin.</param>
        /// <param name="pupilDiameter">The pupil diameter.</param>
        /// <param name="isPupilDiameterValid">The validity of the pupil diameter.</param>
        public GazeDataCollection(Vector2 gazePoint2d, bool isGazePoint2dValid, Vector3 gazePoint3d, bool isGazePoint3dValid, Vector3 gazeOrigin3d, bool isGazeOrigin3dValid, float pupilDiameter, bool isPupilDiameterValid)
        {
            _gazeData2d = new GazeData2d(gazePoint2d, isGazePoint2dValid);
            _gazeData3d = new GazeData3d(gazePoint3d, isGazePoint3dValid, gazeOrigin3d, isGazeOrigin3dValid);
            _eyeData = new EyeData(pupilDiameter, isPupilDiameterValid);
        }
    }
}
