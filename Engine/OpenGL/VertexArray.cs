using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Graphics.OpenGL;
using Engine.OpenGL;
using Engine.OpenGL.Shaders;

namespace Engine.OpenGL
{
    public class VertexArray<TVertex> where TVertex : struct
    {
        private readonly int handle;

        public VertexArray(VertexBuffer<TVertex> vertexBuffer, ShaderProgram program, params VertexAttribute[] attributes)
        {
            // create new vertex array object
            GL.GenVertexArrays(1, out handle);

            // bind the object so we can modify it
            Bind();

            // bind the vertex buffer object
            vertexBuffer.Bind();

            // set all attributes
            foreach (var attribute in attributes)
                attribute.Set(program);

            // unbind objects to reset state
            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        
        /// <summary>
        /// Bind for usage (modification or rendering)
        /// </summary>
        public void Bind() => GL.BindVertexArray(handle);
    }
}
