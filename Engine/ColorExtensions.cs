using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Graphics;

namespace Engine
{
    public static class ColorExtensions
    {
        private static Random random = new Random();
        public static Color4 RandomColor() => new Color4((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble(), 1.0f);
    }
}
