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
        
        public Polygon(List<Vector3> _counterClockwiseVertices)
        {
            vertices = _counterClockwiseVertices;
            Sides = _counterClockwiseVertices.Count;
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
        public ConvexPolygon(List<Vector3> _counterClockwiseVertices) : base(_counterClockwiseVertices) { }

        public ColoredVertexBuffer GetFillBuffer(Color4 _color) => GetFillBuffer(vertices, _color);
        protected static ColoredVertexBuffer GetFillBuffer(List<Vector3> _vertices, Color4 _color)
        {
            var buffer = new ColoredVertexBuffer(PrimitiveType.Triangles);
            Triangulize(_vertices).ForEach(v => buffer.AddVertex(new ColoredVertex(v, _color)));
            return buffer;
        }

        //Takes a list of n vertices and adds more vertices to form triangles consisting of each consecutive group of 3 vertices
        public static List<Vector3> Triangulize(List<Vector3> _counterClockwiseVertices)
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

        public static ConvexPolygon Regular(int sides, float radius, float rotationRad=0)
        {
            var counterClockwiseVertices = new List<Vector3>();
            for(var i = 0; i < sides; i++)
            {
                var angle = Math.PI * 2 * (0.25f + i * 1.0f / sides) + rotationRad;
                counterClockwiseVertices.Add(radius * new Vector3((float)Math.Cos(angle), (float)Math.Sin(angle), 0));
            }
            return new ConvexPolygon(counterClockwiseVertices);
        }
        public static ConvexPolygon Square(float _size) => Regular(4, _size * (float)Math.Sqrt(2));
    }

    public class ConvexPolygonRenderer
    {
        private ConvexPolygon polygon;
        private ColoredVertexBuffer fillBuffer;
        private ColoredVertexBuffer outlineBuffer;
        private ColoredVertexArray fillArray;
        private ColoredVertexArray outlineArray;

        public bool FillVisible;
        public bool OutlineVisible;

        public ConvexPolygonRenderer(List<Vector3> _counterClockwiseVertices, Color4? _color=null, Color4? _outlineColor=null)
            : this(new ConvexPolygon(_counterClockwiseVertices), _color, _outlineColor) { }
        public ConvexPolygonRenderer(ConvexPolygon _polygon, Color4? _fillColor=null, Color4? _outlineColor = null)
        {
            polygon = _polygon;

            FillVisible = true;
            OutlineVisible = true;

            fillBuffer = polygon.GetFillBuffer(_fillColor.HasValue ? _fillColor.Value : Color4.White);
            fillArray = ColoredVertexArray.FromBuffer(fillBuffer);

            outlineBuffer = polygon.GetOutlineBuffer(_outlineColor.HasValue ? _outlineColor.Value : Color4.White);
            outlineArray = ColoredVertexArray.FromBuffer(outlineBuffer);
        }

        public void SetFillColor(Color4 _color) => fillBuffer.SetColor(_color);
        public void SetOutlineColor(Color4 _color) => outlineBuffer.SetColor(_color);

        public void Rotate(float _angle, Vector3? _center = null)
        {
            fillBuffer?.Rotate(_angle, _center ?? polygon.CenterOfMass);
            outlineBuffer?.Rotate(_angle, _center ?? polygon.CenterOfMass);
        }

        public void Move(Vector3 _position)
        {
            fillBuffer?.Move(_position);
            outlineBuffer?.Move(_position);
        }

        public void Render()
        {
            if(FillVisible)
                fillArray?.Render();
            if(OutlineVisible)
                outlineArray?.Render();
        }
    }
}
