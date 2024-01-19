/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
﻿using System;
using System.IO;
using System.Numerics;

namespace GazeUtilityLibrary
{
    /// <summary>
    /// The class describing the Screen area in 3d and 2d space.
    /// </summary>
    public class ScreenArea
    {

        private float _width;
        /// <summary>
        /// The width of the screen.
        /// </summary>
        public float Width { get { return _width; } }

        private float _height;
        /// <summary>
        /// The height of the screen.
        /// </summary>
        public float Height { get { return _height; } }

        private Vector3 _bottomLeft;
        /// <summary>
        /// The coordinates of the bottom left point of the screen.
        /// </summary>
        public Vector3 BottomLeft { get { return _bottomLeft; } }

        private Vector3 _bottomRight;
        /// <summary>
        /// The coordinates of the bottom right point of the screen.
        /// </summary>
        public Vector3 BottomRight { get { return _bottomRight; } }

        private Vector3 _topLeft;
        /// <summary>
        /// The coordinates of the top left point of the screen.
        /// </summary>
        public Vector3 TopLeft { get { return _topLeft; } }

        private Vector3 _topRight;
        /// <summary>
        /// The coordinates of the to right point of the screen.
        /// </summary>
        public Vector3 TopRight { get { return _topRight; } }

        private Vector3 _center;
        /// <summary>
        /// The coordinates of the center point of the screen.
        /// </summary>
        public Vector3 Center { get { return _center; } }

        private Vector2 _origin;
        private Vector3 _norm;

        private Matrix4x4 _m;

        /// <summary>
        /// Constructor. Assigns parameters ann computes the transformation matrix to transform a 3d point into a 2d point.
        /// </summary>
        /// <param name="bottomLeft">The bottom left 3d coordinate of the screen.</param>
        /// <param name="bottomRight">The bottom right 3d coordinate of the screen.</param>
        /// <param name="topLeft">The top left 3d coordinate of the screen.</param>
        /// <param name="topRight">The top right 3d coordinate of the screen</param>
        /// <param name="width">The width of the screen</param>
        /// <param name="height">The heigth of the screen</param>
        public ScreenArea(Vector3 bottomLeft, Vector3 bottomRight, Vector3 topLeft, Vector3 topRight, float width, float height)
        {
            Vector3 bottomCenter = Vector3.Lerp(bottomLeft, bottomRight, 0.5f);
            Vector3 topCenter = Vector3.Lerp(topLeft, topRight, 0.5f);
            _center = Vector3.Lerp(bottomCenter, topCenter, 0.5f);
            _bottomLeft = bottomLeft;
            _topLeft = topLeft;
            _bottomRight = bottomRight;
            _topRight = topRight;
            _width = width;
            _height = height;
            Vector3 e1 = topRight - topLeft;
            Vector3 e2 = bottomLeft - topLeft;
            _norm = Vector3.Cross(e1, e2);
            Vector3 u = topLeft + Vector3.Normalize(e1);
            Vector3 v = topLeft + Vector3.Normalize(e2);
            Vector3 n = topLeft + Vector3.Normalize(_norm);
            Matrix4x4 s = new Matrix4x4(
                topLeft.X, u.X, v.X, n.X,
                topLeft.Y, u.Y, v.Y, n.Y,
                topLeft.Z, u.Z, v.Z, n.Z,
                1, 1, 1, 1
            );
            Matrix4x4 d = new Matrix4x4(
                0, 1, 0, 0,
                0, 0, 1, 0,
                0, 0, 0, 1,
                1, 1, 1, 1
            );
            Matrix4x4 sInv;
            Matrix4x4.Invert(s, out sInv);
            _m = Matrix4x4.Transpose(d * sInv);
            _origin = GetPoint2d(topLeft);
        }

        /// <summary>
        /// Compute the intersection point with the screen plane given a gaze origin and a gaze direction.
        /// Note that this does not compute the intersection with the screen area but with the infinite plane which is co-aligned with the screen.
        /// Pass the here computed intersection point to the method GetPoint2dNormalized to get the normalized intersection point on the sreen area.
        /// </summary>
        /// <param name="gazeOrigin">The origin of the gaze.</param>
        /// <param name="gazeDirection">The direction of the gaze.</param>
        /// <returns>The intersection point with the screen or null if no intersection point exists.</returns>
        public Vector3? GetIntersectionPoint(Vector3 gazeOrigin, Vector3 gazeDirection)
        {
            float d = Vector3.Dot(_norm, gazeOrigin - _topLeft);
            float n = Vector3.Dot(-gazeDirection, _norm);

            if (n == 0)
            {
                return null;
            }

            return gazeOrigin + d / n * gazeDirection;
        }

        /// <summary>
        /// Get the 2d point on the sreen plane given a 3d point on the screen plane.
        /// </summary>
        /// <param name="point">The 3d point on the screen plane to convert.</param>
        /// <returns>The 2d point on the screen</returns>
        public Vector2 GetPoint2d(Vector3 point)
        {
            Vector4 v = Vector4.Transform(point, _m);
            return new Vector2(v.X, v.Y);
        }

        /// <summary>
        /// Get the normalized 2d point on the sreen plane given a 3d point on the screen plane.
        /// Note that values outside of the interval [0, 1] indicate an intersection point outsate of the screen area.
        /// </summary>
        /// <param name="point3d">The 3d point on the screen plane to convert.</param>
        /// <returns>The normalized 2d point on the screen</returns>
        public Vector2 GetPoint2dNormalized(Vector3 point3d)
        {
            Vector2 point2dOffset = GetPoint2d(point3d);
            Vector2 point2d = point2dOffset - _origin;
            return new Vector2(point2d.X / _width, point2d.Y / _height);
        }

        /// <summary>
        /// Get the 3d point on the sreen plane given a 2d point on the screen.
        /// </summary>
        /// <param name="point2d">A normalized 2d point on the screen to convert.</param>
        /// <returns>The 3d point on the screen plane</returns>
        public Vector3 GetPoint3d(Vector2 point2d)
        {
            Vector3 xTop = Vector3.Lerp(_topLeft, _topRight, point2d.X);
            Vector3 xBottom = Vector3.Lerp(_bottomLeft, _bottomRight, point2d.X);
            return Vector3.Lerp(xTop, xBottom, point2d.Y);
        }

        /// <summary>
        /// Dump the four screen corner points to a csv file
        /// </summary>
        /// <param name="path">The folder to store the file.</param>
        /// <param name="prefix">The file prefix.</param>
        /// <returns></returns>
        public bool Dump(string path, string prefix)
        {
            path += $"/{prefix}screenArea.csv";
            StreamWriter? sw = null;
            try
            {
                sw = new StreamWriter($"{path}");
                FileInfo fi = new FileInfo($"{path}");
            }
            catch
            {
                return false;
            }

            sw?.WriteLine("top_left, top_right, bottom_left, bottom_right");
            sw?.WriteLine($"{_topLeft.X}, {_topRight.X}, {_bottomLeft.X}, {_bottomRight.X}");
            sw?.WriteLine($"{_topLeft.Y}, {_topRight.Y}, {_bottomLeft.Y}, {_bottomRight.Y}");
            sw?.WriteLine($"{_topLeft.Z}, {_topRight.Z}, {_bottomLeft.Z}, {_bottomRight.Z}");

            sw?.Close();
            return true;
        }
    }
}
