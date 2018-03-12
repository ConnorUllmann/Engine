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
        /// <summary>
        /// Adds the given value (0-255) to each RGB component to create a new color
        /// </summary>
        /// <param name="_color">the color to lighten</param>
        /// <param name="_lighten">value between 0-255 for how much to lighten each color component (other than alpha)</param>
        /// <returns>a new color with the values of the given color after it has been lightened</returns>
        public static Color4 Lightened(this Color4 _color, float _lighten)
        {
            _lighten /= 255f;
            return new Color4(_color.R + _lighten, _color.G + _lighten, _color.B + _lighten, _color.A);
        }
        public static Color4 Darkened(this Color4 _color, float _darken) => _color.Lightened(-_darken);
    }
}
