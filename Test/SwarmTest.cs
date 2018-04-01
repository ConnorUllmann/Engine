using System;
using System.Collections.Generic;
using System.Linq;
using Basics;
using Engine.Actors;
using Basics.QuadTree;
using Engine;
using OpenTK;
using OpenTK.Graphics;
using Rectangle = Basics.Rectangle;

namespace Test
{
    public class SwarmTest
    {
        private class Swarmer : RocketPolygonActor, ISwarmer
        {
            private SwarmInstinct instinct;

            private Color4 color;
            private float angleNext;

            public Swarmer(float _x, float _y, Color4 _color) : base(ConvexPolygon.Regular(3, 10, (float)-Math.PI / 2), _x, _y)
            {
                instinct = new SwarmInstinct(this);
                SpeedMax = 60;
                Speed = 60;
                Angle = (float)Basics.Utils.RandomAngleRad();
                angleNext = (float)Basics.Utils.RandomAngleRad();
                color = _color;
                BoundingBox = new BoundingBox(20, 20);
            }

            public IEnumerable<ISwarmer> Swarmers => quadtree.QueryRect(instinct.VisibleRectangle).Select(o => (ISwarmer)o);

            public override void Update()
            {
                angleNext = instinct.Angle() ?? angleNext;

                base.Update();
                Position = Game.ScreenWrap(NextPosition);
            }

            public override void PostUpdate()
            {
                var diff = Basics.Utils.AngleDifferenceRadians(Angle, angleNext);
                var amount = 5 * Game.Delta;
                if (Math.Abs(diff) < amount)
                    Angle = angleNext;
                else
                    Angle += amount * Math.Sign(diff);
            }

            public override void Render()
            {
                base.Render();

                Engine.Debug.Draw.Line(this, Position + Engine.Utils.Vector2(Angle, 15), Color4.Red);
            }
        }

        private static QuadTree<Swarmer> quadtree;

        public static void Run()
        {
            var game = new ShellGame(1280, 960);
            quadtree = new QuadTree<Swarmer>(-Game.Width / 2, -Game.Height / 2, Game.Width, Game.Height, 2, 20);


            game.StartHandler += () =>
            {
                100.Repetitions(() => new Swarmer(Game.RandomX(), Game.RandomY(), Color4.Blue).AddToGroup());
            };

            game.UpdateHandler += () =>
            {
                quadtree.Reset();
                var objs = Game.Group.GetActorsOfType<Swarmer>();
                foreach (var obj in objs)
                    quadtree.Insert(obj);
            };

            game.RenderHandler += () =>
            {
                foreach (var r in quadtree.GetRectangles())
                    Engine.Debug.Draw.Rectangle(r, 0, 0, Color4.Green, false);

                (float X, float Y) start = (-50, -50);
                (float X, float Y) target = (Input.Mouse.X, Input.Mouse.Y);
                var elbow = Basics.Utils.SingleJoint(start.X, start.Y, target.X, target.Y, 250, 200, false);
                if (elbow.HasValue)
                {
                    Engine.Debug.Draw.Line(start.X, start.Y, elbow.Value.X, elbow.Value.Y, Color4.Red);
                    Engine.Debug.Draw.Line(target.X, target.Y, elbow.Value.X, elbow.Value.Y, Color4.Orange);
                }
                Engine.Debug.Draw.Line(start.X, start.Y, target.X, target.Y, Color4.Green);
                Engine.Debug.Draw.Rectangle(new Rectangle(-8, -8, 16, 16), start.X, start.Y, Color4.Blue);
                Engine.Debug.Draw.Rectangle(new Rectangle(-8, -8, 16, 16), target.X, target.Y, Color4.Yellow);
            };

            game.Run();

        }
    }
}
