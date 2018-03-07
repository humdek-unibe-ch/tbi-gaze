using System;
using System.IO;
using Newtonsoft.Json;

/// <summary>
/// Simple parser for a json config file.
/// </summary>
namespace GazeHelper
{

    /// <summary>
    /// configuration file class
    /// </summary>
    public class ConfigItem
    {
        public bool WriteDataLog { get; set; }
        public string OutputPath { get; set; }
        public string OutputOrder { get; set; }
        public string FormatTimeStamp { get; set; }
        public string FormatDiameter { get; set; }
        public string[] ValueTitle { get; set; }
        public int OutputCount { get; set; }
        public string TobiiPath { get; set; }
        public string TobiiCalibrate { get; set; }
        public string TobiiCalibrateArguments { get; set; }
        public string TobiiGuestCalibrate { get; set; }
        public string TobiiGuestCalibrateArguments { get; set; }
        public string TobiiTest { get; set; }
        public int ReadyTimer { get; set; }
        public int TobiiSDK { get; set; }
        public int GazeFilter { get; set; }
        public bool HideMouse { get; set; }
        public bool ControlMouse { get; set; }
        public string BlankMouseIconPath { get; set; }
        public string StandardMouseIconPath { get; set; }
    }

    public enum GazeOutputValue
    {
        DataTimeStamp = 0, // timestamp of the gaze data item (uses ValueFormat.TimeStamp)
        XCoord, // x-coordinate of the gaze point of both eyes (pixel value)
        XCoordLeft, // x-coordinate of the gaze point of the left eye (pixel value) [SDK Pro only]
        XCoordRight, // x-coordinate of the gaze point of the right eye (pixel value) [SDK Pro only]
        YCoord, // y-coordinate of the gaze point of both eyes (pixel value)
        YCoordLeft, // y-coordinate of the gaze point of the left eye (pixel value) [SDK Pro only]
        YCoordRight, // y-coordinate of the gaze point of the right eye (pixel value) [SDK Pro only]
        PupilDia, // average pupil diameter of both eyes (uses ValueFormat.Diameter) [SDK Pro only]
        PupilDiaLeft, // pupil diameter of the left eye (uses ValueFormat.Diameter) [SDK Pro only]
        PupilDiaRight, // pupil diameter of the right eye (uses ValueFormat.Diameter) [SDK Pro only]
        ValidLeft, // validity of the data of the left eye [SDK Pro only]
        ValidRight // validity of the data of the right eye [SDK Pro only]
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
                //OutputOrder = "{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}\t{11}",
                OutputOrder =
                    $"{{{(int)GazeOutputValue.DataTimeStamp}}}\t" +
                    $"{{{(int)GazeOutputValue.XCoord}}}\t" + 
                    $"{{{(int)GazeOutputValue.XCoordLeft}}}\t" +
                    $"{{{(int)GazeOutputValue.XCoordRight}}}\t" +
                    $"{{{(int)GazeOutputValue.YCoord}}}\t" +
                    $"{{{(int)GazeOutputValue.YCoordLeft}}}\t" +
                    $"{{{(int)GazeOutputValue.YCoordRight}}}\t" +
                    $"{{{(int)GazeOutputValue.PupilDia}}}\t" +
                    $"{{{(int)GazeOutputValue.PupilDiaLeft}}}\t" +
                    $"{{{(int)GazeOutputValue.PupilDiaRight}}}\t" +
                    $"{{{(int)GazeOutputValue.ValidLeft}}}\t" +
                    $"{{{(int)GazeOutputValue.ValidRight}}}",
                ValueTitle = new string[] {
                    "Timestamp",
                    "x-coord",
                    "x-coord-left",
                    "x-coord-right",
                    "y-coord",
                    "y-coord-left",
                    "y-coord-right",
                    "pupil-dia",
                    "pupil-dia-left",
                    "pupil-dia-right",
                    "valid-left",
                    "valid-right"
                },
                FormatTimeStamp = "hh\\:mm\\:ss\\.fff",
                FormatDiameter = "0.000",
                OutputCount = 200,
                TobiiPath = "C:\\Program Files (x86)\\Tobii\\",
                TobiiCalibrate = "Tobii EyeX Config\\Tobii.EyeX.Configuration.exe",
                TobiiCalibrateArguments = "--calibrate",
                TobiiGuestCalibrate = "Tobii EyeX Config\\Tobii.EyeX.Configuration.exe",
                TobiiGuestCalibrateArguments = "--guest-calibration",
                TobiiTest = "Tobii EyeX Interaction\\Tobii.EyeX.Interaction.TestEyeTracking.exe",
                StandardMouseIconPath = "C:\\Windows\\Cursors\\aero_arrow.cur",
                TobiiSDK = 0,
                GazeFilter = 0,
                ReadyTimer = 5000,
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