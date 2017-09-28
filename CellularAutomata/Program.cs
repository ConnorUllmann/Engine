using System;
using Engine;
using System.Linq;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;

namespace CellularAutomata
{
    class ConcavePolygonTest : Game
    {
        private Actor outlinePolygonActor;
        private Actor fillPolygonActor;

        public ConcavePolygonTest() : base(900, 900, "Concave Polygon Test")
        { }

        private void NewPolygon()
        {
            var points = new List<Vector3>();
            for (float angle = 0; angle < 2 * Math.PI; angle += 0.2f)
            {
                var d = Basics.Utils.RandomDouble() * 500 + 100;
                var p = new Vector3((float)(Math.Cos(angle) * d), (float)(Math.Sin(angle) * d), 0);
                points.Add(p);
            }
            
            if(fillPolygonActor != null)
            {
                fillPolygonActor.Destroy();
                fillPolygonActor = null;
            }
            var fillPolygon = new ConcavePolygon(points);
            fillPolygonActor = AddPolygon(fillPolygon);

            if (outlinePolygonActor != null)
            {
                outlinePolygonActor.Destroy();
                outlinePolygonActor = null;
            }
            var outlinePolygon = new Polygon(points);
            outlinePolygonActor = AddPolygon(outlinePolygon);
        }

        public override void Start()
        {
            NewPolygon();
        }

        public override void Update()
        {
            if (KeyReleased(Key.Space))
                NewPolygon();
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
            if (polygonRenderer is ConcavePolygonRenderer)
                (polygonRenderer as ConcavePolygonRenderer).RandomizeFillColor();
            actor.RenderHandler += polygonRenderer.Render;
            actor.AddToWorld();
            return actor;
        }
    }

    class HexGridTest : Game
    {
        public HexGridTest() : base(1280, 720, "Hex Grid Test")
        { }

        public override void Start()
        {
            var grid1 = new HexGrid(-650, -350, 10, 10, 40, true);
            grid1.ShowNeighbors(2, 2);
            grid1.ShowNeighbors(8, 7);
            grid1.ShowNeighbors(3, 6);
            grid1.ShowNeighbors(7, 3);
            var grid2 = new HexGrid(150, -350, 10, 10, 40, false);
            grid2.ShowNeighbors(2, 2);
            grid2.ShowNeighbors(8, 7);
            grid2.ShowNeighbors(3, 8);
            grid2.ShowNeighbors(7, 1);
        }

        public override void Update()
        {

        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            //new Automata().Run();
            new ConcavePolygonTest().Run();
            //new HexGridTest().Run();
        }
    }
}