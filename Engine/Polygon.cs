using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Engine.OpenGL.Colored;
using Basics;

namespace Engine
{
    public abstract class Polygon : List<Vector3>
    {
        public Vector3 CenterOfMass => this.Avg();
        public int Sides => Count;

        public Polygon(IEnumerable<Vector2> _vertices) : this(_vertices.Select(x => new Vector3(x.X, x.Y, 0)).ToList()) { }
        public Polygon(List<Vector3> _vertices) : base(EnsureCounterClockwise(_vertices)) { }

        public void Clone(List<Vector3> _vertices)
        {
            this.Clear();
            this.AddRange(EnsureCounterClockwise(_vertices));
        }

        public static bool IsCounterClockwise(IEnumerable<Vector2> _vertices)
            => IsCounterClockwise(_vertices.Select(o => new Vector3(o.X, o.Y, 0)).ToList());
        public static bool IsCounterClockwise(List<Vector3> _vertices)
        { //https://stackoverflow.com/questions/1165647/how-to-determine-if-a-list-of-polygon-points-are-in-clockwise-order
            var sum = 0f;
            for(var i = 0; i < _vertices.Count; i++)
            {
                var a = _vertices[i];
                var b = _vertices[(i + 1) % _vertices.Count];
                sum += (b.X - a.X) * (b.Y + a.Y);
            }
            return sum < 0; //>0 means clockwise
        }

        /// <summary>
        /// Determines if the given vertices are in counter-clockwise order and reverses them if they are not.
        /// </summary>
        /// <param name="_vertices">vertices to ensure are in counter-clockwise order</param>
        /// <returns>The given vertices in counter-clockwise order.</returns>
        public static IEnumerable<Vector3> EnsureCounterClockwise(List<Vector3> _vertices)
            => IsCounterClockwise(_vertices) ? _vertices : _vertices.Reversed();

        public Polygon Center()
        {
            Move(-CenterOfMass);
            return this;
        }
        public Polygon Move(Vector3 _position)
        {
            if (_position == Vector3.Zero)
                return this;
            for (var i = 0; i < Count; i++)
                this[i] += _position;
            return this;
        }

        public float MinX() => this.Select(o => o.X).Min();
        public float MaxX() => this.Select(o => o.X).Max();
        public float MinY() => this.Select(o => o.Y).Min();
        public float MaxY() => this.Select(o => o.Y).Max();
        public Basics.Rectangle BoundingRectangle() => BoundingBox.RectangleFromPoints(this);
        
        public ColoredVertexBuffer GetOutlineBuffer() => GetOutlineBuffer(Color4.White);
        public ColoredVertexBuffer GetOutlineBuffer(Color4 _color)
        {
            var buffer = new ColoredVertexBuffer(PrimitiveType.LineLoop);
            ForEach(v => buffer.AddVertex(new ColoredVertex(v, _color)));
            return buffer;
        }
        public virtual ColoredVertexBuffer GetFillBuffer() => throw new NotImplementedException();
        public virtual ColoredVertexBuffer GetFillBuffer(Color4 _color) => throw new NotImplementedException();
        public virtual List<Vector3> ToTriangles() => throw new NotImplementedException();

        public IEnumerable<Polygon> SplitAlongLine(Vector3 pointA, Vector3 pointB)
        {
            if (Count == 0)
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

            for(var j = 0; j < Count; j++)
            {
                var a = this[j];
                var b = this[(j + 1) % Count];
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
                return new List<Polygon>() { new ConcavePolygon(new List<Vector3>(this)) };

            intersections.Sort((x, y) => Math.Sign((x.First() - m).LengthSquared - (y.First() - m).LengthSquared));

            var count = intersections.Count;
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
        public ConvexPolygon(IEnumerable<Vector2> _vertices) : base(_vertices) { }
        public ConvexPolygon(List<Vector3> _vertices) : base(_vertices) { }

        public override ColoredVertexBuffer GetFillBuffer() => GetFillBuffer(Color4.White);
        public override ColoredVertexBuffer GetFillBuffer(Color4 _color)
        {
            //PrimitiveType.Polygon appears to only work for convex polygons
            var buffer = new ColoredVertexBuffer(PrimitiveType.Polygon);
            buffer.AddVertices(this.Select(x => new ColoredVertex(x, _color)));
            return buffer;
        }

        /// <summary>
        /// By definition, a line can be drawn between all vertices, so just make triangles sprouting from one vertex.
        /// </summary>
        /// <returns>list of vertex triplets which define a triangulation of this polygon</returns>
        public override List<Vector3> ToTriangles()
        {
            //Takes a list of n vertices and adds more vertices to form triangles consisting of each consecutive group of 3 vertices
            if (Count <= 3)
                return new List<Vector3>(this);
            var ret = new List<Vector3>();
            var baseVertex = this.First();
            for (var i = 1; i < Count - 1; i++)
            {
                ret.Add(baseVertex);
                ret.Add(this[i]);
                ret.Add(this[i + 1]);
            }
            return ret;
        }

        public static ConvexPolygon Square(float size) => Rectangle(size, size);
        
        public static ConvexPolygon Rectangle(Basics.Rectangle _rectangle)
            => Rectangle(_rectangle.X, _rectangle.Y, _rectangle.W, _rectangle.H);
        public static ConvexPolygon Rectangle(float width, float height)
            => Rectangle(0, 0, width, height);
        public static ConvexPolygon Rectangle(float x, float y, float width, float height)
            => new ConvexPolygon(RectanglePoints(x, y, width, height));
        public static List<Vector3> RectanglePoints(Basics.Rectangle _rectangle)
            => RectanglePoints(_rectangle.X, _rectangle.Y, _rectangle.W, _rectangle.H);
        public static List<Vector3> RectanglePoints(float width, float height)
            => RectanglePoints(0, 0, width, height);
        public static List<Vector3> RectanglePoints(float x, float y, float width, float height)
            => new List<Vector3>()
            {
                new Vector3(x, y, 0),
                new Vector3(x + width, y, 0),
                new Vector3(x + width, y + height, 0),
                new Vector3(x, y + height, 0),
            };
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
        public ConcavePolygon(IEnumerable<Vector2> _vertices) : base(_vertices) { }
        public ConcavePolygon(List<Vector3> _vertices) : base(_vertices) { }

        public override ColoredVertexBuffer GetFillBuffer() => GetFillBuffer(Color4.White);
        public override ColoredVertexBuffer GetFillBuffer(Color4 _color)
        {
            var buffer = new ColoredVertexBuffer(PrimitiveType.Triangles);
            ToTriangles().ForEach(v => buffer.AddVertex(new ColoredVertex(v, _color)));
            return buffer;
        }

        /// <summary>
        /// Ear-clipping algorithm
        /// Takes a list of n vertices and adds more vertices to form triangles consisting of consecutive groups of 3 vertices
        /// </summary>
        /// <returns>list of vertex triplets which define a triangulation of this polygon</returns>
        public override List<Vector3> ToTriangles()
        {
            if (Count <= 3)
                return new List<Vector3>(this);

            var ret = new List<Vector3>();
            var iSkip = new List<int>();
            int i = 0;
            while (iSkip.Count < Count - 2)
            {
                var iCurr = i++ % Count;

                if (iSkip.Contains(iCurr))
                    continue;

                int iPrev = -1;
                int iPrevCount = 1;
                while ((iPrev < 0 || iSkip.Contains(iPrev)) && iPrevCount < Count)
                    iPrev = (iCurr + Count - iPrevCount++) % Count;
                if (iPrevCount >= Count)
                    Console.WriteLine("Cannot find previous vertex in concave polygon triangulation");

                var iNext = -1;
                var iNextCount = 1;
                while ((iNext == -1 || iSkip.Contains(iNext)) && iNextCount < Count)
                    iNext = (iCurr + iNextCount++) % Count;
                if (iNextCount >= Count)
                    Console.WriteLine("Cannot find next vertex in concave polygon triangulation");

                var prev = this[iPrev];
                var curr = this[iCurr];
                var next = this[iNext];
                var IsEar = true;
                if (next.IsRightOfLine(prev, curr))
                    IsEar = false;
                else
                {
                    for (var j = 0; j < Count; j++)
                    {
                        if (j == iPrev || j == iCurr || j == iNext || iSkip.Contains(j))
                            continue;
                        if (Utils.PointInTriangle(this[j], prev, curr, next))
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
