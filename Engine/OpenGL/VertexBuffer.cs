using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;

namespace Engine.OpenGL
{
    public class VertexBuffer<TVertex> where TVertex : struct
    {
        private readonly int vertexSize;
        protected TVertex[] vertices = new TVertex[4];
        private PrimitiveType primitiveType;

        protected int count;
        private readonly int handle;

        public VertexBuffer(int _vertexSize, PrimitiveType _primitiveType = PrimitiveType.Triangles)
        {
            vertexSize = _vertexSize;
            primitiveType = _primitiveType;

            handle = GL.GenBuffer();
        }

        public void Destroy() => GL.DeleteBuffer(handle);

        public void AddVertex(TVertex _vertex)
        {
            //Double size of array once we fill our current array
            if (count == vertices.Length)
                Array.Resize(ref vertices, count * 2);

            vertices[count++] = _vertex;
        }

        public void AddVertices(IEnumerable<TVertex> _vertices)
        {
            foreach (var v in _vertices)
                AddVertex(v);
        }

        public void ForEach(Action<TVertex> _func)
        {
            foreach (var vertex in vertices)
                _func(vertex);
        }

        public void Bind() => GL.BindBuffer(BufferTarget.ArrayBuffer, handle);
        public void BufferData() => GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertexSize * count), vertices, BufferUsageHint.StreamDraw);
        public void Draw() => GL.DrawArrays(primitiveType, 0, count);
    }
}
