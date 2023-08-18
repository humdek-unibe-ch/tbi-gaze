using GazeUtilityLibrary.DataStructs;
using Newtonsoft.Json;
using System;
using System.IO;

namespace GazeUtilityLibrary
{
    public enum EOutputType
    {
        gaze,
        calibration,
        validation
    }

    public class GazeConfiguration
    {
        private ConfigItem? _config;
        private ConfigItem _default_config;
        public ConfigItem Config { get { return _config ?? _default_config; } }
        private TrackerLogger _logger;
        private GazeConfigError _error = new GazeConfigError();
        private string _starttime;
        private StreamWriter? _swGaze = null;
        private StreamWriter? _swCalibration = null;
        private StreamWriter? _swValidation = null;
        private JsonConfigParser _parser;
        public GazeConfiguration(TrackerLogger logger)
        {
            _logger = logger;
            _starttime = $"{DateTime.Now:yyyyMMddTHHmmss}";
            _parser = new JsonConfigParser(logger);
            _default_config = _parser.GetDefaultConfig();
        }

        /// <summary>
        /// Initialise the gaze configuration by parsing and checking the configuration file.
        /// </summary>
        /// <returns>True on success, False on failure.</returns>
        public bool InitConfig()
        {
            _config = _parser.ParseJsonConfig(ref _error);
            if (_config == null)
            {
                return true;
            }

            // check configuration name
            if (!ConfigChecker.CheckConfigName(_config!.ConfigName, _logger))
            {
                _logger.Error($"Bad config name: \"{_config.ConfigName}\"");
                _error.Error = EGazeConfigError.FallbackToDefaultConfigName;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Createa and Opens a data stream to a file where gaze data will be stored.
        /// </summary>
        private StreamWriter? InitOutputFile(string path)
        {
            StreamWriter? sw = null;
            try
            {
                sw = new StreamWriter($"{path}");
                FileInfo fi = new FileInfo($"{path}");
                _logger.Info($"Writing data to \"{getFileSwFullPath(sw)}\"");
            }
            catch (Exception e)
            {
                _logger.Error($"Failed to open file {path}: {e.Message}");
            }
            return sw;
        }

        /// <summary>
        /// Get the full path given a stream writer instance.
        /// </summary>
        /// <param name="sw">The stream writer instance.</param>
        /// <returns>The full path string.</returns>
        private string getFileSwFullPath(StreamWriter sw)
        {
            return ((FileStream)(sw.BaseStream)).Name;
        }

        /// <summary>
        /// Writes a log message if the configuration was not initialised.
        /// </summary>
        private void ReportUninitalisedConfig()
        {
            _logger.Error($"Configuration not initialized: Call InitConfig on the GazeConfiguration instance");
        }

        /// <summary>
        /// Close the gaze outputfile and rename it by appending error codes.
        /// </summary>
        /// <param name="error"></param>
        /// <returns>True on success, False on failure.</returns>
        public bool CleanupGazeOutputFile(string error)
        {
            if (_config == null || _swGaze == null)
            {
                return false;
            }
            if (!_config.DataLogWriteOutput)
            {
                return true;
            }
            string path = getFileSwFullPath(_swGaze!);
            _swGaze?.Close();
            _swGaze?.Dispose();
            _swGaze = null;
            File.Move(path, $"{path}{error}.txt");
            return true;
        }

        /// <summary>
        /// Close the calibration outputfile and rename it by appending error codes.
        /// </summary>
        /// <param name="error"></param>
        /// <returns>True on success, False on failure.</returns>
        public bool CleanupCalibrationOutputFile(string error)
        {
            if (_config == null)
            {
                return false;
            }
            if (!_config.CalibrationLogWriteOutput)
            {
                return true;
            }
            string path = getFileSwFullPath(_swCalibration!);
            _swCalibration?.Close();
            _swCalibration?.Dispose();
            _swCalibration = null;
            File.Move(path, $"{path}{error}.txt", true);
            return true;
        }

        /// <summary>
        /// Close the validation outputfile and rename it by appending error codes.
        /// </summary>
        /// <param name="error"></param>
        /// <returns>True on success, False on failure.</returns>
        public bool CleanupValidationOutputFile(string error)
        {
            if (_config == null)
            {
                return false;
            }
            if (!_config.ValidationLogWriteOutput)
            {
                return true;
            }
            string path = getFileSwFullPath(_swValidation!);
            _swValidation?.Close();
            _swValidation?.Dispose();
            _swValidation = null;
            File.Move(path, $"{path}{error}.txt", true);
            return true;
        }

        /// <summary>
        /// Deletes the old gaze log files.
        /// </summary>
        /// <param name="output_path">The Path to the folder with the old output files.</param>
        /// <param name="output_count">The number of files that are allowd in the path.</param>
        /// <param name="filter">The output data file filter.</param>
        private void DeleteOldGazeLogFiles(string output_path, int output_count, string filter)
        {
            string[] gazeLogFiles = Directory.GetFiles(output_path, filter);
            if ((output_count > 0) && (gazeLogFiles.GetLength(0) > output_count))
            {
                Array.Sort(gazeLogFiles);
                Array.Reverse(gazeLogFiles);
                for (int i = output_count; i < gazeLogFiles.GetLength(0); i++)
                {
                    File.Delete(gazeLogFiles[i]);
                    _logger.Info($"Removing old gaze data file \"{gazeLogFiles[i]}\"");
                }
            }
        }

        /// <summary>
        /// Dump current configuration to the disk.
        /// </summary>
        /// <returns>True on success, False on failure.</returns>
        public bool DumpCurrentConfigurationFile()
        {
            if (_config == null)
            {
                ReportUninitalisedConfig();
                return false;
            }
            _parser.SerializeJsonConfig(_config, $"{_config.DataLogPath}\\{_starttime}"
                + $"_{Environment.MachineName}_{_config.ConfigName}_config{_error.GetGazeConfigErrorString()}.json");
            return true;
        }

        /// <summary>
        /// Prepare the gaze output file based on the configuration.
        /// </summary>
        /// <param name="subjectCode">An optional subject code to be appended to the file name if set.</param>
        /// <param name="outputPath">An optional output path where the file will be stored.</param>
        /// <returns>True on success, False on failure.</returns>
        public bool PrepareGazeOutputFile(string? subjectCode, string? outputPath)
        {
            if (_config == null)
            {
                ReportUninitalisedConfig();
                return false;
            }
            if (!_config.DataLogWriteOutput)
            {
                return true;
            }
            string subjectCodeString = subjectCode != null ? $"_{subjectCode}" : "";
            string filePostfix = $"{Environment.MachineName}_{_config.ConfigName}{subjectCodeString}_{EOutputType.gaze.ToString()}";
            string fileName = $"{_starttime}_{filePostfix}";

            // create gaze data file
            if (outputPath != null)
            {
                _config.DataLogPath = outputPath;
            }

            string outputFilePath = $"{_config.DataLogPath}\\{fileName}";
            _swGaze = InitOutputFile(outputFilePath);
            if (_swGaze == null)
            {
                // something went wrong, write to the current directory
                _config.DataLogPath = Directory.GetCurrentDirectory();
                outputFilePath = $"{_config.DataLogPath}\\{fileName}";
                _logger.Warning($"Writing gaze data to the current directory: \"{outputFilePath}\"");
                _error.Error = EGazeConfigError.FallbackToCurrentOutputDir;
                _swGaze = new StreamWriter(outputFilePath);
            }

            // check output data format
            if (!ConfigChecker.CheckDataLogFormat(DateTime.Now.TimeOfDay, _config.DataLogFormatTimeStamp, _logger))
            {
                _config.DataLogFormatTimeStamp = _default_config.DataLogFormatTimeStamp;
                _logger.Warning($"Using the default output format for timestamps: \"{_config.DataLogFormatTimeStamp}\"");
                _error.Error = EGazeConfigError.FallbackToDefaultTimestampFormat;
            }
            if (!ConfigChecker.CheckDataLogFormat(1.000000, _config.DataLogFormatDiameter, _logger))
            {
                _config.DataLogFormatDiameter = _default_config.DataLogFormatDiameter;
                _logger.Warning($"Using the default output format for pupil diameters: \"{_config.DataLogFormatDiameter}\"");
                _error.Error = EGazeConfigError.FallbackToDefaultDiameterFormat;
            }
            if (!ConfigChecker.CheckDataLogFormat(1.000000, _config.DataLogFormatOrigin, _logger))
            {
                _config.DataLogFormatOrigin = _default_config.DataLogFormatOrigin;
                _logger.Warning($"Using the default output format for gaze origin values: \"{_config.DataLogFormatOrigin}\"");
                _error.Error = EGazeConfigError.FallbackToDefaultOriginFormat;
            }
            if (!ConfigChecker.CheckLogColumnOrder<GazeOutputValue>(_config.DataLogColumnOrder, _logger))
            {
                _config.DataLogColumnOrder = _default_config.DataLogColumnOrder;
                _logger.Warning($"Using the default column order: \"{_config.DataLogColumnOrder}\"");
                _error.Error = EGazeConfigError.FallbackToDefualtColumnOrder;
            }
            if (!ConfigChecker.CheckLogColumnTitles(_config.DataLogColumnOrder, _config.DataLogColumnTitle, _logger))
            {
                _logger.Warning($"Column titles are omitted");
                _error.Error = EGazeConfigError.OmitColumnTitles;
            }

            // write titles
            _swGaze?.WriteLine(String.Format(_config.DataLogColumnOrder, _config.DataLogColumnTitle));

            // delete old files
            DeleteOldGazeLogFiles(_config.DataLogPath, _config.DataLogCount, $"*_{filePostfix}");
            return true;
        }

        /// <summary>
        /// Prepare the calibration output file based on the configuration.
        /// </summary>
        /// <param name="subjectCode">An optional subject code to be appended to the file name if set.</param>
        /// <returns>True on success, False on failure.</returns>
        public bool PrepareCalibrationOutputFile(string? subjectCode)
        {
            if (_config == null)
            {
                ReportUninitalisedConfig();
                return false;
            }
            if (!_config.CalibrationLogWriteOutput)
            {
                return true;
            }

            string subjectCodeString = subjectCode != null ? $"_{subjectCode}" : "";
            string filePostfix = $"{Environment.MachineName}_{_config.ConfigName}{subjectCodeString}_{EOutputType.calibration.ToString()}";
            string fileName = $"{_starttime}_{filePostfix}";

            // create gaze data file
            if (_config.DataLogPath == "")
            {
                _config.DataLogPath = Directory.GetCurrentDirectory();
            }

            string outputFilePath = $"{_config.DataLogPath}\\{fileName}";
            _swCalibration = InitOutputFile(outputFilePath);
            if (_swCalibration == null)
            {
                // something went wrong, write to the current directory
                _config.DataLogPath = Directory.GetCurrentDirectory();
                outputFilePath = $"{_config.DataLogPath}\\{fileName}";
                _logger.Warning($"Writing calibration data to the current directory: \"{outputFilePath}\"");
                _error.Error = EGazeConfigError.FallbackToCurrentOutputDir;
                _swCalibration = new StreamWriter(outputFilePath);
            }

            // check output data format
            if (!ConfigChecker.CheckDataLogFormat(1.000000, _config.DataLogFormatNormalizedPoint, _logger))
            {
                _config.DataLogFormatNormalizedPoint = _default_config.DataLogFormatNormalizedPoint;
                _logger.Warning($"Using the default output format for normaliyed point values: \"{_config.DataLogFormatNormalizedPoint}\"");
                _error.Error = EGazeConfigError.FallbackToDefaultNormalizedFormat;
            }
            if (!ConfigChecker.CheckLogColumnOrder<CalibrationOutputValue>(_config.CalibrationLogColumnOrder, _logger))
            {
                _config.CalibrationLogColumnOrder = _default_config.CalibrationLogColumnOrder;
                _logger.Warning($"Using the default column order: \"{_config.CalibrationLogColumnOrder}\"");
                _error.Error = EGazeConfigError.FallbackToDefualtColumnOrder;
            }
            if (!ConfigChecker.CheckLogColumnTitles(_config.CalibrationLogColumnOrder, _config.CalibrationLogColumnTitle, _logger))
            {
                _logger.Warning($"Column titles are omitted");
                _error.Error = EGazeConfigError.OmitColumnTitles;
            }

            // write titles
            _swCalibration?.WriteLine(String.Format(_config.CalibrationLogColumnOrder, _config.CalibrationLogColumnTitle));

            // delete old files
            DeleteOldGazeLogFiles(_config.DataLogPath, _config.DataLogCount, $"*_{filePostfix}");
            return true;
        }

        /// <summary>
        /// Prepare the validation output file based on the configuration.
        /// </summary>
        /// <param name="subjectCode">An optional subject code to be appended to the file name if set.</param>
        /// <returns>True on success, False on failure.</returns>
        public bool PrepareValidationOutputFile(string? subjectCode)
        {
            if (_config == null)
            {
                ReportUninitalisedConfig();
                return false;
            }
            if (!_config.ValidationLogWriteOutput)
            {
                return true;
            }

            string subjectCodeString = subjectCode != null ? $"_{subjectCode}" : "";
            string filePostfix = $"{Environment.MachineName}_{_config.ConfigName}{subjectCodeString}_{EOutputType.validation.ToString()}";
            string fileName = $"{_starttime}_{filePostfix}";

            // create gaze data file
            if (_config.DataLogPath == "")
            {
                _config.DataLogPath = Directory.GetCurrentDirectory();
            }

            string outputFilePath = $"{_config.DataLogPath}\\{fileName}";
            _swValidation = InitOutputFile(outputFilePath);
            if (_swValidation == null)
            {
                // something went wrong, write to the current directory
                _config.DataLogPath = Directory.GetCurrentDirectory();
                outputFilePath = $"{_config.DataLogPath}\\{fileName}";
                _logger.Warning($"Writing validation data to the current directory: \"{outputFilePath}\"");
                _error.Error = EGazeConfigError.FallbackToCurrentOutputDir;
                _swValidation = new StreamWriter(outputFilePath);
            }

            // check output data format
            if (!ConfigChecker.CheckDataLogFormat(1.000000, _config.DataLogFormatNormalizedPoint, _logger))
            {
                _config.DataLogFormatNormalizedPoint = _default_config.DataLogFormatNormalizedPoint;
                _logger.Warning($"Using the default output format for normaliyed point values: \"{_config.DataLogFormatNormalizedPoint}\"");
                _error.Error = EGazeConfigError.FallbackToDefaultNormalizedFormat;
            }
            if (!ConfigChecker.CheckLogColumnOrder<ValidationOutputValue>(_config.ValidationLogColumnOrder, _logger))
            {
                _config.ValidationLogColumnOrder = _default_config.ValidationLogColumnOrder;
                _logger.Warning($"Using the default column order: \"{_config.ValidationLogColumnOrder}\"");
                _error.Error = EGazeConfigError.FallbackToDefualtColumnOrder;
            }
            if (!ConfigChecker.CheckLogColumnTitles(_config.ValidationLogColumnOrder, _config.ValidationLogColumnTitle, _logger))
            {
                _logger.Warning($"Column titles are omitted");
                _error.Error = EGazeConfigError.OmitColumnTitles;
            }

            // write titles
            _swValidation?.WriteLine(String.Format(_config.ValidationLogColumnOrder, _config.ValidationLogColumnTitle));

            // delete old files
            DeleteOldGazeLogFiles(_config.DataLogPath, _config.DataLogCount, $"*_{filePostfix}");
            return true;
        }

        /// <summary>
        /// Write to the gaze output file
        /// </summary>
        /// <param name="formatted_values">The list of formatted values to be written to the file.</param>
        public void WriteToGazeOutput(string[] formatted_values)
        {
            if (_config == null || _swGaze == null || _swGaze.BaseStream == null)
            {
                return;
            }
            _swGaze.WriteLine(String.Format(_config.DataLogColumnOrder, formatted_values));
        }

        /// <summary>
        /// Write to the calibration output file
        /// </summary>
        /// <param name="formatted_values">The list of formatted values to be written to the file.</param>
        public void WriteToCalibrationOutput(string[] formatted_values)
        {
            if (_config == null)
            {
                return;
            }
            _swCalibration?.WriteLine(String.Format(_config.CalibrationLogColumnOrder, formatted_values));
        }

        /// <summary>
        /// Write to the calibration output file
        /// </summary>
        /// <param name="formatted_values">The list of formatted values to be written to the file.</param>
        public void WriteToValidationOutput(string[] formatted_values)
        {
            if (_config == null)
            {
                return;
            }
            _swValidation?.WriteLine(String.Format(_config.ValidationLogColumnOrder, formatted_values));
        }
    }

    public class ConfigScreenArea
    {
        [JsonProperty(Required = Required.Always)]
        public double Width { get; set; }
        [JsonProperty(Required = Required.Always)]
        public double Height { get; set; }
        [JsonProperty(Required = Required.Always)]
        public double[] Center { get; set; }
        [JsonProperty(Required = Required.Always)]
        public double[] TopLeft { get; set; }
        [JsonProperty(Required = Required.Always)]
        public double[] TopRight { get; set; }
        [JsonProperty(Required = Required.Always)]
        public double[] BottomLeft { get; set; }
        [JsonProperty(Required = Required.Always)]
        public double[] BottomRight { get; set; }

        public ConfigScreenArea()
        {
            Width = 0;
            Height = 0;
            Center = new double[3] { 0, 0, 0 };
            TopLeft = new double[3] { 0, 0, 0 };
            TopRight = new double[3] { 0, 0, 0 };
            BottomLeft = new double[3] { 0, 0, 0 };
            BottomRight = new double[3] { 0, 0, 0 };
        }

        public ConfigScreenArea(ScreenArea screenArea)
        {
            Width = screenArea.Width;
            Height = screenArea.Height;
            Center = new double[3] { screenArea.Center.X, screenArea.Center.Y, screenArea.Center.Z };
            TopLeft = new double[3] { screenArea.TopLeft.X, screenArea.TopLeft.Y, screenArea.TopLeft.Z };
            TopRight = new double[3] { screenArea.TopRight.X, screenArea.TopRight.Y, screenArea.TopRight.Z };
            BottomLeft = new double[3] { screenArea.BottomLeft.X, screenArea.BottomLeft.Y, screenArea.BottomLeft.Z };
            BottomRight = new double[3] { screenArea.BottomRight.X, screenArea.BottomRight.Y, screenArea.BottomRight.Z };
        }
    }

    /// <summary>
    /// configuration file class
    /// </summary>
    public class ConfigItem
    {
        [JsonProperty(Required = Required.Always)]
        public string? ConfigName { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string DataLogColumnOrder { get; set; }
        [JsonProperty(Required = Required.Default)]
        public string[] DataLogColumnTitle { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string CalibrationLogColumnOrder { get; set; }
        [JsonProperty(Required = Required.Default)]
        public string[] CalibrationLogColumnTitle { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string ValidationLogColumnOrder { get; set; }
        [JsonProperty(Required = Required.Default)]
        public string[] ValidationLogColumnTitle { get; set; }
        [JsonProperty(Required = Required.Default)]
        public int DataLogCount { get; set; }
        [JsonProperty(Required = Required.Default)]
        public string DataLogFormatDiameter { get; set; }
        [JsonProperty(Required = Required.Default)]
        public string DataLogFormatOrigin { get; set; }
        [JsonProperty(Required = Required.Default)]
        public string DataLogFormatNormalizedPoint { get; set; }
        [JsonProperty(Required = Required.Default)]
        public string DataLogFormatTimeStamp { get; set; }
        [JsonProperty(Required = Required.Default)]
        public string DataLogFormatTimeStampRelative { get; set; }
        [JsonProperty(Required = Required.Default)]
        public string DataLogPath { get; set; }
        [JsonProperty(Required = Required.Always)]
        public bool DataLogWriteOutput { get; set; }
        [JsonProperty(Required = Required.Always)]
        public bool CalibrationLogWriteOutput { get; set; }
        [JsonProperty(Required = Required.Always)]
        public bool ValidationLogWriteOutput { get; set; }
        [JsonProperty(Required = Required.Default)]
        public double[][] CalibrationPoints { get; set; }
        [JsonProperty(Required = Required.Default)]
        public double[][] ValidationPoints { get; set; }
        [JsonProperty(Required = Required.Default)]
        public bool DataLogDisabledOnStartup { get; set; }
        [JsonProperty(Required = Required.Default)]
        public double DispersionThreshold { get; set; }
        [JsonProperty(Required = Required.Default)]
        public double DriftCompensationTimer { get; set; }
        [JsonProperty(Required = Required.Default)]
        public string? LicensePath { get; set; }
        [JsonProperty(Required = Required.Default)]
        public bool MouseControl { get; set; }
        [JsonProperty(Required = Required.Default)]
        public bool MouseControlHide { get; set; }
        [JsonProperty(Required = Required.Default)]
        public bool MouseCalibrationHide { get; set; }
        [JsonProperty(Required = Required.Default)]
        public string MouseStandardIconPath { get; set; }
        [JsonProperty(Required = Required.Default)]
        public int ReadyTimer { get; set; }
        [JsonProperty(Required = Required.Always)]
        public int TrackerDevice { get; set; }
        [JsonProperty(Required = Required.Default)]
        public string TobiiApplicationPath { get; set; }
        [JsonProperty(Required = Required.Default)]
        public string TobiiCalibrate { get; set; }
        [JsonProperty(Required = Required.Default)]
        public string TobiiCalibrateArguments { get; set; }
        [JsonProperty(Required = Required.Default)]
        public ConfigScreenArea ScreenArea { get; set; }

        public ConfigItem()
        {
            DataLogDisabledOnStartup = false;
            DataLogFormatTimeStamp = "hh\\:mm\\:ss\\.fff";
            DataLogFormatTimeStampRelative = "ss\\.fff";
            DataLogFormatDiameter = "0.000";
            DataLogFormatOrigin = "0.000";
            DataLogFormatNormalizedPoint = "0.000";
            DispersionThreshold = 1;
            DriftCompensationTimer = 5000;
            TobiiApplicationPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\Programs\\TobiiProEyeTrackerManager";
            TobiiCalibrate = "TobiiProEyeTrackerManager.exe";
            TobiiCalibrateArguments = "--device-sn=%S --mode=usercalibration";
            DataLogColumnOrder = "";
            foreach (int i in Enum.GetValues(typeof(GazeOutputValue)))
            {
                if (i == 0)
                {
                    DataLogColumnOrder += $"{{{i}}}";
                }
                else
                {
                    DataLogColumnOrder += $"\t{{{i}}}";
                }
            }
            DataLogColumnTitle = new string[] {
                "timestamp",
                "timestamp_relative",
                "tag",

                "combined_gazePoint2dCompensated_x",
                "combined_gazePoint2dCompensated_y",
                "combined_gazePoint2d_x",
                "combined_gazePoint2d_y",
                "combined_gazePoint2d_isValid",
                "combined_gazePoint3dCompensated_x",
                "combined_gazePoint3dCompensated_y",
                "combined_gazePoint3dCompensated_z",
                "combined_gazePoint3d_x",
                "combined_gazePoint3d_y",
                "combined_gazePoint3d_z",
                "combined_gazePoint3d_isValid",
                "combined_originPoint3d_x",
                "combined_originPoint3d_y",
                "combined_originPoint3d_z",
                "combined_originPoint3d_isValid",
                "combined_gazeDistance",
                "combined_pupilDiameter",
                "combined_pupilDiameter_isValid",

                "left_gazePoint2d_x",
                "left_gazePoint2d_y",
                "left_gazePoint2d_isValid",
                "left_gazePoint3d_x",
                "left_gazePoint3d_y",
                "left_gazePoint3d_z",
                "left_gazePoint3d_isValid",
                "left_gazeOrigin3d_x",
                "left_gazeOrigin3d_y",
                "left_gazeOrigin3d_z",
                "left_gazeOrigin3d_isValid",
                "left_gazeDistance",
                "left_pupilDiameter",
                "left_pupilDiameter_isValid",

                "right_gazePoint2d_x",
                "right_gazePoint2d_y",
                "right_gazePoint2d_isValid",
                "right_gazePoint3d_x",
                "right_gazePoint3d_y",
                "right_gazePoint3d_z",
                "right_gazePoint3d_isValid",
                "right_gazeOrigin3d_x",
                "right_gazeOrigin3d_y",
                "right_gazeOrigin3d_z",
                "right_gazeOrigin3d_isValid",
                "right_gazeDistance",
                "right_pupilDiameter",
                "right_pupilDiameter_isValid"
            };
            CalibrationLogColumnOrder =
                $"{{{(int)CalibrationOutputValue.Point2dX}}}\t" +
                $"{{{(int)CalibrationOutputValue.Point2dY}}}\t" +
                $"{{{(int)CalibrationOutputValue.LeftGazePoint2dX}}}\t" +
                $"{{{(int)CalibrationOutputValue.LeftGazePoint2dY}}}\t" +
                $"{{{(int)CalibrationOutputValue.LeftGazePoint2dIsValid}}}\t" +
                $"{{{(int)CalibrationOutputValue.RightGazePoint2dX}}}\t" +
                $"{{{(int)CalibrationOutputValue.RightGazePoint2dY}}}\t" +
                $"{{{(int)CalibrationOutputValue.RightGazePoint2dIsValid}}}";
            CalibrationLogColumnTitle = new string[] {
                "calibrationPoint_x",
                "calibrationPoint_y",
                "left_gazePoint_x",
                "left_gazePoint_y",
                "left_gazePoint_isValid",
                "right_gazePoint_x",
                "right_gazePoint_y",
                "right_gazePoint_isValid"
            };
            ValidationLogColumnOrder =
                $"{{{(int)ValidationOutputValue.LeftAccuracy}}}\t" +
                $"{{{(int)ValidationOutputValue.LeftPrecision}}}\t" +
                $"{{{(int)ValidationOutputValue.LeftPrecisionRMS}}}\t" +
                $"{{{(int)ValidationOutputValue.RightAccuracy}}}\t" +
                $"{{{(int)ValidationOutputValue.RightPrecision}}}\t" +
                $"{{{(int)ValidationOutputValue.RightPrecisionRMS}}}";
            ValidationLogColumnTitle = new string[] {
                "left_accuracy",
                "left_precision",
                "left_precision_rms",
                "right_accuracy",
                "right_precision",
                "right_precision_rms"
            };
            DataLogCount = 200;
            DataLogPath = Directory.GetCurrentDirectory();
            DataLogWriteOutput = true;
            CalibrationLogWriteOutput = true;
            ValidationLogWriteOutput = true;
            CalibrationPoints = new double[8][];
            CalibrationPoints[0] = new double[2] { 0.7, 0.5 };
            CalibrationPoints[1] = new double[2] { 0.3, 0.5 };
            CalibrationPoints[2] = new double[2] { 0.9, 0.9 };
            CalibrationPoints[3] = new double[2] { 0.1, 0.9 };
            CalibrationPoints[4] = new double[2] { 0.5, 0.1 };
            CalibrationPoints[5] = new double[2] { 0.1, 0.1 };
            CalibrationPoints[6] = new double[2] { 0.9, 0.1 };
            CalibrationPoints[7] = new double[2] { 0.5, 0.9 };
            ValidationPoints = new double[8][];
            ValidationPoints[0] = new double[2] { 0.7, 0.5 };
            ValidationPoints[1] = new double[2] { 0.3, 0.5 };
            ValidationPoints[2] = new double[2] { 0.9, 0.9 };
            ValidationPoints[3] = new double[2] { 0.1, 0.9 };
            ValidationPoints[4] = new double[2] { 0.5, 0.1 };
            ValidationPoints[5] = new double[2] { 0.1, 0.1 };
            ValidationPoints[6] = new double[2] { 0.9, 0.1 };
            ValidationPoints[7] = new double[2] { 0.5, 0.9 };
            MouseControl = false;
            MouseControlHide = false;
            MouseCalibrationHide = false;
            MouseStandardIconPath = "C:\\Windows\\Cursors\\aero_arrow.cur";
            ReadyTimer = 5000;
            TrackerDevice = 0;
            ScreenArea = new ConfigScreenArea();
        }
    }

    /// <summary>
    /// The config file "config.json" is parsed and its values are attributed to the ConfigItem class.
    /// </summary>
    public class JsonConfigParser
    {
        private string ConfigFile = "config.json";
        private TrackerLogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonConfigParser"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public JsonConfigParser(TrackerLogger logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Parses the json configuration.
        /// </summary>
        /// <returns>the updated ConfigItem class.</returns>
        public ConfigItem? ParseJsonConfig(ref GazeConfigError error)
        {
            string json;
            ConfigItem? item = null;
            ConfigItem item_default = GetDefaultConfig();

            // load configuration
            StreamReader? sr = OpenConfigFile(ConfigFile);
            if (sr != null)
            {
                try
                {
                    json = sr.ReadToEnd();
                    item = JsonConvert.DeserializeObject<ConfigItem>(json, new JsonSerializerSettings
                    {
                        MissingMemberHandling = MissingMemberHandling.Error
                    });

                    if (item?.DataLogColumnOrder == "")
                        item.DataLogColumnOrder = item_default.DataLogColumnOrder;
                    if (item?.CalibrationLogColumnOrder == "")
                        item.CalibrationLogColumnOrder = item_default.CalibrationLogColumnOrder;
                    if (item?.DataLogPath == "")
                        item.DataLogPath = item_default.DataLogPath;

                    logger.Info("Successfully parsed the configuration file");
                    sr.Close();
                }
                catch (Exception e)
                {
                    logger.Error($"Failed to parse config: {e.Message}");
                }
            }
            else
            {
                logger.Warning("No config file found, using default values");
                error.Error = EGazeConfigError.FallbackToDefaultConfig;
                item = item_default;
            }
            return item;
        }

        /// <summary>
        /// Serializes the json configuration object to a string and writes it to a file.
        /// </summary>
        /// <param name="item">The json configuration item.</param>
        /// <param name="path">The path where the file will be written.</param>
        public void SerializeJsonConfig(ConfigItem item, string path)
        {
            string json = JsonConvert.SerializeObject(item);
            StreamWriter sw = new StreamWriter(path);
            sw.Write(json);
            sw.Close();
        }

        /// <summary>
        /// Gets the default configuration values.
        /// </summary>
        /// <returns>the default configuration values.</returns>
        public ConfigItem GetDefaultConfig()
        {
            return new ConfigItem();
        }

        /// <summary>
        /// Opens whichever configuration file is availbale.
        /// First, the caller path is searched.
        /// Second, the execution path is searched.
        /// Third, default values are used.
        /// </summary>
        /// <param name="configFile">The configuration file.</param>
        /// <returns></returns>
        private StreamReader? OpenConfigFile(string configFile)
        {
            StreamReader? sr = null;
            try
            {
                sr = new StreamReader(configFile);
                logger.Info($"Parsing config file \"{Directory.GetCurrentDirectory()}\\{configFile}\"");
            }
            catch (FileNotFoundException e)
            {
                string path = AppDomain.CurrentDomain.BaseDirectory;
                logger.Info(e.Message);
                try
                {
                    sr = new StreamReader($"{path}\\{configFile}");
                    logger.Info($"Parsing config file \"{path}{configFile}\"");
                }
                catch (FileNotFoundException e2)
                {
                    logger.Info(e2.Message);
                }
            }
            return sr;
        }
    }
}
