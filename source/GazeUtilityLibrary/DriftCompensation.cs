using GazeUtilityLibrary.DataStructs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;


namespace GazeUtilityLibrary
{
    public class DriftCompensation
    {
        private bool _isCollecting = false;
        private Vector3 _fixationPoint;
        private int _fixationFrameCount;
        private double _normalizedDispersionThreshold;
        private Quaternion _q;
        public Quaternion Q { get { return _q; } }

        private List<GazeData> _samples;

        public DriftCompensation(Vector3 fixationPoint, int fixationFrameCount, double dispersionThreashold)
        {
            _q = Quaternion.Identity;
            _samples = new List<GazeData>();
            _normalizedDispersionThreshold = AngleToDist(dispersionThreashold);
            _fixationPoint = fixationPoint;
            _fixationFrameCount = fixationFrameCount;
        }

        public void Reset()
        {
            _samples.Clear();
            _q = Quaternion.Identity;
        }

        public void Start()
        {
            _isCollecting = true;
        }

        public bool Update(GazeData gazeData)
        {
            if (gazeData.Combined.GazeData3d == null || !gazeData.Combined.GazeData3d.IsGazePointValid || !gazeData.Combined.GazeData3d.IsGazeOriginValid || !_isCollecting)
            {
                return false;
            }

            _samples.Add(gazeData);
            if (_samples.Count >= _fixationFrameCount)
            {
                if (Dispersion() <= MaxDeviation())
                {
                    _q = Compute();
                    _samples.Clear();
                    _isCollecting = false;
                    return true;
                }
                else
                {
                    _samples.RemoveAt(0);
                }
            }
            return false;
        }

        private Quaternion Compute()
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

            return CreateQuaternionFromVectors(gDir, cDir);
        }

        private double MaxDeviation()
        {
            double dist = _samples.Average(sample => sample.Combined?.GazeData3d?.GazeDistance ?? 0);
            return dist * _normalizedDispersionThreshold;
        }

        private double Dispersion()
        {
            float xMax = _samples.Max(sample => sample.Combined.GazeData3d?.GazePoint.X ?? 0);
            float yMax = _samples.Max(sample => sample.Combined.GazeData3d?.GazePoint.Y ?? 0);
            float zMax = _samples.Max(sample => sample.Combined.GazeData3d?.GazePoint.Z ?? 0);
            float xMin = _samples.Min(sample => sample.Combined.GazeData3d?.GazePoint.X ?? 0);
            float yMin = _samples.Min(sample => sample.Combined.GazeData3d?.GazePoint.Y ?? 0);
            float zMin = _samples.Min(sample => sample.Combined.GazeData3d?.GazePoint.Z ?? 0);
            float dispersion = xMax - xMin + yMax - yMin + zMax - zMin;
            return dispersion;
        }

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

        private double AngleToDist(double angle)
        {
            return Math.Sqrt(2 * (1 - Math.Cos(angle * Math.PI / 180)));
        }
    }
}
