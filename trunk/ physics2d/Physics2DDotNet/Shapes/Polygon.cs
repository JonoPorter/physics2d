#region MIT License
/*
 * Copyright (c) 2005-2007 Jonathan Mark Porter. http://physics2d.googlepages.com/
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy 
 * of this software and associated documentation files (the "Software"), to deal 
 * in the Software without restriction, including without limitation the rights to 
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of 
 * the Software, and to permit persons to whom the Software is furnished to do so, 
 * subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be 
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
 * INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
 * PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE 
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
 * TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 */
#endregion




#if UseDouble
using Scalar = System.Double;
#else
using Scalar = System.Single;
#endif
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Threading;

using AdvanceMath;
using AdvanceMath.Geometry2D;
using Physics2DDotNet.Math2D;

namespace Physics2DDotNet
{

    static class BitmapHelper
    {
        static Point2D[] bitmapPoints = new Point2D[]{
            new Point2D (1,1),
            new Point2D (0,1),
            new Point2D (-1,1),
            new Point2D (-1,0),
            new Point2D (-1,-1),
            new Point2D (0,-1),
            new Point2D (1,-1),
            new Point2D (1,0),
        };
        public static Vector2D[] CreateFromBitmap(bool[,] bitmap)
        {
            Point2D first = Point2D.Zero;
            bool test = true;
            for (int x = bitmap.GetLength(0) - 1; x > -1 && test; --x)
            {
                for (int y = 0; y < bitmap.GetLength(1); ++y)
                {
                    if (bitmap[x, y])
                    {
                        first = new Point2D(x, y);
                        test = false;
                    }
                }
            }
            Point2D current = first;
            Point2D last = first - new Point2D(0, 1);
            List<Point2D> result = new List<Point2D>();
            do
            {
                if (result.Count - 2 >= 0)
                {
                    Point2D back1 = result[result.Count - 1];
                    Point2D back2 = result[result.Count - 2];
                    if (IsInLine(ref back1, ref back2, ref current))
                    {
                        result[result.Count - 1] = current;
                    }
                    else
                    {
                        result.Add(current);
                    }
                }
                else
                {
                    result.Add(current);
                }
                Point2D temp = current;
                current = GetNextVertex(bitmap, current, last);
                last = temp;
            } while (current != first);
            if (result.Count - 2 >= 0)
            {
                Point2D back1 = result[result.Count - 1];
                Point2D back2 = result[result.Count - 2];
                if (IsInLine(ref back1, ref back2, ref current))
                {
                    result.RemoveAt(result.Count - 1);
                }
            }
            Vector2D[] rv = new Vector2D[result.Count];
            for (int index = 0; index < rv.Length; ++index)
            {
                rv[index].X = result[index].X;
                rv[index].Y = result[index].Y;
            }
            return rv;
        }
        private static bool IsInLine(ref Point2D v1, ref Point2D v2, ref Point2D v3)
        {
            Scalar div1 = (Scalar)(v1.Y - v3.Y);
            Scalar div2 = (Scalar)(v2.Y - v3.Y);
            if (div1 == 0 && div1 == 0) { return true; }
            div1 = (v1.X - v3.X) / div1;
            div2 = (v2.X - v3.X) / div2;
            return div1 == div2;
        }
        private static Point2D GetNextVertex(bool[,] bitmap, Point2D current, Point2D last)
        {
            int offset = 0;
            Point2D point;
            for (int index = 0; index < bitmapPoints.Length; ++index)
            {
                Point2D.Add(ref current, ref bitmapPoints[index], out point);
                if (Point2D.Equals(ref point, ref last))
                {
                    offset = index + 1;
                    break;
                }
            }
            for (int index = 0; index < bitmapPoints.Length; ++index)
            {
                Point2D.Add(
                    ref current,
                    ref bitmapPoints[(index + offset) % bitmapPoints.Length],
                    out point);
                if (point.X >= 0 && point.X < bitmap.GetLength(0) &&
                    point.Y >= 0 && point.Y < bitmap.GetLength(1) &&
                    bitmap[point.X, point.Y])
                {
                    return point;
                }
            }
            throw new Exception();
        }
    }

    [Serializable]
    public sealed class Polygon : Shape
    {
        #region static methods
        /// <summary>
        /// Takes a 2D boolean array with a true value representing a bitmap
        /// and converts it to an array of vertex that surround that bitmap.
        /// </summary>
        /// <param name="bitmap">a bitmap to be converted. true means its collidable.</param>
        /// <returns>a Vector2D[] representing the bitmap.</returns>
        public static Vector2D[] CreateFromBitmap(bool[,] bitmap)
        {
            if (bitmap == null) { throw new ArgumentNullException("bitmap"); }
            return BitmapHelper.CreateFromBitmap(bitmap);
        }
        /// <summary>
        /// creates vertexes that describe a Rectangle.
        /// </summary>
        /// <param name="length">The length of the Rectangle</param>
        /// <param name="width"></param>
        /// <returns>array of vectors the describe a rectangle</returns>
        public static Vector2D[] CreateRectangle(Scalar length, Scalar width)
        {
            Scalar Ld2 = length / 2;
            Scalar Wd2 = width / 2;
            return new Vector2D[4]
            {
                new Vector2D(Wd2, Ld2),
                new Vector2D(-Wd2, Ld2),
                new Vector2D(-Wd2, -Ld2),
                new Vector2D(Wd2, -Ld2)
            };
        }
        /// <summary>
        /// makes sure the distance between 2 vertexes is under the length passed, by adding vertexes between them.
        /// </summary>
        /// <param name="vertexes">the original vertexes.</param>
        /// <param name="maxLength">the maximum distance allowed between 2 vertexes</param>
        /// <returns>The new vertexes.</returns>
        public static Vector2D[] Subdivide(Vector2D[] vertexes, Scalar maxLength)
        {
            return Subdivide(vertexes, maxLength, true);
        }
        /// <summary>
        /// makes sure the distance between 2 vertexes is under the length passed, by adding vertexes between them.
        /// </summary>
        /// <param name="vertexes">the original vertexes.</param>
        /// <param name="maxLength">the maximum distance allowed between 2 vertexes</param>
        /// <param name="loop">if it should check the distance between the first and last vertex.</param>
        /// <returns>The new vertexes.</returns>
        public static Vector2D[] Subdivide(Vector2D[] vertexes, Scalar maxLength, bool loop)
        {
            if (vertexes == null) { throw new ArgumentNullException("vertexes"); }
            if (vertexes.Length < 2) { throw new ArgumentOutOfRangeException("vertexes"); }
            if (maxLength <= 0) { throw new ArgumentOutOfRangeException("maxLength"); }

            LinkedList<Vector2D> list = new LinkedList<Vector2D>(vertexes);

            LinkedListNode<Vector2D> node = list.First;
            while (node != null)
            {
                Vector2D line;
                if (node.Next == null)
                {
                    if (!loop) { break; }
                    line = list.First.Value - node.Value;
                }
                else
                {
                    line = node.Next.Value - node.Value;
                }
                Scalar mag;
                Vector2D.GetMagnitude(ref line, out mag);
                if (mag > maxLength)
                {
                    int count = (int)MathHelper.Ceiling(mag / maxLength);
                    mag = mag / (mag * count);
                    Vector2D.Multiply(ref line, ref mag, out line);
                    for (int pos = 1; pos < count; ++pos)
                    {
                        node = list.AddAfter(node, line + node.Value);
                    }
                }
                node = node.Next;
            }
            Vector2D[] result = new Vector2D[list.Count];
            list.CopyTo(result, 0);
            return result;
        }
        /// <summary>
        /// Reduces a Polygon to the minumum number or vertexes need to represent it.  Does the opposite of Subdivide. 
        /// </summary>
        /// <param name="vertexes">The bloated vertex array.</param>
        /// <param name="minAngle">The minimum allowed angle anything less then or equal will be removed. </param>
        /// <returns>The reduced vertexes.</returns>
        public static Vector2D[] Reduce(Vector2D[] vertexes, Scalar minAngle)
        {
            if (vertexes == null) { throw new ArgumentNullException("vertexes"); }
            if (vertexes.Length < 2) { throw new ArgumentOutOfRangeException("vertexes"); }
            if (minAngle < 0) { throw new ArgumentOutOfRangeException("minAngle"); }

            List<Vector2D> list = new List<Vector2D>(vertexes.Length);
            Scalar lastAngle = (vertexes[0] - vertexes[vertexes.Length-1]).Angle;
            for (int index = 0; index < vertexes.Length; ++index)
            {
                int index2 = (index + 1) % vertexes.Length;
                Scalar angle = (vertexes[index2] - vertexes[index]).Angle;
                if (Math.Abs(lastAngle - angle) > minAngle)
                {
                    list.Add(vertexes[index]);
                }
                lastAngle = angle;
            }
            return list.ToArray();
        }
        /// <summary>
        /// Calculates the area of a polygon.
        /// </summary>
        /// <param name="vertices">The vertices of the polygon.</param>
        /// <returns>The area.</returns>
        public static Scalar GetArea(Vector2D[] vertices)
        {
            Scalar result;
            BoundingPolygon.GetArea(vertices,out result);
            return result;
        }
        /// <summary>
        /// Calculates the Centroid of a polygon.
        /// </summary>
        /// <param name="vertices">The vertices of the polygon.</param>
        /// <returns>The Centroid of a polygon.</returns>
        /// <remarks>
        /// This is Also known as Center of Gravity/Mass.
        /// </remarks>
        public static Vector2D GetCentroid(Vector2D[] vertices)
        {
            Vector2D result;
            BoundingPolygon.GetCentroid(vertices, out result);
            return result;
        }
        /// <summary>
        /// repositions the polygon so the Centroid is the origin.
        /// </summary>
        /// <param name="vertices">The vertices of the polygon.</param>
        /// <returns>The vertices of the polygon with the Centroid as the Origin.</returns>
        public static Vector2D[] MakeCentroidOrigin(Vector2D[] vertices)
        {
            Vector2D centroid;
            BoundingPolygon.GetCentroid(vertices, out centroid);
            return OperationHelper.ArrayRefOp<Vector2D, Vector2D, Vector2D>(vertices, ref centroid, Vector2D.Subtract);
        }

        #endregion
        #region fields
        private DistanceGrid grid; 
        #endregion
        #region constructors
        /// <summary>
        /// Creates a new Polygon Instance.
        /// </summary>
        /// <param name="vertexes">the vertexes that make up the shape of the Polygon</param>
        /// <param name="gridSpacing">
        /// How large a grid cell is. Usualy you will want at least 2 cells between major vertexes.
        /// The smaller this is the better precision you get, but higher cost in memory. 
        /// The larger the less precision and if it's to high collision detection may fail completely.
        public Polygon(Vector2D[] vertexes, Scalar gridSpacing)
            : this(vertexes, gridSpacing, InertiaOfPolygon(vertexes)) { }
        /// <summary>
        /// Creates a new Polygon Instance.
        /// </summary>
        /// <param name="vertexes">the vertexes that make up the shape of the Polygon</param>
        /// <param name="gridSpacing">
        /// How large a grid cell is. Usualy you will want at least 2 cells between major vertexes.
        /// The smaller this is the better precision you get, but higher cost in memory. 
        /// The larger the less precision and if it's to high collision detection may fail completely.
        /// </param>
        /// <param name="momentOfInertiaMultiplier">
        /// How hard it is to turn the shape. Depending on the construtor in the 
        /// Body this will be multiplied with the mass to determine the moment of inertia.
        /// </param>
        public Polygon(Vector2D[] vertexes, Scalar gridSpacing, Scalar momentOfInertiaMultiplier)
            : base(vertexes)
        {
            if (vertexes == null) { throw new ArgumentNullException("vertexes"); }
            if (vertexes.Length < 3) { throw new ArgumentException("too few", "vertexes"); }
            if (momentOfInertiaMultiplier <= 0) { throw new ArgumentOutOfRangeException("momentofInertiaMultiplier"); }
            if (gridSpacing <= 0) { throw new ArgumentOutOfRangeException("gridSpacing"); }
            this.grid = new DistanceGrid(this, gridSpacing);
            this.inertiaMultiplier = momentOfInertiaMultiplier;
        }
        private Polygon(Polygon copy)
            : base(copy)
        {
            this.grid = copy.grid;
        }
        
        #endregion
        #region properties
        public override bool CanGetIntersection
        {
            get { return true; }
        } 
        #endregion
        #region methods
        public override void CalcBoundingRectangle()
        {
            BoundingRectangle.FromVectors(vertexes, out rect);
        }
        public override void GetDistance(ref Vector2D point,out Scalar result)
        {
            BoundingPolygon.GetDistance(vertexes, ref point, out result);
        }
        public override bool TryGetIntersection(Vector2D vector, out IntersectionInfo info)
        {
            Vector2D local;
            Vector2D.Transform(ref matrix2DInv.VertexMatrix, ref vector, out local);
            if (grid.TryGetIntersection(local, out info))
            {
                Vector2D.Transform(ref matrix2D.NormalMatrix, ref info.normal, out info.normal);
                info.position = vector;
                return true;
            }
            return false;
        }
        public override Shape Duplicate()
        {
            return new Polygon(this);
        } 
        #endregion
    }
}