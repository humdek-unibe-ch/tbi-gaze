using System;
using System.IO;
using Newtonsoft.Json;

/// <summary>
/// Simple parser for a json config file.
/// </summary>
namespace GazeHelper
{
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
        /// configuration file class
        /// </summary>
        public class ConfigItem
        {
            public bool WriteDataLog { get; set; }
            public string OutputPath { get; set; }
            public string OutputFormat { get; set; }
            public int OutputCount { get; set; }
            public string TobiiPath { get; set; }
            public string TobiiCalibrate { get; set; }
            public string TobiiCalibrateArguments { get; set; }
            public string TobiiGuestCalibrate { get; set; }
            public string TobiiGuestCalibrateArguments { get; set; }
            public string TobiiTest { get; set; }
            public int GazeFilter { get; set; }
            public bool HideMouse { get; set; }
            public bool ControlMouse { get; set; }
            public string BlankMouseIconPath { get; set; }
            public string StandardMouseIconPath { get; set; }

        }

        /// <summary>
        /// Parses the json configuration.
        /// </summary>
        /// <returns>the updated ConfigItem class.</returns>
        public ConfigItem ParseJsonConfig()
        {
            string json;
            ConfigItem item = GetDefaultConfig();

            // load configuration
            StreamReader sr = OpenConfigFile(ConfigFile);
            if (sr != null)
            {
                try
                {
                    json = sr.ReadToEnd();
                    item = JsonConvert.DeserializeObject<ConfigItem>(json);
                    logger.Info("Successfully parsed the configuration file");
                    sr.Close();
                }
                catch (JsonReaderException e)
                {
                    logger.Error(e.Message);
                    logger.Warning("Config file could not be parsed, using default configuration values");
                }
            }

            return item;
        }

        /// <summary>
        /// Gets the default configuration values.
        /// </summary>
        /// <returns>the default configuration values.</returns>
        public ConfigItem GetDefaultConfig()
        {
            return new ConfigItem
            {
                WriteDataLog = true,
                OutputPath = "",
                OutputFormat = "{0:hh\\:mm\\:ss\\.fff}\t{1:0.0}\t{2:0.0}",
                OutputCount = 200,
                TobiiPath = "C:\\Program Files (x86)\\Tobii\\",
                TobiiCalibrate = "Tobii EyeX Config\\Tobii.EyeX.Configuration.exe",
                TobiiCalibrateArguments = "--calibrate",
                TobiiGuestCalibrate = "Tobii EyeX Config\\Tobii.EyeX.Configuration.exe",
                TobiiGuestCalibrateArguments = "--guest-calibration",
                TobiiTest = "Tobii EyeX Interaction\\Tobii.EyeX.Interaction.TestEyeTracking.exe",
                StandardMouseIconPath = "C:\\Windows\\Cursors\\aero_arrow.cur",
                GazeFilter = 0,
                HideMouse = false,
                ControlMouse = true
            };
        }

        /// <summary>
        /// Opens whichever configuration file is availbale.
        /// First, the caller path is searched.
        /// Second, the execution path is searched.
        /// Third, default values are used.
        /// </summary>
        /// <param name="configFile">The configuration file.</param>
        /// <returns></returns>
        private StreamReader OpenConfigFile(string configFile)
        {
            StreamReader sr = null;
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
                    logger.Warning("No config file found, using default configuration values");
                }
            }
            return sr;
        }
    }
}