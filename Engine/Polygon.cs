using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Engine
{
    public class Polygon
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

        public ColoredVertexBuffer GetOutlineBuffer(Color4 _color) => GetOutlineBuffer(vertices, _color);
        protected static ColoredVertexBuffer GetOutlineBuffer(List<Vector3> _vertices, Color4 _color)
        {
            var buffer = new ColoredVertexBuffer(PrimitiveType.LineLoop);
            _vertices.ForEach(v => buffer.AddVertex(new ColoredVertex(v, _color)));
            return buffer;
        }

        public void Rotate(float _angleRad, Vector3? _center = null) => vertices.Rotate(_angleRad, _center ?? CenterOfMass);
    }

    public class ConvexPolygon : Polygon
    {
        public ConvexPolygon(List<Vector2> _counterClockwiseVertices) : this(_counterClockwiseVertices.Select(x => new Vector3(x.X, x.Y, 0)).ToList()) { }
        public ConvexPolygon(List<Vector3> _counterClockwiseVertices) : base(_counterClockwiseVertices) { }

        public ColoredVertexBuffer GetFillBuffer(Color4 _color) => GetFillBuffer(vertices, _color);
        protected static ColoredVertexBuffer GetFillBuffer(List<Vector3> _vertices, Color4 _color)
        {
            var buffer = new ColoredVertexBuffer(PrimitiveType.Triangles);
            ToTriangles(_vertices).ForEach(v => buffer.AddVertex(new ColoredVertex(v, _color)));
            return buffer;
        }

        //Takes a list of n vertices and adds more vertices to form triangles consisting of each consecutive group of 3 vertices
        public static List<Vector3> ToTriangles(List<Vector3> _counterClockwiseVertices)
        {
            var vertices = new List<Vector3>();
            if (_counterClockwiseVertices == null)
                return vertices;
            else if (_counterClockwiseVertices.Count <= 3)
                return _counterClockwiseVertices;
            var baseVertex = _counterClockwiseVertices.First();
            for (var i = 0; i < _counterClockwiseVertices.Count - 1; i++)
            {
                vertices.Add(baseVertex);
                vertices.Add(_counterClockwiseVertices[i]);
                vertices.Add(_counterClockwiseVertices[i + 1]);
            }
            return vertices;
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
        public static ConvexPolygon Square(float _size) => Regular(4, _size * (float)Math.Sqrt(2));
    }

    public class ConcavePolygon : Polygon
    {
        public ConcavePolygon(List<Vector2> _counterClockwiseVertices) : this(_counterClockwiseVertices.Select(x => new Vector3(x.X, x.Y, 0)).ToList()) { }
        public ConcavePolygon(List<Vector3> _counterClockwiseVertices) : base(_counterClockwiseVertices) { }

        public ColoredVertexBuffer GetFillBuffer(Color4 _color) => GetFillBuffer(vertices, _color);
        protected static ColoredVertexBuffer GetFillBuffer(List<Vector3> _vertices, Color4 _color)
        {
            var buffer = new ColoredVertexBuffer(PrimitiveType.Triangles);
            ToTriangles(_vertices).ForEach(v => buffer.AddVertex(new ColoredVertex(v, _color)));
            return buffer;
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

        //Takes a list of n vertices and adds more vertices to form triangles consisting of each consecutive group of 3 vertices
        public static List<Vector3> ToTriangles(List<Vector3> _counterClockwiseVertices)
        {
            var vertices = new List<Vector3>();
            if (_counterClockwiseVertices == null)
                return vertices;
            else if (_counterClockwiseVertices.Count <= 3)
                return _counterClockwiseVertices;

            var iSkip = new List<int>();
            int i = 0;
            while(iSkip.Count < _counterClockwiseVertices.Count - 2)
            {
                var iCurr = i++ % _counterClockwiseVertices.Count;

                if (iSkip.Contains(iCurr))
                    continue;

                int iPrev = -1;
                int iPrevCount = 1;
                while((iPrev < 0 || iSkip.Contains(iPrev)) && iPrevCount < _counterClockwiseVertices.Count)
                    iPrev = (iCurr + _counterClockwiseVertices.Count - iPrevCount++) % _counterClockwiseVertices.Count;
                if (iPrevCount >= _counterClockwiseVertices.Count)
                    Console.WriteLine("Cannot find previous vertex in concave polygon triangulation");

                var iNext = -1;
                var iNextCount = 1;
                while((iNext == -1 || iSkip.Contains(iNext)) && iNextCount < _counterClockwiseVertices.Count)
                    iNext = (iCurr + iNextCount++) % _counterClockwiseVertices.Count;
                if (iNextCount >= _counterClockwiseVertices.Count)
                    Console.WriteLine("Cannot find next vertex in concave polygon triangulation");

                var prev = _counterClockwiseVertices[iPrev];
                var curr = _counterClockwiseVertices[iCurr];
                var next = _counterClockwiseVertices[iNext];
                var IsEar = true;
                if (PointIsRightOfLine(next, prev, curr))
                    IsEar = false;
                else
                {
                    for (var j = 0; j < _counterClockwiseVertices.Count; j++)
                    {
                        if (j == iPrev || j == iCurr || j == iNext || iSkip.Contains(j))
                            continue;
                        if (PointInTriangle(_counterClockwiseVertices[j], prev, curr, next))
                        {
                            IsEar = false;
                            break;
                        }
                    }
                }

                if(IsEar)
                {
                    vertices.Add(prev);
                    vertices.Add(curr);
                    vertices.Add(next);
                    iSkip.Add(iCurr);
                }
            }
            return vertices;
        }
    }

    public class PolygonRenderer
    {
        protected Polygon polygon;
        private ColoredVertexBuffer outlineBuffer;
        private ColoredVertexArray outlineArray;
        
        public bool OutlineVisible;

        public PolygonRenderer(List<Vector3> _counterClockwiseVertices, Color4? _outlineColor = null)
            : this(new Polygon(_counterClockwiseVertices), _outlineColor) { }
        public PolygonRenderer(Polygon _polygon, Color4? _outlineColor = null)
        {
            polygon = _polygon;
            
            OutlineVisible = true;

            outlineBuffer = polygon.GetOutlineBuffer(_outlineColor.HasValue ? _outlineColor.Value : Color4.White);
            outlineArray = ColoredVertexArray.FromBuffer(outlineBuffer);
        }
        
        public void SetOutlineColor(Color4 _color) => outlineBuffer.SetColor(_color);

        public virtual void Rotate(float _angle, Vector3? _center = null) => outlineBuffer?.Rotate(_angle, _center ?? polygon.CenterOfMass);

        public virtual void Move(Vector3 _position) => outlineBuffer?.Move(_position);

        public virtual void Render()
        {
            if (OutlineVisible)
                outlineArray?.Render();
        }
    }

    public class ConvexPolygonRenderer : PolygonRenderer
    {
        private ConvexPolygon convexPolygon { get => polygon as ConvexPolygon; }
        private ColoredVertexBuffer fillBuffer;
        private ColoredVertexArray fillArray;

        public bool FillVisible;

        public ConvexPolygonRenderer(List<Vector3> _counterClockwiseVertices, Color4? _color = null, Color4? _outlineColor = null)
            : this(new ConvexPolygon(_counterClockwiseVertices), _color, _outlineColor) { }
        public ConvexPolygonRenderer(ConvexPolygon _polygon, Color4? _fillColor = null, Color4? _outlineColor = null)
            : base(_polygon, _outlineColor)
        {
            FillVisible = true;
            fillBuffer = convexPolygon.GetFillBuffer(_fillColor.HasValue ? _fillColor.Value : Color4.White);
            fillArray = ColoredVertexArray.FromBuffer(fillBuffer);
        }

        public void SetFillColor(Color4 _color) => fillBuffer.SetColor(_color);

        public override void Rotate(float _angle, Vector3? _center = null)
        {
            base.Rotate(_angle, _center);
            fillBuffer?.Rotate(_angle, _center ?? convexPolygon.CenterOfMass);
        }

        public override void Move(Vector3 _position)
        {
            base.Move(_position);
            fillBuffer?.Move(_position);
        }

        public override void Render()
        {
            base.Render();
            if (FillVisible)
                fillArray?.Render();
        }
    }

    public class ConcavePolygonRenderer : PolygonRenderer
    {
        private ConcavePolygon concavePolygon { get => polygon as ConcavePolygon; }
        private ColoredVertexBuffer fillBuffer;
        private ColoredVertexArray fillArray;

        public bool FillVisible;

        public ConcavePolygonRenderer(List<Vector3> _counterClockwiseVertices, Color4? _color = null, Color4? _outlineColor = null)
            : this(new ConcavePolygon(_counterClockwiseVertices), _color, _outlineColor) { }
        public ConcavePolygonRenderer(ConcavePolygon _polygon, Color4? _fillColor = null, Color4? _outlineColor = null)
            : base(_polygon, _outlineColor)
        {
            FillVisible = true;
            fillBuffer = concavePolygon.GetFillBuffer(_fillColor.HasValue ? _fillColor.Value : Color4.White);
            fillArray = ColoredVertexArray.FromBuffer(fillBuffer);
        }

        public void SetFillColor(Color4 _color) => fillBuffer.SetColor(_color);
        public void RandomizeFillColor() => fillBuffer.RandomizeColor();

        public override void Rotate(float _angle, Vector3? _center = null)
        {
            base.Rotate(_angle, _center);
            fillBuffer?.Rotate(_angle, _center ?? concavePolygon.CenterOfMass);
        }

        public override void Move(Vector3 _position)
        {
            base.Move(_position);
            fillBuffer?.Move(_position);
        }

        public override void Render()
        {
            base.Render();
            if (FillVisible)
                fillArray?.Render();
        }
    }
}
