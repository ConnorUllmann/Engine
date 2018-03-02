using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using Rectangle = Basics.Rectangle;

namespace Engine
{
    public static class Utils
    {
        public static Vector2 UnitVector2(float angleRad) => new Vector2((float)Math.Cos(angleRad), (float)Math.Sin(angleRad));
        public static Vector2 RandomUnitVector2() => UnitVector2((float)(Basics.Utils.RandomDouble() * Math.PI * 2));
        public static Vector3 To3D(this Vector2 _vector, float _z=0) => new Vector3(_vector.X, _vector.Y, _z);

        public static void Rotate(this List<Vector3> _vertices, float _radians, Vector3 _center)
        {
            for (var i = 0; i < _vertices.Count; i++)
                _vertices[i] = _vertices[i].Rotate(_radians, _center);
        }
        public static Vector3 Rotate(this Vector3 _vertex, float _radians, Vector3 _center)
        {
            var diff = _vertex - _center;
            diff = new Vector3(
                (float)(diff.X * Math.Cos(_radians) - diff.Y * Math.Sin(_radians)),
                (float)(diff.Y * Math.Cos(_radians) + diff.X * Math.Sin(_radians)),
                diff.Z);
            return diff + _center;
        }

        public static bool PointIsRightOfLine(Vector3 point, Vector3 a, Vector3 b) => PointSideOfLine(point, a, b) < 0;
        public static bool PointIsLeftOfLine(Vector3 point, Vector3 a, Vector3 b) => PointSideOfLine(point, a, b) > 0;
        public static bool PointOnLine(Vector3 point, Vector3 a, Vector3 b) => PointSideOfLine(point, a, b) == 0;
        public static int PointSideOfLine(Vector3 point, Vector3 a, Vector3 b) => Math.Sign((b.X - a.X) * (point.Y - a.Y) - (b.Y - a.Y) * (point.X - a.X));
        public static bool PointInTriangle(Vector3 point, Vector3 a, Vector3 b, Vector3 c)
        {
            var bc = PointIsRightOfLine(point, b, c);
            return PointIsRightOfLine(point, a, b) == bc && bc == PointIsRightOfLine(point, c, a);
        }

        public static bool TrianglesCollide(List<Vector3> a, List<Vector3> b)
        {
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
                    if (LinesIntersect(a[i], a[i1], b[j], b[(j + 1) % 3]))
                        return true;
            }
            return false;
        }
        
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

        public static bool LinesIntersect(Vector3 a1, Vector3 a2, Vector3 b1, Vector3 b2)
            => LinesIntersect(a1.X, a1.Y, a2.X, a2.Y, b1.X, b1.Y, b2.X, b2.Y);
        public static bool LinesIntersect(float a1x, float a1y, float a2x, float a2y, float b1x, float b1y, float b2x, float b2y)
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
        
        public static Vector3? LinesIntersectionPoint(Vector3 a1, Vector3 a2, Vector3 b1, Vector3 b2, bool asSeg = true)
        {
            if (asSeg && !Rectangle.Collide(
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

            if (asSeg &&
                (ip.X - a2.X) * (ip.X - a2.X) + (ip.Y - a2.Y) * (ip.Y - a2.Y) > (a1.X - a2.X) * (a1.X - a2.X) + (a1.Y - a2.Y) * (a1.Y - a2.Y) ||
                (ip.X - a1.X) * (ip.X - a1.X) + (ip.Y - a1.Y) * (ip.Y - a1.Y) > (a1.X - a2.X) * (a1.X - a2.X) + (a1.Y - a2.Y) * (a1.Y - a2.Y) ||
                (ip.X - b2.X) * (ip.X - b2.X) + (ip.Y - b2.Y) * (ip.Y - b2.Y) > (b1.X - b2.X) * (b1.X - b2.X) + (b1.Y - b2.Y) * (b1.Y - b2.Y) ||
                (ip.X - b1.X) * (ip.X - b1.X) + (ip.Y - b1.Y) * (ip.Y - b1.Y) > (b1.X - b2.X) * (b1.X - b2.X) + (b1.Y - b2.Y) * (b1.Y - b2.Y))
                return null;
            return ip;
        }

        public static Vector3 Avg(this IEnumerable<Vector3> list)
        {
            var sum = new Vector3();
            int count = 0;
            foreach (var v in list)
            {
                sum += v;
                count++;
            }
            return sum / count;
        }
        public static Vector3 Sum(this IEnumerable<Vector3> list)
        {
            var sum = new Vector3();
            foreach (var v in list)
                sum += v;
            return sum;
        }
    }
}
