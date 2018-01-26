﻿/**
 * Converts the stream of gaze points, captured by a Tobii eyetracker, to mouse cursor location
 * 
 * @file    Program.cs
 * @author  Simon Maurer, simon.maurer@humdek.unibe.ch
 * @date    January 2018
 */

using System;
using System.IO;
using System.Windows.Forms;
using Tobii.Interaction;
using Tobii.Interaction.Framework;
using GazeHelper;

namespace GazeToMouse
{
    class Program
    {
        private const string COL_DELIM = "\t";
        private static FileStream fs;
        private static StreamWriter sw;
        private static TimeSpan ts_start;
        private static TimeSpan ts_delta;
        private static bool hasRun = false;
        private static Host host;
        private static Logger logger;
        private static MouseHider hider;
        private static JsonConfigParser.ConfigItem config;
        /**
         * @brief Helper class to be added to the application's message pump to filter out a message
         */
        private class TestMessageFilter : IMessageFilter
        {
            /**
             * @brief filters out a message before it is dispatched
             * 
             * The current implementation only cares about the signal that is sent to the application by taskkill (WM_CLOSE).
             * Once WM_CLOSE is received, the event "ApplicationExit" is invoked.
             * 
             * @param m the pre-filtered message
             * @return  true if the received message corresponds to WM_CLOSE
             *          false if any other message is pre-filtered
             */
            public bool PreFilterMessage(ref Message m)
            {
                if (m.Msg == /*WM_CLOSE*/ 0x10)
                {
                    Application.Exit();
                    return true;
                }
                return false;
            }
        }

        /**
         * @brief The main programm entry
         */
        static void Main(string[] args)
        {
            // load configuration
            JsonConfigParser parser = new JsonConfigParser();
            config = parser.ParseJsonConfig();
            logger = new Logger();
            logger.Info(string.Format("Starting \"{0}GazeToMouse.exe\"", System.AppDomain.CurrentDomain.BaseDirectory));

            // open files
            fs = File.Open(config.OutputFile, FileMode.Create);
            sw = new StreamWriter(fs);
            ts_start = DateTime.Now.TimeOfDay;
            sw.WriteLine("Timestamp{0}X{0}Y", COL_DELIM);
            logger.Info(string.Format("Writing gaze data to \"{0}\"", fs.Name));

            Application.ApplicationExit += new EventHandler(OnApplicationExit);

            // hide the mouse cursor
            hider = new MouseHider();
            if (config.HideMouse) hider.HideCursor(config.BlankMouseIconPath);

            // initialize host. Make sure that the Tobii service is running
            host = new Host();

            // create stream
            GazePointDataMode filter;
            switch (config.GazeFilter)
            {
                case 0: filter = GazePointDataMode.Unfiltered; break;
                case 1: filter = GazePointDataMode.LightlyFiltered; break;
                default:
                    filter = GazePointDataMode.Unfiltered;
                    logger.Warning("Unkonwn filter setting, using unfiltered mode");
                    break;
            }
            var gazePointDataStream = host.Streams.CreateGazePointDataStream(filter);
            // whenever a new gaze point is available, run gaze2mouse
            gazePointDataStream.GazePoint((x, y, ts) => gaze2mouse(x, y, ts));

            // add message filter to the application's message pump
            Application.AddMessageFilter(new TestMessageFilter());
            Application.Run();
        }

        /**
         * @brief set the mouse pointer to the location of the gaze point and log the coordiantes
         * 
         * @param x     the x-coordinate of the gaze point
         * @param y     the y-coordinate of the gaye point
         * @param ts    the timestamp of the the capture instant of the gaye point
         *              Note that the timestamp reference represents an arbitrary point in time
         */
        static void gaze2mouse(double x, double y, double ts)
        {
            // create a time reference that corresponds to the local machine
            TimeSpan ts_rec = TimeSpan.FromMilliseconds(ts);
            if (!hasRun) ts_delta = ts_rec - ts_start;
            ts_rec -= ts_delta;

            // set the cursor position to the gaze position
            Cursor.Position = new System.Drawing.Point(Convert.ToInt32(x), Convert.ToInt32(y));

            // write the coordinates to the log file
            sw.WriteLine("{0:hh\\:mm\\:ss\\.fff}{3}{1:0.0}{3}{2:0.0}", ts_rec, x, y, COL_DELIM);
            hasRun = true;
        }

        /**
         * @brief cleanup. To be run on application exit. 
         */
        static void OnApplicationExit(object sender, EventArgs e)
        {
            //sw.WriteLine("done");
            if (config.HideMouse) hider.ShowCursor(config.StandardMouseIconPath);
            sw.Close();
            fs.Close();
            sw.Dispose();
            fs.Dispose();
            host.DisableConnection();
            logger.Info(string.Format("\"{0}GazeToMouse.exe\" terminated gracefully", System.AppDomain.CurrentDomain.BaseDirectory));
        }
    }
}
