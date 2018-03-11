using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using Basics;

namespace Engine
{
    public static class IPositionExtensions
    {
        public static Vector2 Subtract(this IPosition _to, IPosition _from)
            => new Vector2(_to.X - _from.X, _to.Y - _from.Y);
    }
}
