/**
 * @brief Simple parser for a json config file
 * 
 * The config file "config.json" is parsed and its values are attributed to the ConfigItem class
 * 
 * @author  Simon Maurer, simon.maurer@humdek.unibe.ch
 * @file    JsonConfigParser.cs
 * @date    Jamuary 2018
 */

using System.IO;
using Newtonsoft.Json;

namespace GazeHelper
{
    public class JsonConfigParser
    {
        private string ConfigFile = "config.json";
        
        /**
         * @brief configuration file class
         */
        public class ConfigItem
        {
            public string OutputFile { get; set; }
            public string TobiiPath { get; set; }
            public string TobiiCalibrate { get; set; }
            public string TobiiCalibrateArguments { get; set; }
            public string TobiiGuestCalibrate { get; set; }
            public string TobiiGuestCalibrateArguments { get; set; }
            public string TobiiTest { get; set; }
            public int GazeFilter { get; set; }
            public bool HideMouse { get; set; }
            public string BlankMouseIconPath { get; set; }
            public string StandardMouseIconPath { get; set; }

        }

        /**
         * @brief parses the config file
         * 
         * @return the updated ConfigItem class
         */
        public ConfigItem ParseJsonConfig()
        {
            StreamReader sr;
            string json;
            ConfigItem item = new ConfigItem
            {
                OutputFile = "gaze.data",
                TobiiPath = "C:\\Program Files (x86)\\Tobii\\",
                TobiiCalibrate = "Tobii EyeX Config\\Tobii.EyeX.Configuration.exe",
                TobiiCalibrateArguments = "--calibrate",
                TobiiGuestCalibrate = "Tobii EyeX Config\\Tobii.EyeX.Configuration.exe",
                TobiiGuestCalibrateArguments = "--guest-calibration",
                TobiiTest = "Tobii EyeX Interaction\\Tobii.EyeX.Interaction.TestEyeTracking.exe",
                BlankMouseIconPath = "blank.cur",
                StandardMouseIconPath = "C:\\Windows\\Cursors\\aero_arrow.cur",
                GazeFilter = 0,
                HideMouse = false
            };
            Logger logger = new Logger();
            // load configuration
            try
            {
                sr = new StreamReader(ConfigFile);
                json = sr.ReadToEnd();
                item = JsonConvert.DeserializeObject<ConfigItem>(json);
                logger.Info(string.Format("Successfully read the configuration file \"{0}{1}\"", System.AppDomain.CurrentDomain.BaseDirectory, ConfigFile));
                sr.Close();
                sr.Dispose();
            }
            catch (FileNotFoundException e)
            {
                logger.Warning(e.Message);
                logger.Info("Using default configuration values");
            }
            catch (JsonReaderException e)
            {
                logger.Warning(e.Message);
                logger.Info("Using default configuration values");
            }
            return item;
        }
    }
}