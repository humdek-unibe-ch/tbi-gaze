/**
 * Simple wrapper to run Tobii calibration
 * 
 * @file    TobiiCalibrate.cs
 * @author  Simon Maurer, simon.maurer@humdek.unibe.ch
 * @date    January 2018
 */

using System.Diagnostics;
using GazeHelper;

namespace TobiiCalibrate
{
    /**
     * @brief Main entry point to the program TobiiCalibrate
     */
    static class TobiiCalibrate
    {
        static void Main()
        {
            Logger logger = new Logger();
            JsonConfigParser parser = new JsonConfigParser();
            JsonConfigParser.ConfigItem item = parser.ParseJsonConfig();
            logger.Info($"Starting Tobii calibration \"{item.TobiiPath}\\{item.TobiiCalibrate} {item.TobiiCalibrateArguments}\"");
            Process.Start($"{item.TobiiPath}\\{item.TobiiCalibrate}", item.TobiiCalibrateArguments);
        }
    }
}