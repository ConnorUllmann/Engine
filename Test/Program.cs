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
            private Color4 color;
            public TestActor(float _x, float _y, Color4 _color) : base(_x, _y, 150, 150)
            {
                color = _color;
            }

            public override void Render()
            {
                base.Render();

                Engine.Debug.Draw.Rectangle(CollisionBox, _color: color);
            }
        }

        static void Main(string[] args)
        {
            //Basics.VisualTests.Pathfinding();)
            var game = new ShellGame(640, 480);

            var r = new TestActor(-50, -50, Color4.Red);
            var g = new TestActor(0, 0, Color4.Green);
            var b = new TestActor(50, 50, Color4.Blue);

            game.StartHandler += () =>
            {
                b.Depth = 100;
                g.Depth = 0;
                r.Depth = -100;

                r.AddToWorld();
                g.AddToWorld();
                b.AddToWorld();
            };

            game.UpdateHandler += () =>
            {
                b.Depth--;
            };
            
            game.Run();

        }
    }
}
