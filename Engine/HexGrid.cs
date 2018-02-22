using System;
using System.Collections.Generic;
using System.Text;
using Basics;
using Engine;
using System.Linq;
using OpenTK;
using OpenTK.Graphics;

namespace Engine
{
    public class HexTile : Actor
    {
        private HexGrid grid;
        private PolygonFillRenderer polygonRenderer;
        public readonly int i;
        public readonly int j;
        private readonly float Radius;
        private readonly float Altitude;

        public HexTile(HexGrid _grid, int _i, int _j, float _radius)
            : this(_grid, _i, _j, _radius, Color4.White) { }
        public HexTile(HexGrid _grid, int _i, int _j, float _radius, Color4 _color)
        {
            grid = _grid;
            i = _i;
            j = _j;
            Radius = _radius;

            Altitude = GetAltitude();
            X = GetX();
            Y = GetY();

            var rotationRad = grid.Horizontal ? (float)(0.5f * Math.PI) : 0;
            polygonRenderer = new PolygonFillRenderer(ConvexPolygon.Regular(6, Radius, rotationRad), Color4.Red);
            polygonRenderer.Move(new Vector3(X, Y, 0));
        }

        public void SetColor(Color4 _color) => polygonRenderer.SetColor(_color);

        private float GetAltitude() => Basics.Utils.EquilateralAltitude(Radius);

        private float GetX()
        {
            if (grid.Horizontal)
                return grid.X + 1.5f * Radius * i;
            return grid.X + Altitude * i * 2 + (j % 2) * Altitude;
        }
        private float GetY()
        {
            if(grid.Horizontal)
                return grid.Y + 2 * j * Altitude + (i % 2) * Altitude;
            return grid.Y + 1.5f * j * Radius + Altitude;
        }

        public override void Render() => polygonRenderer.Render(X, Y); 
    }

    public class HexGrid
    {
        private Grid<HexTile> grid;
        public readonly float X;
        public readonly float Y;
        public readonly float Radius;
        public readonly bool Horizontal;
        public readonly Vector4 BoundingBox;

        public HexGrid(float _x, float _y, int _width, int _height, float _radius, bool _horizontal = false)
        {
            X = _x;
            Y = _y;
            Radius = _radius;
            Horizontal = _horizontal;

            grid = new Grid<HexTile>(_width, _height);
            for (var i = 0; i < grid.Width; i++)
            {
                for (var j = 0; j < grid.Height; j++)
                {
                    var tile = new HexTile(this, i, j, Radius);
                    grid.Set(tile, i, j);
                    tile.AddToWorld();
                }
            }
            BoundingBox = GetBoundingBox();
        }

        public void ShowNeighbors(int _x, int _y)
        {
            var tiles = GetNeighbors(_x, _y);
            foreach (var tile in tiles)
                tile.SetColor(Color4.Yellow);
            grid.Get(_x, _y)?.SetColor(Horizontal
                ? _x.IsEven()
                    ? Color4.Aqua
                    : Color4.Green
                : _y.IsEven()
                    ? Color4.Magenta
                    : Color4.Orange);
        }

        public Vector4 GetBoundingBox()
        {
            if (grid.Width <= 0 || grid.Height <= 0)
                return new Vector4();

            var altitude = Basics.Utils.EquilateralAltitude(Radius);
            var first = grid.Get(0, 0);
            var txH = first.X - Radius;
            var txV = first.X - altitude;
            var tyH = first.Y - altitude;
            var tyV = first.Y - Radius;
            if (grid.Width == 1 && grid.Height == 1)
            {
                return Horizontal
                    ? new Vector4(txH, tyH, 2 * Radius, 2 * altitude)
                    : new Vector4(txV, tyV, 2 * altitude, 2 * Radius);
            }
            else if (grid.Width == 1)
            {
                var last = grid.Get(0, grid.Height - 1);
                return Horizontal
                    ? new Vector4(txH, tyH, last.X + Radius - txH, last.Y + altitude - tyH)
                    : new Vector4(txV, tyV, last.X + 2 * altitude - txV, last.Y + Radius - tyV);
            }
            else if (grid.Height == 1)
            {
                var last = grid.Get(grid.Width - 1, 0);
                return Horizontal
                    ? new Vector4(txH, tyH, last.X + Radius - txH, last.Y + 2 * altitude - tyH)
                    : new Vector4(txV, tyV, last.X + altitude - txV, last.Y + Radius - tyV);
            }
            var endOfExtrudingRow = grid.Get(grid.Width - 1, 1);
            var endOfExtrudingColumn = grid.Get(1, grid.Height - 1);
            return Horizontal
                ? new Vector4(txH, tyH, endOfExtrudingRow.X + Radius - txH, endOfExtrudingColumn.Y + altitude - tyH)
                : new Vector4(txV, tyV, endOfExtrudingRow.X + altitude - txV, endOfExtrudingColumn.Y + Radius - tyV);
        }
        
        public IEnumerable<HexTile> GetNeighbors(HexTile _tile) => GetNeighbors(_tile.i, _tile.j);
        public IEnumerable<HexTile> GetNeighbors(int _i, int _j)
        {
            return (Horizontal
                ? _i.IsEven()
                    ? HorizontalEvenRowIndexNeighborMap
                    : HorizontalOddRowIndexNeighborMap
                : _j.IsEven()
                    ? VerticalEvenColumnIndexNeighborMap
                    : VerticalOddColumnIndexNeighborMap)
                .Select(n => grid.Get(_i + n.X, _j + n.Y)).Where(n => n != null);
        }

        private static readonly Vector2[] HorizontalEvenRowIndexNeighborMap = new[]
        {
            new Vector2(-1, 0),
            new Vector2(0, -1),
            new Vector2(0, 1),
            new Vector2(1, -1),
            new Vector2(1, 0),
            new Vector2(-1, -1)
        };
        private static readonly Vector2[] HorizontalOddRowIndexNeighborMap = new[]
        {
            new Vector2(-1, 0),
            new Vector2(0, -1),
            new Vector2(0, 1),
            new Vector2(1, 1),
            new Vector2(1, 0),
            new Vector2(-1, 1)
        };
        private static readonly Vector2[] VerticalEvenColumnIndexNeighborMap = new[]
        {
            new Vector2(-1, 0),
            new Vector2(0, -1),
            new Vector2(0, 1),
            new Vector2(-1, -1),
            new Vector2(1, 0),
            new Vector2(-1, 1)
        };
        private static readonly Vector2[] VerticalOddColumnIndexNeighborMap = new[]
        {
            new Vector2(-1, 0),
            new Vector2(0, -1),
            new Vector2(0, 1),
            new Vector2(1, -1),
            new Vector2(1, 0),
            new Vector2(1, 1)
        };
    }


}
