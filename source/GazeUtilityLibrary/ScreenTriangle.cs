/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
﻿using System;
using System.Numerics;

namespace GazeUtilityLibrary
{
    /// <summary>
    /// A class to describe a triangle.
    /// This was supposed to be used to construct the ScreenArea but it turned out that it is
    /// simpler to work with the screen plane and use the normalised intersection points to check
    /// wheter the gaze point is outside the screen area.
    /// </summary>
    public class ScreenTriangle
    {
        private Vector3 _v1;
        /// <summary>
        /// A corner point of the triangle.
        /// </summary>
        public Vector3 V1 { get { return _v1; } }

        private Vector3 _v2;
        /// <summary>
        /// A corner point of the triangle.
        /// </summary>
        public Vector3 V2 { get { return _v2; } }

        private Vector3 _v3;
        /// <summary>
        /// A corner point of the triangle.
        /// </summary>
        public Vector3 V3 { get { return _v3; } }

        private Vector3 _e1;
        /// <summary>
        /// The edge vector from v1 to v2.
        /// </summary>
        public Vector3 E1 { get { return _e1; } }

        private Vector3 _e2;
        /// <summary>
        /// The edge vector from v1 to v3.
        /// </summary>
        public Vector3 E2 { get { return _e2; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScreenTriangle"/> class.
        /// </summary>
        /// <param name="v1">A corner point of the triangle.</param>
        /// <param name="v2">A corner point of the triangle.</param>
        /// <param name="v3">A corner point of the triangle.</param>
        public ScreenTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            _v1 = v1;
            _v2 = v2;
            _v3 = v3;
            _e1 = v2 - v1;
            _e2 = v3 - v1;
        }

        /// <summary>
        /// Compute the intersection point with the triangle with the Moller-Trumbore algorithm.
        /// </summary>
        /// <param name="origin">The origin of the gaze point</param>
        /// <param name="direction">The direction of the gaze point</param>
        /// <returns>The intersection point or null if no intersection point could be computed.</returns>
        public Vector3? GetIntersectionPoint(Vector3 origin, Vector3 direction)
        {
            Vector3 p, t, q, directionScaled;
            float det, invDet, u, v, distTmp;
            float epsilon = 0.0000001f;

            // Möller-Trumbore
            p = Vector3.Cross(direction, _e2);
            det = Vector3.Dot(_e1, p);

            if (Math.Abs(det) < epsilon)
            {
                return null; // line is parallel to the triangle
            }

            invDet = 1 / det;

            t = origin - _v1;
            u = invDet * Vector3.Dot(t, p);
            if (u < 0 || u > 1)
            {
                return null;
            }

            q = Vector3.Cross(t, _e1);
            v = invDet * Vector3.Dot(direction, q);
            if (u < 0 || u + v > 1)
            {
                return null;
            }

            distTmp = invDet * Vector3.Dot(_e2, q);
            if (distTmp > epsilon)
            {
                directionScaled = Vector3.Multiply(direction, distTmp);
                return origin + directionScaled;
            }

            return null;
        }
    }
}
