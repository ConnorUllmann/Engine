using System;
using System.Collections.Generic;
using System.Text;
using Basics;

namespace Engine
{
    public class BoundingBox : Rectangle
    {
        public BoundingBox(float _w, float _h, Align.Horizontal _halign, Align.Vertical _valign)
            : base(_w * GetHAlignNormalizedOffset(_halign), _h * GetVAlignNormalizedOffset(_valign), _w, _h)
        {
        }

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
        
    }
}
