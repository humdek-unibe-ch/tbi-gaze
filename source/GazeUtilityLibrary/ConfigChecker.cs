/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
﻿using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;

namespace GazeUtilityLibrary
{
    /// <summary>
    /// Helper class to check for the valididty of configuration options.
    /// </summary>
    public static class ConfigChecker
    {
        /// <summary>
        /// Checks the name of the configuration.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static bool CheckConfigName(string? name, TrackerLogger? logger = null)
        {
            if (name == null)
            {
                return false;
            }
            if (!Uri.IsWellFormedUriString(name, UriKind.Relative))
            {
               logger?.Warning($"The config file name \"{name}\" is invalid and cannot be used as file name postfix");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Checks the data log format.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="format">The format.</param>
        /// <returns></returns>
        public static bool CheckDataLogFormat(dynamic value, string? format, TrackerLogger? logger = null)
        {
            if (format == null)
            {
                return false;
            }
            try
            {
                string str = value.ToString(format);
                logger?.Debug($"Use format \"{format}\": {str}");
                return true;
            }
            catch (FormatException)
            {
                logger?.Warning($"The output format string \"{format}\" is not valid");
                return false;
            }
        }

        public static bool CheckColor(string color, TrackerLogger? logger = null)
        {
            try
            {
                ColorConverter.ConvertFromString(color);
                return true;
            }
            catch (FormatException)
            {
                logger?.Warning($"The color string \"{color}\" is not valid");
                return false;
            }
        }

        /// <summary>
        /// Checks the data log column order.
        /// </summary>
        /// <param name="order">The order.</param>
        /// <returns></returns>
        public static bool CheckLogColumnOrder<T>(string? order, TrackerLogger? logger = null)
        {
            if (order == null)
            {
                return false;
            }
            try
            {
                int max_col = Enum.GetNames(typeof(T)).Length;
                string[] values = new string[max_col];
                for (int i = 0; i < max_col; i++) values[i] = "";
                String.Format(order, values);
                return true;
            }
            catch (FormatException)
            {
                logger?.Warning($"The column order string \"{order}\" is not valid");
                return false;
            }
        }

        /// <summary>
        /// Checks the data log column titles.
        /// </summary>
        /// <param name="order">The order.</param>
        /// <param name="titles">The titles.</param>
        /// <returns></returns>
        public static bool CheckLogColumnTitles(string order, string[] titles, TrackerLogger? logger = null)
        {
            try
            {
                String.Format(order, titles);
                return true;
            }
            catch (FormatException)
            {
                logger?.Warning($"Column titles do not match with the column order");
                return false;
            }
        }

        public static bool CheckPointList(double[][] list, TrackerLogger? logger = null)
        {
            for(int i = 0; i < list.GetLength(0); i++)
            {
                if (list[i].Length != 2)
                {
                    logger?.Warning($"A 2-dimensional point is expected: got {list.GetLength(1)} dimensions");
                    return false;
                }
            }
            return true;
        }

        public static double[][] SanitizePointList(double[][] list, TrackerLogger? logger = null)
        {
            List<double[]> distinctPoints = new List<double[]>();
            for (int i = 0; i < list.GetLength(0); i++)
            {
                bool addItem = true;
                for (int j = i + 1; j < list.GetLength(0); j++)
                {
                    if (list[i][0] == list[j][0] && list[i][1] == list[j][1])
                    {
                        logger?.Warning($"Point [{list[i][0]}, {list[i][1]}] is defined twice, ignoring");
                        addItem = false;
                        break;
                    }
                }
                if (addItem)
                {
                    distinctPoints.Add(list[i]);
                }
            }
            return distinctPoints.ToArray();
        }
    }
}
