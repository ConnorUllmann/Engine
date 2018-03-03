using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Engine.OpenGL;

namespace Engine.OpenGL.Shaders
{
    public class BasicFragmentShader : FragmentShader
    {
        private const string text = 
@"#version 130
in vec4 fColor; // must match name in vertex shader
out vec4 fragColor; // first out variable is automatically written to the screen
void main() { fragColor = fColor; }";
        public BasicFragmentShader() : base(text) { }
    }

    public class FragmentShader : Shader
    {
        public FragmentShader(string code) : base(ShaderType.FragmentShader, code) { }
    }
}
