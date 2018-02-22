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
    public class Tile : ISolidTile
    {
        public AntGrid Grid;
        public int Size => Grid.TileSize;
        public bool Solid { get; set; } = false;

        public Dictionary<SignalType, List<Signal>> Signals = new Dictionary<SignalType, List<Signal>>();
        public bool HasSignal(SignalType _type) => Signals.ContainsKey(_type);

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

        public void Update()
        {
            UpdateSignals();
        }

        public void UpdateSignals()
        {
            var typesToRemove = new HashSet<SignalType>();
            foreach (var signalType in Signals.Keys)
            {
                var signals = Signals[signalType];
                var signalsToRemove = new HashSet<Signal>();
                foreach(var signal in signals)
                {
                    signal.Update();
                    if (signal.Amount <= 0)
                        signalsToRemove.Add(signal);
                }
                foreach (var signal in signalsToRemove)
                    signals.Remove(signal);
                if (signals.Count == 0)
                    typesToRemove.Add(signalType);
            }
            typesToRemove.ForEach(x => Signals.Remove(x));
        }

        public void AddSignal(SignalType _type, float _amount, float _angle)
        {
            if (!Signals.ContainsKey(_type))
                Signals[_type] = new List<Signal>();
            Signals[_type].Add(new Signal(_type, _amount, _angle));
        }

        public float? GetSignalAngleForResourceType(SignalType _type)
        {
            if (!Signals.TryGetValue(_type, out var signals) || signals.Count == 0)
                return null;
            var weightedAngleSum = signals.Select(o => o.Amount * o.Angle).Sum();
            var weightSum = signals.Select(o => o.Amount).Sum();
            return weightedAngleSum / weightSum; // Radians
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

        public override void Update()
        {
            base.Update();
            grid.ForEachXY(tile =>
            {
                tile.Update();
            });
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

    public enum WorldType
    {
        Underworld,
        Overworld
    }

    public class AntWorld
    {
        public AntGrid Underworld;
        public AntGrid Overworld;


        public bool UnderworldVisible = true;
        public bool OverworldVisible = true;

        public AntWorld(int _width, int _height, int _tileSize)
        {
            Underworld = new VisualAntGrid(-_width / 2, -_height / 2, _width, _height, _tileSize);
            Overworld = new VisualAntGrid(-_width / 2, -_height / 2, _width, _height, _tileSize);
        }

        public bool AddAnt(Ant _ant) => WorldFromType(_ant.Location).AddAnt(_ant);
        public bool RemoveAnt(Ant _ant) => WorldFromType(_ant.Location).RemoveAnt(_ant);
        public void MoveAnt(Ant _ant, WorldType _destination)
        {
            if (_ant.Location == _destination)
                return;
            RemoveAnt(_ant);
            WorldFromType(_destination).AddAnt(_ant);
            _ant.Location = _destination;
        }

        public void Update()
        {
            Underworld.Update();
            Overworld.Update();
        }

        public void Render()
        {
            if (UnderworldVisible) Underworld.Render();
            if (OverworldVisible) Overworld.Render();
        }

        public AntGrid WorldFromType(WorldType _type) => _type == WorldType.Overworld ? Overworld : Underworld;
    }

    public class AntGrid
    {
        /**************************************/
        //TODO: Query quadtree for collisions!//
        /**************************************/
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
                Quadtree.AddObject(ant, ant.X, ant.Y);
        }

        public virtual void Update()
        {
            RefreshQuadtree();
        }

        public virtual void Render()
        {
        }
    }
}
