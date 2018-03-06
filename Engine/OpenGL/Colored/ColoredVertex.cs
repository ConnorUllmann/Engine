using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Linq;
using Basics;
using Engine.OpenGL;
using Engine.OpenGL.Shaders;

namespace Engine.OpenGL.Colored
{
    public struct ColoredVertex : IPosition
    {
        public const int Size = (3 + 4) * 4;

        public Vector3 position;
        public Color4 color;

        public float X { get => position.X; }
        public float Y { get => position.Y; }

        public ColoredVertex(Vector3 _position, Color4 _color)
        {
            position = _position;
            color = _color;
        }

        private VertexAttribute[] Attributes() => new VertexAttribute[]
        {
            new VertexAttribute("vPosition", 3, VertexAttribPointerType.Float, Size, 0),
            new VertexAttribute("vColor", 4, VertexAttribPointerType.Float, Size, 12)
        };
    }
}
