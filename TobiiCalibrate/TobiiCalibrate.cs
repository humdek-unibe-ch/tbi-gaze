/**
 * Simple wrapper to run Tobii calibration
 * 
 * @file    TobiiCalibrate.cs
 * @author  Simon Maurer, simon.maurer@humdek.unibe.ch
 * @date    January 2018
 */

using System;
using System.Threading;
using System.Diagnostics;
using GazeHelper;

namespace TobiiCalibrate
{
    /**
     * @brief Main entry point to the program TobiiCalibrate
     */
    static class TobiiCalibrate
    {
        static Logger logger;
        static ManualResetEvent resetEvent = new ManualResetEvent(false);
        static void Main()
        {
            logger = new Logger();
            logger.Info("Preparing to start Tobii calibration");
            EyeTracker tracker = new EyeTracker(logger);
            tracker.RaiseTrackerReady += HandleTrackerReady;
            resetEvent.WaitOne(); // Blocks until "set"
        }
        
        static void HandleTrackerReady(object sender, EventArgs e)
        {
            JsonConfigParser parser = new JsonConfigParser(logger);
            JsonConfigParser.ConfigItem item = parser.ParseJsonConfig();
            logger.Info($"Starting Tobii calibration \"{item.TobiiPath}\\{item.TobiiCalibrate} {item.TobiiCalibrateArguments}\"");
            Process.Start($"{item.TobiiPath}\\{item.TobiiCalibrate}", item.TobiiCalibrateArguments);
            resetEvent.Set(); // Allow the program to exit
        }
    }
}