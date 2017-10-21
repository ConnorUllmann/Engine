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
        class Particle : PolygonActor
        {
            public float Radius;
            public float Mass;
            private Vector2 Acceleration;
            private Vector2 Velocity;
            
            private float AngularVelocity = (float)(Basics.Utils.RandomDouble() * 2 - 1) * 0.01f;

            private static float GetRadius(float _mass) => 4 * (float)Math.Sqrt(_mass);

            public Particle(float _mass, int _x, int _y) : base(ConvexPolygon.Regular(4, GetRadius(_mass)).Move(new Vector3(_x - Width/2, _y - Height/2, 0)))
            {
                Mass = _mass;
                Radius = GetRadius(Mass);
                FillVisible = false;
            }

            public override void Update()
            {
                Velocity += Acceleration * Delta;
                Position += Velocity * Delta;
                X = Basics.Utils.Clamp(X, -Width/2, Width/2);
                Y = Basics.Utils.Clamp(Y, -Height/2, Height/2);

                Rotate(AngularVelocity * Delta);
            }

            public Vector2 GravityForce(Particle other)
            {
                if (X == other.X && Y == other.Y)
                    return Vector2.Zero;
                var diff = other.Position - Position;
                var radii = Radius + other.Radius;
                return Mass * other.Mass / Basics.Utils.Max(diff.LengthSquared, radii * radii) * diff.Normalized();
            }

            public void ResetAcceleration() => Acceleration = Vector2.Zero;
            public void ApplyForce(Vector2 force) => Acceleration += force / Mass;
        }

        class ParticleSet
        {
            private HashSet<Particle> particles;

            public ParticleSet(int count)
            {
                particles = new HashSet<Particle>();
                for (var i = 0; i < count; i++)
                {
                    var x = (int)(Basics.Utils.RandomDouble() * Width / 2) + Width / 4;
                    var y = (int)(Basics.Utils.RandomDouble() * Height / 2) + Height / 4;
                    var particle = new Particle((float)Basics.Utils.RandomDouble() * 98 + 2, x, y);
                    particles.Add(particle);
                    particle.AddToWorld();
                }
            }

            public void ApplyGravitySynchronously() => particles.ForEach(ApplyGravity);
            public void ApplyGravityAsynchronously()
            {
                var tasks = new List<Task>();
                foreach (var currentParticle in particles)
                    tasks.Add(Task.Factory.StartNew(() => ApplyGravity(currentParticle)));
                Task.WaitAll(tasks.ToArray());
            }

            private void ApplyGravity(Particle _particle) => ApplyGravity(_particle, 0.01f);
            private void ApplyGravity(Particle _particle, float _gravity)
            {
                _particle.ResetAcceleration();
                foreach (var otherParticle in particles)
                    _particle.ApplyForce(_gravity * _particle.GravityForce(otherParticle));
            }
        }

        private ParticleSet particleSet;

        public GravitySim() : base(1400, 1400, "Gravity Simulation")
        { }

        public override void Start()
        {
            particleSet = new ParticleSet(250);
        }

        private List<long> SynchronousResults = new List<long>();
        private List<long> AsynchronousResults = new List<long>();

        public override void PreUpdate()
        {
            //var s = new System.Diagnostics.Stopwatch();

            //s.Start();
            //particleSet.ApplyGravitySynchronously();
            //s.Stop();
            //SynchronousResults.Add(s.ElapsedMilliseconds);
            //Basics.Utils.Log($"{SynchronousResults.Average()}");

            //s.Reset();

            //s.Start();
            particleSet.ApplyGravityAsynchronously();
            //s.Stop();
            //AsynchronousResults.Add(s.ElapsedMilliseconds);
            //Basics.Utils.Log($"{AsynchronousResults.Average()}");

            //Basics.Utils.Log(" --- ");
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