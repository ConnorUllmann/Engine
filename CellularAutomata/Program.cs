using System;
using Engine;
using System.Linq;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;
using System.Runtime.InteropServices;
using Basics;
using System.Threading.Tasks;

namespace CellularAutomata
{
    class ConcavePolygonTest : Game
    {
        private List<PolygonActor> polygonActors = new List<PolygonActor>();

        public ConcavePolygonTest() : base(800, 800, "Concave Polygon Test") { }

        public override void Start() { }

        public override void Update()
        {
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

    public class ImageTest : Game
    {
        public ImageTest() : base(800, 600)
        {
            GL.ClearColor(0, 0.1f, 0.4f, 1);

            texture = LoadTexture();
        }

        private int texture;

        public int LoadTexture()
        {
            Bitmap bitmap = new Bitmap(500, 400);

            int tex;
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

            GL.GenTextures(1, out tex);
            GL.BindTexture(TextureTarget.Texture2D, tex);

            // Initialize unmanaged memory to hold the array.
            byte[] source = new byte[4 * bitmap.Width * bitmap.Height];
            int size = Marshal.SizeOf<byte>() * source.Length;
            IntPtr firstPixel = Marshal.AllocHGlobal(size);
            var color = Color4.Aqua;
            for (var x = 0; x < bitmap.Width; x++)
            {
                for (var y = 0; y < bitmap.Height; y++)
                {
                    source[y * bitmap.Width + x]   = (byte)(255 * color.B);
                    source[y * bitmap.Width + x+1] = (byte)(255 * color.G);
                    source[y * bitmap.Width + x+2] = (byte)(255 * color.R);
                    source[y * bitmap.Width + x+3] = (byte)(255 * color.A);
                }
            }
            Marshal.Copy(source, 0, firstPixel, source.Length);

            byte[] destination = new byte[source.Length];
            Marshal.Copy(firstPixel, destination, 0, source.Length);//, 0, firstPixel, source.Length);



            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bitmap.Width, bitmap.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, firstPixel);


            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            return tex;
        }

        public static void DrawImage(int image)
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.PushMatrix();
            GL.LoadIdentity();

            GL.Ortho(0, 800, 0, 600, -1, 1);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
            GL.LoadIdentity();

            GL.Disable(EnableCap.Lighting);

            GL.Enable(EnableCap.Texture2D);

            GL.Color4(1, 0, 0, 1);

            GL.BindTexture(TextureTarget.Texture2D, image);

            GL.Begin(BeginMode.Quads);

            GL.TexCoord2(0, 0);
            GL.Vertex3(0, 0, 0);

            GL.TexCoord2(1, 0);
            GL.Vertex3(256, 0, 0);

            GL.TexCoord2(1, 1);
            GL.Vertex3(256, 256, 0);

            GL.TexCoord2(0, 1);
            GL.Vertex3(0, 256, 0);

            GL.End();

            GL.Disable(EnableCap.Texture2D);
            GL.PopMatrix();

            GL.MatrixMode(MatrixMode.Projection);
            GL.PopMatrix();

            GL.MatrixMode(MatrixMode.Modelview);
        }



        public override void Render()
        {
            DrawImage(0);
        }
    }

    class GravitySim : Game
    {

        private VectorGrid grid;
        private ParticleSet particleSet;

        public GravitySim() : base(1400, 1400, "Gravity Simulation") { }

        public override void Start()
        {
            particleSet = new ParticleSet(100);
            grid = new VectorGrid(-Width/2f, -Height/2f, Width, Height, 20);
        }

        private Vector2 LastMousePosition;

        public override void PreUpdate()
        {
            particleSet.ApplyGravityAsynchronously();
            particleSet.ApplyVectorGridSynchronously(grid);
            particleSet.ApplyAirResistanceSynchronously();
            ApplyMouseVelocityToVectorGrid();
        }

        private void ApplyMouseVelocityToVectorGrid()
        {
            if (Input.LeftMousePressed)
                LastMousePosition = Input.Mouse;
            else if (Input.LeftMouseDown)
            {
                var diff = Input.Mouse - LastMousePosition;
                if (diff.LengthSquared >= 1)
                {
                    var diffNormal = (diff).Normalized();
                    const int MouseDistanceSq = 100 * 100;
                    for (var x = 0; x < grid.Width; x++)
                    {
                        for (var y = 0; y < grid.Height; y++)
                        {
                            if ((Input.Mouse - grid.GridPositionToWorldPosition(new Vector2(x, y))).LengthSquared <= MouseDistanceSq)
                            {
                                var tile = grid.Get(x, y);
                                if(tile != null)
                                    tile.Vector = diffNormal;
                            }
                        }
                    }
                }
                LastMousePosition = Input.Mouse;
            }
        }

        private List<long> SynchronousResults = new List<long>();
        private List<long> AsynchronousResults = new List<long>();
        private void PreUpdateGravityBenchmarking()
        {

            var s = new System.Diagnostics.Stopwatch();

            s.Start();
            particleSet.ApplyGravitySynchronously();
            s.Stop();
            SynchronousResults.Add(s.ElapsedMilliseconds);
            Basics.Utils.Log($"{SynchronousResults.Average()}");

            s.Reset();

            s.Start();
            particleSet.ApplyGravityAsynchronously();
            s.Stop();
            AsynchronousResults.Add(s.ElapsedMilliseconds);
            Basics.Utils.Log($"{AsynchronousResults.Average()}");

            Basics.Utils.Log(" --- ");

        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            new GravitySim().Run();

            //new Automata().Run();
            //new ConcavePolygonTest().Run();
            //new HexGridTest().Run();
            //new TemplateGame().Run();
            //new ImageTest().Run();
        }
    }
}