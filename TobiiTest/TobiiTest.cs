/**
 * Simple wrapper to run Tobii eye tracker test
 * 
 * @file    TobiiTest.cs
 * @author  Simon Maurer, simon.maurer@humdek.unibe.ch
 * @date    January 2018
 */

using System;
using System.Threading;
using System.Diagnostics;
using GazeHelper;

namespace TobiiTest
{
    /**
     * @brief Main entry point to the program TobiiTest
     */
    static class TobiiTest
    {
        static Logger logger;
        static ManualResetEvent resetEvent = new ManualResetEvent(false);
        static void Main()
        {
            logger = new Logger();
            logger.Info("Preparing to start Tobii eyetracker test");
            EyeTracker tracker = new EyeTracker(logger);
            tracker.RaiseTrackerReady += HandleTrackerReady;
            resetEvent.WaitOne(); // Blocks until "set"
        }

        static void HandleTrackerReady(object sender, EventArgs e)
        {
            JsonConfigParser parser = new JsonConfigParser(logger);
            JsonConfigParser.ConfigItem item = parser.ParseJsonConfig();
            logger.Info($"Starting Tobii eyetracker test \"{item.TobiiPath}\\{item.TobiiTest}\"");
            Process.Start($"{item.TobiiPath}\\{item.TobiiTest}");
            resetEvent.Set(); // Allow the program to exit
        }
    }
}