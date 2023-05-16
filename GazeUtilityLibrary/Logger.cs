using System;
using System.IO;

namespace GazeUtilityLibrary
{
    /// <summary>
    /// Simple logger class.
    /// </summary>
    public class TrackerLogger
    {
        private string logPath = Directory.GetCurrentDirectory();
        private string logFile;
        private string logFileBak;
        private string dateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff";
        private const int max_log_size = 1000000; // 1 MB
        private readonly object _syncObject = new object();
        private enum LogLevel
        {
            Debug,
            Info,
            Warning,
            Error
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackerLogger"/> class.
        /// </summary>
        public TrackerLogger()
        {
            logFile = $"{logPath}\\{Environment.MachineName}_gaze.log";
            logFileBak = $"{logFile}.0";
        }

        /// <summary>
        /// Writes the log messages to the log file.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="message">The message.</param>
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
            if (fi.Exists && fi.Length > max_log_size)
            {
                File.Delete(logFileBak);
                File.Move(logFile, logFileBak);
            }

            try
            {
                lock(_syncObject)
                {
                    StreamWriter sw = new StreamWriter(logFile, true);
                    sw.WriteLine(prefix + message);
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                DumpFatal(e);
            }
        }

        /// <summary>
        /// Dumps exception to a new file if it is not possible to write to the main log file.
        /// </summary>
        /// <param name="e">The exception.</param>
        public void DumpFatal(Exception e)
        {
            StreamWriter sw = new StreamWriter($"{logPath}\\{DateTime.Now.ToString("yyyyMMddTHHmmss")}_{Environment.MachineName}_fatal.log", true);
            sw.WriteLine(e.ToString());
            sw.Close();
        }

        /// <summary>
        /// wrapper function for debug level logging.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Debug(string message)
        {
#if DEBUG
            WriteLog(LogLevel.Debug, message);
#endif
        }

        /// <summary>
        /// wrapper function for info level logging
        /// </summary>
        /// <param name="message">The message.</param>
        public void Info(string message) { WriteLog(LogLevel.Info, message); }

        /// <summary>
        /// wrapper function for warning level logging
        /// </summary>
        /// <param name="message">The message.</param>
        public void Warning(string message) { WriteLog(LogLevel.Warning, message); }

        /// <summary>
        /// wrapper function for error level logging
        /// </summary>
        /// <param name="message">The message.</param>
        public void Error(string message) { WriteLog(LogLevel.Error, message); }
    }
}