using System;
using System.Runtime.InteropServices;

/// <summary>
/// helper class to show and hide the system curser
/// </summary>
namespace GazeHelper
{
    /// <summary>
    /// hide standard mouse pointer and resore it
    /// </summary>
    public class MouseHider
    {
        [DllImport("user32.dll")]
        static extern bool SetSystemCursor(IntPtr hcur, uint id);
        [DllImport("user32.dll")]
        static extern IntPtr LoadCursorFromFile(string lpFileName);

        private TrackerLogger logger;
        private string pathToBlankCur = "assets\\blank.cur";
        private string pathToStandardCur = "C:\\Windows\\Cursors\\aero_arrow.cur";

        const uint OCR_NORMAL = 32512;
        //const uint OCR_APPSTARTING = 32650;

        /// <summary>
        /// Initializes a new instance of the <see cref="MouseHider"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public MouseHider(TrackerLogger logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Hides the cursor.
        /// </summary>
        /// <remarks>
        /// Hides the standard mouse pointer by replacing the current icon with a transparent icon.
        /// </remarks>
        public void HideCursor()
        {
            IntPtr hcur = LoadCursorFromFile(pathToBlankCur);
            if (hcur != null)
            {
                SetSystemCursor(hcur, OCR_NORMAL);
                logger.Info("Hiding standard mouse cursor (pointer)");
            }
            else logger.Error($"Cannot load curser file \"{pathToBlankCur}\"");
        }

        /// <summary>
        /// Shows the cursor.
        /// </summary>
        /// <remarks>
        /// the standard mouse pointer by replacing the current icon with the standard mouse pointer icon
        /// </remarks>
        /// <param name="pathToCur">The path to the standard mouse pointer icon.</param>
        public void ShowCursor(string pathToCur)
        {
            IntPtr hcur = LoadCursorFromFile(pathToCur);
            if (hcur != null)
            {
                SetSystemCursor(hcur, OCR_NORMAL);
                logger.Info("Restoring standard mouse cursor (pointer)");
            }
            else
            {
                logger.Error($"Cannot load curser file \"{pathToCur}\"");
                logger.Info($"Load cursor fromm default cursor path \"{pathToStandardCur}\"");
                hcur = LoadCursorFromFile(pathToStandardCur);
                SetSystemCursor(hcur, OCR_NORMAL);
            }
        }
    }
}
