using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using Engine.OpenGL.Colored;
using Basics;

namespace Engine
{
    public class ColoredVertexRenderer : IPosition
    {
        private ColoredVertexBuffer buffer => array.vertexBuffer;
        private ColoredVertexArray array;

        public float X { get => array?.X ?? 0; }
        public float Y { get => array?.Y ?? 0; }

        public bool Visible { get; set; } = true;

        public ColoredVertexRenderer() { }
        public ColoredVertexRenderer(ColoredVertexBuffer _buffer) => Initialize(_buffer);

        /// <summary>
        /// Used for resetting the renderer back to its base state so it can be recycled (e.g. used in an object pool)
        /// </summary>
        /// <param name="_buffer"></param>
        public void Initialize(ColoredVertexBuffer _buffer) => array = ColoredVertexArray.FromBuffer(_buffer);

        public void Destroy()
        {
            buffer.Destroy();
            array.Destroy();
        }

        public void SetColor(Color4 _color) => buffer.SetColor(_color);
        public void RandomizeColor() => buffer.RandomizeColor();

        /// <summary>
        /// Rotate the renderer by the given angle around the given point
        /// </summary>
        /// <param name="_angleRad">angle by which to rotate the renderer</param>
        /// <param name="_offset">position relative to the renderer's position around which to rotate (if none is given, the renderer rotates around its position)</param>
        public void RotateRelative(float _angleRad, Vector3? _offset) => array?.Rotate(_angleRad, (array?.Position ?? Vector3.Zero) + (_offset ?? Vector3.Zero));

        public void MoveRelative(Vector2 _position) => MoveRelative(_position.To3D());
        public void MoveRelative(IPosition _position) => MoveRelative(new Vector3(_position.X, _position.Y, 0));
        public void MoveRelative(float _x, float _y) => MoveRelative(new Vector3(_x, _y, 0));
        public void MoveRelative(Vector3 _position) => array?.MoveRelative(_position);

        public void MoveAbsolute(Vector2 _position) => MoveAbsolute(_position.To3D());
        public void MoveAbsolute(IPosition _position) => MoveAbsolute(new Vector3(_position.X, _position.Y, 0));
        public void MoveAbsolute(float _x, float _y) => MoveAbsolute(new Vector3(_x, _y, 0));
        public void MoveAbsolute(Vector3 _position) => array?.MoveAbsolute(_position);

        public virtual void Render()
        {
            if (Visible)
                array?.Render();
        }
    }
}
