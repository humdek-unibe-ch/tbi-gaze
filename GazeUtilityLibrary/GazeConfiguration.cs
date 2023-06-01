using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace GazeUtilityLibrary
{
    public enum EOutputType
    {
        gaze,
        calibration
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
            _config = _parser.ParseJsonConfig();
            if (_config == null)
            {
                return false;
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
            if (_config == null)
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
        /// <returns>True on success, False on failure.</returns>
        public bool PrepareGazeOutputFile()
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

            string filePostfix = $"{Environment.MachineName}_{_config.ConfigName}_{EOutputType.gaze.ToString()}";
            string fileName = $"{_starttime}_{filePostfix}";

            // create gaze data file
            if (_config.DataLogPath == "")
            {
                _config.DataLogPath = Directory.GetCurrentDirectory();
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
        /// <returns>True on success, False on failure.</returns>
        public bool PrepareCalibrationOutputFile()
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

            string filePostfix = $"{Environment.MachineName}_{_config.ConfigName}_{EOutputType.calibration.ToString()}";
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
                _error.Error = EGazeConfigError.FallbackToDefaultOriginFormat;
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
    }
}
