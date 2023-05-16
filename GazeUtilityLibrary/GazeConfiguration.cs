using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace GazeUtilityLibrary
{
    public class GazeConfiguration
    {
        private ConfigItem? _config;
        private ConfigItem _default_config;
        public ConfigItem Config { get { return _config ?? _default_config; } }
        private TrackerLogger _logger;
        private GazeConfigError _error = new GazeConfigError();
        private string _starttime;
        private StreamWriter? _sw = null;
        private JsonConfigParser _parser;
        private string? _outputFilePath;
        public GazeConfiguration(TrackerLogger logger)
        {
            _logger = logger;
            _starttime = $"{DateTime.Now:yyyyMMddTHHmmss}";
            _parser = new JsonConfigParser(logger);
            _default_config = _parser.GetDefaultConfig();
        }

        public bool InitConfig()
        {
            _config = _parser.ParseJsonConfig();
            if (_config == null)
            {
                return false;
            }
            _outputFilePath = $"{_config!.DataLogPath}\\out";

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
        private void InitGazeOutputFile()
        {
            try
            {
                _sw = new StreamWriter($"{_outputFilePath}");
                FileInfo fi = new FileInfo($"{_outputFilePath}");
                _outputFilePath = fi.FullName;
                _logger.Info($"Writing gaze data to \"{_outputFilePath}\"");
            }
            catch (Exception e)
            {
                _logger.Error($"Failed to open file {_outputFilePath}: {e.Message}");
            }
        }

        private void ReportUninitalisedConfig()
        {
            _logger.Error($"Configuration not initialized: Call InitConfig on the GazeConfiguration instance");
        }

        public bool CleanupOutputFile(string error)
        {
            if (_config == null)
            {
                return false;
            }
            if (!_config.DataLogWriteOutput)
            {
                return true;
            }
            _sw?.Close();
            _sw?.Dispose();
            _sw = null;
            File.Move(_outputFilePath!, $"{_outputFilePath}{error}.txt");
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

        public bool PrepareOutputFile()
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

            string gazeFilePostfix = $"{Environment.MachineName}_{_config.ConfigName}_gaze";
            string gazeFileName = $"{_starttime}_{gazeFilePostfix}";

            // create gaze data file
            if (_config.DataLogPath == "")
            {
                _config.DataLogPath = Directory.GetCurrentDirectory();
            }

            _outputFilePath = $"{_config.DataLogPath}\\{gazeFileName}";
            InitGazeOutputFile();
            if (_sw == null)
            {
                // something went wrong, write to the current directory
                _config.DataLogPath = Directory.GetCurrentDirectory();
                _outputFilePath = $"{_config.DataLogPath}\\{gazeFileName}";
                _logger.Warning($"Writing gaze data to the current directory: \"{_outputFilePath}\"");
                _error.Error = EGazeConfigError.FallbackToCurrentOutputDir;
                _sw = new StreamWriter(gazeFileName);
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
            if (!ConfigChecker.CheckDataLogColumnOrder(_config.DataLogColumnOrder, _logger))
            {
                _config.DataLogColumnOrder = _default_config.DataLogColumnOrder;
                _logger.Warning($"Using the default column order: \"{_config.DataLogColumnOrder}\"");
                _error.Error = EGazeConfigError.FallbackToDefualtColumnOrder;
            }
            if (!ConfigChecker.CheckDataLogColumnTitles(_config.DataLogColumnOrder, _config.DataLogColumnTitle, _logger))
            {
                _logger.Warning($"Column titles are omitted");
                _error.Error = EGazeConfigError.OmitColumnTitles;
            }

            // delete old files
            DeleteOldGazeLogFiles(_config.DataLogPath, _config.DataLogCount, $"*_{gazeFilePostfix}");
            return true;
        }

        public void WriteToOutput(string[] formatted_values)
        {
            if (_config == null)
            {
                return;
            }
            _sw?.WriteLine(String.Format(_config.DataLogColumnOrder, formatted_values));
        }
    }
}
