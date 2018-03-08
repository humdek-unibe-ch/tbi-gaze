﻿using System;
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
            eyeTracker.GazeDataReceived += OnGazeDataReceivedPro;
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
                GazeFilter(data.LeftEye.Pupil.PupilDiameter, data.RightEye.Pupil.PupilDiameter),
                data.LeftEye.Pupil.PupilDiameter,
                data.RightEye.Pupil.PupilDiameter,
                (data.LeftEye.Pupil.Validity == Validity.Valid),
                (data.RightEye.Pupil.Validity == Validity.Valid)
            );
            OnGazeDataReceived(gazeData);
        }

        private double GazeFilter( double left, double right)
        {
            if (double.IsNaN(left)) return right;
            if (double.IsNaN(right)) return left;
            return (left + right) / 2;
        }
    }
}
