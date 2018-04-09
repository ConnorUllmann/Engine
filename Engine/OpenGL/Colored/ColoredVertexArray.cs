using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using Engine.OpenGL.Shaders;
using Basics;

namespace Engine.OpenGL.Colored
{
    public class ColoredVertexArray : VertexArray<ColoredVertex>, IPosition
    {
        public ColoredVertexBuffer vertexBuffer;
        public ShaderProgram shaderProgram;
        public Vector3 Position = Vector3.Zero;
        public float X { get => Position.X; }
        public float Y { get => Position.Y; }

        public ColoredVertexArray(ColoredVertexBuffer _vertexBuffer, ShaderProgram _shaderProgram) :
            base(_vertexBuffer, _shaderProgram, BasicVertexShader.Position, BasicVertexShader.Color)
        {
            vertexBuffer = _vertexBuffer;
            shaderProgram = _shaderProgram;
        }

        public void SetColor(Color4 _color) => vertexBuffer.SetColor(_color);

        public void MoveRelative(Vector3 _position)
        {
            Position += _position;
            vertexBuffer.MoveRelative(_position);
        }

        /// <summary>
        /// Rotates the buffer by the given angle around the given point
        /// </summary>
        /// <param name="_angleRad">angle by which to rotate the buffer</param>
        /// <param name="_center">position relative to the screen around which to rotate (if no position is given, use the last position the buffer moved to)</param>
        public void Rotate(float _angleRad, Vector3? _center=null) => vertexBuffer.Rotate(_angleRad, _center ?? Position);

        public void MoveAbsolute(Vector3 newPosition)
        {
            if (Position == newPosition)
                return;

            vertexBuffer.MoveRelative(newPosition - Position);
            Position = newPosition;
        }

        /// <summary>
        /// Render the vertex array at its current position
        /// </summary>
        public void Render() => render(Position);
        private void render(Vector3 positionCurr)
        {
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
        
        public static ColoredVertexArray FromBuffer(ColoredVertexBuffer buffer, ShaderProgram _shaderProgram = null) => new ColoredVertexArray(buffer, _shaderProgram ?? BasicShaderProgram.Instance);

        public static void Start()
        {
            // activate shader program and set uniforms
            BasicShaderProgram.Instance.Use();

            // This setter sets the projection matrix to use BasicShaderProgram.Instance
            Game.Camera.Zoom = 1;
        }
    }
}
