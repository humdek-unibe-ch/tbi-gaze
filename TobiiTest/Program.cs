/**
 * Simple wrapper to run Tobii eye tracker test
 * 
 * @file    Program.cs
 * @author  Simon Maurer, simon.maurer@humdek.unibe.ch
 * @date    January 2018
 */

using System.Diagnostics;
using GazeHelper;

namespace TobiiTest
{
    static class Program
    {
        static void Main()
        {
            Logger logger = new Logger();
            JsonConfigParser parser = new JsonConfigParser();
            JsonConfigParser.ConfigItem item = parser.ParseJsonConfig();
            logger.Info(string.Format("Starting Tobii eyetracker test \"{0}{1}\"", item.TobiiPath, item.TobiiTest));
            Process.Start(string.Format("{0}{1}", item.TobiiPath, item.TobiiTest));
        }
    }
}