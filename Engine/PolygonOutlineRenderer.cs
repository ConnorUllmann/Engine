using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Graphics;

namespace Engine
{
    public class PolygonOutlineRenderer : ColoredVertexRenderer
    {
        public PolygonOutlineRenderer(Polygon _polygon, float _x = 0, float _y = 0, Color4? _outlineColor = null)
            : base(_polygon.GetOutlineBuffer(_outlineColor ?? Color4.White))
        {
            MoveRelative(_x, _y);
        }
    }
}
