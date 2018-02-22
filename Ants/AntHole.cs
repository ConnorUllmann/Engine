using System;
using System.Collections.Generic;
using System.Text;
using Basics;
using Engine;
using OpenTK.Graphics;

namespace Ants
{
    public class AntHole : Actor
    {
        private AntGrid overworldGrid;
        private AntGrid underworldGrid;

        private PolygonRenderer renderer;

        public AntHole(float _x, float _y, AntGrid _overworld, AntGrid _underworld) : base(_x, _y)
        {
            overworldGrid = _overworld;
            underworldGrid = _underworld;
            renderer = new PolygonFillRenderer(ConvexPolygon.Square(overworldGrid.TileSize * 2), Color4.Black);
        }

        public override void Update()
        {
        }

        public override void Render() => renderer?.Render(X, Y);
    }
}
