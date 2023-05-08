using System.Windows;

namespace CustomCalibrate.Models
{
    public class CalibrationModel
    {
        private Point[]? _positions;
        public Point[]? Positions {
            get { return _positions; }
        }

        public void Init(double ActualWidth, double ActualHeight)
        {
            double XDelta = ActualWidth * 0.1;
            double YDelta = ActualHeight * 0.1;
            Point Center = new Point(ActualWidth / 2, ActualHeight / 2);
            _positions = new Point[8];
            _positions[5] = new Point(XDelta, YDelta); // top left
            _positions[4] = new Point(Center.X, YDelta); // top middle
            _positions[6] = new Point(ActualWidth - XDelta, YDelta); // top right
            _positions[3] = new Point(XDelta, ActualHeight - YDelta); // bottom left
            _positions[7] = new Point(Center.X, ActualHeight - YDelta); // bottom middle
            _positions[2] = new Point(ActualWidth - XDelta, ActualHeight - YDelta); // bottom right
            _positions[0] = new Point(Center.X + Center.X * 0.3, Center.Y); // center right
            _positions[1] = new Point(Center.X - Center.X * 0.3, Center.Y); // center left
        }
    }
}
