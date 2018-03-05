using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using Engine.OpenGL.Colored;

namespace Engine
{
    public abstract class ColoredVertexRenderer
    {
        private ColoredVertexBuffer buffer => array.vertexBuffer;
        private ColoredVertexArray array;

        public bool Visible { get; set; } = true;

        public ColoredVertexRenderer(ColoredVertexBuffer _buffer)
        {
            array = ColoredVertexArray.FromBuffer(_buffer);
        }

        public void Destroy()
        {
            buffer.Destroy();
            array.Destroy();
        }

        public void SetColor(Color4 _color) => buffer.SetColor(_color);
        public void RandomizeColor() => buffer.RandomizeColor();

        public void Rotate(float _angleRad, Vector3 _center) => array?.Rotate(_angleRad, _center);
        public void Move(float _x, float _y) => Move(new Vector3(_x, _y, 0));
        public void Move(Vector3 _position) => array?.Move(_position);

        public void Render(float _x, float _y) => Render(new Vector3(_x, _y, 0));
        public void Render(Vector3 _position)
        {
            if (Visible)
                array?.Render(_position);
        }
    }

    public class PolygonOutlineRenderer : ColoredVertexRenderer
    {
        public PolygonOutlineRenderer(Polygon _polygon, float _x = 0, float _y = 0, Color4? _outlineColor = null) 
            : base(_polygon.GetOutlineBuffer(_outlineColor ?? Color4.White))
        {
            Move(_x, _y);
        }
    }

    public class PolygonFillRenderer : ColoredVertexRenderer
    {
        public PolygonFillRenderer(Polygon _polygon, float _x = 0, float _y = 0, Color4? _fillColor = null) 
            : base(_polygon.GetFillBuffer(_fillColor ?? Color4.White))
        {
            Move(_x, _y);
        }
    }
}
