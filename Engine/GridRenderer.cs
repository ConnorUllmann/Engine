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
        protected readonly Grid<T> grid;
        protected ColoredVertexBuffer buffer;
        private readonly float tileWidth;
        private readonly float tileHeight;

        public GridRenderer(Grid<T> _grid, Func<T, Color4> _colorSelector, float _tileWidth, float _tileHeight)
        {
            grid = _grid;

            tileWidth = _tileWidth;
            tileHeight = _tileHeight;
            var rectanglesVertices = new List<IEnumerable<ColoredVertex>>();
            for (var i = 0; i < _grid.Width; i++)
            {
                for (var j = 0; j < _grid.Height; j++)
                {
                    var rectangle = new Rectangle(i * tileWidth, j * tileHeight, tileWidth, tileHeight);
                    var vertices = rectangle.ToVertices();
                    var tile = _grid.Get(i, j);
                    var color = _colorSelector(tile);
                    var coloredVertices = vertices.Select(v => new ColoredVertex(new Vector3(v.X, v.Y, 0), color));
                    rectanglesVertices.Add(coloredVertices);
                }
            }

            buffer = new ColoredVertexBuffer(PrimitiveType.Quads);
            buffer.AddVertices(rectanglesVertices.SelectMany(x => x));

            Initialize(buffer);
        }

        private float scale = 1;
        public float Scale
        {
            get => scale;
            set
            {
                if (value == scale)
                    return;
                buffer.Scale(scale == 0 ? value : value / scale);
                scale = value;
            }
        }

        private int bufferIndex(int _i, int _j) => (_i * grid.Height + _j) * 4;

        public void SetPixel(Color4 _color, int _i, int _j)
        {
            var index = bufferIndex(_i, _j);
            buffer.SetColor(_color, index);
            buffer.SetColor(_color, index+1);
            buffer.SetColor(_color, index+2);
            buffer.SetColor(_color, index+3);
        }

        public Color4 GetPixel(int _i, int _j) => buffer.GetColor(bufferIndex(_i, _j));
    }
}
