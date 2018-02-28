using System;
using System.Collections.Generic;
using System.Text;
using Basics;
using Engine;
using Basics.QuadTree;
using OpenTK.Graphics;
using System.Linq;
using Ants.AntControllers;

namespace Ants
{
    public class AntGrid
    {
        public QuadTree<Actor> Quadtree;
        protected Grid<Tile> grid;
        public float X;
        public float Y;
        public readonly int TileSize;
        public readonly int Width;
        public readonly int Height;
        public readonly int WidthCells;
        public readonly int HeightCells;
        public HashSet<Ant> Ants = new HashSet<Ant>();

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
            Quadtree = new QuadTree<Actor>(_width, _height);
        }
        public bool AddAnt(Ant _ant) => Ants.Add(_ant);
        public bool RemoveAnt(Ant _ant) => Ants.Remove(_ant);

        private Tile NewTile(int _i, int _j) => new Tile(this, _i, _j);

        public float GetX(int _i) => X + _i * TileSize;
        public float GetY(int _j) => Y + _j * TileSize;
        public int GetI(float _x) => (int)((_x - X) / TileSize);
        public int GetJ(float _y) => (int)((_y - Y) / TileSize);
        public Tile Get(float _x, float _y) => grid.Get(GetI(_x), GetJ(_y));

        public void RefreshQuadtree()
        {
            Quadtree.Reset();
            foreach (var ant in Ants)
                Quadtree.AddObject(ant, ant?.BoundingBox);
        }

        public virtual void Update()
        {
            RefreshQuadtree();
        }

        public virtual void Render()
        {
        }

        public List<Actor> GetAntsThatCollide(Rectangle _r) => Quadtree.QueryRect(_r.X, _r.Y, _r.W, _r.H);
    }
    
    public class VisualAntGrid : AntGrid
    {
        private PolygonRenderer squareRenderer;

        public VisualAntGrid(float _x, float _y, int _width, int _height, int _tileSize)
            : base(_x, _y, _width, _height, _tileSize)
        {
            squareRenderer = new PolygonOutlineRenderer(ConvexPolygon.Square(TileSize), Color4.Red);
        }

        public override void Update()
        {
            base.Update();
            grid.ForEachXY(tile => tile.Update());
        }

        public override void Render()
        {
            base.Render();
            grid.ForEachXY(tile =>
            {
                squareRenderer.SetColor(tile.Signals.Select(x => x.Value.Select(y => y.Amount).Sum()).Sum() > 5 ? Color4.Red : Color4.Blue);
                squareRenderer.Render(tile.X, tile.Y);
            });
        }
    }
}
