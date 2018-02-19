﻿/**
 * @brief Simple logger class
 * 
 * @author  Simon Maurer, simon.maurer@humdek.unibe.ch
 * @file    Logger.cs
 * @date    January 2018
 */

using System;
using System.IO;

namespace GazeHelper
{
    public class Logger
    {
        private string logPath = Directory.GetCurrentDirectory();
        private string logFile;
        private string logFileBak;
        private string dateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff";
        private const int max_log_size = 1000000; // 1 MB
        private enum LogLevel
        {
            Debug,
            Info,
            Warning,
            Error
        }

        public Logger()
        {
            logFile = $"{logPath}\\{Environment.MachineName}_gaze.log";
            logFileBak = $"{logFile}.0";
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

            // use a file rotation of two files. If a log file gets too big, a
            // new log file is created whil the existing one is moved to the
            // backup file (overwriting an existing one)
            FileInfo fi = new FileInfo(logFile);
            if(fi.Exists && fi.Length > max_log_size)
            {
                File.Delete(logFileBak);
                File.Move(logFile, logFileBak);
            }

            try
            {
                StreamWriter sw = new StreamWriter(logFile, true);
                sw.WriteLine(prefix + message);
                sw.Close();
                sw.Dispose();
            }
            catch(Exception e)
            {
                DumpFatal(e);
            }
        }

        public void DumpFatal(Exception e)
        {
            StreamWriter sw = new StreamWriter($"{logPath}\\{DateTime.Now.ToString("yyyyMMddTHHmmss")}_{Environment.MachineName}_fatal.log", true);
            sw.WriteLine(e.ToString());
            sw.Close();
            sw.Dispose();
        }

        /**
         * @brief wrapper function for debug level logging
         * 
         * @param message to be logged
         */
        public void Debug(string message) {
#if DEBUG
            WriteLog(LogLevel.Debug, message);
#endif
        }
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
