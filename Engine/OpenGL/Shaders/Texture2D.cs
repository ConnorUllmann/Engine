using System;
using System.Collections.Generic;
using System.Text;

namespace Engine.OpenGL.Shaders
{
    struct Texture2D
    {
        private int id;
        private int width;
        private int height;

        public int ID => id;
        public int Width => width;
        public int Height => height;

        // BMP, GIF, EXIF, JPG, PNG, TIFF
        public Texture2D(int _id, int _width, int _height)
        {
            id = _id;
            width = _width;
            height = _height;
        }
    }
}
