/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
﻿using System;
using System.Runtime.InteropServices;

/// <summary>
/// helper class to show and hide the system curser
/// </summary>
namespace GazeUtilityLibrary
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
        private string _pathToBlankCur = "assets\\blank.cur";
        private string _pathToStandardCur = "C:\\Windows\\Cursors\\aero_arrow.cur";

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
            LoadCursor(_pathToBlankCur);
        }

        /// <summary>
        /// Shows the cursor.
        /// </summary>
        /// <remarks>
        /// the standard mouse pointer by replacing the current icon with the standard mouse pointer icon
        /// </remarks>
        /// <param name="pathToCur">The path to the standard mouse pointer icon.</param>
        public void ShowCursor(string? pathToCur)
        {
            if (pathToCur == null)
            {
                logger.Info($"Try loading cursor fromm default cursor path \"{_pathToStandardCur}\"");
                LoadCursor(_pathToStandardCur);
            }
            else
            {
                if (!LoadCursor(pathToCur))
                {
                    logger.Info($"Try loading cursor fromm default cursor path \"{_pathToStandardCur}\"");
                    LoadCursor(_pathToStandardCur);
                }
            }
        }

        /// <summary>
        /// Load a mouse cursor given a path to the cursor file.
        /// </summary>
        /// <param name="pathToCur">Path to the cursor file.</param>
        /// <returns></returns>
        private bool LoadCursor(string pathToCur)
        {
            IntPtr hcur = LoadCursorFromFile(pathToCur);
            if ((int)hcur == 0)
            {
                logger.Error($"Cannot load curser file \"{pathToCur}\"");
                return false;
            }

            SetSystemCursor(hcur, OCR_NORMAL);
            return true;
        }
    }
}
