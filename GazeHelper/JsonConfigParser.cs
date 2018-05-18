﻿using System;
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
        [JsonProperty(Required = Required.Always)]
        public string ConfigName { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string DataLogColumnOrder { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string[] DataLogColumnTitle { get; set; }
        [JsonProperty(Required = Required.Always)]
        public int DataLogCount { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string DataLogFormatDiameter { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string DataLogFormatOrigin { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string DataLogFormatTimeStamp { get; set; }
        [JsonProperty(Required = Required.Always)]
        public bool DataLogIgnoreInvalid { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string DataLogPath { get; set; }
        [JsonProperty(Required = Required.Always)]
        public bool DataLogWriteOutput { get; set; }
        [JsonProperty(Required = Required.Always)]
        public int GazeFilterCore { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string LicensePath { get; set; }
        [JsonProperty(Required = Required.Always)]
        public bool MouseControl { get; set; }
        [JsonProperty(Required = Required.Always)]
        public bool MouseHide { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string MouseStandardIconPath { get; set; }
        [JsonProperty(Required = Required.Always)]
        public int ReadyTimer { get; set; }
        [JsonProperty(Required = Required.Always)]
        public int TrackerDevice { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string TobiiApplicationPath { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string TobiiCalibrate { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string TobiiCalibrateArguments { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string TobiiGuestCalibrate { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string TobiiGuestCalibrateArguments { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string TobiiTest { get; set; }
    }


    /// <summary>
    /// enummerates output values produced by the eyetracker
    /// </summary>
    public enum GazeOutputValue
    {
        DataTimeStamp = 0, // timestamp of the gaze data item (uses ValueFormat.TimeStamp)
        XCoord, // x-coordinate of the gaze point of both eyes (pixel value)
        XCoordLeft, // x-coordinate of the gaze point of the left eye (pixel value) [SDK Pro only]
        XCoordRight, // x-coordinate of the gaze point of the right eye (pixel value) [SDK Pro only]
        YCoord, // y-coordinate of the gaze point of both eyes (pixel value)
        YCoordLeft, // y-coordinate of the gaze point of the left eye (pixel value) [SDK Pro only]
        YCoordRight, // y-coordinate of the gaze point of the right eye (pixel value) [SDK Pro only]
        ValidCoordLeft, // validity of the gaze data of the left eye [SDK Pro only]
        ValidCoordRight, // validity of the gaze data of the right eye [SDK Pro only]
        PupilDia, // average pupil diameter of both eyes (uses ValueFormat.Diameter) [SDK Pro only]
        PupilDiaLeft, // pupil diameter of the left eye (uses ValueFormat.Diameter) [SDK Pro only]
        PupilDiaRight, // pupil diameter of the right eye (uses ValueFormat.Diameter) [SDK Pro only]
        ValidPupilLeft, // validity of the pupil data of the left eye [SDK Pro only]
        ValidPupilRight, // validity of the pupil data of the right eye [SDK Pro only]
        XOriginLeft, // x-coordinate of the gaze origin of the left eye (pixel value) [SDK Pro only]
        XOriginRight, // x-coordinate of the gaze origin of the right eye (pixel value) [SDK Pro only]
        YOriginLeft, // y-coordinate of the gaze origin of the left eye (pixel value) [SDK Pro only]
        YOriginRight, // y-coordinate of the gaze origin of the right eye (pixel value) [SDK Pro only]
        ZOriginLeft, // z-coordinate of the gaze origin of the left eye (pixel value) [SDK Pro only]
        ZOriginRight, // z-coordinate of the gaze origin of the right eye (pixel value) [SDK Pro only]
        DistOrigin, // distance of the gaze origin of the average of both eyes to the eyetracker [SDK Pro only]
        DistOriginLeft, // distance of the gaze origin of the left eye to the eyetracker [SDK Pro only]
        DistOriginRight, // distance of the gaze origin of the right eye to the eyetracker [SDK Pro only]
        ValidOriginLeft, // validity of the gaze origin data of the left eye [SDK Pro only]
        ValidOriginRight // validity of the gaze origin data of the right eye [SDK Pro only]
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
            ConfigItem item = null;
            ConfigItem item_default = GetDefaultConfig();

            // load configuration
            StreamReader sr = OpenConfigFile(ConfigFile);
            if (sr != null)
            {
                try
                {
                    json = sr.ReadToEnd();
                    item = JsonConvert.DeserializeObject<ConfigItem>(json, new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Error });

                    if (item.DataLogColumnOrder == "")
                        item.DataLogColumnOrder = item_default.DataLogColumnOrder;
                    logger.Info("Successfully parsed the configuration file");
                    sr.Close();
                }
                catch (Exception e)
                {
                    logger.Error(e.Message);
                }
            }
            else
            {
                logger.Warning("No config file found, using default values");
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
            return new ConfigItem
            {
                ConfigName = "experiment_x",
                DataLogColumnOrder =
                    $"{{{(int)GazeOutputValue.DataTimeStamp}}}\t" +
                    $"{{{(int)GazeOutputValue.XCoord}}}\t" +
                    $"{{{(int)GazeOutputValue.XCoordLeft}}}\t" +
                    $"{{{(int)GazeOutputValue.XCoordRight}}}\t" +
                    $"{{{(int)GazeOutputValue.YCoord}}}\t" +
                    $"{{{(int)GazeOutputValue.YCoordLeft}}}\t" +
                    $"{{{(int)GazeOutputValue.YCoordRight}}}\t" +
                    $"{{{(int)GazeOutputValue.ValidCoordLeft}}}\t" +
                    $"{{{(int)GazeOutputValue.ValidCoordRight}}}\t" +
                    $"{{{(int)GazeOutputValue.PupilDia}}}\t" +
                    $"{{{(int)GazeOutputValue.PupilDiaLeft}}}\t" +
                    $"{{{(int)GazeOutputValue.PupilDiaRight}}}\t" +
                    $"{{{(int)GazeOutputValue.ValidPupilLeft}}}\t" +
                    $"{{{(int)GazeOutputValue.ValidPupilRight}}}\t" +
                    $"{{{(int)GazeOutputValue.XOriginLeft}}}\t" +
                    $"{{{(int)GazeOutputValue.YOriginLeft}}}\t" +
                    $"{{{(int)GazeOutputValue.ZOriginLeft}}}\t" +
                    $"{{{(int)GazeOutputValue.XOriginRight}}}\t" +
                    $"{{{(int)GazeOutputValue.YOriginRight}}}\t" +
                    $"{{{(int)GazeOutputValue.ZOriginRight}}}\t" +
                    $"{{{(int)GazeOutputValue.DistOrigin}}}\t" +
                    $"{{{(int)GazeOutputValue.DistOriginLeft}}}\t" +
                    $"{{{(int)GazeOutputValue.DistOriginRight}}}\t" +
                    $"{{{(int)GazeOutputValue.ValidOriginLeft}}}\t" +
                    $"{{{(int)GazeOutputValue.ValidOriginRight}}}",
                DataLogColumnTitle = new string[] {
                    "Timestamp",
                    "coord-x",
                    "coord-x-left",
                    "coord-x-right",
                    "coord-y",
                    "coord-y-left",
                    "coord-y-right",
                    "coord-valid-left",
                    "coord-valid-right",
                    "pupil-dia",
                    "pupil-dia-left",
                    "pupil-dia-right",
                    "pupil-valid-left",
                    "pupil-valid-right",
                    "origin-x-left",
                    "origin-y-left",
                    "origin-z-left",
                    "origin-x-right",
                    "origin-y-right",
                    "origin-z-right",
                    "origin-dist",
                    "origin-dist-left",
                    "origin-dist-right",
                    "origin-valid-left",
                    "origin-valid-right"
                },
                DataLogCount = 200,
                DataLogFormatDiameter = "0.000",
                DataLogFormatOrigin = "0.000",
                DataLogFormatTimeStamp = "hh\\:mm\\:ss\\.fff",
                DataLogIgnoreInvalid = false,
                DataLogPath = "",
                DataLogWriteOutput = true,
                GazeFilterCore = 0,
                LicensePath = "licenses",
                MouseControl = true,
                MouseHide = false,
                MouseStandardIconPath = "C:\\Windows\\Cursors\\aero_arrow.cur",
                ReadyTimer = 5000,
                TrackerDevice = 0,
                TobiiApplicationPath = "C:\\Program Files (x86)\\Tobii\\",
                TobiiCalibrate = "Tobii EyeX Config\\Tobii.EyeX.Configuration.exe",
                TobiiCalibrateArguments = "--calibrate",
                TobiiGuestCalibrate = "Tobii EyeX Config\\Tobii.EyeX.Configuration.exe",
                TobiiGuestCalibrateArguments = "--guest-calibration",
                TobiiTest = "Tobii EyeX Interaction\\Tobii.EyeX.Interaction.TestEyeTracking.exe"
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
                }
            }
            return sr;
        }
    }
}