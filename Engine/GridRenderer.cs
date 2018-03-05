using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Basics;
using Engine;
using Engine.OpenGL;
using Engine.OpenGL.Colored;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Rectangle = Basics.Rectangle;

namespace Engine
{
    public class GridRenderer<T>
    {
        private Grid<T> grid;

        public GridRenderer(Grid<T> _grid, float _tileWidth, float _tileHeight)
        {
            grid = _grid;

            var rectangles = new List<Rectangle>();
            for (var i = 0; i < _grid.Width; i++)
                for (var j = 0; j < _grid.Height; j++)
                    rectangles.Add(new Rectangle(i * _tileWidth, j * _tileHeight, _tileWidth, _tileHeight));
            var vertices = rectangles.SelectMany(o => o.ToVertices().Select(p => new ColoredVertex(new Vector3(p.X, p.Y, 0), Color4.Aqua)));

            var buffer = new ColoredVertexBuffer(PrimitiveType.Quads);
            buffer.AddVertices(vertices);
            var array = ColoredVertexArray.FromBuffer(buffer);
        }
    }
}
