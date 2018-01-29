﻿/**
 * Simple wrapper to run Tobii guest calibration
 * 
 * @file    Program.cs
 * @author  Simon Maurer, simon.maurer@humdek.unibe.ch
 * @date    January 2018
 */

using System.Diagnostics;
using GazeHelper;

namespace TobiiGuestCalibrate
{
    static class Program
    {
        static void Main()
        {
            Logger logger = new Logger();
            JsonConfigParser parser = new JsonConfigParser();
            JsonConfigParser.ConfigItem item = parser.ParseJsonConfig();
            logger.Info($"Starting Tobii guest calibration \"{item.TobiiPath}\\{item.TobiiGuestCalibrate} {item.TobiiGuestCalibrateArguments}\"");
            Process.Start($"{item.TobiiPath}\\{item.TobiiGuestCalibrate}", item.TobiiGuestCalibrateArguments);
        }
    }
}