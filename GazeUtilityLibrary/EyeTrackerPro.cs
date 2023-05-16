using System;
using System.Windows;
using Tobii.Research;

namespace GazeUtilityLibrary
{
    /// <summary>
    /// Interface to the Tobii SDK Pro engine
    /// </summary>
    /// <seealso cref="GazeHelper.TrackerHandler" />
    public class EyeTrackerPro : TrackerHandler
    {
        private bool hasLicense = true;
        private IEyeTracker? eyeTracker = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="EyeTrackerPro"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="ready_timer">The ready timer.</param>
        /// <param name="license_path">The license path.</param>
        public EyeTrackerPro(TrackerLogger logger, int ready_timer, string? license_path) : base(logger, ready_timer, "Tobii SDK Pro")
        {
            State = DeviceStatus.Configuring;
            foreach (IEyeTracker _eyeTracker in EyeTrackingOperations.FindAllEyeTrackers())
            {
                logger.Debug($"Tracker: {_eyeTracker.Address}");
                eyeTracker = _eyeTracker;
                break;
            }

            if (eyeTracker == null)
            {
                logger.Error("No eye tracker connected");
                return;
            }
            eyeTracker.GazeDataReceived += OnGazeDataReceivedPro;
            eyeTracker.ConnectionLost += OnConnectionLost;
            eyeTracker.ConnectionRestored += OnConnectionRestored;
            
            if(license_path != null)
            {
                ApplyLicense(eyeTracker, PatternReplace(license_path));
            }
        }

        /// <summary>
        /// Applies a license to a tobii eyetracker.
        /// </summary>
        /// <param name="eyeTracker">The eye tracker.</param>
        /// <param name="licensePath">The license path.</param>
        private void ApplyLicense(IEyeTracker eyeTracker, string licensePath)
        {
            logger.Info($"Applying license {licensePath}");
            hasLicense = false;
            byte[] license_file;
            try
            {
                license_file = System.IO.File.ReadAllBytes(licensePath);
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
                logger.Error("Failed to apply license");
                return;
            }
            // Create a collection with the license.
            var licenseCollection = new LicenseCollection(
                new System.Collections.Generic.List<LicenseKey>
                {
                    new LicenseKey(System.IO.File.ReadAllBytes(licensePath))
                });
            // See if we can apply the license.
            FailedLicenseCollection failedLicenses;
            if (eyeTracker.TryApplyLicenses(licenseCollection, out failedLicenses))
            {
                State = DeviceStatus.Initializing;
                hasLicense = true;
                logger.Info("Successfully applied license");
            }
            else
            {
                State = DeviceStatus.InvalidConfiguration;
                logger.Error($"Failed to apply license. The validation result is {failedLicenses[0].ValidationResult}.");
            }
        }

        /// <summary>
        /// Computes the distance of an eye to the tracker device.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="z">The z coordinate.</param>
        /// <returns></returns>
        private double ComputeEyeDistance(double x, double y, double z)
        {
            return Math.Sqrt(x * x + y * y + z * z);
        }

        /// <summary>
        /// Combines the data values form the left and the right eye.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns></returns>
        private double GazeFilter(double left, double right)
        {
            if (double.IsNaN(left)) return right;
            if (double.IsNaN(right)) return left;
            return (left + right) / 2;
        }

        /// <summary>
        /// Determines whether the license is applied to the eyetracker device 
        /// </summary>
        /// <returns>
        ///   <c>true</c> if [is license ok]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsLicenseOk()
        {
            return hasLicense;
        }

        /// <summary>
        /// Replaces a patten string with information from the eye tracker. 
        /// </summary>
        /// <returns>
        ///   The string where patterns were replaced.
        /// </returns>
        override public string PatternReplace(string pattern)
        {
            string res = pattern;
            res = res.Replace("%S", eyeTracker?.SerialNumber);
            res = res.Replace("%A", eyeTracker?.Address.ToString());
            return res;
        }

        /// <summary>
        /// Called when [connection lost].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ConnectionLostEventArgs"/> instance containing the event data.</param>
        private void OnConnectionLost(object? sender, ConnectionLostEventArgs e)
        {
            State = DeviceStatus.DeviceNotConnected;
        }

        /// <summary>
        /// Called when [connection restored].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ConnectionRestoredEventArgs"/> instance containing the event data.</param>
        private void OnConnectionRestored(object? sender, ConnectionRestoredEventArgs e)
        {
            State = DeviceStatus.Initializing;
        }

        /// <summary>
        /// Called when [gaze data received].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="data">The <see cref="GazeDataEventArgs"/> instance containing the event data.</param>
        private void OnGazeDataReceivedPro(object? sender, GazeDataEventArgs data)
        {
            State = DeviceStatus.Tracking;
            double left_gaze_x = data.LeftEye.GazePoint.PositionOnDisplayArea.X * SystemParameters.PrimaryScreenWidth;
            double right_gaze_x = data.RightEye.GazePoint.PositionOnDisplayArea.X * SystemParameters.PrimaryScreenWidth;
            double left_gaze_y = data.LeftEye.GazePoint.PositionOnDisplayArea.Y * SystemParameters.PrimaryScreenHeight;
            double right_gaze_y = data.RightEye.GazePoint.PositionOnDisplayArea.Y * SystemParameters.PrimaryScreenHeight;
            double distance_left = ComputeEyeDistance(
                data.LeftEye.GazeOrigin.PositionInUserCoordinates.X,
                data.LeftEye.GazeOrigin.PositionInUserCoordinates.Y,
                data.LeftEye.GazeOrigin.PositionInUserCoordinates.Z
            );
            double distance_right = ComputeEyeDistance(
                data.RightEye.GazeOrigin.PositionInUserCoordinates.X,
                data.RightEye.GazeOrigin.PositionInUserCoordinates.Y,
                data.RightEye.GazeOrigin.PositionInUserCoordinates.Z
            );
            GazeDataArgs gazeData = new GazeDataArgs(
                TimeSpan.FromMilliseconds(data.SystemTimeStamp/1000),
                Math.Round(GazeFilter(left_gaze_x, right_gaze_x), 0),
                Math.Round(left_gaze_x, 0),
                Math.Round(right_gaze_x, 0),
                Math.Round(GazeFilter(left_gaze_y, right_gaze_y), 0),
                Math.Round(left_gaze_y, 0),
                Math.Round(right_gaze_y, 0),
                (data.LeftEye.GazePoint.Validity == Validity.Valid),
                (data.RightEye.GazePoint.Validity == Validity.Valid),
                GazeFilter(data.LeftEye.Pupil.PupilDiameter, data.RightEye.Pupil.PupilDiameter),
                data.LeftEye.Pupil.PupilDiameter,
                data.RightEye.Pupil.PupilDiameter,
                (data.LeftEye.Pupil.Validity == Validity.Valid),
                (data.RightEye.Pupil.Validity == Validity.Valid),
                data.LeftEye.GazeOrigin.PositionInUserCoordinates.X,
                data.LeftEye.GazeOrigin.PositionInUserCoordinates.Y,
                data.LeftEye.GazeOrigin.PositionInUserCoordinates.Z,
                data.RightEye.GazeOrigin.PositionInUserCoordinates.X,
                data.RightEye.GazeOrigin.PositionInUserCoordinates.Y,
                data.RightEye.GazeOrigin.PositionInUserCoordinates.Z,
                GazeFilter(distance_left, distance_right),
                distance_left,
                distance_right,
                (data.LeftEye.GazeOrigin.Validity == Validity.Valid),
                (data.RightEye.GazeOrigin.Validity == Validity.Valid)
            );
            OnGazeDataReceived(gazeData);
        }
    }
}
