/**
 * @brief Simple logger class
 * 
 * @author  Simon Maurer, simon.maurer@humdek.unibe.ch
 * @file    Logger.cs
 * @date    Jamuary 2018
 */

using System;
using System.IO;

namespace GazeHelper
{
    public class Logger
    {
        private string logFile = "gaze.log";
        private string dateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff";
        private enum LogLevel
        {
            Debug,
            Info,
            Warning,
            Error
        }

        /**
         * @brief Write log messages to log file
         * 
         * @param level     log level
         * @param message   the message to be logged
         */
        private void WriteLog(LogLevel level, string message)
        {
            string prefix = DateTime.Now.ToString(dateTimeFormat);
            switch (level)
            {
                case LogLevel.Debug: prefix += " [DEBUG]   "; break;
                case LogLevel.Info: prefix += " [INFO]    "; break;
                case LogLevel.Warning: prefix += " [WARNING] "; break;
                case LogLevel.Error: prefix += " [ERROR]   "; break;
            }

            try
            {
                FileStream fs = File.Open(logFile, FileMode.Append);
                StreamWriter sw = new StreamWriter(fs);
                sw.WriteLine(prefix + message);
                sw.Dispose();
                fs.Close();
                fs.Dispose();
            }
            catch
            {
                throw;
            }
        }

        /**
         * @brief wrapper function for debug level logging
         * 
         * @param message to be logged
         */
        public void Debug(string message) { WriteLog(LogLevel.Debug, message); }
        /**
         * @brief wrapper function for info level logging
         * 
         * @param message to be logged
         */
        public void Info(string message) { WriteLog(LogLevel.Info, message); }
        /**
         * @brief wrapper function for warning level logging
         * 
         * @param message to be logged
         */
        public void Warning(string message) { WriteLog(LogLevel.Warning, message); }
        /**
         * @brief wrapper function for error level logging
         * 
         * @param message to be logged
         */
        public void Error(string message) { WriteLog(LogLevel.Error, message); }
    }
}
