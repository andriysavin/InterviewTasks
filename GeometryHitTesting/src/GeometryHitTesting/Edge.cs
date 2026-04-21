using System.Windows;

namespace GeometryHitTesting
{
    public struct Edge
    {
        private const double Epsilon = 0.01;

        private Point start;
        private Point end;

        public Edge(Point start, Point end)
        {
            this.start = start;
            this.end = end;
        }

        public Point Start
        {
            get { return start; }
            set { start = value; }
        }

        public Point End
        {
            get { return end; }
            set { end = value; }
        }

        public PointLocation ClassifyPoint(Point point)
        {
            Vector edgeVector = End - Start;
            Vector startToPointVector = point - Start;

            double crossProduct = Vector.CrossProduct(edgeVector, startToPointVector);

            if (crossProduct > Epsilon)
            {
                return PointLocation.Left;
            }
            else if (crossProduct < -Epsilon)
            {
                return PointLocation.Right;
            }

            if (edgeVector.X * startToPointVector.X < 0.0 || edgeVector.Y * startToPointVector.Y < 0.0)
            {
                return PointLocation.Behind;
            }

            if (edgeVector.LengthSquared < startToPointVector.LengthSquared)
            {
                return PointLocation.Beyond;
            }

            return PointLocation.Other;
        }
    }
}
