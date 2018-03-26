﻿using System;
using System.Collections.Generic;
using System.Text;
using Basics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Engine.OpenGL.Colored;

namespace Engine
{
    public class VectorTile : PolygonActor
    {
        private int i;
        private int j;
        public Vector2 Vector { get; private set; }
        public void SetColor(Color4 _color) => FillRenderer.SetColor(_color);
        
        private const float vectorLineLength = 10;

        public VectorTile(int _i, int _j, VectorGrid _grid, Vector2 _vector)
            : base(ConvexPolygon.Square(_grid.CellSize))
        {
            i = _i;
            j = _j;
            X = (i + 0.5f) * _grid.CellSize + _grid.X;
            Y = (j + 0.5f) * _grid.CellSize + _grid.Y;

            Vector = _vector;
            
            FillVisible = false;
            OutlineVisible = false;
        }

        public override void Render()
        {
            base.Render();
            Debug.Draw.Line(Position, Position + vectorLineLength * Vector.Normalized(), Color4.White);
        }
    }

    public class VectorGrid : Grid<VectorTile>, IPosition
    {
        public float X { get; private set; }
        public float Y { get; private set; }

        public readonly float WorldWidth;
        public readonly float WorldHeight;
        public float CellSize;

        public VectorGrid(float _x, float _y, float _worldWidth, float _worldHeight, float _cellSize)
            : base((int)Math.Ceiling(_worldWidth/_cellSize), (int)Math.Ceiling(_worldHeight/_cellSize))
        {
            X = _x;
            Y = _y;
            WorldWidth = _worldWidth;
            WorldHeight = _worldHeight;
            CellSize = _cellSize;

            for (var x = 0; x < Width; x++)
            {
                for (var y = 0; y < Height; y++)
                {
                    var tile = new VectorTile(x, y, this, Vector2.Zero);
                    Set(tile, x, y);
                    tile.AddToGroup();
                }
            }
        }

        public void ResetColor() => ForEachXY(x => x.SetColor(Color4.White));

        public bool Inside(Vector2 _position) => Basics.Rectangle.Collide(_position.X, _position.Y, X, Y, Width * CellSize, Height * CellSize);
        public VectorTile Get(Vector2 _indices) => Get(_indices.X, _indices.Y);
        public VectorTile GetFromWorldPosition(Vector2 _worldPoint) => Get(WorldPositionToGridPosition(_worldPoint));
        public Vector2 WorldPositionToGridPosition(Vector2 _worldPoint) => new Vector2((_worldPoint.X - X) / CellSize, (_worldPoint.Y - Y) / CellSize);
        public Vector2 GridPositionToWorldPosition(Vector2 _gridPoint) => new Vector2(_gridPoint.X * CellSize + X, _gridPoint.Y * CellSize + Y);
    }
}
