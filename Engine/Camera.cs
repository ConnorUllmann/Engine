using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Basics;
using Engine;
using Engine.OpenGL;
using Engine.OpenGL.Colored;
using Engine.OpenGL.Shaders;

namespace Engine
{
    public class Camera : IPosition
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
            RefreshProjectionMatrix();
        }

        public void RefreshProjectionMatrix()
        {
            projectionMatrix.Matrix = Matrix4.CreateTranslation(new Vector3(-x, -y, 0)) * Matrix4.CreateOrthographic(width, height, float.MinValue, float.MaxValue) * Matrix4.CreateScale(Scale);
            projectionMatrix.Set(BasicShaderProgram.Instance);
        }

        //x-position of center of camera view
        private float x = 0;
        public float X
        {
            get => x;
            set
            {
                if (x == value)
                    return;
                x = value;
                RefreshProjectionMatrix();
            }
        }

        //y-position of center of camera view
        private float y = 0;
        public float Y
        {
            get => y;
            set
            {
                if (y == value)
                    return;
                y = value;
                RefreshProjectionMatrix();
            }
        }

        private float scale = 1;
        public float Scale
        {
            get => scale;
            set
            {
                if (scale == value)
                    return;
                scale = value;
                RefreshProjectionMatrix();
            }
        }
    }
}
