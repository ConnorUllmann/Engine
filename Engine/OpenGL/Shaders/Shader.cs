using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Engine.OpenGL.Shaders;

namespace Engine.OpenGL.Shaders
{
    public class Shader
    {
        private readonly int handle;
        public int Handle => handle;

        public Shader(ShaderType type, string code)
        {
            handle = GL.CreateShader(type);

            //Set source and compile shader
            GL.ShaderSource(handle, code);
            GL.CompileShader(handle);
        }
    }    
}
