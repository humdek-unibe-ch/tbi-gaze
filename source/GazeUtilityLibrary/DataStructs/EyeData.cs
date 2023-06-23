
namespace GazeUtilityLibrary.DataStructs
{
    /// <summary>
    /// The eye data set, including pupil information.
    /// </summary>
    public class EyeData
    {
        private float _pupilDiameter;
        /// <summary>
        /// The diameter of the pupil
        /// </summary>
        public float PupilDiameter { get { return _pupilDiameter; } }

        private bool _isPupilDiameterValid;
        /// <summary>
        /// The validity flag of th epupil diameter
        /// </summary>
        public bool IsPupilDiameterValid { get { return _isPupilDiameterValid; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="EyeData"/> class.
        /// </summary>
        /// <param name="pupilDiameter">The pupil diameter.</param>
        /// <param name="isPupilDiameterValid">The validity of the pupil diameter.</param>
        public EyeData(float pupilDiameter, bool isPupilDiameterValid)
        {
            _pupilDiameter = pupilDiameter;
            _isPupilDiameterValid = isPupilDiameterValid;
        }
    }
}
