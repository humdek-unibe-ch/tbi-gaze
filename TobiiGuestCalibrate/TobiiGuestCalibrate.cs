/**
 * Simple wrapper to run Tobii guest calibration
 * 
 * @file    TobiiGuestCalibrate.cs
 * @author  Simon Maurer, simon.maurer@humdek.unibe.ch
 * @date    January 2018
 */

using System;
using System.Threading;
using System.Diagnostics;
using GazeHelper;

namespace TobiiGuestCalibrate
{
    /**
     * @brief Main entry point to the program TobiiGuestCalibrate
     */
    static class TobiiGuestCalibrate
    {
        static Logger logger;
        static ManualResetEvent resetEvent = new ManualResetEvent(false);
        static void Main()
        {
            logger = new Logger();
            logger.Info("Preparing to start Tobii guest calibration");
            EyeTracker tracker = new EyeTracker(logger);
            tracker.RaiseTrackerReady += HandleTrackerReady;
            resetEvent.WaitOne(); // Blocks until "set"
        }

        static void HandleTrackerReady(object sender, EventArgs e)
        {
            JsonConfigParser parser = new JsonConfigParser(logger);
            JsonConfigParser.ConfigItem item = parser.ParseJsonConfig();
            logger.Info($"Starting Tobii guest calibration \"{item.TobiiPath}\\{item.TobiiGuestCalibrate} {item.TobiiGuestCalibrateArguments}\"");
            Process.Start($"{item.TobiiPath}\\{item.TobiiGuestCalibrate}", item.TobiiGuestCalibrateArguments);
            resetEvent.Set(); // Allow the program to exit
        }
    }
}