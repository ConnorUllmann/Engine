using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Graphics;

namespace Engine
{
    public class PolygonFillRenderer : ColoredVertexRenderer
    {
        public PolygonFillRenderer(Polygon _polygon, float _x = 0, float _y = 0, Color4? _fillColor = null)
            : base(_polygon.GetFillBuffer(_fillColor ?? Color4.White))
        {
            MoveRelative(_x, _y);
        }
    }
}
