using System;
using System.Collections.Generic;
using System.Text;
using Basics;
using OpenTK;
using Rectangle = Basics.Rectangle;

namespace Engine
{
    public struct BoundingBox : IPosition
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float W;
        public float H;

        public BoundingBox(float _x, float _y, float _w, float _h)
        {
            X = _x;
            Y = _y;
            W = _w;
            H = _h;
        }
        public BoundingBox(float _w, float _h, Align.Horizontal _halign, Align.Vertical _valign)
            : this(_w * GetHAlignNormalizedOffset(_halign), _h * GetVAlignNormalizedOffset(_valign), _w, _h)
        { }

        public void UpdateSize(float _w, float _h, Align.Horizontal _halign, Align.Vertical _valign)
        {
            UpdateWidth(_w, _halign);
            UpdateHeight(_h, _valign);
        }

        public void UpdateWidth(float _w, Align.Horizontal _halign)
        {
            W = _w;
            SetHorizontalAlign(_halign);
        }

        public void UpdateHeight(float _h, Align.Vertical _valign)
        {
            H = _h;
            SetVerticalAlign(_valign);
        }

        public void SetHorizontalAlign(Align.Horizontal _halign) => X = W * GetHAlignNormalizedOffset(_halign);
        private static float GetHAlignNormalizedOffset(Align.Horizontal _align)
        {
            switch (_align)
            {
                case Align.Horizontal.Left: return 0;
                case Align.Horizontal.Center: return -0.5f;
                case Align.Horizontal.Right: return -1;
                default: throw new Exception($"Unexpected HorizontalAlign type {_align}");
            }
        }

        public void SetVerticalAlign(Align.Vertical _valign) => Y = H * GetVAlignNormalizedOffset(_valign);
        private static float GetVAlignNormalizedOffset(Align.Vertical _align)
        {
            switch (_align)
            {
                case Align.Vertical.Top: return 0;
                case Align.Vertical.Middle: return -0.5f;
                case Align.Vertical.Bottom: return -1;
                default: throw new Exception($"Unexpected VerticalAlign type {_align}");
            }
        }

        //Collide against _r if we were offset by (_xoff, _yoff)
        public bool Collides(Rectangle _r, float _xoff, float _yoff) 
            => Collides(_r.X, _r.Y, _r.W, _r.H, _xoff, _yoff);
        public bool Collides(float _x, float _y, float _w, float _h, float _xoff, float _yoff)
            => Rectangle.Collide(_x, _y, _w, _h, _xoff + X, _yoff + Y, W, H);

        public Rectangle ToRectangle() => new Rectangle(X, Y, W, H);
        public static Rectangle RectangleFromPoints(IEnumerable<Vector3> _points)
        {
            float? minX = null, minY = null, maxX = null, maxY = null;
            foreach (var v in _points)
            {
                if (!minX.HasValue || v.X < minX.Value)
                    minX = v.X;
                if (!maxX.HasValue || v.X > maxX)
                    maxX = v.X;
                if (!minY.HasValue || v.Y < minY)
                    minY = v.Y;
                if (!maxY.HasValue || v.Y > maxY)
                    maxY = v.Y;
            }
            return new Rectangle(minX ?? 0, minY ?? 0, (maxX - minX) ?? 0, (maxY - minY) ?? 0);
        }

        public static List<Vector3> PointsFromRectangle(Rectangle _r)
            => PointsFromRectangle(_r.X, _r.Y, _r.W, _r.H);
        public static List<Vector3> PointsFromRectangle(float _w, float _h)
            => PointsFromRectangle(0, 0, _w, _h);
        public static List<Vector3> PointsFromRectangle(float _x, float _y, float _w, float _h)
            => new List<Vector3>()
            {
                new Vector3(_x, _y, 0),
                new Vector3(_x + _w, _y, 0),
                new Vector3(_x + _w, _y + _h, 0),
                new Vector3(_x, _y + _h, 0),
            };

        /// <summary>
        /// Sets this bounding box's position/dimensions to match the given rectangle
        /// </summary>
        /// <param name="_rectangle">rectangle to match</param>
        public void MatchPositionAndDimensions(Rectangle _rectangle)
        {
            X = _rectangle.X;
            Y = _rectangle.Y;
            W = _rectangle.W;
            H = _rectangle.H;
        }
        public void MatchPositionAndDimensions(BoundingBox _box) => MatchPositionAndDimensions(_box.ToRectangle());
    }
}
