/**
 * @brief Simple parser for a json config file
 * 
 * The config file "config.json" is parsed and its values are attributed to the ConfigItem class
 * 
 * @author  Simon Maurer, simon.maurer@humdek.unibe.ch
 * @file    JsonConfigParser.cs
 * @date    Jamuary 2018
 */

using System;
using System.IO;
using Newtonsoft.Json;

namespace GazeHelper
{
    public class JsonConfigParser
    {
        private string ConfigFile = "config.json";
        private static Logger logger;
        
        /**
         * @brief configuration file class
         */
        public class ConfigItem
        {
            public string OutputPath { get; set; }
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
            
            string json;
            ConfigItem item = new ConfigItem
            {
                OutputPath = "",
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
            logger = new Logger();
            // load configuration
            StreamReader sr = OpenConfigFile( ConfigFile );
            if( sr != null )
            {
                try
                {
                    json = sr.ReadToEnd();
                    item = JsonConvert.DeserializeObject<ConfigItem>(json);
                    logger.Info("Successfully read the configuration file");
                    sr.Close();
                    sr.Dispose();
                }
                catch (JsonReaderException e)
                {
                    logger.Error(e.Message);
                    logger.Warning("Using default configuration values");
                }
            }

            return item;
        }

        /**
         * @brief open whichever configfile is available
         * 
         * First, the caller path is searched.
         * Second, the execution path is searched.
         * Third, default values are used.
         * 
         * @param configFile the name of the configuarion file
         */
        private StreamReader OpenConfigFile( string configFile )
        {
            StreamReader sr = null;
            try
            {
                sr = new StreamReader(configFile);
                logger.Info($"Found config file \"{Directory.GetCurrentDirectory()}\\{configFile}\"");
            }
            catch (FileNotFoundException e)
            {
                string path = AppDomain.CurrentDomain.BaseDirectory;
                logger.Info(e.Message);
                try
                {
                    sr = new StreamReader($"{path}\\{configFile}");
                    logger.Info($"Found config file \"{path}{configFile}\"");
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