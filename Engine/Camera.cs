using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Engine.OpenGL;
using Engine.OpenGL.Shaders;

namespace Engine
{
    public class Camera
    {
        private Matrix4Uniform projectionMatrix;
        public Matrix4Uniform ProjectionMatrix => projectionMatrix;

        public Camera(int _width, int _height)
        {
            projectionMatrix = new Matrix4Uniform("projectionMatrix");
            projectionMatrix.Matrix = Matrix4.CreateOrthographic(_width, _height, float.MinValue, float.MaxValue);
        }
     }
}
