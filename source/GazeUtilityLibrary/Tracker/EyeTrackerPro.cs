using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;
using GazeUtilityLibrary.DataStructs;
using Tobii.Research;
using Tobii.Research.Addons;

namespace GazeUtilityLibrary.Tracker
{
    /// <summary>
    /// Interface to the Tobii SDK Pro engine
    /// </summary>
    /// <seealso cref="GazeHelper.TrackerHandler" />
    public class EyeTrackerPro : BaseTracker
    {
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
                _screenBasedCalibrationValidation = new ScreenBasedCalibrationValidation(_eyeTracker, GetFixationFrameCount(), 3000);
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
            driftCompensation = new DriftCompensation(screenArea.Center, GetFixationFrameCount(), config.DispersionThreshold);
        }

        /// <summary>
        /// Collects gaze data of a calibration point.
        /// </summary>
        /// <param name="point"></param>
        /// <returns>True on success, false on failure, wrapped by an async handler.</returns>
        override public async Task<bool> CollectCalibrationDataAsync(Point point)
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
            if (_screenBasedCalibration == null)
            {
                return result;
            }

            CalibrationResult calibrationResult;

            calibrationResult = await _screenBasedCalibration.ComputeAndApplyAsync();

            logger.Info($"Calibration returned {calibrationResult.Status} and collected {calibrationResult.CalibrationPoints.Count} points.");

            foreach (Tobii.Research.CalibrationPoint point in calibrationResult.CalibrationPoints)
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
                    result.Add(new GazeCalibrationData(
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
        /// </summary>
        /// <returns>60</returns>
        protected override int GetFixationFrameCount()
        {
            // 1000ms at 60 Hz
            return 60;
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
            State = DeviceStatus.Tracking;
            GazeData gazeData = new GazeData(
                TimeSpan.FromMilliseconds(data.SystemTimeStamp / 1000),
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