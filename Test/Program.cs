using System;
using Basics;
using Engine;
using Engine.Actors;
using OpenTK.Graphics;

namespace Test
{
    class Program
    {
        private class TestActor : Actor
        {
            private Mover mover;
            public float Angle
            {
                get => mover.Angle;
                private set => mover.Angle = value;
            }
            public float Speed
            {
                get => mover.Speed;
                private set => mover.Speed = value;
            }
            public float SpeedMax
            {
                get => mover.SpeedMax;
                private set => mover.SpeedMax = value;
            }

            private Color4 color;
            public TestActor(float _x, float _y, Color4 _color) : base(_x, _y, 10, 10)
            {
                mover = new Mover(null, 0, 10);
                Speed = 10;
                Angle = (float)Basics.Utils.RandomAngleRad();
                color = _color;
            }

            public override void Update()
            {
                Angle += Game.Delta;
                Position += mover.DeltaPosition();
            }

            public override void Render()
            {
                Engine.Debug.Draw.Rectangle(CollisionBox, _color: color);
            }
        }

        static void Main(string[] args)
        {
            var game = new ShellGame(640, 480);

            var r = new TestActor(-50, -50, Color4.Red);
            var g = new TestActor(0, 0, Color4.Green);
            var b = new TestActor(50, 50, Color4.Blue);

            game.StartHandler += () =>
            {
                r.AddToGroup();
                g.AddToGroup();
                b.AddToGroup();
            };

            game.UpdateHandler += () =>
            {
            };
            
            game.Run();

        }
    }
}
