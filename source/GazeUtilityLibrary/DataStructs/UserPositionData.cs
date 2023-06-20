
namespace GazeUtilityLibrary.DataStructs
{
    public class UserPositionData
    {
        private double _xCoordLeft;
        public double XCoordLeft { get { return _xCoordLeft; } }

        private double _yCoordLeft;
        public double YCoordLeft { get { return _yCoordLeft; } }

        private double _zCoordLeft;
        public double ZCoordLeft { get { return _zCoordLeft; } }

        private double _xCoordRight;
        public double XCoordRight { get { return _xCoordRight; } }

        private double _yCoordRight;
        public double YCoordRight { get { return _yCoordRight; } }

        private double _zCoordRight;
        public double ZCoordRight { get { return _zCoordRight; } }

        public UserPositionData()
        {
            _xCoordLeft = 0.5;
            _yCoordLeft = 0.5;
            _zCoordLeft = 0.5;
            _xCoordRight = 0.5;
            _yCoordRight = 0.5;
            _zCoordRight = 0.5;
        }
        public UserPositionData(double xCoordLeft, double yCoordLeft, double zCoordLeft, double xCoordRight, double yCoordRight, double zCoordRight)
        {
            _xCoordLeft = xCoordLeft;
            _yCoordLeft = yCoordLeft;
            _zCoordLeft = zCoordLeft;
            _xCoordRight = xCoordRight;
            _yCoordRight = yCoordRight;
            _zCoordRight = zCoordRight;
        }
    }
}
