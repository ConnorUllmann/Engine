using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.IO;
using Engine.OpenGL.Colored;

namespace Engine.OpenGL.Shaders
{
    public class BasicVertexShader : VertexShader
    {
        private const string text =
@"#version 130
// a projection transformation to apply to the vertex' position
uniform mat4 projectionMatrix;
// attributes of our vertex
in vec3 vPosition;
in vec4 vColor;
out vec4 fColor; // must match name in fragment shader
void main()
{
    // gl_Position is a special variable of OpenGL that must be set
    gl_Position = projectionMatrix * vec4(vPosition, 1.0);
    fColor = vColor;
}";
        public BasicVertexShader() : base(text) { }

        public readonly static VertexAttribute Position = new VertexAttribute("vPosition", 3, VertexAttribPointerType.Float, ColoredVertex.Size, 0);
        public readonly static VertexAttribute Color = new VertexAttribute("vColor", 4, VertexAttribPointerType.Float, ColoredVertex.Size, 12);
    }

    public class VertexShader : Shader
    {
        public VertexShader(string code) : base(ShaderType.VertexShader, code) { }
    }
}
