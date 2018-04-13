using OpenTK.Graphics.OpenGL;
using OpenTK;
using Engine.OpenGL.Shaders;

namespace Engine.OpenGL
{
    public class Matrix4Uniform
    {
        private readonly string name;
        private Matrix4 matrix;

        public Matrix4 Matrix { get => matrix; set => matrix = value; }

        public Matrix4Uniform(string _name)
        {
            name = _name;
        }

        public void Set(ShaderProgram program)
        {
            // get uniform location
            var i = program.GetUniformLocation(name);

            // set uniform value
            GL.UniformMatrix4(i, false, ref matrix);
        }
    }
}
