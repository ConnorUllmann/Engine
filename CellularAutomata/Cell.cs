using System;
using System.Collections.Generic;
using System.Text;
using Engine;
using Basics;
using System.Linq;
using OpenTK.Graphics;

namespace CellularAutomata
{
    public class CustomCell : Cell
    {
        private ColoredVertexArray rectangleArray;
        
        public CustomCell(int _i, int _j, CustomCellGrid _grid) : base(_i, _j, _grid)
        {
            value = (float)Basics.Utils.RandomDouble();
            NextValue = Algorithm;
            rectangleArray = ColoredVertexArray.FromBuffer(rectangleBuffer);
        }

        public override void Render()
        {
            var v = value;
            v = Basics.Utils.Clamp(v, 0, 1);
            if (true)//v > 0.5f)
            {
                var r = 1 - v + v * v;
                var g = 1 - v;
                var b = v * v;
                rectangleBuffer.SetColor(new Color4(r * 0.25f + 0.75f, g * 0.25f + 0.5f, b * 0.25f + 0.25f, 1));
                rectangleArray.Render(X, Y);
            }
        }

        private float Algorithm()
        {
            //return Utils.Clamp(Neighbors().Select(x => x.value < 0.25f ? x.value / 5f : -0.1f).Sum(), 0, 1);
            //return Utils.Clamp(Neighbors().Select(x => x.value < 0.1f ? x.value : x.value < 0.8f ? x.value / 10f : -0.1f).Sum(), 0, 1);
            return Basics.Utils.Clamp(Neighbors().Select(x => x.value < 0.05f ? 0.15f : x.value > 0.95f ? -0.05f : (x.value > 0.6f ? -1 : 1) * (float)Math.Sin((grid as CustomCellGrid).MillisecondsSinceStart / 1000f) * 0.5f).Sum(), 0, 1);
        }
        public IEnumerable<CustomCell> Neighbors() => grid.GetNeighborsSquare(i, j).Select(o => o as CustomCell).Where(o => o != null);
    }

    public class CustomCellGrid : CellGrid
    {
        internal float MillisecondsSinceStart;

        public CustomCellGrid(int _x, int _y, int _width, int _height, int _cellSize = 16) :
            base(_x, _y, _width, _height, _cellSize)
        { }

        public override void Start() => base.Start((i, j) => new CustomCell(i, j, this));

        public void Update()
        {
            MillisecondsSinceStart = Game.MillisecondsSinceStart;
        }
    }

    public class ConwayCell : Cell
    {
        private ColoredVertexArray rectangleArray;

        public ConwayCell(int _i, int _j, ConwayCellGrid _grid) : base(_i, _j, _grid)
        {
            value = Basics.Utils.RandomInt(0, 4) == 0 ? 1 : 0;
            NextValue = ConwayAlgorithm;
            rectangleBuffer.SetColor(Color4.White);
            rectangleArray = ColoredVertexArray.FromBuffer(rectangleBuffer);
        }

        public override void Render()
        {
            if (Alive)
                rectangleArray.Render(X, Y);
        }

        private bool Alive => value >= 0.5f;

        private float ConwayAlgorithm()
        {
            switch (AliveNeighbors().Count())
            {
                case 2:
                    return Alive ? 1 : 0;
                case 3:
                    return 1;
                default:
                    return 0;
            }
        }
        public IEnumerable<ConwayCell> Neighbors() => grid.GetNeighborsSquare(i, j).Select(o => o as ConwayCell).Where(o => o != null);
        public IEnumerable<ConwayCell> AliveNeighbors() => Neighbors().Where(o => o.Alive);
    }

    public class ConwayCellGrid : CellGrid
    {
        public ConwayCellGrid(int _x, int _y, int _width, int _height, int _cellSize = 16) :
            base(_x, _y, _width, _height, _cellSize) { }

        public override void Start() => base.Start((i, j) => new ConwayCell(i, j, this));
    }

    public class Cell : Actor
    {
        public int i;
        public int j;
        public float value;
        private float nextValue;
        public ColoredVertexBuffer rectangleBuffer;
        public CellGrid grid;
        public Func<float> NextValue;

        public override float X => i * grid.CellSize + grid.X;
        public override float Y => j * grid.CellSize + grid.Y;

        public Cell(int _i, int _j, CellGrid _grid)
        {
            i = _i;
            j = _j;
            grid = _grid;
            value = 0;
            rectangleBuffer = ConvexPolygon.Square(_grid.CellSize).GetFillBuffer();//new ColoredRectangleBuffer(X, Y, _grid.CellSize, _grid.CellSize, -1.5f);
        }

        public override void PreUpdate() => nextValue = NextValue();
        public override void PostUpdate() => value = nextValue;
    }

    public class CellGrid : Grid<Cell>
    {
        private int x;
        private int y;
        private int cellSize;

        public int X => x;
        public int Y => y;
        public int CellSize => cellSize;

        private Func<int, int, Cell> AddCell;

        public CellGrid(int _x, int _y, int _width, int _height, int _cellSize) : base(_width, _height)
        {
            x = _x;
            y = _y;
            cellSize = _cellSize;
        }

        public virtual void Start() => Start((i, j) => new Cell(i, j, this));
        protected void Start(Func<int, int, Cell> addCell)
        {
            AddCell = addCell;
            for (var i = 0; i < Width; i++)
            {
                for (var j = 0; j < Height; j++)
                {
                    var cell = AddCell(i, j);
                    Set(cell, i, j);
                    ActorGroup.World.AddOnNextFrame(cell);
                }
            }
        }
    }

}
