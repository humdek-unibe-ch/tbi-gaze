﻿/**
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

        const uint OCR_NORMAL = 32512;

        /**
         * @brief hide standard mouse pointer by replacing the current icon with a transparent icon
         * 
         * @param pathToCur path to the transparent mouse icon
         */
        public void HideCursor(string pathToCur)
        {
            IntPtr hcur = LoadCursorFromFile(pathToCur);
            SetSystemCursor(hcur, OCR_NORMAL);
        }
        /**
         * @restore the standard mouse pointer by replacing the current icon with the standard mouse pointer icon
         * 
         * @param pathToCur path to the standard mouse pointer icon
         */
        public void ShowCursor(string pathToCur)
        {
            IntPtr hcur = LoadCursorFromFile(pathToCur);
            SetSystemCursor(hcur, OCR_NORMAL);
        }
    }
}
