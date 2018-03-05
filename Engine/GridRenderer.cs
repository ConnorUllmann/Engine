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
    public class GridRenderer<T> : ColoredVertexRenderer
    {
        private Grid<T> grid;

        public GridRenderer(Grid<T> _grid, Func<T, Color4> _colorSelector, float _tileWidth, float _tileHeight)
        {
            grid = _grid;
            Initialize(GetBuffer(_grid, _colorSelector, _tileWidth, _tileHeight));
        }

        public ColoredVertexBuffer GetBuffer(Grid<T> _grid, Func<T, Color4> _colorSelector, float _tileWidth, float _tileHeight)
        {
            var rectanglesVertices = new List<IEnumerable<ColoredVertex>>();
            for (var i = 0; i < _grid.Width; i++)
            {
                for (var j = 0; j < _grid.Height; j++)
                {
                    var rectangle = new Rectangle(i * _tileWidth, j * _tileHeight, _tileWidth, _tileHeight);
                    var vertices = rectangle.ToVertices();
                    var tile = _grid.Get(i, j);
                    var color = _colorSelector(tile);
                    var coloredVertices = vertices.Select(v => new ColoredVertex(new Vector3(v.X, v.Y, 0), color));
                    rectanglesVertices.Add(coloredVertices);
                }
            }

            var buffer = new ColoredVertexBuffer(PrimitiveType.Quads);
            buffer.AddVertices(rectanglesVertices.SelectMany(x => x));
            return buffer;
        }
    }
}
