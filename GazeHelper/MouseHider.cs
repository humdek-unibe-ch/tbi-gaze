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
    public class MouseHider
    {
        [DllImport("user32.dll")]
        static extern bool SetSystemCursor(IntPtr hcur, uint id);
        [DllImport("user32.dll")]
        static extern IntPtr LoadCursorFromFile(string lpFileName);

        const uint OCR_NORMAL = 32512;

        public void HideCursor()
        {
            IntPtr hcur = LoadCursorFromFile("blank.cur");
            SetSystemCursor(hcur, OCR_NORMAL);
        }
        public void ShowCursor()
        {
            IntPtr hcur = LoadCursorFromFile("C:\\Windows\\Cursors\\aero_arrow.cur");
            SetSystemCursor(hcur, OCR_NORMAL);
        }
    }
}
