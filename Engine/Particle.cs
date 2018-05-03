using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using Basics;
using Engine;
using Engine.Actors;
using Engine.OpenGL.Colored;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Rectangle = Basics.Rectangle;
using System.Threading.Tasks;

namespace Engine
{
    public interface IParticleSystem
    {
        Color4 StartColor { get; }
        float StartSize { get; }
        float StartSpeed { get; }
        float StartSpeedMin { get; }
        float StartSpeedMax { get; }

        /// <summary>
        /// Start angle (in radians). It's a good idea usually to use random in your implementation
        /// </summary>
        float StartAngle { get; }
        
        void Update(Particle _particle);
        IEnumerable<(ParticlePrimitive PrimitiveType, IEnumerable<(float X, float Y)> Vertices)> Vertices(Particle _particle);
        bool ShouldDestroy(Particle _particle);
    }

    public enum ParticlePrimitive
    {
        Points = PrimitiveType.Points,
        Lines = PrimitiveType.Lines,
        Triangles = PrimitiveType.Triangles,
        Quads = PrimitiveType.Quads
    }


    public interface IRegion
    {
        Vector2 RandomPosition();
    }

    public class RectangleRegion : IRegion
    {
        private readonly Rectangle rectangle;

        public RectangleRegion(Rectangle _rectangle) 
            => rectangle = _rectangle;

        public Vector2 RandomPosition() 
            => new Vector2(rectangle.X + Basics.Utils.RandomFloat() * rectangle.W, 
                           rectangle.Y + Basics.Utils.RandomFloat() * rectangle.H);
    }

    public class Particle : IPosition
    {
        public class Emitter : Actor
        {
            private static Pool<Particle> particlePool = new Pool<Particle>(() => new Particle());

            private readonly IParticleSystem system;
            private readonly IRegion region;            
            private readonly List<Particle> particles;

            public Emitter(float _x, float _y, IParticleSystem _system, IRegion _region)
                : base(_x, _y)
            {
                system = _system;
                region = _region;
                particles = new List<Particle>();
            }

            public void Emit(int _count)
            {
                _count.Repetitions(() =>
                {
                    var position = region.RandomPosition();
                    var particle = particlePool.Get();
                    particle.Initialize(X + position.X, Y + position.Y, system.StartColor, system.StartSize, system.StartSpeedMin, system.StartSpeedMax);
                    particle.Rocket.Angle = system.StartAngle;
                    particle.Rocket.Speed = system.StartSpeed;
                    particles.Add(particle);
                });
            }

            public override void Update()
            {
                var toDestroy = new HashSet<Particle>();
                foreach (var particle in particles)
                {
                    system.Update(particle);

                    if (system.ShouldDestroy(particle))
                        toDestroy.Add(particle);
                }
                toDestroy.ForEach(destroyParticle);
            }

            private ColoredVertexRenderer particleRenderer = new ColoredVertexRenderer();

            private async Task<ConcurrentDictionary<ParticlePrimitive, ConcurrentQueue<IEnumerable<ColoredVertex>>>> RenderSetupAsync()
            {
                //Batching by 10, 100, 1000 didn't improve performance beyond sync version
                //Not yet sure why this is slower

                var results = new ConcurrentDictionary<ParticlePrimitive, ConcurrentQueue<IEnumerable<ColoredVertex>>>();
                foreach (var value in Basics.Utils.GetValues<ParticlePrimitive>())
                    results[value] = new ConcurrentQueue<IEnumerable<ColoredVertex>>();

                var tasks = new Task[particles.Count];
                for (var i = 0; i < particles.Count; i++)
                {
                    var particle = particles[i];
                    var task = Task.Factory.StartNew(() =>
                    {
                        var renders = system.Vertices(particle).Select(o => ((ParticlePrimitive PrimitiveType, IEnumerable<ColoredVertex> Vertices))(o.PrimitiveType, o.Vertices.Select(v => new ColoredVertex(new Vector3(v.X, v.Y, 0), particle.Color))));
                        foreach (var render in renders)
                            results[render.PrimitiveType].Enqueue(render.Vertices);
                    });
                    tasks[i] = task;
                }

                await Task.WhenAll(tasks.ToArray());
                return results;
            }

            private async Task<ConcurrentDictionary<ParticlePrimitive, ConcurrentQueue<IEnumerable<ColoredVertex>>>> RenderSetupSync()
            {
                var results = new ConcurrentDictionary<ParticlePrimitive, ConcurrentQueue<IEnumerable<ColoredVertex>>>();
                foreach (var value in Basics.Utils.GetValues<ParticlePrimitive>())
                    results[value] = new ConcurrentQueue<IEnumerable<ColoredVertex>>();
                
                for (var i = 0; i < particles.Count; i++)
                {
                    var particle = particles[i];
                    var renders = system.Vertices(particle).Select(o => ((ParticlePrimitive PrimitiveType, IEnumerable<ColoredVertex> Vertices))(o.PrimitiveType, o.Vertices.Select(v => new ColoredVertex(new Vector3(v.X, v.Y, 0), particle.Color))));
                    foreach (var render in renders)
                        results[render.PrimitiveType].Enqueue(render.Vertices);
                }
                return await Task.FromResult(results);
            }

            public override void Render()
            {
                ConcurrentDictionary<ParticlePrimitive, ConcurrentQueue<IEnumerable<ColoredVertex>>> verticesByType = null;

                var ms = Basics.Utils.MillisecondsDuration(() =>
                {
                    verticesByType = RenderSetupSync().Result;
                });
                Game.LogWarning(ms.ToString());

                foreach ((ParticlePrimitive primitiveType, ConcurrentQueue<IEnumerable<ColoredVertex>> vertexLists) in verticesByType)
                {
                    var buffer = new ColoredVertexBuffer((PrimitiveType)primitiveType);
                    buffer.AddVertices(vertexLists.SelectMany(o => o));
                    particleRenderer.Initialize(buffer);
                    particleRenderer.Render();
                    buffer.Destroy();
                }
            }

            private void destroyParticle(Particle _particle)
            {
                particles.Remove(_particle);
                particlePool.Add(_particle);
            }
        }

        /// <summary>
        /// Position in the Game relative to the camera
        /// </summary>
        public float X { get; set; }
        public float Y { get; set; }

        /// <summary>
        /// Current color of the Particle
        /// </summary>
        public Color4 Color { get; set; }

        /// <summary>
        /// Current size of the Particle
        /// </summary>
        public float Size { get; set; }

        public Rocket Rocket;

        /// <summary>
        /// Time since the Particle was born, in milliseconds
        /// </summary>
        public float Age => Game.MillisecondsSinceStart - birthday;
        private float birthday;

        public void Initialize(float _x=0, float _y=0, float _size=1, float _speedMin = 0, float _speedMax = float.MaxValue) => Initialize(_x, _y, Color4.White, _size, _speedMin, _speedMax);
        public void Initialize(float _x, float _y, Color4 _color, float _size=1, float _speedMin=0, float _speedMax=float.MaxValue)
        {
            X = _x;
            Y = _y;
            Color = _color;
            Size = _size;
            birthday = Game.MillisecondsSinceStart;
            Rocket = new Rocket(_speedMin:_speedMin, _speedMax:_speedMax);
        }
    }
}
