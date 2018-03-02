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
        //Relative coordinates, rather than world coordinates
        public Vector3 CenterOfMass => vertices.Avg();
        public readonly int Sides;

        protected readonly List<Vector3> vertices;

        public Polygon(List<Vector2> _counterClockwiseVertices) : this(_counterClockwiseVertices.Select(x => new Vector3(x.X, x.Y, 0)).ToList()) { }
        public Polygon(List<Vector3> _counterClockwiseVertices)
        {
            vertices = _counterClockwiseVertices;
            Sides = vertices.Count;
        }

        public Polygon Center()
        {
            Move(-CenterOfMass);
            return this;
        }
        public Polygon Move(Vector3 _position)
        {
            if (_position == Vector3.Zero)
                return this;
            for (var i = 0; i < vertices.Count; i++)
                vertices[i] += _position;
            return this;
        }
        public Polygon Rotate(float _angleRad, Vector3? _center = null)
        {
            vertices.Rotate(_angleRad, _center ?? CenterOfMass);
            return this;
        }

        public float MinX() => vertices.Select(o => o.X).Min();
        public float MaxX() => vertices.Select(o => o.X).Max();
        public float MinY() => vertices.Select(o => o.Y).Min();
        public float MaxY() => vertices.Select(o => o.Y).Max();
        public Basics.Rectangle BoundingRectangle() => BoundingBox.RectangleFromPoints(vertices);
        
        public ColoredVertexBuffer GetOutlineBuffer() => GetOutlineBuffer(Color4.White);
        public ColoredVertexBuffer GetOutlineBuffer(Color4 _color)
        {
            var buffer = new ColoredVertexBuffer(PrimitiveType.LineLoop);
            vertices.ForEach(v => buffer.AddVertex(new ColoredVertex(v, _color)));
            return buffer;
        }
        public virtual ColoredVertexBuffer GetFillBuffer() => throw new NotImplementedException();
        public virtual ColoredVertexBuffer GetFillBuffer(Color4 _color) => throw new NotImplementedException();
        public virtual List<Vector3> ToTriangles() => throw new NotImplementedException();

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
                m = Utils.PointOnLineAtX(pointA, pointB, xmin);
                n = Utils.PointOnLineAtX(pointA, pointB, xmax);
            }
            else
            {
                ymin = MinY() - 10;
                ymax = MaxY() + 10;
                m = Utils.PointOnLineAtY(pointA, pointB, ymin);
                n = Utils.PointOnLineAtY(pointA, pointB, ymax);
            }

            var points = new List<Vector3>();
            var intersections = new List<List<Vector3>>();
            var uncheckedPoints = new List<int>();
            var count = vertices.Count;

            for(var j = 0; j < count; j++)
            {
                var a = vertices[j];
                var b = vertices[(j + 1) % count];
                var p = Utils.LinesIntersectionPoint(m, n, a, b, true);
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
        public override List<Vector3> ToTriangles()
        {
            //Takes a list of n vertices and adds more vertices to form triangles consisting of each consecutive group of 3 vertices
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

        public static ConvexPolygon Square(float size) => Rectangle(size, size);
        public static ConvexPolygon Rectangle(float width, float height)
        {
            var counterClockwiseVertices = new List<Vector3>()
            {
                new Vector3(0, 0, 0),
                new Vector3(width, 0, 0),
                new Vector3(width, height, 0),
                new Vector3(0, height, 0),
            };
            return new ConvexPolygon(counterClockwiseVertices);
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
        public override List<Vector3> ToTriangles()
        {
            //Takes a list of n vertices and adds more vertices to form triangles consisting of each consecutive group of 3 vertices
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
                if (Utils.PointIsRightOfLine(next, prev, curr))
                    IsEar = false;
                else
                {
                    for (var j = 0; j < vertices.Count; j++)
                    {
                        if (j == iPrev || j == iCurr || j == iNext || iSkip.Contains(j))
                            continue;
                        if (Utils.PointInTriangle(vertices[j], prev, curr, next))
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
    }
}
