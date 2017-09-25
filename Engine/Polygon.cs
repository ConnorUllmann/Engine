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
        protected ColoredVertexBuffer buffer;
        public Polygon(List<Vector3> _vertices) : this(_vertices, Color4.White) { }
        public Polygon(List<Vector3> _vertices, Color4 _color)
        {
            buffer = new ColoredVertexBuffer();
            _vertices.ForEach(v => buffer.AddVertex(new ColoredVertex(v, _color)));
        }

        public void SetColor(Color4 _color) => buffer.SetColor(_color);
    }

    public class ConvexPolygon : Polygon
    {
        private readonly Vector3 CenterOfMass;
        public readonly int Sides;
        private ColoredVertexArray array;

        public ConvexPolygon(List<Vector3> _counterClockwiseVertices, Color4 _color) : base(Triangulate(_counterClockwiseVertices), _color)
        {
            Sides = _counterClockwiseVertices.Count;
            var sum = new Vector3();
            _counterClockwiseVertices.ForEach(v => sum += v);
            CenterOfMass = sum / _counterClockwiseVertices.Count;

            array = ColoredVertexArray.FromBuffer(buffer);
        }

        public static List<Vector3> Triangulate(List<Vector3> _counterClockwiseVertices)
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

        public void Rotate(float _angle, Vector3? _center = null) => buffer.Rotate(_angle, _center ?? CenterOfMass);
        public void Move(Vector3 _position) => buffer.Move(_position);

        public static ConvexPolygon Regular(int sides, float radius, float rotationRad=0) => Regular(sides, radius, Color4.White, rotationRad);
        public static ConvexPolygon Regular(int sides, float radius, Color4 color, float rotationRad=0)
        {
            var counterClockwiseVertices = new List<Vector3>();
            for(var i = 0; i < sides; i++)
            {
                var angle = Math.PI * 2 * (0.25f + i * 1.0f / sides) + rotationRad;
                counterClockwiseVertices.Add(radius * new Vector3((float)Math.Cos(angle), (float)Math.Sin(angle), 0));
            }
            return new ConvexPolygon(counterClockwiseVertices, color);
        }
        public static ConvexPolygon Square(float _size) => Square(_size, Color4.White);
        public static ConvexPolygon Square(float _size, Color4 _color) => Regular(4, _size * (float)Math.Sqrt(2));

        public void Render() => array.Render();
    }
}
