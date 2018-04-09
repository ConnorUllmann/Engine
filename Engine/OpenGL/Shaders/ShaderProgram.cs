using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Engine.OpenGL.Shaders
{

    public class BasicShaderProgram : ShaderProgram
    {
        public static BasicShaderProgram Instance = new BasicShaderProgram();
        public BasicShaderProgram() : base(new BasicVertexShader(), new BasicFragmentShader()) { }
    }

    public class ShaderProgram
    {
        private readonly int handle;

        public ShaderProgram(params Shader[] shaders)
        {
            handle = GL.CreateProgram();

            foreach (var shader in shaders)
                GL.AttachShader(handle, shader.Handle);

            //Link (compile) program
            GL.LinkProgram(handle);

            foreach (var shader in shaders)
                GL.DetachShader(handle, shader.Handle);

        }

        public int GetAttributeLocation(string name)
        {
            // get the location of a vertex attribute
            return GL.GetAttribLocation(this.handle, name);
        }

        public int GetUniformLocation(string name)
        {
            // get the location of a uniform variable
            return GL.GetUniformLocation(this.handle, name);
        }

        public void Use()
        {
            GL.UseProgram(handle);
        }
    }
}
