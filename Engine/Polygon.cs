using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Engine
{
    public abstract class Polygon
    {
        public Vector3 CenterOfMass
        {
            get
            {
                var sum = new Vector3();
                vertices.ForEach(v => sum += v);
                return sum / vertices.Count;
            }
        }
        public readonly int Sides;
        protected readonly List<Vector3> vertices;

        public Polygon(List<Vector2> _counterClockwiseVertices) : this(_counterClockwiseVertices.Select(x => new Vector3(x.X, x.Y, 0)).ToList()) { }
        public Polygon(List<Vector3> _counterClockwiseVertices)
        {
            vertices = _counterClockwiseVertices;
            Sides = vertices.Count;
        }

        public ColoredVertexBuffer GetOutlineBuffer() => GetOutlineBuffer(Color4.White);
        public ColoredVertexBuffer GetOutlineBuffer(Color4 _color)
        {
            var buffer = new ColoredVertexBuffer(PrimitiveType.LineLoop);
            vertices.ForEach(v => buffer.AddVertex(new ColoredVertex(v, _color)));
            return buffer;
        }

        public void Center() => Move(-CenterOfMass);
        public void Move(Vector3 _position)
        {
            for (var i = 0; i < vertices.Count; i++)
                vertices[i] += _position;
        }

        public void Rotate(float _angleRad, Vector3? _center = null) => vertices.Rotate(_angleRad, _center ?? CenterOfMass);

        public float MinX()
        {
            if (vertices.Count == 0)
                return 0;
            var minX = vertices.First().X;
            for (var i = 1; i < vertices.Count; i++)
                minX = Basics.Utils.Min(minX, vertices[i].X);
            return minX;
        }
        public float MaxX()
        {
            if (vertices.Count == 0)
                return 0;
            var maxX = vertices.First().X;
            for (var i = 1; i < vertices.Count; i++)
                maxX = Basics.Utils.Max(maxX, vertices[i].X);
            return maxX;
        }
        public float MinY()
        {
            if (vertices.Count == 0)
                return 0;
            var minY = vertices.First().Y;
            for (var i = 1; i < vertices.Count; i++)
                minY = Basics.Utils.Min(minY, vertices[i].Y);
            return minY;
        }
        public float MaxY()
        {
            if (vertices.Count == 0)
                return 0;
            var maxY = vertices.First().Y;
            for (var i = 1; i < vertices.Count; i++)
                maxY = Basics.Utils.Max(maxY, vertices[i].Y);
            return maxY;
        }

        public virtual ColoredVertexBuffer GetFillBuffer() => throw new NotImplementedException();
        public virtual ColoredVertexBuffer GetFillBuffer(Color4 _color) => throw new NotImplementedException();
        public virtual List<Vector3> ToTriangles() => throw new NotImplementedException();

        private static Vector3 PointOnLineAtX(Vector3 a, Vector3 b, float x)
        {
            var diffX = b.X - a.X;
            return diffX == 0 ? (a + b) / 2 : new Vector3(x, (x - a.X) / diffX * (b.Y - a.Y) + a.Y, 0);
        }

        private static Vector3 PointOnLineAtY(Vector3 a, Vector3 b, float y)
        {
            var diffY = b.Y - a.Y;
            return diffY == 0 ? (a + b) / 2 : new Vector3((y - a.Y) / diffY * (b.X - a.X) + a.X, y, 0);
        }

        private static bool RectanglesCollide(Vector4 a, Vector4 b) => RectanglesCollide(a.X, a.Y, a.Z, a.W, b.X, b.Y, b.Z, b.W);
        private static bool RectanglesCollide(float ax, float ay, float aw, float ah, float bx, float by, float bw, float bh)
            => ax + aw >= bx && bx + bw >= ax && ay + ah >= by && by + bh >= ay;

        private static Vector3? LinesIntersectionPoint(Vector3 A, Vector3 B, Vector3 E, Vector3 F, bool asSeg = true)
        {
            if (asSeg && !RectanglesCollide(
                Basics.Utils.Min(A.X, B.X), Basics.Utils.Min(A.Y, B.Y), Math.Abs(A.X - B.X), Math.Abs(A.Y - B.Y),
                Basics.Utils.Min(E.X, F.X), Basics.Utils.Min(E.Y, F.Y), Math.Abs(E.X - F.X), Math.Abs(E.Y - F.Y)))
                return null;

            var a1 = B.Y - A.Y;
            var a2 = F.Y - E.Y;
            var b1 = A.X - B.X;
            var b2 = E.X - F.X;
            var c1 = B.X * A.Y - A.X * B.Y;
            var c2 = F.X * E.Y - E.X * F.Y;

            var denom = a1 * b2 - a2 * b1;
            if(denom == 0)
                return null;
            var ip = new Vector3((b1 * c2 - b2 * c1) / denom, (a2 * c1 - a1 * c2) / denom, 0);

            if (asSeg &&
                (ip.X - B.X) * (ip.X - B.X) + (ip.Y - B.Y) * (ip.Y - B.Y) > (A.X - B.X) * (A.X - B.X) + (A.Y - B.Y) * (A.Y - B.Y) ||
                (ip.X - A.X) * (ip.X - A.X) + (ip.Y - A.Y) * (ip.Y - A.Y) > (A.X - B.X) * (A.X - B.X) + (A.Y - B.Y) * (A.Y - B.Y) ||
                (ip.X - F.X) * (ip.X - F.X) + (ip.Y - F.Y) * (ip.Y - F.Y) > (E.X - F.X) * (E.X - F.X) + (E.Y - F.Y) * (E.Y - F.Y) ||
                (ip.X - E.X) * (ip.X - E.X) + (ip.Y - E.Y) * (ip.Y - E.Y) > (E.X - F.X) * (E.X - F.X) + (E.Y - F.Y) * (E.Y - F.Y))
                return null;
            return ip;
        }

        public IEnumerable<Polygon> SplitAlongLine(Vector3 pointA, Vector3 pointB)
        {
            if (vertices.Count == 0)
                return new List<Polygon>();

            float xmin, ymin, xmax, ymax;
            Vector3 m, n;
            if (pointA.Y == pointB.Y)
            {
                xmin = MinX() - 10;
                xmax = MaxX() + 10;
                m = PointOnLineAtX(pointA, pointB, xmin);
                n = PointOnLineAtX(pointA, pointB, xmax);
            }
            else
            {
                ymin = MinY() - 10;
                ymax = MaxY() + 10;
                m = PointOnLineAtY(pointA, pointB, ymin);
                n = PointOnLineAtY(pointA, pointB, ymax);
            }

            var points = new List<Vector3>();
            var intersections = new List<List<Vector3>>();
            var uncheckedPoints = new List<int>();
            var count = vertices.Count;

            for(var j = 0; j < count; j++)
            {
                var a = vertices[j];
                var b = vertices[(j + 1) % count];
                var p = LinesIntersectionPoint(m, n, a, b, true);
                uncheckedPoints.Add(points.Count);
                points.Add(a);
                if (p.HasValue)
                {
                    intersections.Add(new List<Vector3>() { p.Value, b });
                    points.Add(p.Value);
                }
            }

            if (intersections.Count <= 0)
                return new List<Polygon>() { new ConcavePolygon(vertices) };

            intersections.Sort((x, y) => Math.Sign((x.First() - m).LengthSquared - (y.First() - m).LengthSquared));

            count = intersections.Count;
            for(var i = 0; i < count; i+=2)
            {
                var c = intersections[i];
                if(i+1 >= count)
                {
                    c.Add(c.First());
                    break;
                }
                var d = intersections[i + 1];
                c.Add(d.First());
                d.Add(c.First());
            }

            var polygonsPoints = new List<List<Vector3>>() { new List<Vector3>() };
            var k = uncheckedPoints.First();
            while(true)
            {
                if (uncheckedPoints.Contains(k))
                    uncheckedPoints.Remove(k);

                if (polygonsPoints.Last().Count > 0 && polygonsPoints.Last().First() == points[k])
                {
                    polygonsPoints.Add(new List<Vector3>());
                    if(uncheckedPoints.Count > 0)
                    {
                        k = uncheckedPoints.First();
                        continue;
                    }
                    if (polygonsPoints.Last().Count <= 0)
                        polygonsPoints.RemoveAt(polygonsPoints.Count - 1);
                    return polygonsPoints.Select(p => new ConcavePolygon(p.ToList()) as Polygon);
                }

                polygonsPoints.Last().Add(new Vector3(points[k]));
                var j = (k + 1) % points.Count;
                foreach(var intersectionInfo in intersections)
                {
                    if(intersectionInfo.First() == points[k])
                    {
                        foreach(var intersectionInfoOther in intersections)
                        {
                            if(intersectionInfo[2] == intersectionInfoOther.First())
                            {
                                j = points.IndexOf(intersectionInfoOther[1]);
                                break;
                            }
                        }
                        polygonsPoints.Last().Add(new Vector3(intersectionInfo[2]));
                        break;
                    }
                }
                k = j;
            }
        }
    }

    public class ConvexPolygon : Polygon
    {
        public ConvexPolygon(List<Vector2> _counterClockwiseVertices) : base(_counterClockwiseVertices) { }
        public ConvexPolygon(List<Vector3> _counterClockwiseVertices) : base(_counterClockwiseVertices) { }

        public override ColoredVertexBuffer GetFillBuffer() => GetFillBuffer(Color4.White);
        public override ColoredVertexBuffer GetFillBuffer(Color4 _color)
        {
            var buffer = new ColoredVertexBuffer(PrimitiveType.Triangles);
            ToTriangles().ForEach(v => buffer.AddVertex(new ColoredVertex(v, _color)));
            return buffer;
        }

        //Takes a list of n vertices and adds more vertices to form triangles consisting of each consecutive group of 3 vertices
        public override List<Vector3> ToTriangles()
        {
            var ret = new List<Vector3>();
            if (vertices == null)
                return ret;
            else if (vertices.Count <= 3)
                return vertices;
            var baseVertex = vertices.First();
            for (var i = 1; i < vertices.Count - 1; i++)
            {
                ret.Add(baseVertex);
                ret.Add(vertices[i]);
                ret.Add(vertices[i + 1]);
            }
            return ret;
        }

        public static ConvexPolygon Regular(int sides, float radius, float rotationRad = 0)
        {
            var counterClockwiseVertices = new List<Vector3>();
            for (var i = 0; i < sides; i++)
            {
                var angle = Math.PI * 2 * (0.25f + i * 1.0f / sides) + rotationRad;
                counterClockwiseVertices.Add(radius * new Vector3((float)Math.Cos(angle), (float)Math.Sin(angle), 0));
            }
            return new ConvexPolygon(counterClockwiseVertices);
        }
        public static ConvexPolygon Square(float _size) => Regular(4, (float)(_size * Math.Sqrt(2) / 2), (float)(Math.PI / 4));
    }

    public class ConcavePolygon : Polygon
    {
        public ConcavePolygon(List<Vector2> _counterClockwiseVertices) : base(_counterClockwiseVertices) { }
        public ConcavePolygon(List<Vector3> _counterClockwiseVertices) : base(_counterClockwiseVertices) { }

        public override ColoredVertexBuffer GetFillBuffer() => GetFillBuffer(Color4.White);
        public override ColoredVertexBuffer GetFillBuffer(Color4 _color)
        {
            var buffer = new ColoredVertexBuffer(PrimitiveType.Triangles);
            ToTriangles().ForEach(v => buffer.AddVertex(new ColoredVertex(v, _color)));
            return buffer;
        }

        //Takes a list of n vertices and adds more vertices to form triangles consisting of each consecutive group of 3 vertices
        public override List<Vector3> ToTriangles()
        {
            var ret = new List<Vector3>();
            if (vertices == null)
                return ret;
            else if (vertices.Count <= 3)
                return vertices;

            var iSkip = new List<int>();
            int i = 0;
            while (iSkip.Count < vertices.Count - 2)
            {
                var iCurr = i++ % vertices.Count;

                if (iSkip.Contains(iCurr))
                    continue;

                int iPrev = -1;
                int iPrevCount = 1;
                while ((iPrev < 0 || iSkip.Contains(iPrev)) && iPrevCount < vertices.Count)
                    iPrev = (iCurr + vertices.Count - iPrevCount++) % vertices.Count;
                if (iPrevCount >= vertices.Count)
                    Console.WriteLine("Cannot find previous vertex in concave polygon triangulation");

                var iNext = -1;
                var iNextCount = 1;
                while ((iNext == -1 || iSkip.Contains(iNext)) && iNextCount < vertices.Count)
                    iNext = (iCurr + iNextCount++) % vertices.Count;
                if (iNextCount >= vertices.Count)
                    Console.WriteLine("Cannot find next vertex in concave polygon triangulation");

                var prev = vertices[iPrev];
                var curr = vertices[iCurr];
                var next = vertices[iNext];
                var IsEar = true;
                if (PointIsRightOfLine(next, prev, curr))
                    IsEar = false;
                else
                {
                    for (var j = 0; j < vertices.Count; j++)
                    {
                        if (j == iPrev || j == iCurr || j == iNext || iSkip.Contains(j))
                            continue;
                        if (PointInTriangle(vertices[j], prev, curr, next))
                        {
                            IsEar = false;
                            break;
                        }
                    }
                }

                if (IsEar)
                {
                    ret.Add(prev);
                    ret.Add(curr);
                    ret.Add(next);
                    iSkip.Add(iCurr);
                }
            }
            return ret;
        }

        private static bool PointIsRightOfLine(Vector3 point, Vector3 a, Vector3 b) => PointSideOfLine(point, a, b) < 0;
        private static bool PointIsLeftOfLine(Vector3 point, Vector3 a, Vector3 b) => PointSideOfLine(point, a, b) > 0;
        private static bool PointOnLine(Vector3 point, Vector3 a, Vector3 b) => PointSideOfLine(point, a, b) == 0;
        private static int PointSideOfLine(Vector3 point, Vector3 a, Vector3 b) =>  Math.Sign((b.X - a.X) * (point.Y - a.Y) - (b.Y - a.Y) * (point.X - a.X));
        private static bool PointInTriangle(Vector3 point, Vector3 a, Vector3 b, Vector3 c)
        {
            var bc = PointIsRightOfLine(point, b, c);
            return PointIsRightOfLine(point, a, b) == bc && bc == PointIsRightOfLine(point, c, a);
        }
    }

    public class PolygonOutlineRenderer
    {
        private ColoredVertexBuffer outlineBuffer;
        private ColoredVertexArray outlineArray;
        
        public Vector3 CenterOfMass;
        public bool Visible = true;

        public PolygonOutlineRenderer(Polygon _polygon) : this(_polygon, Color4.White) { }
        public PolygonOutlineRenderer(Polygon _polygon, Color4 _outlineColor)
        {
            CenterOfMass = _polygon.CenterOfMass;
            outlineBuffer = _polygon.GetOutlineBuffer(_outlineColor);
            outlineArray = ColoredVertexArray.FromBuffer(outlineBuffer);
        }
        
        public void SetOutlineColor(Color4 _color) => outlineBuffer.SetColor(_color);
        public void RandomizeOutlineColor() => outlineBuffer.RandomizeColor();

        public void Rotate(float _angle, Vector3? _center = null) => outlineArray?.Rotate(_angle, _center ?? CenterOfMass);

        public void Move(Vector3 _position) => outlineArray?.Move(_position);

        public void Render(float _x, float _y) => Render(new Vector3(_x, _y, 0));
        public void Render(Vector3 _position)
        {
            if (Visible)
                outlineArray?.Render(_position);
        }
    }

    public class PolygonFillRenderer
    {
        private ColoredVertexBuffer fillBuffer;
        private ColoredVertexArray fillArray;

        public Vector3 CenterOfMass;
        public bool Visible = true;

        public PolygonFillRenderer(Polygon _polygon) : this(_polygon, Color4.White) { }
        public PolygonFillRenderer(Polygon _polygon, Color4 _fillColor)
        {
            CenterOfMass = _polygon.CenterOfMass;
            fillBuffer = _polygon.GetFillBuffer(_fillColor);
            fillArray = ColoredVertexArray.FromBuffer(fillBuffer);
        }

        public void SetFillColor(Color4 _color) => fillBuffer.SetColor(_color);
        public void RandomizeFillColor() => fillBuffer.RandomizeColor();

        public void Rotate(float _angle, Vector3? _center = null) => fillArray?.Rotate(_angle, _center ?? CenterOfMass);

        public void Move(Vector3 _position) => fillArray?.Move(_position);

        public void Render(float _x, float _y) => Render(new Vector3(_x, _y, 0));
        public void Render(Vector3 _position)
        {
            if (Visible)
                fillArray?.Render(_position);
        }
    }
    
    public class PolygonActor : Actor
    {
        private Polygon polygon;
        private PolygonFillRenderer fillRenderer;
        private PolygonOutlineRenderer outlineRenderer;

        public PolygonActor(Polygon _polygon)
        {
            polygon = _polygon;
            CenterPolygon();
            fillRenderer = new PolygonFillRenderer(polygon, ColorExtensions.RandomColor());
            outlineRenderer = new PolygonOutlineRenderer(polygon, ColorExtensions.RandomColor());
        }

        private void CenterPolygon()
        {
            var com = polygon.CenterOfMass;
            X += com.X;
            Y += com.Y;
            polygon.Center();
        }

        public IEnumerable<Polygon> SplitAlongLine(Vector3 pointA, Vector3 pointB)
        {
            var v = new Vector3(X, Y, 0);
            var ret = polygon.SplitAlongLine(pointA - v, pointB - v).ToList();
            ret.ForEach(o => o.Move(v));
            return ret;
        }

        public override void Render()
        {
            fillRenderer.Render(X, Y);
            outlineRenderer.Render(X, Y);
        }
    }
}
