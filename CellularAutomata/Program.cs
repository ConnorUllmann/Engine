using System;
using Engine;
using System.Linq;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using Engine;

namespace CellularAutomata
{
    class Program
    {
        static void Main(string[] args)
        {
            //var automata = new Automata(1280, 720);
            //automata.Run();

            var game = new Game(1280, 720);
            //var grid1 = new HexGrid(-600, -500, 10, 10, 40, true);
            //grid1.ShowNeighbors(2, 2);
            //grid1.ShowNeighbors(8, 7);
            //grid1.ShowNeighbors(3, 6);
            //grid1.ShowNeighbors(7, 3);
            //var grid2 = new HexGrid(200, -500, 10, 10, 40, false);
            //grid2.ShowNeighbors(2, 2);
            //grid2.ShowNeighbors(8, 7);
            //grid2.ShowNeighbors(3, 8);
            //grid2.ShowNeighbors(7, 1);
            //var grid3 = new HexGrid(-500, -300, 1, 1, 20, true);
            //var grid4 = new HexGrid(-400, -300, 1, 1, 30, false);
            //var grid5 = new HexGrid(-300, -300, 1, 3, 40, true);
            //var grid6 = new HexGrid(-200, -300, 1, 3, 40, false);
            //var grid7 = new HexGrid(-50, -300, 3, 1, 10, true);
            //var grid8 = new HexGrid(200, -300, 3, 1, 100, false);


            var points = new List<Vector2>()
            {
                new Vector2(0, 0),
                new Vector2(300, -100),
                new Vector2(300, 0),
                new Vector2(100, 100),
                new Vector2(300, 200),
                new Vector2(-300, 200),
                new Vector2(0, -200)
            };
            var polygon = new ConcavePolygon(points);
            AddPolygon(polygon);
            var outlinePolygon = new Polygon(points);
            AddPolygon(outlinePolygon);

            game.Run();
        }

        private static Actor AddRegularPolygon(int _sides, float _radius) => AddPolygon(ConvexPolygon.Regular(_sides, _radius));
        private static Actor AddPolygon(Polygon _polygon)
        {
            var actor = new ShellActor();
            var polygonRenderer = _polygon is ConvexPolygon 
                ? new ConvexPolygonRenderer(_polygon as ConvexPolygon, Color4.Blue, Color4.Red)
                : _polygon is ConcavePolygon
                    ? new ConcavePolygonRenderer(_polygon as ConcavePolygon, Color4.Green, Color4.Red)
                    : new PolygonRenderer(_polygon, Color4.Red);
            actor.RenderHandler += polygonRenderer.Render;
            actor.AddToWorld();
            return actor;
        }
    }
}