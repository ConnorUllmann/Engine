using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using Engine.OpenGL.Shaders;

namespace Engine.OpenGL.Colored
{
    public class ColoredVertexArray : VertexArray<ColoredVertex>
    {
        public ColoredVertexBuffer vertexBuffer;
        public ShaderProgram shaderProgram;
        private Vector3 positionPrev = Vector3.Zero;

        public ColoredVertexArray(ColoredVertexBuffer _vertexBuffer, ShaderProgram _shaderProgram) :
            base(_vertexBuffer, _shaderProgram, BasicVertexShader.Position, BasicVertexShader.Color)
        {
            vertexBuffer = _vertexBuffer;
            shaderProgram = _shaderProgram;
        }

        public void SetColor(Color4 _color) => vertexBuffer.SetColor(_color);

        public void Move(Vector3 _position)
        {
            positionPrev += _position;
            vertexBuffer.Move(_position);
        }

        public void Rotate(float _angleRad, Vector3 _center) => vertexBuffer.Rotate(_angleRad, _center);

        public void MoveTo(Vector3 positionCurr)
        {
            if (positionPrev == positionCurr)
                return;

            vertexBuffer.Move(positionCurr - positionPrev);
            positionPrev = positionCurr;
        }

        public void Render() => Render(positionPrev);
        public void Render(float _x, float _y) => Render(new Vector3(_x, _y, 0));
        public void Render(Vector3 positionCurr)
        {
            MoveTo(positionCurr);

            // bind vertex buffer and array objects
            vertexBuffer.Bind();
            Bind();

            // upload vertices to GPU and draw them
            vertexBuffer.BufferData();
            vertexBuffer.Draw();

            // reset state for potential further draw calls (optional, but good practice)
            //GL.BindVertexArray(0);
            //GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            //GL.UseProgram(0);
        }

        private static BasicShaderProgram basicShaderProgram;
        public static ColoredVertexArray FromBuffer(ColoredVertexBuffer buffer, ShaderProgram _shaderProgram = null)
        {
            if (_shaderProgram == null)
                basicShaderProgram = basicShaderProgram ?? new BasicShaderProgram();
            return new ColoredVertexArray(buffer, _shaderProgram ?? basicShaderProgram);
        }

        public static void Start()
        {
            basicShaderProgram = basicShaderProgram ?? new BasicShaderProgram();
            // activate shader program and set uniforms
            basicShaderProgram.Use();
            Game.Camera.ProjectionMatrix.Set(basicShaderProgram);
        }
    }
}
