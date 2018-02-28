using System;
using System.Collections.Generic;
using System.Text;
using Basics;
using Engine;

namespace Engine.Debug
{
    public static class Draw
    {
        public static void Rectangle(Rectangle _r, float _x=0, float _y=0, bool _filled = true)
            => Rectangle(_x + _r.X, _y + _r.Y, _r.W, _r.H, _filled);
        public static void Rectangle(float _x, float _y, float _w, float _h, bool _filled = true)
        {
            var polygon = ConvexPolygon.Rectangle(_w, _h);
            var renderer = _filled ? (PolygonRenderer)new PolygonFillRenderer(polygon) : new PolygonOutlineRenderer(polygon);
            renderer.Render(_x, _y);
        }
    }
}
