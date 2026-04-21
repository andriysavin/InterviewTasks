using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Xml.Linq;

namespace GeometryHitTesting
{
    internal static class SerializationUtils
    {
        public static IEnumerable<Edge> LoadPolygon(string path)
        {
            var polygonElement = XElement.Load(path);

            var edges = polygonElement.Descendants("Edge")
                .Select(edge => ParseEdge(edge));

            return edges;
        }

        private static Edge ParseEdge(XElement edgeElement)
        {
            var edgePoints = edgeElement.Elements("Point");

            Point point1 = ParsePoint(edgePoints.First());
            Point point2 = ParsePoint(edgePoints.Last());

            return new Edge(point1, point2);
        }

        private static Point ParsePoint(XElement pointElement)
        {
            return new Point(Convert.ToDouble(pointElement.Element("X").Value, CultureInfo.InvariantCulture),
                             Convert.ToDouble(pointElement.Element("Y").Value, CultureInfo.InvariantCulture));
        }
    }
}