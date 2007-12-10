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
using AdvanceMath;
using AdvanceMath.Geometry2D;
using Physics2DDotNet.Math2D;
using System.Xml.Serialization;

namespace Physics2DDotNet
{
    /// <summary>
    /// A abstract class used to define the Shape of a Body.
    /// </summary>
    [Serializable]
    public abstract class Shape : IDuplicateable<Shape>
    {
        #region static fields
        protected readonly static Vector2D[] Empty = new Vector2D[0];
        #endregion
        #region static methods
        public static Scalar InertiaOfCylindricalShell(Scalar radius)
        {
            return radius * radius;
        }
        public static Scalar InertiaOfHollowCylinder(Scalar innerRadius, Scalar outerRadius)
        {
            return .5f * (innerRadius * innerRadius + outerRadius * outerRadius);
        }
        public static Scalar InertiaOfSolidCylinder(Scalar radius)
        {
            return .5f * (radius * radius);
        }
        public static Scalar InertiaOfRectangle(Scalar length, Scalar width)
        {
            return (1f / 12f) * (length * length + width * width);
        }
        public static Scalar InertiaOfSquare(Scalar sideLength)
        {
            return (1f / 6f) * (sideLength * sideLength);
        }
        public static Scalar InertiaOfPolygon(Vector2D[] vertexes)
        {
            if (vertexes == null) { throw new ArgumentNullException("vertexes"); }
            if (vertexes.Length == 0) { throw new ArgumentOutOfRangeException("vertexes"); }
            if (vertexes.Length == 1) { return 0; }

            Scalar denom = 0;
            Scalar numer = 0;
            Scalar a, b, c, d;
            Vector2D v1, v2;
            v1 = vertexes[vertexes.Length - 1];
            for (int index = 0; index < vertexes.Length; index++, v1 = v2)
            {
                v2 = vertexes[index];
                Vector2D.Dot(ref v2, ref v2, out a);
                Vector2D.Dot(ref v2, ref v1, out b);
                Vector2D.Dot(ref v1, ref v1, out c);
                Vector2D.ZCross(ref v1, ref v2, out d);
                d = Math.Abs(d);
                numer += d;
                denom += (a + b + c) * d;
            }
            return denom / (numer * 6);
        }
        public static Vector2D[] CreateCircle(Scalar radius, int vertexCount)
        {
            if (radius <= 0) { throw new ArgumentOutOfRangeException("radius", "Must be greater then zero."); }
            if (vertexCount < 3) { throw new ArgumentOutOfRangeException("vertexCount", "Must be equal or greater then 3"); }
            Vector2D[] result = new Vector2D[vertexCount];
            Scalar angleIncrement = MathHelper.TwoPi / vertexCount;
            for (int index = 0; index < vertexCount; ++index)
            {
                Scalar angle = angleIncrement * index;
                Vector2D.FromLengthAndAngle(ref radius, ref angle, out result[index]);
            }
            return result;
        }
        protected static void GetProjectedBounds(Vector2D[] vertexes, int offset, int length, Vector2D tangent, out Scalar min, out Scalar max)
        {
            Scalar value;
            Vector2D.Dot(ref vertexes[offset], ref tangent, out value);
            min = value;
            max = value;
            for (int index = 1; index < length; ++index)
            {
                Vector2D.Dot(ref vertexes[index + offset], ref tangent, out value);
                if (value > max)
                {
                    max = value;
                }
                else if (value < min)
                {
                    min = value;
                }
            }
        }
        #endregion
        #region fields
        object tag;
        protected Matrix2D matrix2D;
        protected Matrix2D matrix2DInv;
        protected BoundingRectangle rect;
        protected Scalar inertiaMultiplier;
        protected Vector2D[] originalVertexes;
        protected Vector2D[] vertexes;
        private Body parent;
        bool ignoreVertexes;


        #endregion
        #region constructors
        protected Shape(Vector2D[] vertexes, Scalar momentOfInertiaMultiplier)
        {
            if (vertexes == null) { throw new ArgumentNullException("vertexes"); }
            if (momentOfInertiaMultiplier <= 0) { throw new ArgumentOutOfRangeException("momentOfInertiaMultiplier"); }
            this.matrix2D = Matrix2D.Identity;
            this.matrix2DInv = Matrix2D.Identity;
            this.originalVertexes = vertexes;
            this.vertexes = (Vector2D[])vertexes.Clone();
            this.inertiaMultiplier = momentOfInertiaMultiplier;
        }
        protected Shape(Shape copy)
        {
            if (copy == null) { throw new ArgumentNullException("copy"); }
            this.ignoreVertexes = copy.ignoreVertexes;
            this.matrix2D = copy.matrix2D;
            this.matrix2DInv = copy.matrix2DInv;
            this.inertiaMultiplier = copy.inertiaMultiplier;
            this.rect = copy.rect;
            if (copy.tag is ICloneable)
            {
                this.tag = ((ICloneable)copy.tag).Clone();
            }
            else
            {
                this.tag = copy.tag;
            }
            this.originalVertexes = copy.originalVertexes;
            this.vertexes = (Vector2D[])copy.vertexes.Clone();
        }
        #endregion
        #region properties
        public abstract bool CanGetCentroid { get;}
        public abstract bool CanGetArea { get;}
        public abstract bool CanGetInertia { get;}

        /// <summary>
        /// Gets and Sets if this shape's Vertexes will not be used in collision detection.
        /// </summary>
        public bool IgnoreVertexes
        {
            get { return ignoreVertexes; }
            set { ignoreVertexes = value; }
        }
        public abstract bool CanGetIntersection { get;}
        public abstract bool CanGetDragInfo { get;}
        public abstract bool CanGetDistance { get;}
        public abstract bool CanGetCustomIntersection { get;}
        /// <summary>
        /// Gets if this detects collisions only with bounding boxes 
        /// and if it does then only bodies colliding it will also generate collision events as well.
        /// if this is true it can allow you to write your own collision Solver just for this Shape. 
        /// Or you can use this to do clipping.
        /// </summary>
        public abstract bool BroadPhaseDetectionOnly { get;}
        /// <summary>
        /// Gets the Body this Shape is part of.
        /// </summary>
        public Body Parent
        {
            get { return parent; }
        }
        /// <summary>
        /// Gets the Moment of Inertia Multiplier. Which is the ratio of inertia to mass of a Body.
        /// </summary>
        public Scalar MomentofInertiaMultiplier
        {
            get { return inertiaMultiplier; }
        }
        /// <summary>
        /// Gets the Bounding Rectangle of the Shape. (This is only calculated when ApplyMatrix() is called.)
        /// </summary>
        public BoundingRectangle Rectangle
        {
            get { return rect; }
        }
        /// <summary>
        /// Gets and Sets a User Defined Object.
        /// </summary>
        [XmlIgnore]
        public object Tag
        {
            get { return tag; }
            set { tag = value; }
        }
        /// <summary>
        /// Gets The current Matrix being Applied to the Shape.
        /// </summary>
        public Matrix2D Matrix
        {
            get { return matrix2D; }
        }
        /// <summary>
        /// Gets The Inverse of the current Matrix being Applied to the Shape.
        /// </summary>
        public Matrix2D MatrixInv
        {
            get { return matrix2DInv; }
        }
        /// <summary>
        /// Gets the original (body/local) Vertices with the origin being the center of the Body.
        /// </summary>
        public Vector2D[] OriginalVertices
        {
            get { return originalVertexes; }
        }
        /// <summary>
        /// Gets the transformed (world) Vertices with the origin being the center of the world.
        /// </summary>
        public Vector2D[] Vertices
        {
            get { return vertexes; }
        }
        #endregion
        #region methods
        protected abstract void CalcBoundingRectangle();
        public virtual void ApplyMatrix(ref Matrix2D matrix)
        {
            this.matrix2D = matrix;
            Matrix2D.Invert(ref matrix, out matrix2DInv);
            ApplyMatrixToVertexes();
            CalcBoundingRectangle();
        }
        protected void ApplyMatrixToVertexes()
        {
            Matrix3x3 matrix = matrix2D.VertexMatrix;
            for (int index = 0; index < originalVertexes.Length; ++index)
            {
                Vector2D.Transform(ref matrix, ref originalVertexes[index], out vertexes[index]);
            }
        }
        public virtual void UpdateTime(TimeStep step) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="point">(In World Coordinates)</param>
        /// <param name="info"></param>
        /// <returns></returns>
        public abstract bool TryGetIntersection(Vector2D point, out IntersectionInfo info);
        public abstract bool TryGetCustomIntersection(Body other, out object customIntersectionInfo);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="point">(In World Coordinates)</param>
        /// <param name="result"></param>
        public abstract void GetDistance(ref Vector2D point, out Scalar result);
        public abstract DragInfo GetDragInfo(Vector2D tangent);



        /// <summary>
        /// Gets the Centroid of the Shape without the Matrix Applied. (In Body Coordinates)
        /// </summary>
        /// <returns>the Centroid of the Shape without the Matrix Applied.</returns>
        public abstract Vector2D GetCentroid();
        public abstract Scalar GetArea();
        public abstract Scalar GetInertia();

            

        public object Clone()
        {
            return Duplicate();
        }

        internal void OnAdded(Body parent)
        {
            if (this.parent != null) { throw new InvalidOperationException("must be orphan"); }
            this.parent = parent;
        }
        internal void OnRemoved()
        {
            this.parent = null;
        }

        public abstract Shape Duplicate();
        #endregion
    }
}