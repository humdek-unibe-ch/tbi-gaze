/**
 * @brief helper class to show and hide the system curser
 * 
 * @author  Simon Maurer, simon.maurer@humdek.unibe.ch
 * @file    MouseHider.cs
 * @date    Jamuary 2018
 */

using System;
using System.Runtime.InteropServices;

namespace GazeHelper
{
    /**
     * @brief hide standard mouse pointer and resore it
     */
    public class MouseHider
    {
        [DllImport("user32.dll")]
        static extern bool SetSystemCursor(IntPtr hcur, uint id);
        [DllImport("user32.dll")]
        static extern IntPtr LoadCursorFromFile(string lpFileName);

        private Logger logger = new Logger();
        private string pathToBlankCur = "blank.cur";
        private string pathToStandardCur = "C:\\Windows\\Cursors\\aero_arrow.cur";

        const uint OCR_NORMAL = 32512;
        //const uint OCR_APPSTARTING = 32650;

        /**
         * @brief hide standard mouse pointer by replacing the current icon with a transparent icon
         */
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
        /**
         * @restore the standard mouse pointer by replacing the current icon with the standard mouse pointer icon
         * 
         * @param pathToCur path to the standard mouse pointer icon
         */
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
