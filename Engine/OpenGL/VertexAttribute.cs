using OpenTK.Graphics.OpenGL;
using Engine.OpenGL.Shaders;

namespace Engine.OpenGL
{
    public struct VertexAttribute
    {
        private readonly string name;
        private readonly int size;
        private readonly VertexAttribPointerType type;
        private readonly bool normalize;
        private readonly int stride;
        private readonly int offset;

        public VertexAttribute(string _name, int _size, VertexAttribPointerType _type,
            int _stride, int _offset, bool _normalize = false)
        {
            name = _name;
            size = _size;
            type = _type;
            stride = _stride;
            offset = _offset;
            normalize = _normalize;
        }

        public void Set(ShaderProgram program)
        {
            // get location of attribute from shader program
            int index = program.GetAttributeLocation(name);

            // enable and set attribute
            GL.EnableVertexAttribArray(index);
            GL.VertexAttribPointer(index, size, type, normalize, stride, offset);
        }
    }
}
