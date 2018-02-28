using System;
using System.Collections.Generic;
using System.Text;
using Basics;
using Engine;
using OpenTK.Graphics;

namespace Engine.Debug
{
    public static class Draw
    {
        public static void Rectangle(Rectangle _r, float _x=0, float _y=0, Color4? _color=null, bool _filled = true)
            => Rectangle(_x + _r.X, _y + _r.Y, _r.W, _r.H, _color, _filled);
        public static void Rectangle(float _x, float _y, float _w, float _h, Color4? _color=null, bool _filled = true)
        {
            var polygon = ConvexPolygon.Rectangle(_w, _h);
            _color = _color ?? Color4.White;
            var renderer = _filled ? (PolygonRenderer)new PolygonFillRenderer(polygon, _color.Value) : new PolygonOutlineRenderer(polygon, _color.Value);
            renderer.Render(_x, _y);
        }
    }
}
