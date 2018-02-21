using System;
using System.Collections.Generic;
using System.Text;
using Basics;
using Engine;
using OpenTK.Graphics;

namespace Ants
{
    public class Tile : ISolidTile
    {
        public AntGrid Grid;
        public int Size => Grid.TileSize;
        public bool Solid { get; set; } = false;

        //Indices within Grid's tiles
        public int I;
        public int J;

        //Position in real world space
        public float X { get => Grid.GetX(I); }
        public float Y { get => Grid.GetY(J); }
        
        public Tile(AntGrid _grid, int _i, int _j)
        {
            Grid = _grid;
            I = _i;
            J = _j;
        }
    }

    public class VisualAntGrid : AntGrid
    {
        private PolygonRenderer squareRenderer;

        public VisualAntGrid(float _x, float _y, int _width, int _height, int _tileSize)
            : base(_x, _y, _width, _height, _tileSize)
        {
            squareRenderer = new PolygonOutlineRenderer(ConvexPolygon.Square(TileSize), Color4.Red);
        }

        public override void Render()
        {
            grid.ForEachXY(tile =>
            {
                squareRenderer.SetColor(tile.Solid ? Color4.Red : Color4.Blue);
                squareRenderer.Render(tile.X, tile.Y);
            });
        }
    }

    public class AntGrid
    {
        protected Grid<Tile> grid;
        public float X;
        public float Y;
        public readonly int TileSize;
        public readonly int Width;
        public readonly int Height;
        public readonly int WidthCells;
        public readonly int HeightCells;

        public AntGrid(float _x, float _y, int _width, int _height, int _tileSize)
        {
            X = _x;
            Y = _y;
            Width = _width;
            Height = _height;
            TileSize = _tileSize;
            WidthCells = (int)Math.Ceiling(_width * 1f / TileSize);
            HeightCells = (int)Math.Ceiling(_height * 1f / TileSize);
            grid = new Grid<Tile>(WidthCells, HeightCells, NewTile);
        }

        private Tile NewTile(int _i, int _j) => new Tile(this, _i, _j);

        public float GetX(int _i) => X + _i * TileSize;
        public float GetY(int _j) => Y + _j * TileSize;
        public int GetI(float _x) => (int)((_x - X) / TileSize);
        public int GetJ(float _y) => (int)((_y - Y) / TileSize);
        public Tile Get(float _x, float _y) => grid.Get(GetI(_x), GetJ(_y));

        public virtual void Render() { }
    }
}
