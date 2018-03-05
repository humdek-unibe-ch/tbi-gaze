using System;
using System.Diagnostics;
using System.Windows;
using GazeHelper;

/// <summary>
/// Simple wrapper to run Tobii eye tracker test
/// </summary>
namespace TobiiTest
{
    static class TobiiTest
    {
        static TrackerLogger logger;
        private static bool IsTested = false;

        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application app = new Application
            {
                ShutdownMode = ShutdownMode.OnExplicitShutdown
            };
            app.Exit += new ExitEventHandler(OnApplicationExit);
            logger = new TrackerLogger();
            EyeTracker tracker = new EyeTracker(logger);
            tracker.TrackerEnabled += OnTrackerReady;
            logger.Info($"Starting \"{AppDomain.CurrentDomain.BaseDirectory}TobiiTest.exe\"");
            logger.Info("Preparing to start Tobii eyetracker test application");
            app.Run();
        }

        /// <summary>
        /// is executed once the tracker is ready
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        static void OnTrackerReady(object sender, EventArgs e)
        {
            JsonConfigParser parser = new JsonConfigParser(logger);
            JsonConfigParser.ConfigItem item = parser.ParseJsonConfig();
            logger.Info($"Starting Tobii eyetracker test \"{item.TobiiPath}\\{item.TobiiTest}\"");
            Process tobii_test = Process.Start($"{item.TobiiPath}\\{item.TobiiTest}");
            tobii_test.WaitForExit();
            IsTested = true;
            logger.Info($"\"{item.TobiiPath}\\{item.TobiiTest}\" terminated ");
            Application.Current.Dispatcher.Invoke(callback: () => { Application.Current.Shutdown(); });
        }

        /// <summary>
        /// Called when [application exit].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        static void OnApplicationExit(object sender, EventArgs e)
        {
            if(!IsTested) logger.Error("Unable to start Tobii eyetracker test application");
            logger.Info($"\"{AppDomain.CurrentDomain.BaseDirectory}TobiiTest.exe\" terminated gracefully{Environment.NewLine}");
        }
    }
}