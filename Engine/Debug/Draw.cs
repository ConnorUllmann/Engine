using System;
using System.Collections.Generic;
using System.Linq;
using Basics;
using Engine;
using Engine.OpenGL;
using Engine.OpenGL.Colored;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Rectangle = Basics.Rectangle;

namespace Engine.Debug
{
    public static class Draw
    {
        private static Polygon debugPolygon;
        private static ColoredVertexRenderer debugPolygonRenderer(bool _filled, Color4 _color)
            => _filled ? (ColoredVertexRenderer)new PolygonFillRenderer(debugPolygon, 0, 0, _color)
                       : new PolygonOutlineRenderer(debugPolygon, 0, 0, _color);
        private static Polygon setRectangleDebugPolygon(float _w, float _h)
        {
            if (debugPolygon == null)
                debugPolygon = ConvexPolygon.Rectangle(_w, _h);
            else
                debugPolygon.Clone(BoundingBox.PointsFromRectangle(_w, _h));
            return debugPolygon;
        }

        public static void Rectangle(Rectangle _r, float _x=0, float _y=0, Color4? _color=null, bool _filled = true)
            => Rectangle(_x + _r.X, _y + _r.Y, _r.W, _r.H, _color, _filled);
        public static void Rectangle(float _x, float _y, float _w, float _h, Color4? _color=null, bool _filled = true)
        {
            setRectangleDebugPolygon(_w, _h);
            var renderer = debugPolygonRenderer(_filled, _color ?? Color4.White);
            renderer.Update(_x, _y);
            renderer.Render();
            renderer.Destroy();
        }

        public static void Line(Vector2 a, IPosition b, Color4? _color = null)
            => Line(a.X, a.Y, b.X, b.Y, _color);
        public static void Line(IPosition a, Vector2 b, Color4? _color = null)
            => Line(a.X, a.Y, b.X, b.Y, _color);
        public static void Line(Vector2 a, Vector2 b, Color4? _color = null)
            => Line(a.X, a.Y, b.X, b.Y, _color);
        public static void Line(IPosition a, IPosition b, Color4? _color = null)
            => Line(a.X, a.Y, b.X, b.Y, _color);
        public static void Line(float x1, float y1, float x2, float y2, Color4? _color=null)
        {
            //Create
            var color = _color ?? Color4.White;
            var buffer = new ColoredVertexBuffer(PrimitiveType.Lines);
            buffer.AddVertex(new ColoredVertex(new Vector3(x1, y1, 0), color));
            buffer.AddVertex(new ColoredVertex(new Vector3(x2, y2, 0), color));
            var array = ColoredVertexArray.FromBuffer(buffer);

            //Use
            array.Render();

            //Destroy
            buffer.Destroy();
            array.Destroy();
        }

        public static void Lines(IEnumerable<(Vector2 a, Vector2 b)> _positions, Color4? _color = null)
        {
            //Create
            var color = _color ?? Color4.White;
            var buffer = new ColoredVertexBuffer(PrimitiveType.Lines);
            foreach (var position in _positions)
            {
                buffer.AddVertex(new ColoredVertex(new Vector3(position.a.X, position.a.Y, 0), color));
                buffer.AddVertex(new ColoredVertex(new Vector3(position.b.X, position.b.Y, 0), color));
            }
            var array = ColoredVertexArray.FromBuffer(buffer);

            //Use
            array.Render();

            //Destroy
            buffer.Destroy();
            array.Destroy();
        }

        public static void LineStrip<T>(IEnumerable<T> _positions, Color4? _color = null) where T : IPosition
            => LineStrip(_positions.Select(p => new Vector2(p.X, p.Y)), _color);
        public static void LineStrip(IEnumerable<Vector2> _positions, Color4? _color = null)
        {
            //Create
            var color = _color ?? Color4.White;
            var buffer = new ColoredVertexBuffer(PrimitiveType.LineStrip);
            foreach (var position in _positions)
                buffer.AddVertex(new ColoredVertex(new Vector3(position.X, position.Y, 0), color));
            var array = ColoredVertexArray.FromBuffer(buffer);

            //Use
            array.Render();

            //Destroy
            buffer.Destroy();
            array.Destroy();
        }

        public static void Circle(IPosition _center, float _radius, Color4? _color = null)
            => Circle(_center.X, _center.Y, _radius, _color);
        public static void Circle(Vector2 _center, float _radius, Color4? _color = null)
            => Circle(_center.X, _center.Y, _radius, _color);
        public static void Circle(float _centerX, float _centerY, float _radius, Color4? _color = null)
        {
            var color = _color ?? Color4.White;
            var buffer = new ColoredVertexBuffer(PrimitiveType.LineLoop);

            var segments = 10 * Math.Sqrt(_radius);
            var theta = (float)(2 * Math.PI / segments);
            var cos = (float)Math.Cos(theta);
            var sin = (float)Math.Sin(theta);

            var x = _radius;
            var xt = 0f;
            var y = 0f;

            for (var i = 0; i < segments; i++)
            {
                var vertex = new Vector3(x + _centerX, y + _centerY, 0);
                buffer.AddVertex(new ColoredVertex(vertex, color));

                xt = x;
                x = cos * x - sin * y;
                y = sin * xt + cos * y;
            }
            var array = ColoredVertexArray.FromBuffer(buffer);

            //Use
            array.Render();

            //Destroy
            buffer.Destroy();
            array.Destroy();
        }
    }
}
