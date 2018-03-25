using System;
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
        private class TestActor : PolygonActor
        {
            //R_O_ATTRACTION > R_O_ALIGNMENT > R_O_REPULSION
            public const float RADIUS_OF_REPULSION = 20;
            public const float RADIUS_OF_ALIGNMENT = 50;
            public const float RADIUS_OF_ATTRACTION = 60;
            public const float RADIUS2_OF_REPULSION = RADIUS_OF_REPULSION * RADIUS_OF_REPULSION;
            public const float RADIUS2_OF_ALIGNMENT = RADIUS_OF_ALIGNMENT * RADIUS_OF_ALIGNMENT;
            public const float RADIUS2_OF_ATTRACTION = RADIUS_OF_ATTRACTION * RADIUS_OF_ATTRACTION;
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

            private Vector2 direction = new Vector2(0, 1);
            private Vector2 lastDirection = new Vector2(0, 1);
            private float targetAngle;


            public TestActor(float _x, float _y, Color4 _color) : base(ConvexPolygon.Regular(3, 10, (float)-Math.PI/2), _x, _y)
            {
                mover = new Mover((_a) => Rotate(_a, CenterOfMass), 0, 60);
                Speed = 60;
                Angle = (float)Basics.Utils.RandomAngleRad();
                targetAngle = (float)Basics.Utils.RandomAngleRad();
                color = _color;
                BoundingBox = new BoundingBox(20, 20);
            }

            public override void PreUpdate()
            {
                base.PreUpdate();

                lastDirection = direction;
            }

            public override void Update()
            {
                base.Update();

                direction = GetDirection() ?? direction;
                targetAngle = direction.Radians();
                Angle += 10 * Game.Delta * Math.Sign(Basics.Utils.AngleDifferenceRadians(Angle, targetAngle));

                Position += mover.DeltaPosition();
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

            private HashSet<TestActor> actorsInRadius(float _radius)
            {
                var actorsPotentiallyInRadius = quadtree.QueryRect(X - _radius, Y - _radius, 2 * _radius, 2 * _radius);
                var radiusSquared = _radius * _radius;
                actorsPotentiallyInRadius.RemoveWhere(o => o.DistanceSquared(this) > radiusSquared);
                actorsPotentiallyInRadius.Remove(this);
                return actorsPotentiallyInRadius;
            }

            public Vector2? GetDirection()
            {
                var result = Vector2.Zero;

                var neighbors = actorsInRadius(RADIUS_OF_ATTRACTION);
                if (neighbors.Count == 0)
                    return null;
                foreach (var neighbor in neighbors)
                {
                    var distanceSquared = neighbor.Position.DistanceSquared(Position);
                    var directionToNeighbor = distanceSquared == 0 ? Vector2.Zero : (neighbor.Position - Position).Normalized();
                    
                    if (distanceSquared <= RADIUS2_OF_REPULSION)
                    {
                        //REPULSION influence
                        result += 100 * (RADIUS2_OF_REPULSION - distanceSquared) / RADIUS2_OF_REPULSION * -directionToNeighbor;
                    }
                    else if (distanceSquared <= RADIUS2_OF_ALIGNMENT)
                    {
                        //ALIGNMENT influence
                        result += neighbor.lastDirection;
                    }
                    else
                    {
                        //ATTRACTION influence
                        result += directionToNeighbor;
                    }
                }

                return result == Vector2.Zero ? (Vector2?)null : result.Normalized();
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
