/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;
using GazeUtilityLibrary.DataStructs;
using Tobii.Research;
using Tobii.Research.Addons;
using Tobii.Research.Addons.Utility;
using Point3D = Tobii.Research.Point3D;

namespace GazeUtilityLibrary.Tracker
{
    /// <summary>
    /// Helper class to hold the approximated gaze origin during the data collection of a calibration point.
    /// </summary>
    public class CalibrationOrigin
    {
        private NormalizedPoint2D _calibrationPoint;
        /// <summary>
        /// The calibration point
        /// </summary>
        public NormalizedPoint2D CalibrationPoint { get { return _calibrationPoint; } }

        private Point3D _left;
        /// <summary>
        /// The approximated gaze origin of the left eye.
        /// </summary>
        public Point3D Left { get { return _left; } }

        private Point3D _right;
        /// <summary>
        /// The approximated gaze origin of the right eye.
        /// </summary>
        public Point3D Right { get { return _right; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="left">The approximated gaze origin of the left eye.</param>
        /// <param name="right">The approximated gaze origin of the right eye.</param>
        /// <param name="calibrationPoint">The calibration point</param>
        public CalibrationOrigin(Point3D left, Point3D right, NormalizedPoint2D calibrationPoint)
        {
            _left = left;
            _right = right;
            _calibrationPoint = calibrationPoint;
        }
    }

    /// <summary>
    /// Interface to the Tobii SDK Pro engine
    /// </summary>
    /// <seealso cref="GazeHelper.TrackerHandler" />
    public class EyeTrackerPro : BaseTracker
    {
        private List<CalibrationOrigin> _calibrationOriginPoints = new List<CalibrationOrigin>();
        private float _outputFrequency = 60;
        private bool _hasLicense = true;
        private IEyeTracker? _eyeTracker = null;
        private ScreenBasedCalibration? _screenBasedCalibration = null;
        private ScreenBasedCalibrationValidation? _screenBasedCalibrationValidation = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="EyeTrackerPro"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="config">The config item.</param>
        public EyeTrackerPro(TrackerLogger logger, ConfigItem config) : base(logger, config, "Tobii SDK Pro")
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
            _outputFrequency = _eyeTracker.GetGazeOutputFrequency();
            _eyeTracker.GazeDataReceived += OnGazeDataReceivedPro;
            _eyeTracker.ConnectionLost += OnConnectionLost;
            _eyeTracker.ConnectionRestored += OnConnectionRestored;
            _eyeTracker.UserPositionGuideReceived += OnUserPositionDataReceivedPro;
            if (_eyeTracker.DeviceCapabilities.HasFlag(Capabilities.CanSetDisplayArea))
            {
                _eyeTracker.DisplayAreaChanged += (s, e) =>
                {
                    UpdateScreenArea(e.DisplayArea);
                };
            }
            UpdateScreenArea(_eyeTracker.GetDisplayArea());
            InitDriftCompensation();

            if (config.LicensePath != null && config.LicensePath != "")
            {
                ApplyLicense(_eyeTracker, PatternReplace(config.LicensePath));
            }

            logger.Info($"device capabilities: {_eyeTracker.DeviceCapabilities}");
        }

        /// <summary>
        /// Transforms the Tobii display structure into the screenArea structure.
        /// </summary>
        /// <param name="displayArea">The display area structure</param>
        private void UpdateScreenArea(DisplayArea displayArea)
        {
            screenArea = new ScreenArea(
                new Vector3(displayArea.BottomLeft.X, displayArea.BottomLeft.Y, displayArea.BottomLeft.Z),
                new Vector3(displayArea.BottomRight.X, displayArea.BottomRight.Y, displayArea.BottomRight.Z),
                new Vector3(displayArea.TopLeft.X, displayArea.TopLeft.Y, displayArea.TopLeft.Z),
                new Vector3(displayArea.TopRight.X, displayArea.TopRight.Y, displayArea.TopRight.Z),
                displayArea.Width,
                displayArea.Height
            );
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
                new List<LicenseKey>
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

        /// <summary>
        /// Initialise the screen based calibration.
        /// </summary>
        /// <returns>An async handler</returns>
        override public async Task InitCalibrationAsync()
        {
            _screenBasedCalibration = new ScreenBasedCalibration(_eyeTracker);
            await _screenBasedCalibration.EnterCalibrationModeAsync();
        }

        /// <summary>
        /// Initialise the screen based calibration.
        /// </summary>
        override public void InitCalibration()
        {
            _screenBasedCalibration = new ScreenBasedCalibration(_eyeTracker);
            _screenBasedCalibration.EnterCalibrationMode();
        }

        /// <summary>
        /// Initialise the screen based calibration.
        /// </summary>
        override public void InitValidation()
        {
            if (_eyeTracker != null)
            {
                _screenBasedCalibrationValidation = new ScreenBasedCalibrationValidation(_eyeTracker, GetFixationFrameCount(config.ValidationDurationThreshold), config.ValidationTimer);
                _screenBasedCalibrationValidation.EnterValidationMode();
            }
        }

        /// <summary>
        /// Initialise the drift compensation.
        /// </summary>
        protected override void InitDriftCompensation()
        {
            if (screenArea == null)
            {
                logger.Warning("Failed to initialise drift compensation: screenArea is not defined");
                return;
            }
            driftCompensation = new DriftCompensation(screenArea.Center, GetFixationFrameCount(config.DriftCompensationDurationThreshold), config.DriftCompensationDispersionThreshold, config.DriftCompensationDispersionThresholdMax);
            logger.Info($"Drift compensation initialised ({GetFixationFrameCount(config.DriftCompensationDurationThreshold)}, {config.DriftCompensationDispersionThreshold}, {config.DriftCompensationDispersionThresholdMax})");
        }

        /// <summary>
        /// Collects gaze data of a calibration point.
        /// </summary>
        /// <param name="point"></param>
        /// <returns>True on success, false on failure, wrapped by an async handler.</returns>
        override public async Task<bool> CollectCalibrationDataAsync(Point point)
        {
            if (_screenBasedCalibration == null || _eyeTracker == null)
            {
                return false;
            }

            NormalizedPoint2D normalizedPoint = new NormalizedPoint2D((float)point.X, (float)point.Y);
            int fail_count = 0;
            CalibrationStatus status = CalibrationStatus.Failure;

            List<Point3D> leftOrigins = new List<Point3D>();
            List<Point3D> rightOrigins = new List<Point3D>();
            EventHandler<GazeDataEventArgs> handler = (s, data) =>
            {
                if (data.LeftEye.GazeOrigin.Validity == Validity.Valid)
                {
                    leftOrigins.Add(data.LeftEye.GazeOrigin.PositionInUserCoordinates);
                }
                if (data.RightEye.GazeOrigin.Validity == Validity.Valid)
                {
                    rightOrigins.Add(data.LeftEye.GazeOrigin.PositionInUserCoordinates);
                }
            };
            _eyeTracker.GazeDataReceived += handler;

            status = await _screenBasedCalibration.CollectDataAsync(normalizedPoint);
            while (status != CalibrationStatus.Success && fail_count < 3)
            {
                // Try again if it didn't go well the first time.
                // Not all eye tracker models will fail at this point, but instead fail on ComputeAndApply.
                logger.Warning($"Data collection failed, retry #{fail_count}");
                fail_count++;
                leftOrigins.Clear();
                rightOrigins.Clear();
                status = await _screenBasedCalibration.CollectDataAsync(normalizedPoint);
            }

            _eyeTracker.GazeDataReceived -= handler;

            if (status != CalibrationStatus.Success)
            {
                return false;
            }

            if (leftOrigins.Count > 0 && rightOrigins.Count > 0)
            {
                // make sure, the handler is no longer modifying the lists
                List<Point3D> lo = leftOrigins.ToList();
                List<Point3D> ro = rightOrigins.ToList();

                Point3D leftOrigin = new Point3D(lo.Average(v => v.X), lo.Average(v => v.Y), lo.Average(v => v.Z));
                Point3D rightOrigin = new Point3D(ro.Average(v => v.X), ro.Average(v => v.Y), ro.Average(v => v.Z));
                _calibrationOriginPoints.Add(new CalibrationOrigin(leftOrigin, rightOrigin, normalizedPoint));
            }

            return true;
        }

        /// <summary>
        /// Collects gaze data of a validation point.
        /// </summary>
        /// <param name="point"></param>
        /// <returns>True on success, false on failure, wrapped by an async handler.</returns>
        override public async Task<bool> CollectValidationDataAsync(Point point)
        {
            if (_screenBasedCalibrationValidation == null)
            {
                return false;
            }

            NormalizedPoint2D normalizedPoint = new NormalizedPoint2D((float)point.X, (float)point.Y);
            _screenBasedCalibrationValidation.StartCollectingData(normalizedPoint);
            while (_screenBasedCalibrationValidation.State == ScreenBasedCalibrationValidation.ValidationState.CollectingData)
            {
                await Task.Delay(25);
            }

            return true;
        }

        /// <summary>
        /// Finish the screen based async calibration process.
        /// </summary>
        /// <returns>An async handler</returns>
        override public async Task FinishCalibrationAsync()
        {
            if (_screenBasedCalibration == null)
            {
                return;
            }

            await _screenBasedCalibration.LeaveCalibrationModeAsync();
        }

        /// <summary>
        /// Finish the screen based calibration process.
        /// </summary>
        override public void FinishCalibration()
        {
            if (_screenBasedCalibration == null)
            {
                return;
            }

            _screenBasedCalibration.LeaveCalibrationMode();
        }

        /// <summary>
        /// Finish the screen based validation process.
        /// </summary>
        override public void FinishValidation()
        {
            if (_screenBasedCalibrationValidation == null)
            {
                return;
            }

            _screenBasedCalibrationValidation.LeaveValidationMode();
        }

        /// <summary>
        /// Compute and apply the calibration data. Transform the Tobi calibration result into the GazeCalibrationData structure.
        /// </summary>
        /// <returns>The calibration data result wrapped by an async handler.</returns>
        override public async Task<List<GazeCalibrationData>> ApplyCalibration()
        {
            List<GazeCalibrationData> result = new List<GazeCalibrationData>();
            if (_screenBasedCalibration == null || screenArea == null)
            {
                return result;
            }

            CalibrationResult calibrationResult;
            calibrationResult = await _screenBasedCalibration.ComputeAndApplyAsync();

            logger.Info($"Calibration returned {calibrationResult.Status} and collected {calibrationResult.CalibrationPoints.Count} points.");

            foreach (Tobii.Research.CalibrationPoint point in calibrationResult.CalibrationPoints)
            {
                Vector3 calibrationPointTmp = screenArea.GetPoint3d(new Vector2(point.PositionOnDisplayArea.X, point.PositionOnDisplayArea.Y));
                Point3D calibrationPoint = new Point3D(calibrationPointTmp.X, calibrationPointTmp.Y, calibrationPointTmp.Z);
                CalibrationOrigin? origin = _calibrationOriginPoints.Find(v =>
                        Math.Abs(v.CalibrationPoint.X - point.PositionOnDisplayArea.X) < 0.00001
                        && Math.Abs(v.CalibrationPoint.Y - point.PositionOnDisplayArea.Y) < 0.00001);

                Vector2 leftGazePointScreen = new Vector2(point.CalibrationSamples.Average(s =>
                        s.LeftEye.PositionOnDisplayArea.X), point.CalibrationSamples.Average(s => s.LeftEye.PositionOnDisplayArea.Y));
                Vector3 leftGazePoint = screenArea.GetPoint3d(leftGazePointScreen);
                double leftAccuracy = 0;
                if (origin != null)
                {
                    leftAccuracy = ComputeCalibrationAccuracy(
                        origin.Left,
                        calibrationPoint,
                        new Point3D(leftGazePoint.X, leftGazePoint.Y, leftGazePoint.Z)
                    );
                }

                Vector2 rightGazePointScreen = new Vector2(point.CalibrationSamples.Average(s =>
                        s.RightEye.PositionOnDisplayArea.X), point.CalibrationSamples.Average(s => s.RightEye.PositionOnDisplayArea.Y));
                Vector3 rightGazePoint = screenArea.GetPoint3d(rightGazePointScreen);
                double rightAccuracy = 0;
                if (origin != null)
                {
                    rightAccuracy = ComputeCalibrationAccuracy(
                       origin.Right,
                       calibrationPoint,
                       new Point3D(rightGazePoint.X, rightGazePoint.Y, rightGazePoint.Z)
                    );
                }

                result.Add(new GazeCalibrationData(
                    point.PositionOnDisplayArea.X,
                    point.PositionOnDisplayArea.Y,
                    leftGazePointScreen.X,
                    leftGazePointScreen.Y,
                    point.CalibrationSamples.Aggregate(false, (acc, s) => acc || s.LeftEye.Validity == CalibrationEyeValidity.ValidAndUsed),
                    leftAccuracy,
                    rightGazePointScreen.X,
                    rightGazePointScreen.Y,
                    point.CalibrationSamples.Aggregate(false, (acc, s) => acc || s.RightEye.Validity == CalibrationEyeValidity.ValidAndUsed),
                    rightAccuracy
                ));
            }
            return result;
        }

        /// <summary>
        /// Given a calibration point, a gaze origin, and a gaze point compute the accuracy of the calibration.
        /// </summary>
        /// <param name="origin">The gaze origin</param>
        /// <param name="calibrationPoint">The calibration target</param>
        /// <param name="gazePoint">The gaze point</param>
        /// <returns>The calibration accuracy</returns>
        private double ComputeCalibrationAccuracy(Point3D origin, Point3D calibrationPoint, Point3D gazePoint)
        {
            Point3D directionCalibrationPoint = origin.NormalizedDirection(calibrationPoint);
            Point3D directionGazePoint = origin.NormalizedDirection(gazePoint);

            return directionCalibrationPoint.Angle(directionGazePoint);
        }

        /// <summary>
        /// Compute the validation data.
        /// </summary>
        /// <returns>The validation data result.</returns>
        override public GazeValidationData? ComputeValidation()
        {
            GazeValidationData data;

            if (_screenBasedCalibrationValidation == null)
            {
                return null;
            }

            CalibrationValidationResult result = _screenBasedCalibrationValidation.Compute();

            logger.Info($"Validation accuracy: left: {result.AverageAccuracyLeftEye},  right: {result.AverageAccuracyRightEye}.");
            logger.Info($"Validation precision: left: {result.AveragePrecisionLeftEye},  right: {result.AveragePrecisionRightEye}.");
            logger.Info($"Validation precision RMS: left: {result.AveragePrecisionRMSLeftEye},  right: {result.AveragePrecisionRMSRightEye}.");

            data = new GazeValidationData(result.AverageAccuracyLeftEye, result.AverageAccuracyRightEye,
                result.AveragePrecisionLeftEye, result.AveragePrecisionRightEye,
                result.AveragePrecisionRMSLeftEye, result.AveragePrecisionRMSRightEye );

            foreach(CalibrationValidationPoint point in result.Points)
            {
                data.AddPoint(new Vector2(point.Coordinates.X, point.Coordinates.Y), point.AccuracyLeftEye, point.AccuracyRightEye,
                    point.PrecisionLeftEye, point.PrecisionRightEye, point.PrecisionRMSLeftEye, point.PrecisionRMSRightEye);
            }

            return data;
        }

        /// <summary>
        /// Get the number of required gaze samples to compute a fixation.
        /// This is based on the duration threshold and the sample rate of the device.
        /// </summary>
        /// <param name="durationThreshold">The required fixation duration in milliseconds.</param>
        /// <returns>The number of required samples.</returns>
        protected override int GetFixationFrameCount(int durationThreshold)
        {
            float period = 1000 / _outputFrequency;
            return durationThreshold / (int)period;
        }

        /// <summary>
        /// Get the unit vector pointing in the direction of the gaze vector.
        /// </summary>
        /// <returns>The unit vector pointing in the negative z direction.</returns>
        protected override Vector3 GetUnitDirection()
        {
            return new Vector3(0, 0, -1);
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

        /// <summary>
        /// Checks if the tracker device exists.
        /// </summary>
        /// <returns>True if the tracker device exists, false otherwise.</returns>
        public override bool IsInitialised()
        {
            return _eyeTracker != null;
        }

        /// <summary>
        /// Replaces a patten string with information from the eye tracker.
        /// Supported patterns are %S for the serial number and %A for the address.
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
        /// Called when gaze data is received.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="data">The <see cref="GazeDataEventArgs"/> instance containing the event data.</param>
        private void OnGazeDataReceivedPro(object? sender, GazeDataEventArgs data)
        {
            long latency = (EyeTrackingOperations.GetSystemTimeStamp() - data.SystemTimeStamp) / 1000;
            long nowSystem = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            State = DeviceStatus.Tracking;
            GazeData gazeData = new GazeData(
                TimeSpan.FromMilliseconds(nowSystem - latency),
                TimeSpan.FromMilliseconds(nowSystem),
                new Vector2(data.LeftEye.GazePoint.PositionOnDisplayArea.X, data.LeftEye.GazePoint.PositionOnDisplayArea.Y),
                data.LeftEye.GazePoint.Validity == Validity.Valid,
                new Vector2(data.RightEye.GazePoint.PositionOnDisplayArea.X, data.RightEye.GazePoint.PositionOnDisplayArea.Y),
                data.RightEye.GazePoint.Validity == Validity.Valid,
                new Vector3(data.LeftEye.GazePoint.PositionInUserCoordinates.X, data.LeftEye.GazePoint.PositionInUserCoordinates.Y, data.LeftEye.GazePoint.PositionInUserCoordinates.Z),
                data.LeftEye.GazeOrigin.Validity == Validity.Valid,
                new Vector3(data.RightEye.GazePoint.PositionInUserCoordinates.X, data.RightEye.GazePoint.PositionInUserCoordinates.Y, data.RightEye.GazePoint.PositionInUserCoordinates.Z),
                data.RightEye.GazeOrigin.Validity == Validity.Valid,
                new Vector3(data.LeftEye.GazeOrigin.PositionInUserCoordinates.X, data.LeftEye.GazeOrigin.PositionInUserCoordinates.Y, data.LeftEye.GazeOrigin.PositionInUserCoordinates.Z),
                data.LeftEye.GazeOrigin.Validity == Validity.Valid,
                new Vector3(data.RightEye.GazeOrigin.PositionInUserCoordinates.X, data.RightEye.GazeOrigin.PositionInUserCoordinates.Y, data.RightEye.GazeOrigin.PositionInUserCoordinates.Z),
                data.RightEye.GazeOrigin.Validity == Validity.Valid,
                data.LeftEye.Pupil.PupilDiameter,
                data.LeftEye.Pupil.Validity == Validity.Valid,
                data.RightEye.Pupil.PupilDiameter,
                data.RightEye.Pupil.Validity == Validity.Valid
            );
            OnGazeDataReceived(gazeData);
        }

        /// <summary>
        /// Called when user position data is received.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="data">The <see cref="UserPositionGuideEventArgs"/> instance containing the event data.</param>
        private void OnUserPositionDataReceivedPro(object? sender, UserPositionGuideEventArgs data)
        {
            UserPositionData userPositionData = new UserPositionData(
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