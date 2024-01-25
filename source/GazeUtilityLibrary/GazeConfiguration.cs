/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
using GazeUtilityLibrary.DataStructs;
using Newtonsoft.Json;
using System;
using System.IO;

namespace GazeUtilityLibrary
{
    /// <summary>
    /// A list of output files.
    /// </summary>
    public enum EOutputType
    {
        gaze,
        calibration,
        validation,
        control
    }

    /// <summary>
    /// The gaze configuration handler.
    /// </summary>
    public class GazeConfiguration
    {
        private ConfigItem? _config;
        private ConfigItem _default_config;
        /// <summary>
        /// The JSON structure holding the configuratyion options.
        /// </summary>
        public ConfigItem Config { get { return _config ?? _default_config; } }
        private TrackerLogger _logger;
        private GazeConfigError _error = new GazeConfigError();
        private string _starttime;
        private StreamWriter? _swGaze = null;
        private StreamWriter? _swCalibration = null;
        private StreamWriter? _swValidation = null;
        private JsonConfigParser _parser;

        /// <summary>
        /// Initializes a new instance of the <see cref="GazeConfiguration"/> class.
        /// </summary>
        /// <param name="logger">The log handler.</param>
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
                return false;
            }

            if (!ConfigChecker.CheckPointList(_config.CalibrationPoints, _logger))
            {
                _logger.Error($"Invalid calibration point list");
                return false;
            }
            _config.CalibrationPoints = ConfigChecker.SanitizePointList(_config.CalibrationPoints, _logger);
            _logger.Info($"Added {_config.CalibrationPoints.GetLength(0)} calibration points");

            if (!ConfigChecker.CheckPointList(_config.ValidationPoints, _logger))
            {
                _logger.Error($"Invalid validation point list");
                return false;
            }
            _config.ValidationPoints = ConfigChecker.SanitizePointList(_config.ValidationPoints, _logger);
            _logger.Info($"Added {_config.ValidationPoints.GetLength(0)} validation points");

            if (!ConfigChecker.CheckColor(_config.BackgroundColor, _logger))
            {
                _logger.Warning($"Using background color 'Black' instead");
                _config.BackgroundColor = "Black";
            }

            if (!ConfigChecker.CheckColor(_config.FrameColor, _logger))
            {
                _logger.Warning($"Using background color '#202124' instead");
                _config.FrameColor = "#202124";
            }

            if (!ConfigChecker.CheckColor(_config.ForegroundColor, _logger))
            {
                _logger.Warning($"Using foreground color 'White' instead");
                _config.ForegroundColor = "White";
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
            File.Move(path, $"{path}{error}.csv");
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
            File.Move(path, $"{path}{error}.csv", true);
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
            File.Move(path, $"{path}{error}.csv", true);
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

    /// <summary>
    /// The JSON structure of the screen area.
    /// </summary>
    public class ConfigScreenArea
    {
        /// <summary>
        /// The width of the screen.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public double Width { get; set; }
        /// <summary>
        /// The height of the screen.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public double Height { get; set; }
        /// <summary>
        /// The coordinates of the center point of the screen.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public double[] Center { get; set; }
        /// <summary>
        /// The coordinates of the top left point of the screen.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public double[] TopLeft { get; set; }
        /// <summary>
        /// The coordinates of the to right point of the screen.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public double[] TopRight { get; set; }
        /// <summary>
        /// The coordinates of the bottom left point of the screen.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public double[] BottomLeft { get; set; }
        /// <summary>
        /// The coordinates of the bottom right point of the screen.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public double[] BottomRight { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigScreenArea"/> class.
        /// </summary>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigScreenArea"/> class.
        /// </summary>
        /// <param name="screenArea">A screen area object.</param>
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
        /// <summary>
        /// Allows to define the order and the delimiters between the different calibration data values.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string CalibrationLogColumnOrder { get; set; }
        /// <summary>
        /// Defines the titles of the calibration data log value columns.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public string[] CalibrationLogColumnTitle { get; set; }
        /// <summary>
        /// Defines whether gaze calibration data is written to a log file.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public bool CalibrationLogWriteOutput { get; set; }
        /// <summary>
        /// Define the calibration points to be shown during the calibration process.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public double[][] CalibrationPoints { get; set; }
        /// <summary>
        /// Define the calibration accuracy threshold in degrees.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public double CalibrationAccuracyThreshold { get; set; }
        /// <summary>
        /// The number of automatic retries if the calibration fails due to a missed CalibrationAccuracyThreshold.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public int CalibrationRetries { get; set;  }

        /// <summary>
        /// In order to detect a fixation with the I-DT algorithm a dispersion threshold is required.
        /// Provide an angle in degrees.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public double DriftCompensationDispersionThreshold { get; set; }
        /// <summary>
        /// In order to prevent drift compensation from getting out of hand limit the maximal allowed dispresion.
        /// If the drift compensation angle is larger than the here defined degrees, no compensation is applied.
        /// Provide an angle in degrees.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public double DriftCompensationDispersionThresholdMax { get; set; }
        /// <summary>
        /// Specifies the amount of time (in milliseconds) required to fixate the target during drift compensation.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public int DriftCompensationDurationThreshold { get; set; }
        /// <summary>
        /// Specifies the amount of time (in milliseconds) to wait for a fixation point during drift compensation.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public int DriftCompensationTimer { get; set; }
        /// <summary>
        /// If set to true the drift compensation window is shown on the drift compensation command.
        /// Otherwise only the drift compensation process is done without showing the window.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public bool DriftCompensationWindowShow { get; set; }

        /// <summary>
        /// Specifies the amount of time (in milliseconds) required to fixate the target during validation.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public int ValidationDurationThreshold { get; set; }
        /// <summary>
        /// Allows to define the order and the delimiters between the different validation data values.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string ValidationLogColumnOrder { get; set; }
        /// <summary>
        /// Defines the titles of the validation data log value columns.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public string[] ValidationLogColumnTitle { get; set; }
        /// <summary>
        /// Defines whether gaze validation data is written to a log file.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public bool ValidationLogWriteOutput { get; set; }
        /// <summary>
        /// Define the validation points to be shown during the validation process.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public double[][] ValidationPoints { get; set; }
        /// <summary>
        /// Specifies the amount of time (in milliseconds) to wait for a fixation point during validation.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public int ValidationTimer { get; set; }
        /// <summary>
        /// Define the validation accuracy threshold in degrees.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public double ValidationAccuracyThreshold { get; set; }
        /// <summary>
        /// Define the validation precision threshold in degrees.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public double ValidationPrecisionThreshold { get; set; }
        /// <summary>
        /// The number of automatic retries if the validation fails due to a missed ValidationAccuracyThreshold.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public int ValidationRetries { get; set; }

        /// <summary>
        /// Allows to define the order and the delimiters between the different gaze data values.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string DataLogColumnOrder { get; set; }
        /// <summary>
        /// Defines the titles of the gaze data log value columns.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public string[] DataLogColumnTitle { get; set; }
        /// <summary>
        /// Number of maximal allowed output data files in the output path. Oldest files are deleted first.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public int DataLogCount { get; set; }
        /// <summary>
        /// Allows to define the format of how the pupil diameter (in millimetres) will be logged.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public string DataLogFormatDiameter { get; set; }
        /// <summary>
        /// Allows to define the format of how the gaze origin values (in millimetres) will be logged.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public string DataLogFormatOrigin { get; set; }
        /// <summary>
        /// Allows to define the format of how normalized data points will be logged.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public string DataLogFormatNormalizedPoint { get; set; }
        /// <summary>
        /// Allows to define the format of the timestamp.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public string DataLogFormatTimeStamp { get; set; }
        /// <summary>
        /// Allows to define the format of the relative timestamp in milliseconds.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public string DataLogFormatTimeStampRelative { get; set; }
        /// <summary>
        /// Allows to define the format of the validation values.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public string DataLogFormatValidation { get; set; }
        /// <summary>
        /// Defines the location of the output file. It must be the path to a folder (not a file).
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public string DataLogPath { get; set; }
        /// <summary>
        /// Defines whether gaze data is written to a log file.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public bool DataLogWriteOutput { get; set; }
        /// <summary>
        /// Defines whether gaze data storing is disabled on Gaze application start.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public bool DataLogDisabledOnStartup { get; set; }

        /// <summary>
        /// The name of the experiment.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string? ConfigName { get; set; }
        /// <summary>
        /// Defines the location of the license files. It must be the path to a folder (not a file).
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public string? LicensePath { get; set; }
        /// <summary>
        /// Specifies the amount of time (in milliseconds) to wait for the eye tracker to become ready while it is in any other state.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public int ReadyTimer { get; set; }
        /// <summary>
        /// Choose the tracker device (1: Tobii Pro SDK, 2: Mouse Tracker).
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public int TrackerDevice { get; set; }
        /// <summary>
        /// Specifies the amount of time (in milliseconds) to wait during the loading screen.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public int LoadingTimer { get; set; }

        /// <summary>
        /// Defines the background color of the calibration and validation canvas.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public string BackgroundColor { get; set; }
        /// <summary>
        /// Defines the color of the calibration and validation frames where titles and buttons are rendered.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public string FrameColor { get; set; }
        /// <summary>
        /// Defines the text and calibration point color of the calibration and validation.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public string ForegroundColor { get; set; }

        /// <summary>
        /// Defines whether the mouse cursor shall be controlled by the gaze of the subject during the experiment.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public bool MouseControl { get; set; }
        /// <summary>
        /// Defines whether the mouse cursor shall be hidden during the experiment.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public bool MouseControlHide { get; set; }
        /// <summary>
        /// Defines whether the mouse cursor shall be hidden on the calibration window.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public bool MouseCalibrationHide { get; set; }
        /// <summary>
        /// Defines the Path to the standard mouse pointer icon.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public string MouseStandardIconPath { get; set; }
        /// <summary>
        /// Flag to enable the system tray icon.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public bool EnableSystrayIcon { get; set; }

        /// <summary>
        /// Defines the Tobii installation path. It must be the path to a folder (not a file).
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public string TobiiApplicationPath { get; set; }
        /// <summary>
        /// The Tobii application to run a calibration.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public string TobiiCalibrate { get; set; }
        /// <summary>
        /// The arguments to pass to the calibration application. Use %S as a placeholder for the device serial number
        /// and %A as a placeholder for the device address.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public string TobiiCalibrateArguments { get; set; }

        /// <summary>
        /// Hold the screen area once the config file is dumped during experimentation.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public ConfigScreenArea ScreenArea { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigItem"/> class.
        /// </summary>
        public ConfigItem()
        {
            ConfigName = "default";
            DataLogDisabledOnStartup = false;
            DataLogFormatTimeStamp = "hh\\:mm\\:ss\\.fff";
            DataLogFormatTimeStampRelative = "0.000";
            DataLogFormatDiameter = "0.000";
            DataLogFormatOrigin = "0.000";
            DataLogFormatNormalizedPoint = "0.000";
            DataLogFormatValidation = "0.00";
            DriftCompensationDispersionThreshold = 0.5;
            DriftCompensationDispersionThresholdMax = 3;
            DriftCompensationDurationThreshold = 500;
            DriftCompensationTimer = 5000;
            ValidationTimer = 3000;
            ValidationDurationThreshold = 1000;
            DriftCompensationWindowShow = true;
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
                    DataLogColumnOrder += $",{{{i}}}";
                }
            }
            DataLogColumnTitle = new string[] {
                "timestamp",
                "timestamp_received",
                "timestamp_relative",
                "trial_id",
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
                $"{{{(int)CalibrationOutputValue.Point2dX}}}," +
                $"{{{(int)CalibrationOutputValue.Point2dY}}}," +
                $"{{{(int)CalibrationOutputValue.LeftGazePoint2dX}}}," +
                $"{{{(int)CalibrationOutputValue.LeftGazePoint2dY}}}," +
                $"{{{(int)CalibrationOutputValue.LeftGazePoint2dIsValid}}}," +
                $"{{{(int)CalibrationOutputValue.LeftAccuarcy}}}," +
                $"{{{(int)CalibrationOutputValue.RightGazePoint2dX}}}," +
                $"{{{(int)CalibrationOutputValue.RightGazePoint2dY}}}," +
                $"{{{(int)CalibrationOutputValue.RightGazePoint2dIsValid}}}," +
                $"{{{(int)CalibrationOutputValue.RightAccuarcy}}}";
            CalibrationLogColumnTitle = new string[] {
                "calibrationPoint_x",
                "calibrationPoint_y",
                "left_gazePoint_x",
                "left_gazePoint_y",
                "left_gazePoint_isValid",
                "left_accuracy",
                "right_gazePoint_x",
                "right_gazePoint_y",
                "right_gazePoint_isValid",
                "right_accuracy"
            };
            CalibrationAccuracyThreshold = double.PositiveInfinity;
            CalibrationRetries = 0;
            ValidationLogColumnOrder =
                $"{{{(int)ValidationOutputValue.Point2dX}}}," +
                $"{{{(int)ValidationOutputValue.Point2dY}}}," +
                $"{{{(int)ValidationOutputValue.LeftAccuracy}}}," +
                $"{{{(int)ValidationOutputValue.LeftPrecision}}}," +
                $"{{{(int)ValidationOutputValue.LeftPrecisionRMS}}}," +
                $"{{{(int)ValidationOutputValue.RightAccuracy}}}," +
                $"{{{(int)ValidationOutputValue.RightPrecision}}}," +
                $"{{{(int)ValidationOutputValue.RightPrecisionRMS}}}";
            ValidationLogColumnTitle = new string[] {
                "validationPoint_x",
                "validationPoint_y",
                "left_accuracy",
                "left_precision",
                "left_precision_rms",
                "right_accuracy",
                "right_precision",
                "right_precision_rms"
            };
            ValidationAccuracyThreshold = double.PositiveInfinity;
            ValidationPrecisionThreshold = double.PositiveInfinity;
            ValidationRetries = 0;
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
            BackgroundColor = "Black";
            FrameColor = "#202124";
            ForegroundColor = "White";
            MouseControl = false;
            MouseControlHide = false;
            MouseCalibrationHide = false;
            MouseStandardIconPath = "C:\\Windows\\Cursors\\aero_arrow.cur";
            ReadyTimer = 5000;
            LoadingTimer = 1000;
            TrackerDevice = 1;
            ScreenArea = new ConfigScreenArea();
            EnableSystrayIcon = true;
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
                    {
                        item.DataLogColumnOrder = item_default.DataLogColumnOrder;
                    }
                    if (item?.CalibrationLogColumnOrder == "")
                    {
                        item.CalibrationLogColumnOrder = item_default.CalibrationLogColumnOrder;
                    }
                    if (item?.ValidationLogColumnOrder == "")
                    {
                        item.ValidationLogColumnOrder = item_default.ValidationLogColumnOrder;
                    }
                    if (item?.DataLogPath == "")
                    {
                        item.DataLogPath = item_default.DataLogPath;
                    }

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
