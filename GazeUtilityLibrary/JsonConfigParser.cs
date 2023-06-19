using Newtonsoft.Json;
using System;
using System.IO;

/// <summary>
/// Simple parser for a json config file.
/// </summary>
namespace GazeUtilityLibrary
{

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
        public string DataLogPath { get; set; }
        [JsonProperty(Required = Required.Always)]
        public bool DataLogWriteOutput { get; set; }
        [JsonProperty(Required = Required.Always)]
        public bool CalibrationLogWriteOutput { get; set; }
        [JsonProperty(Required = Required.Default)]
        public double[][] CalibrationPoints { get; set; }
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

        public ConfigItem()
        {
            DataLogDisabledOnStartup = false;
            DataLogFormatTimeStamp = "hh\\:mm\\:ss\\.fff";
            DataLogFormatDiameter = "0.000";
            DataLogFormatOrigin = "0.000";
            DataLogFormatNormalizedPoint = "0.000";
            DispersionThreshold = 1;
            DriftCompensationTimer = 5000;
            TobiiApplicationPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\Programs\\TobiiProEyeTrackerManager";
            TobiiCalibrate = "TobiiProEyeTrackerManager.exe";
            TobiiCalibrateArguments = "--device-sn=%S --mode=usercalibration";
            DataLogColumnOrder =
                $"{{{(int)GazeOutputValue.DataTimeStamp}}}\t" +

                $"{{{(int)GazeOutputValue.CombinedGazePoint2dCompensatedX}}}\t" +
                $"{{{(int)GazeOutputValue.CombinedGazePoint2dCompensatedY}}}\t" +
                $"{{{(int)GazeOutputValue.CombinedGazePoint2dX}}}\t" +
                $"{{{(int)GazeOutputValue.CombinedGazePoint2dY}}}\t" +
                $"{{{(int)GazeOutputValue.CombinedGazePoint2dIsValid}}}\t" +
                $"{{{(int)GazeOutputValue.CombinedGazePoint3dCompensatedX}}}\t" +
                $"{{{(int)GazeOutputValue.CombinedGazePoint3dCompensatedY}}}\t" +
                $"{{{(int)GazeOutputValue.CombinedGazePoint3dCompensatedZ}}}\t" +
                $"{{{(int)GazeOutputValue.CombinedGazePoint3dX}}}\t" +
                $"{{{(int)GazeOutputValue.CombinedGazePoint3dY}}}\t" +
                $"{{{(int)GazeOutputValue.CombinedGazePoint3dZ}}}\t" +
                $"{{{(int)GazeOutputValue.CombinedGazePoint3dIsValid}}}\t" +
                $"{{{(int)GazeOutputValue.CombinedGazeOrigin3dX}}}\t" +
                $"{{{(int)GazeOutputValue.CombinedGazeOrigin3dY}}}\t" +
                $"{{{(int)GazeOutputValue.CombinedGazeOrigin3dZ}}}\t" +
                $"{{{(int)GazeOutputValue.CombinedGazeOrigin3dIsValid}}}\t" +
                $"{{{(int)GazeOutputValue.CombinedGazeDistance}}}\t" +
                $"{{{(int)GazeOutputValue.CombinedPupilDiameter}}}\t" +
                $"{{{(int)GazeOutputValue.CombinedPupilDiameterIsValid}}}\t" +

                $"{{{(int)GazeOutputValue.LeftGazePoint2dX}}}\t" +
                $"{{{(int)GazeOutputValue.LeftGazePoint2dY}}}\t" +
                $"{{{(int)GazeOutputValue.LeftGazePoint2dIsValid}}}\t" +
                $"{{{(int)GazeOutputValue.LeftGazePoint3dX}}}\t" +
                $"{{{(int)GazeOutputValue.LeftGazePoint3dY}}}\t" +
                $"{{{(int)GazeOutputValue.LeftGazePoint3dZ}}}\t" +
                $"{{{(int)GazeOutputValue.LeftGazePoint3dIsValid}}}\t" +
                $"{{{(int)GazeOutputValue.LeftGazeOrigin3dX}}}\t" +
                $"{{{(int)GazeOutputValue.LeftGazeOrigin3dY}}}\t" +
                $"{{{(int)GazeOutputValue.LeftGazeOrigin3dZ}}}\t" +
                $"{{{(int)GazeOutputValue.LeftGazeOrigin3dIsValid}}}\t" +
                $"{{{(int)GazeOutputValue.LeftGazeDistance}}}\t" +
                $"{{{(int)GazeOutputValue.LeftPupilDiameter}}}\t" +
                $"{{{(int)GazeOutputValue.LeftPupilDiameterIsValid}}}\t" +

                $"{{{(int)GazeOutputValue.RightGazePoint2dX}}}\t" +
                $"{{{(int)GazeOutputValue.RightGazePoint2dY}}}\t" +
                $"{{{(int)GazeOutputValue.RightGazePoint2dIsValid}}}\t" +
                $"{{{(int)GazeOutputValue.RightGazePoint3dX}}}\t" +
                $"{{{(int)GazeOutputValue.RightGazePoint3dY}}}\t" +
                $"{{{(int)GazeOutputValue.RightGazePoint3dZ}}}\t" +
                $"{{{(int)GazeOutputValue.RightGazePoint3dIsValid}}}\t" +
                $"{{{(int)GazeOutputValue.RightGazeOrigin3dX}}}\t" +
                $"{{{(int)GazeOutputValue.RightGazeOrigin3dY}}}\t" +
                $"{{{(int)GazeOutputValue.RightGazeOrigin3dZ}}}\t" +
                $"{{{(int)GazeOutputValue.RightGazeOrigin3dIsValid}}}\t" +
                $"{{{(int)GazeOutputValue.RightGazeDistance}}}\t" +
                $"{{{(int)GazeOutputValue.RightPupilDiameter}}}\t" +
                $"{{{(int)GazeOutputValue.RightPupilDiameterIsValid}}}";
            DataLogColumnTitle = new string[] {
                "timestamp",

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
                "right_pupilDiameter_isValid",
            };
            CalibrationLogColumnOrder =
                $"{{{(int)CalibrationOutputValue.XCoord}}}\t" +
                $"{{{(int)CalibrationOutputValue.YCoord}}}\t" +
                $"{{{(int)CalibrationOutputValue.XCoordLeft}}}\t" +
                $"{{{(int)CalibrationOutputValue.YCoordLeft}}}\t" +
                $"{{{(int)CalibrationOutputValue.ValidCoordLeft}}}\t" +
                $"{{{(int)CalibrationOutputValue.XCoordRight}}}\t" +
                $"{{{(int)CalibrationOutputValue.YCoordRight}}}\t" +
                $"{{{(int)CalibrationOutputValue.ValidCoordRight}}}";
            CalibrationLogColumnTitle = new string[] {
                "coord-x",
                "coord-y",
                "coord-x-left",
                "coord-y-left",
                "coord-valid-left",
                "coord-x-right",
                "coord-y-right",
                "coord-valid-right"
            };
            DataLogCount = 200;
            DataLogPath = Directory.GetCurrentDirectory();
            DataLogWriteOutput = true;
            CalibrationLogWriteOutput = true;
            CalibrationPoints = new double[8][];
            CalibrationPoints[0] = new double[2] { 0.7, 0.5 };
            CalibrationPoints[1] = new double[2] { 0.3, 0.5 };
            CalibrationPoints[2] = new double[2] { 0.9, 0.9 };
            CalibrationPoints[3] = new double[2] { 0.1, 0.9 };
            CalibrationPoints[4] = new double[2] { 0.5, 0.1 };
            CalibrationPoints[5] = new double[2] { 0.1, 0.1 };
            CalibrationPoints[6] = new double[2] { 0.9, 0.1 };
            CalibrationPoints[7] = new double[2] { 0.5, 0.9 };
            MouseControl = false;
            MouseControlHide = false;
            MouseCalibrationHide = false;
            MouseStandardIconPath = "C:\\Windows\\Cursors\\aero_arrow.cur";
            ReadyTimer = 5000;
            TrackerDevice = 0;
        }
    }

    /// <summary>
    /// enummerates output values produced by the eyetracker
    /// </summary>
    public enum GazeOutputValue
    {
        DataTimeStamp = 0, // timestamp of the gaze data item (uses ValueFormat.TimeStamp)

        CombinedGazePoint2dCompensatedX,
        CombinedGazePoint2dCompensatedY,
        CombinedGazePoint2dX,
        CombinedGazePoint2dY,
        CombinedGazePoint2dIsValid,
        CombinedGazePoint3dCompensatedX,
        CombinedGazePoint3dCompensatedY,
        CombinedGazePoint3dCompensatedZ,
        CombinedGazePoint3dX,
        CombinedGazePoint3dY,
        CombinedGazePoint3dZ,
        CombinedGazePoint3dIsValid,
        CombinedGazeOrigin3dX,
        CombinedGazeOrigin3dY,
        CombinedGazeOrigin3dZ,
        CombinedGazeOrigin3dIsValid,
        CombinedGazeDistance,
        CombinedPupilDiameter,
        CombinedPupilDiameterIsValid,

        LeftGazePoint2dX,
        LeftGazePoint2dY,
        LeftGazePoint2dIsValid,
        LeftGazePoint3dX,
        LeftGazePoint3dY,
        LeftGazePoint3dZ,
        LeftGazePoint3dIsValid,
        LeftGazeOrigin3dX,
        LeftGazeOrigin3dY,
        LeftGazeOrigin3dZ,
        LeftGazeOrigin3dIsValid,
        LeftGazeDistance,
        LeftPupilDiameter,
        LeftPupilDiameterIsValid,

        RightGazePoint2dX,
        RightGazePoint2dY,
        RightGazePoint2dIsValid,
        RightGazePoint3dX,
        RightGazePoint3dY,
        RightGazePoint3dZ,
        RightGazePoint3dIsValid,
        RightGazeOrigin3dX,
        RightGazeOrigin3dY,
        RightGazeOrigin3dZ,
        RightGazeOrigin3dIsValid,
        RightGazeDistance,
        RightPupilDiameter,
        RightPupilDiameterIsValid
    }

    /// <summary>
    /// enummerates output values produced by the eyetracker
    /// </summary>
    public enum CalibrationOutputValue
    {
        XCoord, // x-coordinate of the calibration point (normalised value)
        YCoord, // y-coordinate of the gaze calibration (normalised value)
        XCoordLeft, // x-coordinate of the gaze point of the left eye (normalised value)
        YCoordLeft, // y-coordinate of the gaze point of the left eye (normalised value)
        ValidCoordLeft, // validity of the gaze data of the left eye
        XCoordRight, // x-coordinate of the gaze point of the right eye (normalised value)
        YCoordRight, // y-coordinate of the gaze point of the right eye (normalised value)
        ValidCoordRight // validity of the gaze data of the right eye
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
                    item = JsonConvert.DeserializeObject<ConfigItem>(json, new JsonSerializerSettings {
                        MissingMemberHandling = MissingMemberHandling.Error
                    });

                    if (item?.DataLogColumnOrder == "")
                        item.DataLogColumnOrder = item_default.DataLogColumnOrder;
                    if (item?.CalibrationLogColumnOrder == "")
                        item.CalibrationLogColumnOrder = item_default.CalibrationLogColumnOrder;
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
        public void SerializeJsonConfig( ConfigItem item, string path )
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