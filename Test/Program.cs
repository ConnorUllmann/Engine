using System;
using System.Linq;
using System.Collections.Generic;
using Basics;
using Basics.QuadTree;
using Engine;
using Engine.Actors;
using Engine.OpenGL;
using Engine.OpenGL.Colored;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Rectangle = Basics.Rectangle;

namespace Test
{
    class Program
    {

        public class TestCircleParticleSystem : IParticleSystem
        {
            public Color4 StartColor => Color4.Red;
            public float StartSize => (Basics.Utils.RandomFloat() * 0.5f + 1) * 20;
            public float StartSpeedMin => 200;
            public float StartSpeedMax => 450;
            public float StartAngle => (float)Basics.Utils.RandomAngleRad() / 4 + (float)Math.PI * 3 / 8;
            public float StartSpeed => StartSpeedMin + (StartSpeedMax - StartSpeedMin) * Basics.Utils.RandomFloat();

            public void Update(Particle _particle)
            {
                var nextPosition = _particle.Position() + _particle.Rocket.DeltaPosition();
                _particle.X = nextPosition.X;
                _particle.Y = nextPosition.Y;
                _particle.Rocket.Speed -= 50 * Game.Delta;
                _particle.Size -= Basics.Utils.RandomFloat() * _particle.Age * 0.0005f;

                var size = _particle.Size / 6f;
                var size4 = size * size * size * size;
                if (Basics.Utils.RandomDouble() >= size4)
                    _particle.Rocket.Angle += (float)(Math.PI * 2 * Basics.Utils.RandomSign() * Basics.Utils.RandomFloat() * Game.Delta);

                _particle.Color = ColorExtensions.Darkened(_particle.Color, (_particle.Rocket.SpeedMax - _particle.Rocket.Speed) * 0.8f * Game.Delta);
            }

            public bool ShouldDestroy(Particle _particle) => _particle.Size <= 0;

            private static readonly IEnumerable<(float X, float Y)> unitCircleSegments = Basics.Utils.CircleSegments(0, 0, 1).SelectMany(o => new[] { o.a, o.b });

            public IEnumerable<(ParticlePrimitive PrimitiveType, IEnumerable<(float X, float Y)> Vertices)> Vertices(Particle _particle)
            {
                //Engine.Debug.Draw.Circle(_particle.X, _particle.Y, _particle.Size, _particle.Color);
                return new List<(ParticlePrimitive, IEnumerable<(float X, float Y)>)>()
                {
                    (ParticlePrimitive.Lines, unitCircleSegments.Select(o => (_particle.X + o.X * _particle.Size, _particle.Y + o.Y * _particle.Size)))
                };
            }
        }

        public class TestRectangleParticleSystem : IParticleSystem
        {
            public Color4 StartColor => ColorExtensions.Lightened(ColorExtensions.RandomColor(), 60);
            public float StartSize => (Basics.Utils.RandomFloat() * 0.5f + 1) * 10;
            public float StartSpeedMin => 0;
            public float StartSpeedMax => 250;
            public float StartAngle => (float)Basics.Utils.RandomAngleRad();
            public float StartSpeed => StartSpeedMax - 100 * Basics.Utils.RandomFloat();

            public void Update(Particle _particle)
            {
                var nextPosition = _particle.Position() + _particle.Rocket.DeltaPosition();
                _particle.X = nextPosition.X;
                _particle.Y = nextPosition.Y;
                if (_particle.Rocket.Speed > 1f)
                    _particle.Rocket.Angle += 1f * Game.Delta;
                _particle.Rocket.Speed -= 100 * Game.Delta;
                _particle.Size -= Basics.Utils.RandomFloat() * _particle.Age * 0.00005f;

                _particle.Color = ColorExtensions.Darkened(_particle.Color, (_particle.Rocket.SpeedMax - _particle.Rocket.Speed) * 0.5f * Game.Delta);
            }

            public bool ShouldDestroy(Particle _particle) => _particle.Size <= 0;

            public IEnumerable<(ParticlePrimitive PrimitiveType, IEnumerable<(float X, float Y)> Vertices)> Vertices(Particle _particle)
            {
                var rectangle = new Rectangle(_particle.X - _particle.Size / 2, _particle.Y - _particle.Size / 2, _particle.Size, _particle.Size);
                return new List<(ParticlePrimitive, IEnumerable<(float X, float Y)>)>()
                {
                    (ParticlePrimitive.Quads, rectangle.ToVertices().Rotated(_particle.Rocket.Angle, _particle.X, _particle.Y))
                    //(ParticlePrimitive.Points, new List<(float X, float Y)> () { (_particle.X, _particle.Y) })
                };
            }
        }

        static void Main(string[] args)
        {
            var game = new ShellGame(600, 600);
            Game.LogShouldPrintTime = true;
            Game.LogShouldPrintLevel = false;

            Particle.Emitter mouseEmitter = null;
            Particle.Emitter regionEmitter = null;

            game.StartHandler += () =>
            {
                mouseEmitter = new Particle.Emitter(0, 0, new TestRectangleParticleSystem(), new RectangleRegion(new Rectangle(-10, -10, 20, 20)));
                mouseEmitter.AddToGroup();

                regionEmitter = new Particle.Emitter(0, -300, new TestCircleParticleSystem(), new RectangleRegion(new Rectangle(-300, -320, 600, 10)));
                regionEmitter.AddToGroup();
            };

            game.UpdateHandler += () =>
            {
                mouseEmitter.X = Input.Mouse.X;
                mouseEmitter.Y = Input.Mouse.Y;
                if(Input.LeftMouseDown)
                    mouseEmitter.Emit(5);

                regionEmitter.Emit(15);
            };

            game.RenderHandler += () =>
            {
            };

            game.Run();

        }
    }
}
