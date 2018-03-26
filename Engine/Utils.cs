using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using Rectangle = Basics.Rectangle;
using Basics;

namespace Engine
{
    public static class Utils
    {
        #region Vectors

        public static float Distance(this Vector2 _v, IPosition _u) => Basics.Utils.EuclideanDistance(_v.X, _v.Y, _u.X, _u.Y);
        public static float Distance(this Vector2 _v, Vector2 _u) => Basics.Utils.EuclideanDistance(_v.X, _v.Y, _u.X, _u.Y);
        public static float Distance(this Vector2 _v, float _x, float _y) => Basics.Utils.EuclideanDistance(_v.X, _v.Y, _x, _y);
        public static float DistanceSquared(this Vector2 _v, IPosition _u) => Basics.Utils.EuclideanDistanceSquared(_v.X, _v.Y, _u.X, _u.Y);
        public static float DistanceSquared(this Vector2 _v, Vector2 _u) => Basics.Utils.EuclideanDistanceSquared(_v.X, _v.Y, _u.X, _u.Y);
        public static float DistanceSquared(this Vector2 _v, float _x, float _y) => Basics.Utils.EuclideanDistanceSquared(_v.X, _v.Y, _x, _y);
        public static Vector2 Midpoint(this Vector2 a, IPosition b) => Basics.Utils.Midpoint(a.X, a.Y, b.X, b.Y).ToVector2();
        public static Vector2 Midpoint(this Vector2 a, Vector2 b) => Basics.Utils.Midpoint(a.X, a.Y, b.X, b.Y).ToVector2();
        public static Vector2 ToVector2(this (float X, float Y) _tuple) => new Vector2(_tuple.X, _tuple.Y);

        public static Vector2 Vector2(float _radians, float _length=1) => _length * new Vector2((float)Math.Cos(_radians), (float)Math.Sin(_radians));
        public static Vector2 RandomUnitVector2() => Vector2((float)(Basics.Utils.RandomDouble() * Math.PI * 2));

        public static Vector3 To3D(this Vector2 _vector, float _z = 0) => new Vector3(_vector.X, _vector.Y, _z);
        public static Vector2 To2D(this Vector3 _vector) => new Vector2(_vector.X, _vector.Y);

        public static bool IsNaN(this Vector3 _vector) => float.IsNaN(_vector.X) || float.IsNaN(_vector.Y) || float.IsNaN(_vector.Z);
        public static bool IsNaN(this Vector2 _vector) => float.IsNaN(_vector.X) || float.IsNaN(_vector.Y);

        /// <summary>
        /// Determines the angle of the given vector (in radians)
        /// </summary>
        /// <param name="_vector">the vector whose angle is being calculated.</param>
        /// <returns>The angle (in radians) of the given vector (or null if the vector is zero)</returns>
        public static float? Radians(this Vector2 _vector)
        {
            if (_vector == OpenTK.Vector2.Zero)
                return null;
            return (float)Math.Atan2(_vector.Y, _vector.X);
        }
        public static float? Radians(this Vector3 _vector) => _vector.To2D().Radians();

        public static void RotateRelative(this List<Vector3> _vertices, float _radians, Vector3 _center)
        {
            for (var i = 0; i < _vertices.Count; i++)
                _vertices[i] = _vertices[i].Rotate(_radians, _center);
        }
        public static Vector3 Rotate(this Vector3 _vertex, float _radians, Vector3 _center) => Rotate(_vertex.X, _vertex.Y, _radians, _center.X, _center.Y).To3D();
        public static Vector2 Rotate(this IPosition _vertex, float _radians, IPosition _center=null) => Rotate(_vertex.X, _vertex.Y, _radians, _center?.X ?? 0, _center?.Y ?? 0);
        public static Vector2 Rotate(this Vector2 _vertex, float _radians, IPosition _center=null) => Rotate(_vertex.X, _vertex.Y, _radians, _center?.X ?? 0, _center?.Y ?? 0);
        public static Vector2 Rotate(this IPosition _vertex, float _radians, Vector2 _center) => Rotate(_vertex.X, _vertex.Y, _radians, _center.X, _center.Y);
        public static Vector2 Rotate(this Vector2 _vertex, float _radians, Vector2 _center) => Rotate(_vertex.X, _vertex.Y, _radians, _center.X, _center.Y);
        public static Vector2 Rotate(this IPosition _vertex, float _radians) => Rotate(_vertex.X, _vertex.Y, _radians);
        public static Vector2 Rotate(this Vector2 _vertex, float _radians) => Rotate(_vertex.X, _vertex.Y, _radians);
        public static Vector2 Rotate(float _vertexX, float _vertexY, float _radians, float _centerX=0, float _centerY=0)
        {
            var diffX = _vertexX - _centerX;
            var diffY = _vertexY - _centerY;
            return new Vector2((float)(diffX * Math.Cos(_radians) - diffY * Math.Sin(_radians)) + _centerX,
                               (float)(diffY * Math.Cos(_radians) + diffX * Math.Sin(_radians)) + _centerY);
        }
        
        public static Vector3 Avg(this IEnumerable<Vector3> _vectors)
        {
            var sum = Vector3.Zero;
            int count = 0;
            foreach (var v in _vectors)
            {
                sum += v;
                count++;
            }
            return sum / count;
        }
        public static Vector2 Avg(this IEnumerable<Vector2> _vectors)
        {
            var sum = OpenTK.Vector2.Zero;
            int count = 0;
            foreach (var v in _vectors)
            {
                sum += v;
                count++;
            }
            return sum / count;
        }

        public static Vector3 Sum(this IEnumerable<Vector3> _vectors)
        {
            var sum = Vector3.Zero;
            foreach (var _vector in _vectors)
                sum += _vector;
            return sum;
        }
        public static Vector2 Sum(this IEnumerable<Vector2> _vectors)
        {
            var sum = OpenTK.Vector2.Zero;
            foreach (var _vector in _vectors)
                sum += _vector;
            return sum;
        }

        #endregion


        #region Points & lines

        /// <summary>
        /// Determines the point that is closest to 'point' on line [a, b]
        /// </summary>
        /// <param name="point">point to find the nearest point to</param>
        /// <param name="a">line point a</param>
        /// <param name="b">line point b</param>
        /// <returns>The point on the line [a, b] that is closest to 'point'></returns>
        public static Vector3 NearestPointOnLine(this Vector3 point, Vector3 a, Vector3 b) => PointOnLineNearestPoint(point.X, point.Y, a.X, a.Y, b.X, b.Y).To3D();
        public static Vector2 NearestPointOnLine(this IPosition point, IPosition a, IPosition b) => PointOnLineNearestPoint(point.X, point.Y, a.X, a.Y, b.X, b.Y);
        public static Vector2 NearestPointOnLine(this Vector2 point, IPosition a, IPosition b) => PointOnLineNearestPoint(point.X, point.Y, a.X, a.Y, b.X, b.Y);
        public static Vector2 NearestPointOnLine(this IPosition point, Vector2 a, IPosition b) => PointOnLineNearestPoint(point.X, point.Y, a.X, a.Y, b.X, b.Y);
        public static Vector2 NearestPointOnLine(this Vector2 point, Vector2 a, IPosition b) => PointOnLineNearestPoint(point.X, point.Y, a.X, a.Y, b.X, b.Y);
        public static Vector2 NearestPointOnLine(this IPosition point, IPosition a, Vector2 b) => PointOnLineNearestPoint(point.X, point.Y, a.X, a.Y, b.X, b.Y);
        public static Vector2 NearestPointOnLine(this Vector2 point, IPosition a, Vector2 b) => PointOnLineNearestPoint(point.X, point.Y, a.X, a.Y, b.X, b.Y);
        public static Vector2 NearestPointOnLine(this IPosition point, Vector2 a, Vector2 b) => PointOnLineNearestPoint(point.X, point.Y, a.X, a.Y, b.X, b.Y);
        public static Vector2 NearestPointOnLine(this Vector2 point, Vector2 a, Vector2 b) => PointOnLineNearestPoint(point.X, point.Y, a.X, a.Y, b.X, b.Y);
        public static Vector2 PointOnLineNearestPoint(float pointX, float pointY, float aX, float aY, float bX, float bY)
        {
            if (aX == bX)
                return new Vector2(aX, Basics.Utils.Clamp(pointY, Basics.Utils.Min(aY, bY), Basics.Utils.Max(aY, bY)));

            var m = (bY - aY) / (bX - aX);
            var f = aY - m * aX;
            var q = new Vector2();
            q.X = (m * (pointY - f) + pointX) / (m * m + 1);
            if (m == 0)
                q.Y = pointY;
            else
                q.Y = -1 / m * (q.X - pointX) + pointY;
            return q;
        }

        /// <summary>
        /// Determines the point that is closest to 'point' on segment [a, b]
        /// </summary>
        /// <param name="point">point to find the nearest point to</param>
        /// <param name="a">segment starting point</param>
        /// <param name="b">segment ending point</param>
        /// <returns>The point on the segment [a, b] that is closest to 'point' (it will always lie on the segment [a, b])</returns>
        public static Vector3 NearestPointOnSegment(this Vector3 point, Vector3 a, Vector3 b) => PointOnSegmentNearestPoint(point.X, point.Y, a.X, a.Y, b.X, b.Y).To3D();
        public static Vector2 NearestPointOnSegment(this IPosition point, IPosition a, IPosition b) => PointOnSegmentNearestPoint(point.X, point.Y, a.X, a.Y, b.X, b.Y);
        public static Vector2 NearestPointOnSegment(this Vector2 point, IPosition a, IPosition b) => PointOnSegmentNearestPoint(point.X, point.Y, a.X, a.Y, b.X, b.Y);
        public static Vector2 NearestPointOnSegment(this IPosition point, Vector2 a, IPosition b) => PointOnSegmentNearestPoint(point.X, point.Y, a.X, a.Y, b.X, b.Y);
        public static Vector2 NearestPointOnSegment(this Vector2 point, Vector2 a, IPosition b) => PointOnSegmentNearestPoint(point.X, point.Y, a.X, a.Y, b.X, b.Y);
        public static Vector2 NearestPointOnSegment(this IPosition point, IPosition a, Vector2 b) => PointOnSegmentNearestPoint(point.X, point.Y, a.X, a.Y, b.X, b.Y);
        public static Vector2 NearestPointOnSegment(this Vector2 point, IPosition a, Vector2 b) => PointOnSegmentNearestPoint(point.X, point.Y, a.X, a.Y, b.X, b.Y);
        public static Vector2 NearestPointOnSegment(this IPosition point, Vector2 a, Vector2 b) => PointOnSegmentNearestPoint(point.X, point.Y, a.X, a.Y, b.X, b.Y);
        public static Vector2 NearestPointOnSegment(this Vector2 point, Vector2 a, Vector2 b) => PointOnSegmentNearestPoint(point.X, point.Y, a.X, a.Y, b.X, b.Y);
        public static Vector2 PointOnSegmentNearestPoint(float pointX, float pointY, float aX, float aY, float bX, float bY)
        {
            var q = PointOnLineNearestPoint(pointX, pointY, aX, aY, bX, bY);
            q.X = Basics.Utils.Clamp(q.X, Math.Min(aX, bX), Math.Max(aX, bX));
            q.Y = Basics.Utils.Clamp(q.Y, Math.Min(aY, bY), Math.Max(aY, bY));
            return q;
        }

        /// <summary>
        /// Finds the distance between 'point' and the nearest point on the segment [a, b]
        /// </summary>
        /// <param name="point">point for which to find the distance to [a, b]</param>
        /// <param name="a">segment starting point</param>
        /// <param name="b">segment ending point</param>
        /// <returns>Distance between 'point' and the nearest point on the segment [a, b]</returns>
        public static float DistanceToSegment(this Vector3 point, Vector3 a, Vector3 b) => DistanceToSegment(point.X, point.Y, a.X, a.Y, b.X, b.Y);
        public static float DistanceToSegment(this IPosition point, IPosition a, IPosition b) => DistanceToSegment(point.X, point.Y, a.X, a.Y, b.X, b.Y);
        public static float DistanceToSegment(this Vector2 point, IPosition a, IPosition b) => DistanceToSegment(point.X, point.Y, a.X, a.Y, b.X, b.Y);
        public static float DistanceToSegment(this IPosition point, Vector2 a, IPosition b) => DistanceToSegment(point.X, point.Y, a.X, a.Y, b.X, b.Y);
        public static float DistanceToSegment(this Vector2 point, Vector2 a, IPosition b) => DistanceToSegment(point.X, point.Y, a.X, a.Y, b.X, b.Y);
        public static float DistanceToSegment(this IPosition point, IPosition a, Vector2 b) => DistanceToSegment(point.X, point.Y, a.X, a.Y, b.X, b.Y);
        public static float DistanceToSegment(this Vector2 point, IPosition a, Vector2 b) => DistanceToSegment(point.X, point.Y, a.X, a.Y, b.X, b.Y);
        public static float DistanceToSegment(this IPosition point, Vector2 a, Vector2 b) => DistanceToSegment(point.X, point.Y, a.X, a.Y, b.X, b.Y);
        public static float DistanceToSegment(this Vector2 point, Vector2 a, Vector2 b) => DistanceToSegment(point.X, point.Y, a.X, a.Y, b.X, b.Y);
        public static float DistanceToSegment(float pointX, float pointY, float aX, float aY, float bX, float bY)
            => PointOnSegmentNearestPoint(pointX, pointY, aX, aY, bX, bY).Distance(pointX, pointY);

        /// <summary>
        /// Finds the distance between 'point' and the nearest point on the line [a, b]
        /// </summary>
        /// <param name="point">point for which to find the distance to [a, b]</param>
        /// <param name="a">segment starting point</param>
        /// <param name="b">segment ending point</param>
        /// <returns>Distance between 'point' and the nearest point on the line [a, b]</returns>
        public static float DistanceToLine(this Vector3 point, Vector3 a, Vector3 b) => DistanceToLine(point.X, point.Y, a.X, a.Y, b.X, b.Y);
        public static float DistanceToLine(this IPosition point, IPosition a, IPosition b) => DistanceToLine(point.X, point.Y, a.X, a.Y, b.X, b.Y);
        public static float DistanceToLine(this Vector2 point, IPosition a, IPosition b) => DistanceToLine(point.X, point.Y, a.X, a.Y, b.X, b.Y);
        public static float DistanceToLine(this IPosition point, Vector2 a, IPosition b) => DistanceToLine(point.X, point.Y, a.X, a.Y, b.X, b.Y);
        public static float DistanceToLine(this Vector2 point, Vector2 a, IPosition b) => DistanceToLine(point.X, point.Y, a.X, a.Y, b.X, b.Y);
        public static float DistanceToLine(this IPosition point, IPosition a, Vector2 b) => DistanceToLine(point.X, point.Y, a.X, a.Y, b.X, b.Y);
        public static float DistanceToLine(this Vector2 point, IPosition a, Vector2 b) => DistanceToLine(point.X, point.Y, a.X, a.Y, b.X, b.Y);
        public static float DistanceToLine(this IPosition point, Vector2 a, Vector2 b) => DistanceToLine(point.X, point.Y, a.X, a.Y, b.X, b.Y);
        public static float DistanceToLine(this Vector2 point, Vector2 a, Vector2 b) => DistanceToLine(point.X, point.Y, a.X, a.Y, b.X, b.Y);
        public static float DistanceToLine(float pointX, float pointY, float aX, float aY, float bX, float bY)
            => PointOnLineNearestPoint(pointX, pointY, aX, aY, bX, bY).Distance(pointX, pointY);

        /// <summary>
        /// Finds the squared distance between 'point' and the nearest point on the segment [a, b]
        /// </summary>
        /// <param name="point">point for which to find the distance to [a, b]</param>
        /// <param name="a">segment starting point</param>
        /// <param name="b">segment ending point</param>
        /// <returns>Squared distance between 'point' and the nearest point on the segment [a, b]</returns>
        public static float DistanceSquaredToSegment(this Vector3 point, Vector3 a, Vector3 b) => DistanceSquaredToSegment(point.X, point.Y, a.X, a.Y, b.X, b.Y);
        public static float DistanceSquaredToSegment(this IPosition point, IPosition a, IPosition b) => DistanceSquaredToSegment(point.X, point.Y, a.X, a.Y, b.X, b.Y);
        public static float DistanceSquaredToSegment(this Vector2 point, IPosition a, IPosition b) => DistanceSquaredToSegment(point.X, point.Y, a.X, a.Y, b.X, b.Y);
        public static float DistanceSquaredToSegment(this IPosition point, Vector2 a, IPosition b) => DistanceSquaredToSegment(point.X, point.Y, a.X, a.Y, b.X, b.Y);
        public static float DistanceSquaredToSegment(this Vector2 point, Vector2 a, IPosition b) => DistanceSquaredToSegment(point.X, point.Y, a.X, a.Y, b.X, b.Y);
        public static float DistanceSquaredToSegment(this IPosition point, IPosition a, Vector2 b) => DistanceSquaredToSegment(point.X, point.Y, a.X, a.Y, b.X, b.Y);
        public static float DistanceSquaredToSegment(this Vector2 point, IPosition a, Vector2 b) => DistanceSquaredToSegment(point.X, point.Y, a.X, a.Y, b.X, b.Y);
        public static float DistanceSquaredToSegment(this IPosition point, Vector2 a, Vector2 b) => DistanceSquaredToSegment(point.X, point.Y, a.X, a.Y, b.X, b.Y);
        public static float DistanceSquaredToSegment(this Vector2 point, Vector2 a, Vector2 b) => DistanceSquaredToSegment(point.X, point.Y, a.X, a.Y, b.X, b.Y);
        public static float DistanceSquaredToSegment(float pointX, float pointY, float aX, float aY, float bX, float bY)
            => PointOnSegmentNearestPoint(pointX, pointY, aX, aY, bX, bY).DistanceSquared(pointX, pointY);

        /// <summary>
        /// Finds the squared distance between 'point' and the nearest point on the line [a, b]
        /// </summary>
        /// <param name="point">point for which to find the distance to [a, b]</param>
        /// <param name="a">segment starting point</param>
        /// <param name="b">segment ending point</param>
        /// <returns>Squared distance between 'point' and the nearest point on the line [a, b]</returns>
        public static float DistanceSquaredToLine(this Vector3 point, Vector3 a, Vector3 b) => DistanceSquaredToLine(point.X, point.Y, a.X, a.Y, b.X, b.Y);
        public static float DistanceSquaredToLine(this IPosition point, IPosition a, IPosition b) => DistanceSquaredToLine(point.X, point.Y, a.X, a.Y, b.X, b.Y);
        public static float DistanceSquaredToLine(this Vector2 point, IPosition a, IPosition b) => DistanceSquaredToLine(point.X, point.Y, a.X, a.Y, b.X, b.Y);
        public static float DistanceSquaredToLine(this IPosition point, Vector2 a, IPosition b) => DistanceSquaredToLine(point.X, point.Y, a.X, a.Y, b.X, b.Y);
        public static float DistanceSquaredToLine(this Vector2 point, Vector2 a, IPosition b) => DistanceSquaredToLine(point.X, point.Y, a.X, a.Y, b.X, b.Y);
        public static float DistanceSquaredToLine(this IPosition point, IPosition a, Vector2 b) => DistanceSquaredToLine(point.X, point.Y, a.X, a.Y, b.X, b.Y);
        public static float DistanceSquaredToLine(this Vector2 point, IPosition a, Vector2 b) => DistanceSquaredToLine(point.X, point.Y, a.X, a.Y, b.X, b.Y);
        public static float DistanceSquaredToLine(this IPosition point, Vector2 a, Vector2 b) => DistanceSquaredToLine(point.X, point.Y, a.X, a.Y, b.X, b.Y);
        public static float DistanceSquaredToLine(this Vector2 point, Vector2 a, Vector2 b) => DistanceSquaredToLine(point.X, point.Y, a.X, a.Y, b.X, b.Y);
        public static float DistanceSquaredToLine(float pointX, float pointY, float aX, float aY, float bX, float bY)
            => PointOnLineNearestPoint(pointX, pointY, aX, aY, bX, bY).DistanceSquared(pointX, pointY);

        public static bool IsRightOfLine(this Vector3 point, Vector3 a, Vector3 b) => PointIsRightOfLine(point.X, point.Y, a.X, a.Y, b.X, b.Y);
        public static bool IsRightOfLine(this Vector2 point, Vector2 a, Vector2 b) => PointIsRightOfLine(point.X, point.Y, a.X, a.Y, b.X, b.Y);
        public static bool PointIsRightOfLine(float pointX, float pointY, float aX, float aY, float bX, float bY) => PointSideOfLine(pointX, pointY, aX, aY, bX, bY) < 0;

        public static bool IsLeftOfLine(this Vector3 point, Vector3 a, Vector3 b) => PointIsLeftOfLine(point.X, point.Y, a.X, a.Y, b.X, b.Y);
        public static bool IsLeftOfLine(this Vector2 point, Vector2 a, Vector2 b) => PointIsLeftOfLine(point.X, point.Y, a.X, a.Y, b.X, b.Y);
        public static bool PointIsLeftOfLine(float pointX, float pointY, float aX, float aY, float bX, float bY) => PointSideOfLine(pointX, pointY, aX, aY, bX, bY) > 0;

        public static bool Colinear(Vector3 a, Vector3 b, Vector3 c) => Colinear(a.X, a.Y, b.X, b.Y, c.X, c.Y);
        public static bool Colinear(Vector2 a, Vector2 b, Vector2 c) => Colinear(a.X, a.Y, b.X, b.Y, c.X, c.Y);
        public static bool Colinear(float aX, float aY, float bX, float bY, float cX, float cY) => PointSideOfLine(aX, aY, bX, bY, cX, cY) == 0;

        public static int SideOfLine(this Vector3 point, Vector3 a, Vector3 b) => PointSideOfLine(point.X, point.Y, a.X, a.Y, b.X, b.Y);
        public static int SideOfLine(this Vector2 point, Vector2 a, Vector2 b) => PointSideOfLine(point.X, point.Y, a.X, a.Y, b.X, b.Y);
        public static int PointSideOfLine(float pointX, float pointY, float aX, float aY, float bX, float bY) => Math.Sign((bX - aX) * (pointY - aY) - (bY - aY) * (pointX - aX));

        public static Vector3 PointOnLineAtX(Vector3 a, Vector3 b, float x)
        {
            var diffX = b.X - a.X;
            return diffX == 0 ? (a + b) / 2 : new Vector3(x, (x - a.X) / diffX * (b.Y - a.Y) + a.Y, 0);
        }
        public static Vector3 PointOnLineAtY(Vector3 a, Vector3 b, float y)
        {
            var diffY = b.Y - a.Y;
            return diffY == 0 ? (a + b) / 2 : new Vector3((y - a.Y) / diffY * (b.X - a.X) + a.X, y, 0);
        }

        /// <summary>
        /// Returns a number between 0 and 1 that represents the percentage between points 'a' -> 'b' that 'point' is relative to 'a'.
        /// (point == a will return 0, point == b will return 1)
        /// </summary>
        /// <param name="point">point to see how far along the line it is</param>
        /// <param name="a">first point on line</param>
        /// <param name="b">second point on line</param>
        /// <returns>Returns a number between 0 and 1 that represents the percentage along line 'a' -> 'b' that 'point' is relative to 'a'</returns>
        public static float PercentAlongLine(Vector3 point, Vector3 a, Vector3 b) => PercentAlongLine(point.X, point.Y, a.X, a.Y, b.X, b.Y);
        public static float PercentAlongLine(Vector2 point, Vector2 a, Vector2 b) => PercentAlongLine(point.X, point.Y, a.X, a.Y, b.X, b.Y);
        public static float PercentAlongLine(float pointX, float pointY, float aX, float aY, float bX, float bY)
        {
            if (!Colinear(pointX, pointY, aX, aY, bX, bY))
            {
                var temp = PointOnLineNearestPoint(pointX, pointY, aX, aY, bX, bY);
                pointX = temp.X;
                pointY = temp.Y;
            }
            return aX != bX
                ? (pointX - aX) / (bX - aX)
                : aY != bY
                    ? (pointY - aY) / (bY - aY)
                    : 0;
        }

        /// <summary>
        /// Returns a number between 0 and 1 that represents the percentage along line 'a' -> 'b' that 'point' is relative to 'a'.
        /// (point == a will return 0, point == b will return 1)
        /// </summary>
        /// <param name="point">point to see how far along the line it is</param>
        /// <param name="a">first point on segment</param>
        /// <param name="b">second point on lisegmentne</param>
        /// <returns>Returns a number between 0 and 1 that represents the percentage along segment 'a' -> 'b' that 'point' is relative to 'a'</returns>
        public static float PercentAlongSegment(Vector3 point, Vector3 a, Vector3 b) => PercentAlongSegment(point.X, point.Y, a.X, a.Y, b.X, b.Y);
        public static float PercentAlongSegment(Vector2 point, Vector2 a, Vector2 b) => PercentAlongSegment(point.X, point.Y, a.X, a.Y, b.X, b.Y);
        public static float PercentAlongSegment(float pointX, float pointY, float aX, float aY, float bX, float bY) => Basics.Utils.Clamp(PercentAlongLine(pointX, pointY, aX, aY, bX, bY), 0, 1);

        #endregion


        #region Points & triangles

        /// <summary>
        /// Checks whether a given point is inside a given triangle.
        /// </summary>
        /// <param name="point">point that is being checked</param>
        /// <param name="a">first point of triangle</param>
        /// <param name="b">second point of triangle</param>
        /// <param name="c">third point of triangle</param>
        /// <returns>Whether the given point is inside the given triangle.</returns>
        public static bool PointInTriangle(Vector3 point, Vector3 a, Vector3 b, Vector3 c)
        {
            var bc = point.IsRightOfLine(b, c);
            return point.IsRightOfLine(a, b) == bc && bc == point.IsRightOfLine(c, a);
        }

        /// <summary>
        /// Checks whether a triangle collides with another triangle.
        /// </summary>
        /// <param name="a">first triangle</param>
        /// <param name="b">second triangle</param>
        /// <returns>Whether the two given triangles collide.</returns>
        public static bool TrianglesCollide(List<Vector3> a, List<Vector3> b)
        {
            if (a == null || b == null)
                return false;

            if (a.Count != 3 || b.Count != 3)
                throw new ArgumentException("A triangle cannot have more than 3 vertices");

            var aRectangle = BoundingBox.RectangleFromPoints(a);
            var bRectangle = BoundingBox.RectangleFromPoints(b);
            if (!aRectangle.Collides(bRectangle))
                return false;

            //Check if center of each triangle is in the other
            if (PointInTriangle(a.Avg(), b[0], b[1], b[2]) || PointInTriangle(b.Avg(), a[0], a[1], a[2]))
                return true;

            for (var i = 0; i < 3; i++)
            {
                var i1 = (i + 1) % 3;
                for (var j = 0; j < 3; j++)
                    if (SegmentsIntersect(a[i], a[i1], b[j], b[(j + 1) % 3]))
                        return true;
            }
            return false;
        }

        #endregion


        #region Line intersection

        private static bool OnSegment(Vector3 p, Vector3 q, Vector3 r)
            => OnSegment(p.X, p.Y, q.X, q.Y, r.X, r.Y);
        private static bool OnSegment(float px, float py, float qx, float qy, float rx, float ry)
            => qx <= Math.Max(px, rx) && qx >= Math.Min(px, rx) &&
               qy <= Math.Max(py, ry) && qy >= Math.Min(py, ry);

        private enum TripletOrientation
        {
            Colinear,
            Clockwise,
            Counterclockwise
        }
        private static TripletOrientation GetTripletOrientation(Vector3 p, Vector3 q, Vector3 r)
            => GetTripletOrientation(p.X, p.Y, q.X, q.Y, r.X, r.Y);
        private static TripletOrientation GetTripletOrientation(float px, float py, float qx, float qy, float rx, float ry)
        {
            var val = (qy - py) * (rx - qx) - (qx - px) * (ry - qy);
            if (val == 0) return TripletOrientation.Colinear;
            return val > 0 ? TripletOrientation.Clockwise : TripletOrientation.Counterclockwise;
        }

        /// <summary>
        /// Checks if two given line segments (a1, a2) and (b1, b2) intersect.
        /// </summary>
        /// <param name="a1">1st point of 1st segment</param>
        /// <param name="a2">2nd point of 1st segment</param>
        /// <param name="b1">1st point of 2nd segment</param>
        /// <param name="b2">2nd point of 2nd segment</param>
        /// <returns>Whether the two line segments (a1, a2) and (b1, b2) intersect.</returns>
        public static bool SegmentsIntersect(Vector3 a1, Vector3 a2, Vector3 b1, Vector3 b2)
            => SegmentsIntersect(a1.X, a1.Y, a2.X, a2.Y, b1.X, b1.Y, b2.X, b2.Y);

        /// <summary>
        /// Checks if two given line segments (a1, a2) and (b1, b2) intersect.
        /// </summary>
        /// <param name="a1x">x-position of 1st point of 1st segment</param>
        /// <param name="a1y">y-position of 1st point of 1st segment</param>
        /// <param name="a2x">x-position of 2nd point of 1st segment</param>
        /// <param name="a2y">y-position of 2nd point of 1st segment</param>
        /// <param name="b1x">x-position of 1st point of 2nd segment</param>
        /// <param name="b1y">y-position of 1st point of 2nd segment</param>
        /// <param name="b2x">x-position of 2nd point of 2nd segment</param>
        /// <param name="b2y">y-position of 2nd point of 2nd segment</param>
        /// <returns>Whether the two line segments (a1, a2) and (b1, b2) intersect.</returns>
        public static bool SegmentsIntersect(float a1x, float a1y, float a2x, float a2y, float b1x, float b1y, float b2x, float b2y)
        { //https://www.geeksforgeeks.org/check-if-two-given-line-segments-intersect/
            var o1 = GetTripletOrientation(a1x, a1y, a2x, a2y, b1x, b1y);
            var o2 = GetTripletOrientation(a1x, a1y, a2x, a2y, b2x, b2y);
            var o3 = GetTripletOrientation(b1x, b1y, b2x, b2y, a1x, a1y);
            var o4 = GetTripletOrientation(b1x, b1y, b2x, b2y, a2x, a2y);

            if (o1 != o2 && o3 != o4)
                return true;
            if (o1 == TripletOrientation.Colinear && OnSegment(a1x, a1y, b1x, b1y, a2x, a2y))
                return true;
            if (o2 == TripletOrientation.Colinear && OnSegment(a1x, a1y, b2x, b2y, a2x, a2y))
                return true;
            if (o3 == TripletOrientation.Colinear && OnSegment(b1x, b1y, a1x, a1y, b2x, b2y))
                return true;
            if (o4 == TripletOrientation.Colinear && OnSegment(b1x, b1y, a2x, a2y, b2x, b2y))
                return true;
            return false;
        }

        /// <summary>
        /// Finds where two given lines (or line segments if asSeg=true) intersect and returns the position
        /// </summary>
        /// <param name="a1">1st point of 1st line/segment</param>
        /// <param name="a2">2nd point of 1st line/segment</param>
        /// <param name="b1">1st point of 2nd line/segment</param>
        /// <param name="b2">2nd point of 2nd line/segment</param>
        /// <param name="areSegments">whether the points define lines or segments</param>
        /// <returns>The position (if it exists) where the two lines/segments intersect</returns>
        public static Vector3? LinesIntersectionPoint(Vector3 a1, Vector3 a2, Vector3 b1, Vector3 b2, bool areSegments = true)
        {
            if (areSegments && !Rectangle.Collide(
                Basics.Utils.Min(a1.X, a2.X), Basics.Utils.Min(a1.Y, a2.Y), Math.Abs(a1.X - a2.X), Math.Abs(a1.Y - a2.Y),
                Basics.Utils.Min(b1.X, b2.X), Basics.Utils.Min(b1.Y, b2.Y), Math.Abs(b1.X - b2.X), Math.Abs(b1.Y - b2.Y)))
                return null;

            var adx = a1.X - a2.X;
            var ady = a2.Y - a1.Y;
            var bdx = b1.X - b2.X;
            var bdy = b2.Y - b1.Y;
            var c1 = a2.X * a1.Y - a1.X * a2.Y;
            var c2 = b2.X * b1.Y - b1.X * b2.Y;

            var denom = ady * bdx - bdy * adx;
            if (denom == 0)
                return null;
            var ip = new Vector3((adx * c2 - bdx * c1) / denom, (bdy * c1 - ady * c2) / denom, 0);

            if (areSegments &&
                (ip.X - a2.X) * (ip.X - a2.X) + (ip.Y - a2.Y) * (ip.Y - a2.Y) > (a1.X - a2.X) * (a1.X - a2.X) + (a1.Y - a2.Y) * (a1.Y - a2.Y) ||
                (ip.X - a1.X) * (ip.X - a1.X) + (ip.Y - a1.Y) * (ip.Y - a1.Y) > (a1.X - a2.X) * (a1.X - a2.X) + (a1.Y - a2.Y) * (a1.Y - a2.Y) ||
                (ip.X - b2.X) * (ip.X - b2.X) + (ip.Y - b2.Y) * (ip.Y - b2.Y) > (b1.X - b2.X) * (b1.X - b2.X) + (b1.Y - b2.Y) * (b1.Y - b2.Y) ||
                (ip.X - b1.X) * (ip.X - b1.X) + (ip.Y - b1.Y) * (ip.Y - b1.Y) > (b1.X - b2.X) * (b1.X - b2.X) + (b1.Y - b2.Y) * (b1.Y - b2.Y))
                return null;
            return ip;
        }

        #endregion
    }
}
