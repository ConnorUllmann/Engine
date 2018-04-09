using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Engine;
using Engine.OpenGL;
using Engine.OpenGL.Colored;
using Engine.OpenGL.Shaders;

namespace Engine
{
    public class Camera
    {
        private readonly int width;
        private readonly int height;

        private Matrix4Uniform projectionMatrix;
        public Matrix4Uniform ProjectionMatrix => projectionMatrix;

        public Camera(int _width, int _height)
        {
            width = _width;
            height = _height;
            projectionMatrix = new Matrix4Uniform("projectionMatrix");
        }

        private float zoom = 1;
        public float Zoom
        {
            get => zoom;
            set
            {
                zoom = value;
                projectionMatrix.Matrix = Matrix4.CreateOrthographic(width / zoom, height / zoom, float.MinValue, float.MaxValue);
                ProjectionMatrix.Set(BasicShaderProgram.Instance);
            }
        }
    }
}
