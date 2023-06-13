using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private bool _hasLicense = true;
        private IEyeTracker? _eyeTracker = null;
        private ScreenBasedCalibration? _screenBasedCalibration = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="EyeTrackerPro"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="ready_timer">The ready timer.</param>
        /// <param name="license_path">The license path.</param>
        public EyeTrackerPro(TrackerLogger logger, int ready_timer, string? license_path) : base(logger, ready_timer, "Tobii SDK Pro")
        {
            State = DeviceStatus.Configuring;
            foreach (IEyeTracker eyeTracker in EyeTrackingOperations.FindAllEyeTrackers())
            {
                logger.Debug($"Tracker: {eyeTracker.Address}");
                _eyeTracker = eyeTracker;
                break;
            }

            if (_eyeTracker == null)
            {
                logger.Error("No eye tracker connected");
                return;
            }
            _eyeTracker.GazeDataReceived += OnGazeDataReceivedPro;
            _eyeTracker.ConnectionLost += OnConnectionLost;
            _eyeTracker.ConnectionRestored += OnConnectionRestored;
            _eyeTracker.UserPositionGuideReceived += OnUserPositionDataReceivedPro;

            if (license_path != null && license_path != "")
            {
                ApplyLicense(_eyeTracker, PatternReplace(license_path));
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
            _hasLicense = false;
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
                _hasLicense = true;
                logger.Info("Successfully applied license");
            }
            else
            {
                State = DeviceStatus.InvalidConfiguration;
                logger.Error($"Failed to apply license. The validation result is {failedLicenses[0].ValidationResult}.");
            }
        }

        override public async Task InitCalibration()
        {
            _screenBasedCalibration = new ScreenBasedCalibration(_eyeTracker);
            await _screenBasedCalibration.EnterCalibrationModeAsync();
        }

        override public async Task<bool> CollectData(Point point)
        {
            if (_screenBasedCalibration == null)
            {
                return false;
            }

            NormalizedPoint2D normalizedPoint = new NormalizedPoint2D((float)point.X, (float)point.Y);
            int fail_count = 0;
            CalibrationStatus status = CalibrationStatus.Failure;
            status = await _screenBasedCalibration.CollectDataAsync(normalizedPoint);
            while (status != CalibrationStatus.Success && fail_count < 3)
            {
                // Try again if it didn't go well the first time.
                // Not all eye tracker models will fail at this point, but instead fail on ComputeAndApply.
                logger.Warning($"Data collection failed, retry #{fail_count}");
                fail_count++;
                status = await _screenBasedCalibration.CollectDataAsync(normalizedPoint);
            }

            if (status != CalibrationStatus.Success)
            {
                return false;
            }

            return true;
        }

        override public async Task FinishCalibration()
        {
            if (_screenBasedCalibration == null)
            {
                return;
            }

            await _screenBasedCalibration.LeaveCalibrationModeAsync();
        }

        override public async Task<List<CalibrationDataArgs>> ApplyCalibration()
        {
            List<CalibrationDataArgs> result = new List<CalibrationDataArgs>();
            if (_screenBasedCalibration == null)
            {
                return result;
            }

            CalibrationResult calibrationResult;

            calibrationResult = await _screenBasedCalibration.ComputeAndApplyAsync();

            logger.Info($"Calibration returned {calibrationResult.Status} and collected {calibrationResult.CalibrationPoints.Count} points.");

            foreach (CalibrationPoint point in calibrationResult.CalibrationPoints)
            {
                double xLeft = 0;
                double yLeft = 0;
                double xRight = 0;
                double yRight = 0;
                double xLeftSum = 0;
                double yLeftSum = 0;
                double xRightSum = 0;
                double yRightSum = 0;
                int leftCount = 0;
                int rightCount = 0;
                foreach (CalibrationSample sample in point.CalibrationSamples)
                {
                    if (sample.LeftEye.Validity == CalibrationEyeValidity.ValidAndUsed)
                    {
                        xLeftSum += sample.LeftEye.PositionOnDisplayArea.X;
                        yLeftSum += sample.LeftEye.PositionOnDisplayArea.Y;
                        leftCount++;
                    }
                    if (sample.RightEye.Validity == CalibrationEyeValidity.ValidAndUsed)
                    {
                        xRightSum += sample.RightEye.PositionOnDisplayArea.X;
                        yRightSum += sample.RightEye.PositionOnDisplayArea.Y;
                        rightCount++;
                    }
                    if (leftCount > 0)
                    {
                        xLeft = xLeftSum / leftCount;
                        yLeft = yLeftSum / leftCount;
                    }
                    if (rightCount > 0)
                    {
                        xRight = xRightSum / rightCount;
                        yRight = yRightSum / rightCount;
                    }
                    result.Add(new CalibrationDataArgs(
                        point.PositionOnDisplayArea.X,
                        point.PositionOnDisplayArea.Y,
                        xLeft,
                        yLeft,
                        sample.LeftEye.Validity == CalibrationEyeValidity.ValidAndUsed,
                        xRight,
                        yRight,
                        sample.RightEye.Validity == CalibrationEyeValidity.ValidAndUsed
                    ));
                }
            }
            return result;
        }

        /// <summary>
        /// Computes the distance of an eye to the tracker device.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="z">The z coordinate.</param>
        /// <returns></returns>
        private double ComputeEyeDistance(Point3D point)
        {
            return Math.Sqrt(point.X * point.X + point.Y * point.Y + point.Z * point.Z);
        }
        private double ComputeGazeDistance(Point3D gaze, Point3D origin)
        {
            Point3D distance = new Point3D(gaze.X - origin.X, gaze.Y - origin.Y, gaze.Z - origin.Z);
            return ComputeEyeDistance(distance);
        }

        protected override int GetFixationFrameCount()
        {
            // 500ms at 60 Hz
            return 30;
        }

        override public bool UpdateDriftCompensation(GazeDataArgs args)
        {
            if (args.ArgsPro == null)
            {
                return false;
            }
            if (args.ArgsPro.LeftEye.GazePoint.Validity == Validity.Invalid
                || args.ArgsPro.LeftEye.GazeOrigin.Validity == Validity.Invalid
                || args.ArgsPro.RightEye.GazePoint.Validity == Validity.Invalid
                || args.ArgsPro.RightEye.GazeOrigin.Validity == Validity.Invalid)
            {
                return false;
            }

            return base.UpdateDriftCompensation(args);
        }

        override protected DriftCompensation ComputeDriftCompensation(ref List<GazeDataArgs> samples)
        {
            double xCoordLeft = samples.Average(sample => sample.ArgsPro?.LeftEye.GazePoint.PositionOnDisplayArea.X ?? 0);
            double yCoordLeft = samples.Average(sample => sample.ArgsPro?.LeftEye.GazePoint.PositionOnDisplayArea.Y ?? 0);
            double xCoordRight = samples.Average(sample => sample.ArgsPro?.RightEye.GazePoint.PositionOnDisplayArea.X ?? 0);
            double yCoordRight = samples.Average(sample => sample.ArgsPro?.RightEye.GazePoint.PositionOnDisplayArea.Y ?? 0);

            return new DriftCompensation(
                0.5 - xCoordLeft,
                0.5 - yCoordLeft,
                0.5 - xCoordRight,
                0.5 - yCoordRight
            );
        }

        override protected double ComputeDispersion(ref List<GazeDataArgs> samples)
        {
            double xMax = samples.Max(sample => sample.ArgsPro?.LeftEye.GazePoint.PositionInUserCoordinates.X ?? double.MinValue);
            double yMax = samples.Max(sample => sample.ArgsPro?.LeftEye.GazePoint.PositionInUserCoordinates.Y ?? double.MinValue);
            double zMax = samples.Max(sample => sample.ArgsPro?.LeftEye.GazePoint.PositionInUserCoordinates.Z ?? double.MinValue);
            double xMin = samples.Min(sample => sample.ArgsPro?.LeftEye.GazePoint.PositionInUserCoordinates.X ?? double.MaxValue);
            double yMin = samples.Min(sample => sample.ArgsPro?.LeftEye.GazePoint.PositionInUserCoordinates.Y ?? double.MaxValue);
            double zMin = samples.Min(sample => sample.ArgsPro?.LeftEye.GazePoint.PositionInUserCoordinates.Z ?? double.MaxValue);
            double dispersionLeft = xMax - xMin + yMax - yMin + zMax - zMin;
            xMax = samples.Max(sample => sample.ArgsPro?.RightEye.GazePoint.PositionInUserCoordinates.X ?? double.MinValue);
            yMax = samples.Max(sample => sample.ArgsPro?.RightEye.GazePoint.PositionInUserCoordinates.Y ?? double.MinValue);
            zMax = samples.Max(sample => sample.ArgsPro?.RightEye.GazePoint.PositionInUserCoordinates.Z ?? double.MinValue);
            xMin = samples.Min(sample => sample.ArgsPro?.RightEye.GazePoint.PositionInUserCoordinates.X ?? double.MaxValue);
            yMin = samples.Min(sample => sample.ArgsPro?.RightEye.GazePoint.PositionInUserCoordinates.Y ?? double.MaxValue);
            zMin = samples.Min(sample => sample.ArgsPro?.RightEye.GazePoint.PositionInUserCoordinates.Z ?? double.MaxValue);
            double dispersionRight = xMax - xMin + yMax - yMin + zMax - zMin;
            return Math.Max(dispersionLeft, dispersionRight);
        }

        override protected double ComputeMaxDeviation(ref List<GazeDataArgs> samples, double normalizedDispersionThreshold)
        {
            double distLeft = samples.Average(sample => {
                if (sample.ArgsPro == null)
                {
                    return 0;
                }
                return ComputeGazeDistance(
                    sample.ArgsPro.LeftEye.GazePoint.PositionInUserCoordinates,
                    sample.ArgsPro.LeftEye.GazeOrigin.PositionInUserCoordinates);
            });
            double distRight = samples.Average(sample => {
                if (sample.ArgsPro == null)
                {
                    return 0;
                }
                return ComputeGazeDistance(
                    sample.ArgsPro.RightEye.GazePoint.PositionInUserCoordinates,
                    sample.ArgsPro.RightEye.GazeOrigin.PositionInUserCoordinates);
            });
            double dist = (distLeft + distRight) / 2;

            return dist * normalizedDispersionThreshold;
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
            return _hasLicense;
        }

        public override bool IsInitialised()
        {
            return _eyeTracker != null;
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
            res = res.Replace("%S", _eyeTracker?.SerialNumber);
            res = res.Replace("%A", _eyeTracker?.Address.ToString());
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
            double left_gaze_x = data.LeftEye.GazePoint.PositionOnDisplayArea.X + driftCompensation.XCoordLeft;
            double right_gaze_x = data.RightEye.GazePoint.PositionOnDisplayArea.X + driftCompensation.XCoordRight;
            double left_gaze_y = data.LeftEye.GazePoint.PositionOnDisplayArea.Y + driftCompensation.YCoordLeft;
            double right_gaze_y = data.RightEye.GazePoint.PositionOnDisplayArea.Y + driftCompensation.YCoordRight;
            double distance_left = ComputeEyeDistance(
                data.LeftEye.GazeOrigin.PositionInUserCoordinates
            );
            double distance_right = ComputeEyeDistance(
                data.RightEye.GazeOrigin.PositionInUserCoordinates
            );
            GazeDataArgs gazeData = new GazeDataArgs(
                TimeSpan.FromMilliseconds(data.SystemTimeStamp/1000),
                GazeFilter(left_gaze_x, right_gaze_x),
                left_gaze_x,
                right_gaze_x,
                GazeFilter(left_gaze_y, right_gaze_y),
                left_gaze_y,
                right_gaze_y,
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
                (data.RightEye.GazeOrigin.Validity == Validity.Valid),
                data
            );
            OnGazeDataReceived(gazeData);
        }

        private void OnUserPositionDataReceivedPro(object? sender, UserPositionGuideEventArgs data)
        {
            UserPositionDataArgs userPositionData = new UserPositionDataArgs(
                1 - data.LeftEye.UserPosition.X,
                data.LeftEye.UserPosition.Y,
                1 - data.LeftEye.UserPosition.Z,
                1 - data.RightEye.UserPosition.X,
                data.RightEye.UserPosition.Y,
                1 - data.RightEye.UserPosition.Z
            );
            OnUserPositionDataReceived(userPositionData);
        }
    }
}
