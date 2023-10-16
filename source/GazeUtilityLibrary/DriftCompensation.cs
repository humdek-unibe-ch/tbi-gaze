/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
﻿using GazeUtilityLibrary.DataStructs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;


namespace GazeUtilityLibrary
{
    /// <summary>
    /// The class to handle drift compensation.
    /// </summary>
    public class DriftCompensation
    {
        private bool _isCollecting = false;
        private Vector3 _fixationPoint;
        private int _fixationFrameCount;
        private double _normalizedDispersionThreshold;
        private double _deviationAngleMax;
        private Quaternion _q;
        /// <summary>
        /// The drift compensation quatrenion.
        /// </summary>
        public Quaternion Q { get { return _q; } }
        private double _deviationAngle;
        /// <summary>
        /// The deviation angle of the drift compensation.
        /// </summary>
        public double DeviationAngle { get { return _deviationAngle; } }
        private double _dispersion;
        /// <summary>
        /// The dispersion of the drift compensation fixation.
        /// </summary>
        public double Dispersion
        {
            get { return _dispersion; }
        }

        private List<GazeData> _samples;

        /// <summary>
        /// Initializes a new instance of the <see cref="DriftCompensation"/> class.
        /// </summary>
        /// <param name="fixationPoint">The target fixation point.</param>
        /// <param name="fixationFrameCount">The required number of frames during fixation.</param>
        /// <param name="dispersionThreashold">The dispersion threashold for the fixation.</param>
        /// <param name="dispersionThreasholdMax">The maximal allowed deviation angle.</param>
        public DriftCompensation(Vector3 fixationPoint, int fixationFrameCount, double dispersionThreashold, double dispersionThreasholdMax)
        {
            _q = Quaternion.Identity;
            _samples = new List<GazeData>();
            _normalizedDispersionThreshold = AngleToDist(dispersionThreashold);
            _deviationAngleMax = dispersionThreasholdMax;
            _fixationPoint = fixationPoint;
            _fixationFrameCount = fixationFrameCount;
        }

        /// <summary>
        /// Reset the drift compensation quaternion to the identity.
        /// </summary>
        public void Reset()
        {
            _samples.Clear();
            _q = Quaternion.Identity;
        }

        /// <summary>
        /// Start the drift compensation.
        /// </summary>
        public void Start()
        {
            _isCollecting = true;
        }

        /// <summary>
        /// Collect gaze data samples of a fixation and once enough samples are collected, compute the drift compensation quaternion.
        /// </summary>
        /// <param name="gazeData">The gaze data sample to collect if it belongs to a fixation.</param>
        /// <returns>True if new drift compensation is computed, false if the process is ongoning.</returns>
        public bool Update(GazeData gazeData)
        {
            bool res = false;
            if (gazeData.Combined.GazeData3d == null || !gazeData.Combined.GazeData3d.IsGazePointValid || !gazeData.Combined.GazeData3d.IsGazeOriginValid || !_isCollecting)
            {
                return false;
            }

            _samples.Add(gazeData);
            if (_samples.Count >= _fixationFrameCount)
            {
                _dispersion = ComputeDispersion();
                if (_dispersion <= MaxDeviation())
                {
                    res = Compute();
                    _samples.Clear();
                    _isCollecting = false;
                }
                else
                {
                    _samples.RemoveAt(0);
                }
            }
            return res;
        }

        /// <summary>
        /// Compute the drift compensation based on the collected samples.
        /// </summary>
        /// <returns>The drift compenstaion quaternion.</returns>
        private bool Compute()
        {
            Vector3 oAvg = Vector3.Zero;
            Vector3 gAvg = Vector3.Zero;

            foreach (GazeData sample in _samples)
            {
                oAvg += sample.Combined.GazeData3d!.GazeOrigin;
                gAvg += sample.Combined.GazeData3d!.GazePoint;
            }
            gAvg /= _fixationFrameCount;
            oAvg /= _fixationFrameCount;

            Vector3 gDir = Vector3.Normalize(gAvg - oAvg);
            Vector3 cDir = Vector3.Normalize(_fixationPoint - oAvg);

            float dot = Vector3.Dot(gDir, cDir);
            _deviationAngle = Math.Acos(dot) / (gDir.Length() * cDir.Length()) * 180 / Math.PI;
            if (_deviationAngle > _deviationAngleMax)
            {
                return false;
            }
            _q = CreateQuaternionFromVectors(gDir, cDir);

            return true;
        }

        /// <summary>
        /// Compute the maximal allowed gaze deviation withn a fixation.
        /// </summary>
        /// <returns>The computed maximal gaze deviation (also called dispersion threshold)</returns>
        private double MaxDeviation()
        {
            double dist = _samples.Average(sample => sample.Combined?.GazeData3d?.GazeDistance ?? 0);
            return dist * _normalizedDispersionThreshold;
        }

        /// <summary>
        /// Compute the dispersion of a gaze sample.
        /// </summary>
        /// <returns>The dispersion of a gaze sample.</returns>
        private double ComputeDispersion()
        {
            float xMax = _samples.Max(sample => sample.Combined.GazeData3d?.GazePoint.X ?? float.NegativeInfinity);
            float yMax = _samples.Max(sample => sample.Combined.GazeData3d?.GazePoint.Y ?? float.NegativeInfinity);
            float zMax = _samples.Max(sample => sample.Combined.GazeData3d?.GazePoint.Z ?? float.NegativeInfinity);
            float xMin = _samples.Min(sample => sample.Combined.GazeData3d?.GazePoint.X ?? float.PositiveInfinity);
            float yMin = _samples.Min(sample => sample.Combined.GazeData3d?.GazePoint.Y ?? float.PositiveInfinity);
            float zMin = _samples.Min(sample => sample.Combined.GazeData3d?.GazePoint.Z ?? float.PositiveInfinity);
            float dispersion = xMax - xMin + yMax - yMin + zMax - zMin;
            return dispersion / 3;
        }

        /// <summary>
        /// Helper function to create a quaternion describing the rotation from vector v1 to v2.
        /// </summary>
        /// <param name="v1">The initial vector</param>
        /// <param name="v2">The target vector into which the initial vector is rotated by the resulting quaternion.</param>
        /// <returns>The quaternion to rotate v1 to v2</returns>
        private Quaternion CreateQuaternionFromVectors(Vector3 v1, Vector3 v2)
        {
            float dot = Vector3.Dot(v1, v2);
            if (dot > 0.999999)
            {
                return Quaternion.Identity;
            }
            else
            {
                Vector3 axis = Vector3.Cross(v1, v2);
                return Quaternion.Normalize(new Quaternion(axis, 1 + dot));

            }
        }

        /// <summary>
        /// A helper function to compute a normalized dispersion factor given an angle.
        /// Multiplied by a distance this gives a deviation distance.
        /// </summary>
        /// <param name="angle">The angle for which to compute the dispersion.</param>
        /// <returns>The normalized dispersion.</returns>
        private double AngleToDist(double angle)
        {
            //return Math.Sqrt(2 * (1 - Math.Cos(angle * Math.PI / 180)));
            return Math.Tan(angle * Math.PI / 180);
        }
    }
}
