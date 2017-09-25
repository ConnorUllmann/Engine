﻿using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using Engine.Shaders;
using System.Linq;

namespace Engine
{
    public struct ColoredVertex
    {
        public const int Size = (3 + 4) * 4;

        public Vector3 position;
        public Color4 color;

        public ColoredVertex(Vector3 _position, Color4 _color)
        {
            position = _position;
            color = _color;
        }

        private VertexAttribute[] Attributes() => new VertexAttribute[]
        {
            new VertexAttribute("vPosition", 3, VertexAttribPointerType.Float, Size, 0),
            new VertexAttribute("vColor", 4, VertexAttribPointerType.Float, Size, 12)
        };
    }
    
    public class ColoredRectangleBuffer : ColoredVertexBuffer
    {
        private float x;
        private float y;
        private float width;
        private float height;

        public ColoredRectangleBuffer(float x, float y, float width, float height, float depth = 0) :
            this(x, y, width, height, depth, Color4.White, Color4.White, Color4.White, Color4.White)
        { }
        public ColoredRectangleBuffer(float x, float y, float width, float height, Color4 color, float depth = 0) :
            this(x, y, width, height, depth, color, color, color, color)
        { }
        public ColoredRectangleBuffer(float _x, float _y, float _width, float _height, float depth,
            Color4 _bottomLeftCorner, Color4 _bottomRightCorner, Color4 _topRightCorner, Color4 _topLeftCorner) :
            base(PrimitiveType.Quads)
        {
            x = _x;
            y = _y;
            width = _width;
            height = _height;

            var a = x + width;
            var b = y + height;

            AddVertex(new ColoredVertex(new Vector3(a, y, depth), _bottomRightCorner));
            AddVertex(new ColoredVertex(new Vector3(x, y, depth), _bottomLeftCorner));
            AddVertex(new ColoredVertex(new Vector3(x, b, depth), _topLeftCorner));
            AddVertex(new ColoredVertex(new Vector3(a, b, depth), _topRightCorner));
        }

        public float X
        {
            get => x;
            set
            {
                Move(new Vector3(value - x, 0, 0));
                x = value;
            }
        }

        public float Y
        {
            get => y;
            set
            {
                Move(new Vector3(0, value - y, 0));
                y = value;
            }
        }

        public float Width
        {
            get => width;
            set
            {
                var diff = value - width;
                vertices[0].position.X += diff;
                vertices[3].position.X += diff;
            }
        }

        public float Height
        {
            get => height;
            set
            {
                var diff = height - value;
                vertices[0].position.Y += diff;
                vertices[1].position.Y += diff;
            }
        }
    }

    public class ColoredVertexBuffer : VertexBuffer<ColoredVertex>
    {
        private ColoredVertexArray array;

        public ColoredVertexBuffer(PrimitiveType _primitiveType = PrimitiveType.Triangles) : base(ColoredVertex.Size, _primitiveType) { }

        public void Move(int _index, Vector3 _position)
        {
            if (_index >= 0 && _index < vertices.Length)
                vertices[_index].position = _position;
        }

        public void Move(Vector3 _position)
        {
            for (var i = 0; i < count; i++)
                vertices[i].position += _position;
        }

        public void Rotate(float angle, Vector3 center)
        {
            for (var i = 0; i < count; i++)
            {
                var diff = vertices[i].position - center;
                diff = new Vector3(
                    (float)(diff.X * Math.Cos(angle) - diff.Y * Math.Sin(angle)),
                    (float)(diff.Y * Math.Cos(angle) + diff.X * Math.Sin(angle)),
                    diff.Z);
                vertices[i].position = diff + center;
            }
        }

        public void SetColor(Color4 _color)
        {
            for (var i = 0; i < count; i++)
                vertices[i].color = _color;
        }

        public void Render()
        {
            if(array == null)
                array = ColoredVertexArray.FromBuffer(this);
            array.Render();
        }
    }

    public class VertexBuffer<TVertex> where TVertex : struct
    {
        private readonly int vertexSize;
        protected TVertex[] vertices = new TVertex[4];
        private PrimitiveType primitiveType;

        protected int count;
        private readonly int handle;

        public VertexBuffer(int _vertexSize, PrimitiveType _primitiveType=PrimitiveType.Triangles)
        {
            vertexSize = _vertexSize;
            primitiveType = _primitiveType;

            this.handle = GL.GenBuffer();
        }

        public void AddVertex(TVertex v)
        {
            //Double size of array once we fill our current array
            if (count == vertices.Length)
                Array.Resize(ref vertices, count * 2);

            vertices[count++] = v;
        }

        public void ForEach(Action<TVertex> func)
        {
            foreach (var vertex in vertices)
                func(vertex);
        }

        public void Bind()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, this.handle);
        }

        public void BufferData()
        {
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertexSize * count), vertices, BufferUsageHint.StreamDraw);
        }

        public void Draw()
        {
            GL.DrawArrays(primitiveType, 0, count);
        }

    }

    public class ColoredVertexArray : VertexArray<ColoredVertex>
    {
        public VertexBuffer<ColoredVertex> vertexBuffer;
        public ShaderProgram shaderProgram;

        public ColoredVertexArray(VertexBuffer<ColoredVertex> _vertexBuffer, ShaderProgram _shaderProgram) :
            base(_vertexBuffer, _shaderProgram, BasicVertexShader.Position, BasicVertexShader.Color)
        {
            vertexBuffer = _vertexBuffer;
            shaderProgram = _shaderProgram;
        }

        public void SetColor(Color4 _color) => vertexBuffer.ForEach(o => o.color = _color);

        public void Render() => Render(Game.Camera);
        private void Render(Camera _camera) => Render(_camera.ProjectionMatrix);
        private void Render(Matrix4Uniform _projectionMatrix)
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
        

        private static BasicShaderProgram basicShaderProgram;
        public static ColoredVertexArray FromBuffer(VertexBuffer<ColoredVertex> buffer, ShaderProgram _shaderProgram = null)
        {
            if (_shaderProgram == null && basicShaderProgram == null)
                basicShaderProgram = new BasicShaderProgram();
            return new ColoredVertexArray(buffer, _shaderProgram ?? basicShaderProgram);
        }

        public static void Start()
        {
            if (basicShaderProgram == null)
                basicShaderProgram = new BasicShaderProgram();
            // activate shader program and set uniforms
            basicShaderProgram.Use();
            Game.Camera.ProjectionMatrix.Set(basicShaderProgram);
        }
    }

    public class VertexArray<TVertex> where TVertex : struct
    {
        private readonly int handle;

        public VertexArray(VertexBuffer<TVertex> vertexBuffer, ShaderProgram program, params VertexAttribute[] attributes)
        {
            // create new vertex array object
            GL.GenVertexArrays(1, out this.handle);

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

        public void Bind()
        {
            // bind for usage (modification or rendering)
            GL.BindVertexArray(this.handle);
        }
    }

    public class VertexAttribute
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

    public class Matrix4Uniform
    {
        private readonly string name;
        private Matrix4 matrix;

        public Matrix4 Matrix { get { return this.matrix; } set { this.matrix = value; } }

        public Matrix4Uniform(string name)
        {
            this.name = name;
        }

        public void Set(ShaderProgram program)
        {
            // get uniform location
            var i = program.GetUniformLocation(this.name);

            // set uniform value
            GL.UniformMatrix4(i, false, ref this.matrix);
        }
    }
}
