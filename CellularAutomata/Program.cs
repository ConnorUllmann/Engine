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
        private List<PolygonActor> polygonActors = new List<PolygonActor>();

        public ConcavePolygonTest() : base(800, 800, "Concave Polygon Test")
        { }

        public override void Start()
        {
        }

        public override void Update()
        {
            //Console.WriteLine((Input.LeftMousePressed   ? "*" : "0") + " " 
            //                + (Input.LeftMouseReleased  ? "*" : "0") + " " 
            //                + (Input.RightMousePressed  ? "*" : "0") + " " 
            //                + (Input.RightMouseReleased ? "*" : "0"));

            if (Input.KeyReleased(Key.Space))
            {
                ClearPolygonActors();
                polygonActors = new List<PolygonActor>() { RandomPolygon() };
            }

            if(Input.KeyReleased(Key.C))
            {
                ClearPolygonActors();
                polygonActors = SplitPolygonsAlongRandomLine(polygonActors).ToList();
            }
        }

        private PolygonActor RandomPolygon()
        {
            var points = new List<Vector3>();
            for (float angle = 0; angle < 2 * Math.PI; angle += 0.2f)
            {
                var d = Basics.Utils.RandomDouble() * 300 + 100;
                var p = new Vector3((float)(Math.Cos(angle) * d), (float)(Math.Sin(angle) * d), 0);
                points.Add(p);
            }
            return AddPolygon(new ConcavePolygon(points));
        }

        private void ClearPolygonActors()
        {
            for (var i = 0; i < polygonActors.Count; i++)
                polygonActors[i].Destroy();
        }

        private IEnumerable<PolygonActor> SplitPolygonsAlongRandomLine(IEnumerable<PolygonActor> polygonActors)
        {
            var length = 1000;
            var angle = Basics.Utils.RandomDouble() * Math.PI * 2;
            var a = new Vector3((float)(Width / 2f * Basics.Utils.RandomDouble()) - Width/4f, (float)(Height / 2f * Basics.Utils.RandomDouble()) - Height/4f, 0);
            var b = a + new Vector3((float)(Math.Cos(angle) * length), (float)(Math.Sin(angle) * length), 0);
            return SplitPolygonsAlongLine(a, b, polygonActors);
        }

        private IEnumerable<PolygonActor> SplitPolygonsAlongLine(Vector3 _pointA, Vector3 _pointB, IEnumerable<PolygonActor> _polygonActors)
        {
            var ret = new List<Polygon>();
            foreach(var polygonActor in _polygonActors)
                ret.AddRange(polygonActor.SplitAlongLine(_pointA, _pointB));
            return ret.Select(p => AddPolygon(p));
        }
        
        private static PolygonActor AddPolygon(Polygon _polygon) => new PolygonActor(_polygon).AddToWorld() as PolygonActor;
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

    class TemplateGame : Game
    {
        private ShellActor actor;

        public TemplateGame() : base(1280, 720, "Untitled")
        { }

        public override void Start()
        {
            var polygon = ConvexPolygon.Regular(5, 100);
            var renderer = new PolygonFillRenderer(polygon, Color4.Blue);
            actor = new ShellActor();
            actor.UpdateHandler += () =>
            {
                var t = MillisecondsSinceStart / 1000;
                var length = 100;
                var angle = (Math.Sin(t) + 1.5f) * Math.PI;
                actor.X = (float)(Math.Cos(angle) * length);
                actor.Y = (float)(Math.Sin(angle) * length);
                renderer.Rotate((float)t * 0.01f);
            };
            actor.RenderHandler += () => renderer.Render(actor.X, actor.Y);
            actor.AddToWorld();
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
            //new TemplateGame().Run();
        }
    }
}