using System;
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
        public Vector3 V1 { get { return _v1; } }

        private Vector3 _v2;
        public Vector3 V2 { get { return _v2; } }

        private Vector3 _v3;
        public Vector3 V3 { get { return _v3; } }

        private Vector3 _e1;
        public Vector3 E1 { get { return _e1; } }

        private Vector3 _e2;
        public Vector3 E2 { get { return _e2; } }

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
