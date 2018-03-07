using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tobii.Research;
using Tobii.Interaction.Framework;

namespace GazeHelper
{
    public class EyeTrackerPro : EyeTrackerHandler
    {
        public event Action<double?, double?, double?, long> GazeDataReceived;
        public EyeTrackerPro(TrackerLogger logger, int ready_timer) : base(logger, ready_timer)
        {
            logger.Info("Using Tobii SDK Pro");
            State = EyeTrackingDeviceStatus.Configuring;
            IEyeTracker eyeTracker = null;
            foreach (IEyeTracker _eyeTracker in EyeTrackingOperations.FindAllEyeTrackers())
            {
                eyeTracker = _eyeTracker;
                break;
            }
            eyeTracker.GazeDataReceived += OnGazeDataReceived;
            eyeTracker.ConnectionLost += OnConnectionLost;
            eyeTracker.ConnectionRestored += OnConnectionRestored;

            ApplyLicense(eyeTracker, @"C:\Users\Simon Maurer\Documents\IS404-100107249362");
        }

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

        private void OnConnectionLost(object sender, ConnectionLostEventArgs e)
        {
            State = EyeTrackingDeviceStatus.DeviceNotConnected;
        }

        private void OnConnectionRestored(object sender, ConnectionRestoredEventArgs e)
        {
            State = EyeTrackingDeviceStatus.Initializing;
        }

        private void OnGazeDataReceived(object sender, GazeDataEventArgs data)
        {
            State = EyeTrackingDeviceStatus.Tracking;
            double left_x = data.LeftEye.GazePoint.PositionOnDisplayArea.X * SystemParameters.PrimaryScreenWidth;
            double left_y = data.LeftEye.GazePoint.PositionOnDisplayArea.Y * SystemParameters.PrimaryScreenHeight;
            double right_x = data.RightEye.GazePoint.PositionOnDisplayArea.X * SystemParameters.PrimaryScreenWidth;
            double right_y = data.RightEye.GazePoint.PositionOnDisplayArea.Y * SystemParameters.PrimaryScreenHeight;
            GazeDataReceived?.Invoke(GazeFilter(left_x, right_x), GazeFilter(left_y, right_y), GazeFilter(data.LeftEye.Pupil.PupilDiameter, data.RightEye.Pupil.PupilDiameter), data.DeviceTimeStamp);
            //Console.WriteLine($"left x: {left_x}, y: {left_y}, dia: {data.LeftEye.Pupil.PupilDiameter} ({data.LeftEye.Pupil.Validity})");
            //Console.WriteLine($"right x: {right_x}, y: {right_y}, dia: {data.RightEye.Pupil.PupilDiameter} ({data.RightEye.Pupil.Validity})");
        }

        private double? GazeFilter( double left, double right)
        {
            if (double.IsNaN(left) && double.IsNaN(right)) return null;
            if (double.IsNaN(left)) return right;
            if (double.IsNaN(right)) return left;
            return (left + right) / 2;
        }
    }
}
