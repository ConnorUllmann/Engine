using System;
using System.Linq;
using System.Collections.Generic;
using Basics;
using Basics.QuadTree;
using Engine;
using Engine.Actors;
using OpenTK;
using OpenTK.Graphics;

namespace Test
{
    class Program
    {
        private class TestActor : RocketPolygonActor
        {
            //R_O_ATTRACTION > R_O_ALIGNMENT > R_O_REPULSION
            public const float RADIUS_OF_REPULSION = 20;
            public const float RADIUS_OF_ALIGNMENT = 30;
            public const float RADIUS_OF_ATTRACTION = 60;
            public const float RADIUS2_OF_REPULSION = RADIUS_OF_REPULSION * RADIUS_OF_REPULSION;
            public const float RADIUS2_OF_ALIGNMENT = RADIUS_OF_ALIGNMENT * RADIUS_OF_ALIGNMENT;
            public const float RADIUS2_OF_ATTRACTION = RADIUS_OF_ATTRACTION * RADIUS_OF_ATTRACTION;

            private const float RepulsionMultiplier = 10;
            private const float AlignmentMultiplier = 1;
            private const float AttractionMultiplier = 1;

            private Color4 color;

            private Vector2 direction = new Vector2(0, 1);
            private float targetAngle;


            public TestActor(float _x, float _y, Color4 _color) : base(ConvexPolygon.Regular(3, 10, (float)-Math.PI/2), _x, _y)
            {
                SpeedMax = 60;
                Speed = 60;
                Angle = (float)Basics.Utils.RandomAngleRad();
                targetAngle = (float)Basics.Utils.RandomAngleRad();
                color = _color;
                BoundingBox = new BoundingBox(20, 20);
            }

            public override void Update()
            {

                direction = GetDirection() ?? direction;
                targetAngle = direction.Radians();

                /* Move to PostUpdate */
                var diff = Basics.Utils.AngleDifferenceRadians(Angle, targetAngle);
                var amount = 5 * Game.Delta;
                if (Math.Abs(diff) < amount)
                    Angle = targetAngle;
                else
                    Angle += amount * Math.Sign(diff);
                /**/

                //Do position update after setting the Angle
                base.Update();
                Position = Game.ScreenClamp(Position);

            }

            public override void Render()
            {
                base.Render();

                Engine.Debug.Draw.Line(this, Position + Engine.Utils.Vector2(Angle, 15), Color4.Red);
                //var neighbors = new List<TestActor>(actorsInRadius(RADIUS_OF_ATTRACTION));
                //foreach (var neighbor in neighbors)
                //    Engine.Debug.Draw.Line(this, neighbor, Color4.Gray);
                //Engine.Debug.Draw.Rectangle(CollisionBox, _color: color);
            }
            
            private HashSet<TestActor> actorsWithinRange(float _radius)
            {
                var actors = actorsPotentiallyWithinRange(_radius);
                actors.Remove(this);
                actors.RemoveWhereOutsideRange(this, _radius);
                return actors;
            }

            private HashSet<TestActor> actorsPotentiallyWithinRange(float _radius)
                => quadtree.QueryRect(X - _radius, Y - _radius, 2 * _radius, 2 * _radius);

            public Vector2? GetDirection()
            {
                var neighbors = actorsWithinRange(RADIUS_OF_ATTRACTION);
                if (neighbors.Count == 0)
                    return null;

                var result = neighbors.Select(GetDirectionForNeighbor).Sum();
                return result == Vector2.Zero ? (Vector2?)null : result.Normalized();
            }

            public Vector2 GetDirectionForNeighbor(TestActor _neighbor)
            {
                var distanceSquared = _neighbor.Position.DistanceSquared(Position);
                if (distanceSquared < 0.001f)
                    return Vector2.Zero;

                return distanceSquared <= RADIUS2_OF_REPULSION
                    ? RepulsionMultiplier * (Position - _neighbor.Position).Normalized()
                    : distanceSquared <= RADIUS2_OF_ALIGNMENT
                        ? AlignmentMultiplier * Engine.Utils.Vector2(_neighbor.Angle)
                        : AttractionMultiplier * (_neighbor.Position - Position).Normalized();
            }
        }

        private static QuadTree<TestActor> quadtree;

        static void Main(string[] args)
        {
            var game = new ShellGame(640, 480);
            quadtree = new QuadTree<TestActor>(-Game.Width/2, -Game.Height/2, Game.Width, Game.Height, 2, 20);


            game.StartHandler += () =>
            {
                100.Repetitions(() => new TestActor(Game.RandomX(), Game.RandomY(), Color4.Blue).AddToGroup());
            };

            game.UpdateHandler += () =>
            {
                quadtree.Reset();
                var objs = Game.Group.GetActorsOfType<TestActor>();
                foreach (var obj in objs)
                    quadtree.Insert(obj);
            };

            game.RenderHandler += () =>
            {
                foreach (var r in quadtree.GetRectangles())
                    Engine.Debug.Draw.Rectangle(r, 0, 0, Color4.Green, false);
            };

            game.Run();

        }
    }
}
