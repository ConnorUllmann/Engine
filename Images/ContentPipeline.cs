using System;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Images
{
    public class ContentPipeline
    {
        public static Texture2D LoadTexture(string filePath)
        {
            var x = new Bitmap("test.png");
            return new Texture2D(0, 0, 0);
        }
    }
}
