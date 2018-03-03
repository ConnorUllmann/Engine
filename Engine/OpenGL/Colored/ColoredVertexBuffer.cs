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

        public void Move(Vector3 _position)
        {
            for (var i = 0; i < count; i++)
                vertices[i].position += _position;
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

        public void RandomizeColor()
        {
            for (var i = 0; i < count; i++)
                vertices[i].color = ColorExtensions.RandomColor();
        }
    }
}
