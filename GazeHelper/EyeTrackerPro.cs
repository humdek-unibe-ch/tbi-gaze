using System;
using System.Windows;
using Tobii.Research;
using Tobii.Interaction.Framework;

namespace GazeHelper
{
    /// <summary>
    /// Interface to the Tobii SDK Pro engine
    /// </summary>
    /// <seealso cref="GazeHelper.EyeTrackerHandler" />
    public class EyeTrackerPro : EyeTrackerHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EyeTrackerPro"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="ready_timer">The ready timer.</param>
        /// <param name="license_path">The license path.</param>
        public EyeTrackerPro(TrackerLogger logger, int ready_timer, string license_path) : base(logger, ready_timer)
        {
            logger.Info("Using Tobii SDK Pro");
            State = EyeTrackingDeviceStatus.Configuring;
            IEyeTracker eyeTracker = null;
            foreach (IEyeTracker _eyeTracker in EyeTrackingOperations.FindAllEyeTrackers())
            {
                eyeTracker = _eyeTracker;
                break;
            }
            eyeTracker.GazeDataReceived += OnGazeDataReceivedPro;
            eyeTracker.ConnectionLost += OnConnectionLost;
            eyeTracker.ConnectionRestored += OnConnectionRestored;

            ApplyLicense(eyeTracker, $"{license_path}\\{eyeTracker.SerialNumber}");
        }

        /// <summary>
        /// Applies a license to a tobii eyetracker.
        /// </summary>
        /// <param name="eyeTracker">The eye tracker.</param>
        /// <param name="licensePath">The license path.</param>
        private void ApplyLicense(IEyeTracker eyeTracker, string licensePath)
        {
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
                State = EyeTrackingDeviceStatus.Initializing;
                logger.Info("Successfully applied license");
            }
            else
            {
                State = EyeTrackingDeviceStatus.InvalidConfiguration;
                logger.Error($"Failed to apply license. The validation result is {failedLicenses[0].ValidationResult}.");
            }
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
        /// Called when [connection lost].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ConnectionLostEventArgs"/> instance containing the event data.</param>
        private void OnConnectionLost(object sender, ConnectionLostEventArgs e)
        {
            State = EyeTrackingDeviceStatus.DeviceNotConnected;
        }

        /// <summary>
        /// Called when [connection restored].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ConnectionRestoredEventArgs"/> instance containing the event data.</param>
        private void OnConnectionRestored(object sender, ConnectionRestoredEventArgs e)
        {
            State = EyeTrackingDeviceStatus.Initializing;
        }

        /// <summary>
        /// Called when [gaze data received].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="data">The <see cref="GazeDataEventArgs"/> instance containing the event data.</param>
        private void OnGazeDataReceivedPro(object sender, GazeDataEventArgs data)
        {
            State = EyeTrackingDeviceStatus.Tracking;
            double left_x = data.LeftEye.GazePoint.PositionOnDisplayArea.X * SystemParameters.PrimaryScreenWidth;
            double right_x = data.RightEye.GazePoint.PositionOnDisplayArea.X * SystemParameters.PrimaryScreenWidth;
            double left_y = data.LeftEye.GazePoint.PositionOnDisplayArea.Y * SystemParameters.PrimaryScreenHeight;
            double right_y = data.RightEye.GazePoint.PositionOnDisplayArea.Y * SystemParameters.PrimaryScreenHeight;
            GazeDataArgs gazeData = new GazeDataArgs(
                TimeSpan.FromMilliseconds(data.DeviceTimeStamp/1000),
                Math.Round(GazeFilter(left_x, right_x), 0),
                Math.Round(left_x, 0),
                Math.Round(right_x, 0),
                Math.Round(GazeFilter(left_y, right_y), 0),
                Math.Round(left_y, 0),
                Math.Round(right_y, 0),
                (data.LeftEye.GazePoint.Validity == Validity.Valid),
                (data.RightEye.GazePoint.Validity == Validity.Valid),
                GazeFilter(data.LeftEye.Pupil.PupilDiameter, data.RightEye.Pupil.PupilDiameter),
                data.LeftEye.Pupil.PupilDiameter,
                data.RightEye.Pupil.PupilDiameter,
                (data.LeftEye.Pupil.Validity == Validity.Valid),
                (data.RightEye.Pupil.Validity == Validity.Valid)
            );
            OnGazeDataReceived(gazeData);
        }
    }
}
