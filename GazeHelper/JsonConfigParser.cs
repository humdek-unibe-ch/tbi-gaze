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
            public string TobiiGuestCalibrate { get; set; }
            public string TobiiTest { get; set; }
            public int GazeFilter { get; set; }
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
                TobiiCalibrate = "Tobii EyeX Config\\Tobii.EyeX.Configuration.exe -C",
                TobiiGuestCalibrate = "Tobii EyeX Config\\Tobii.EyeX.Configuration.exe -G",
                TobiiTest = "Tobii EyeX Interaction\\Tobii.EyeX.Interaction.TestEyeTracking.exe",
                GazeFilter = 0
            };
            Logger logger = new Logger();
            // load configuration
            try
            {
                sr = new StreamReader(ConfigFile);
                json = sr.ReadToEnd();
                sr.Dispose();
                item = JsonConvert.DeserializeObject<ConfigItem>(json);
            }
            catch (FileNotFoundException e)
            {
                logger.Warning(e.Message);
                logger.Info("using default configuration values");
            }
            catch (JsonReaderException e)
            {
                logger.Warning(e.Message);
                logger.Info("using default configuration values");
            }
            return item;
        }
    }
}
