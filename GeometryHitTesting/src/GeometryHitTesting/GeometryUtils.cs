/*
 * Copyright (c) 2026 Andriy Savin
 *
 * This code is licensed under the MIT License.
 * See the LICENSE file in the repository root for full license text.
 * 
 * Attribution is appreciated when reusing this code.
 * 
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace GeometryHitTesting
{
    /// <summary>
    /// Provides methods for working with geometry objects.
    /// </summary>
    public static class GeometryUtils
    {
        /// <summary>
        /// Checks if a given <paramref name="point"/> is inside a given <paramref name="polygon"/>.
        /// </summary>
        /// <param name="polygon">
        /// A polygon, represented by a collection of edges. 
        /// </param>
        /// <param name="point">
        /// A point to be checked.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Is thrown if the <paramref name="polygon"/> is <c>null</c>.
        /// </exception>
        /// <returns><c>true</c>, if the point is inside the polygon, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// This method supposes, that the <paramref name="polygon"/> collection 
        /// contains correct number of edges (at least three), and the edges ends are sequentially 
        /// connected (the last edge is also connected with the first).
        /// </remarks>
        public static bool IsPointInPolygon(IEnumerable<Edge> polygon, Point point)
        {
            if (polygon == null)
                throw new ArgumentNullException("polygon");

            // To check if a point lies inside a polygon
            // we use ray algorithm. The idea is to "draw" an 
            // arbitrary ray starting at the point we're checking.
            // Then, we count how many edges the ray intersects.
            // If the point is inside the polygon, the number of 
            // intersections will be odd.
            int intersectionCount = 0;

            // We use vertical ray directed down so the X component
            // of the intersection point is fixed. A vertical ray 
            // (opposed to horizontal) allows to use line function
            // if the usual "y = F(X)" form.
            double intersectionX = point.X;

            foreach (var edge in polygon)
            {
                // Define bounds for X component of intersection point.
                double minX = Math.Min(edge.Start.X, edge.End.X);
                double maxX = Math.Max(edge.Start.X, edge.End.X);

                // If an intersection point (of the ray and an edge line) lies outside of the edge
                // bounds in X-direction, such intersection isn't interesting for us.
                // Note, that if the left edge point lies ON the ray in X-direction
                // (intersectionX == minX) AND the right edge point lies strictly
                // righter to the ray (intersectionX < maxX), such intersection is counted. 
                // And the opposite case is not.
                // Also this check will catch the case
                // when the X components of both edge points are the same
                // (edge is vertical and collinear with the ray).
                if (intersectionX < minX || intersectionX >= maxX)
                    continue;

                // Calculate the X component of the ray and edge line intersection point.
                // Its safe to call this method without checking for vertical edge,
                // as previous check filters this case out.
                double intersectionY = LineFunction(edge.End, edge.Start, intersectionX);

                // The ray is started at the point and is directed down,
                // so to belong to the ray the intersection point must
                // lie lower then the point.
                if (intersectionY > point.Y)
                    intersectionCount++;
            }

            // If the intersection count is odd - the point is inside.
            return intersectionCount % 2 == 1;
        }

        public static IEnumerable<Point> GetPolygonPoints(IEnumerable<Edge> polygon)
        {
            return polygon.Select(edge => edge.Start);
        }

        public static bool IsPolygonConvex(List<Edge> polygon)
        {
            PointLocation prevPointLocation =
                polygon.Last().ClassifyPoint(polygon.First().End);

            for (int i = 0; i < polygon.Count - 1; i++)
            {
                PointLocation currPointLocation = polygon[i].ClassifyPoint(polygon[i + 1].End);

                if (prevPointLocation == PointLocation.Left && currPointLocation == PointLocation.Right ||
                    prevPointLocation == PointLocation.Right && currPointLocation == PointLocation.Left)
                    return false;

                prevPointLocation = currPointLocation;
            }

            return true;
        }

        public static List<Edge> ToConvex(List<Edge> polygon)
        {
            var points = GetPolygonPoints(polygon).ToList();

            int minXIndex = 0;
            for (int i = 1; i < points.Count; i++)
            {
                if (points[i].X < points[minXIndex].X)
                    minXIndex = i;
            }

            points.Add(points[minXIndex]);

            List<Point> convexPolygonPoints = new List<Point>();

            int mostOuterPointIndex = minXIndex;

            for (int i = 0; i < points.Count - 1 && mostOuterPointIndex < points.Count - 1; i++)
            {
                convexPolygonPoints.Add(points[mostOuterPointIndex]);
                SwapElements(points, i, mostOuterPointIndex);
                mostOuterPointIndex = FindMostOuterPointIndex(i, points);
            }
            
            List<Edge> result = new List<Edge>();

            for (int i = 0; i < convexPolygonPoints.Count - 1; i++)
            {
                result.Add(new Edge(convexPolygonPoints[i], convexPolygonPoints[i + 1]));
            }

            result.Add(new Edge(convexPolygonPoints[convexPolygonPoints.Count - 1], convexPolygonPoints[0]));

            return result;
        }

        /// <summary>
        /// Represents a function for a line on a plane, defined by two points.
        /// </summary>
        /// <param name="p1">The first point the line passes through.</param>
        /// <param name="p2">The second point the line passes through.</param>
        /// <param name="x">
        /// The X coordinate of a point lying on the line, 
        /// for which to calculate the Y coordinate.
        /// </param>
        /// <returns>
        /// The Y coordinate corresponding to the given X coordinate.
        /// </returns>
        private static double LineFunction(Point p1, Point p2, double x)
        {
            // Use two-point line equation to calculate Y for given X.
            // This equation assumes p1.X != p2.X. We check this only for debug builds,
            // as this method is private.
            Debug.Assert(p1.X != p2.X);

            double y =
                (x - p1.X) *
                (p2.Y - p1.Y) /
                (p2.X - p1.X) +
                p1.Y;

            return y;
        }

        private static void SwapElements<T>(IList<T> list, int firstElementIndex, int secondElementIndex)
        {
            T temp = list[firstElementIndex];
            list[firstElementIndex] = list[secondElementIndex];
            list[secondElementIndex] = temp;
        }

        private static int FindMostOuterPointIndex(int anchorPointIndex, List<Point> points)
        {
            Point p1 = points[anchorPointIndex];
            Point p2 = points[anchorPointIndex + 1];

            Edge e = new Edge(p1, p2);

            int mostOuterPointIndex = anchorPointIndex + 1;

            for (int candidateIndex = anchorPointIndex + 2; candidateIndex < points.Count; candidateIndex++)
            {
                var candidatePointLocation = e.ClassifyPoint(points[candidateIndex]);

                if (candidatePointLocation == PointLocation.Left ||
                    candidatePointLocation == PointLocation.Beyond)
                {
                    e.End = points[candidateIndex];
                    mostOuterPointIndex = candidateIndex;
                }
            }

            return mostOuterPointIndex;
        }       
    }
}
