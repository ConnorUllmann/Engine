using System;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace Engine.OpenGL.Colored
{
    public class ColoredVertexBuffer : VertexBuffer<ColoredVertex>
    {
        public ColoredVertexBuffer(PrimitiveType _primitiveType = PrimitiveType.Triangles)
            : base(ColoredVertex.Size, _primitiveType) { }

        public void Move(int _index, Vector3 _position)
        {
            if (_index >= 0 && _index < vertices.Length)
                vertices[_index].position = _position;
        }

        public void MoveRelative(Vector3 _position)
        {
            for (var i = 0; i < count; i++)
                vertices[i].position += _position;
        }

        public void Scale(float _scalar)
        {
            for (var i = 0; i < count; i++)
                vertices[i].position *= _scalar;
        }

        public void Rotate(float _angleRad, Vector3 _center)
        {
            for (var i = 0; i < count; i++)
                vertices[i].position = vertices[i].position.Rotate(_angleRad, _center);
        }

        public void SetColor(Color4 _color)
        {
            for (var i = 0; i < count; i++)
                vertices[i].color = _color;
        }

        public void SetColor(Color4 _color, int _i)
        {
            if (_i < 0 || _i >= count)
                throw new ArgumentException($"Index ({_i}) is out of bounds [0, {count})");
            vertices[_i].color = _color;
        }

        public Color4 GetColor(int _i)
        {
            if (_i < 0 || _i >= count)
                throw new ArgumentException($"Index ({_i}) is out of bounds [0, {count})");
            return vertices[_i].color;
        }

        public void RandomizeColor()
        {
            for (var i = 0; i < count; i++)
                vertices[i].color = ColorExtensions.RandomColor();
        }
    }
}
