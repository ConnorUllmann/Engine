using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using System.Linq;
using Engine.OpenGL;
using Engine.OpenGL.Shaders;

namespace Engine.OpenGL.Colored
{
    public class ColoredRectangleBuffer : ColoredVertexBuffer
    {
        private float x;
        private float y;
        private float width;
        private float height;

        public ColoredRectangleBuffer(float x, float y, float width, float height, float depth = 0) :
            this(x, y, width, height, depth, Color4.White, Color4.White, Color4.White, Color4.White)
        { }
        public ColoredRectangleBuffer(float x, float y, float width, float height, Color4 color, float depth = 0) :
            this(x, y, width, height, depth, color, color, color, color)
        { }
        public ColoredRectangleBuffer(float _x, float _y, float _width, float _height, float depth,
            Color4 _bottomLeftCorner, Color4 _bottomRightCorner, Color4 _topRightCorner, Color4 _topLeftCorner) :
            base(PrimitiveType.Quads)
        {
            x = _x;
            y = _y;
            width = _width;
            height = _height;

            AddVertex(new ColoredVertex(new Vector3(x + width, y, depth), _bottomRightCorner));
            AddVertex(new ColoredVertex(new Vector3(x, y, depth), _bottomLeftCorner));
            AddVertex(new ColoredVertex(new Vector3(x, y + height, depth), _topLeftCorner));
            AddVertex(new ColoredVertex(new Vector3(x + width, y + height, depth), _topRightCorner));
        }

        public float X
        {
            get => x;
            set
            {
                MoveRelative(new Vector3(value - x, 0, 0));
                x = value;
            }
        }

        public float Y
        {
            get => y;
            set
            {
                MoveRelative(new Vector3(0, value - y, 0));
                y = value;
            }
        }

        public float Width
        {
            get => width;
            set
            {
                var diff = value - width;
                vertices[0].position.X += diff;
                vertices[3].position.X += diff;
                width = value;
            }
        }

        public float Height
        {
            get => height;
            set
            {
                var diff = height - value;
                vertices[0].position.Y += diff;
                vertices[1].position.Y += diff;
                height = value;
            }
        }
    }
}
