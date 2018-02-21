using System;
using System.Collections.Generic;
using OpenTK;
using Basics;
using Engine;

namespace Fighter
{
    public class Fighter : PolygonActor
    {
        private float anglePrevious;
        private float angle;
        public float Angle
        {
            get => angle;
            set
            {
                if (value == angle)
                    return;
                anglePrevious = angle;
                angle = value;
                Rotate(angle - anglePrevious);
            }
        }
        public float Speed;

        public Fighter(float radius) : base(new ConvexPolygon(new List<Vector2> { new Vector2(-radius, radius), new Vector2(-radius, -radius), new Vector2(2 * radius, 0) }))
        {
            anglePrevious = angle = 0;
            Speed = 0;
        }

        public override void Update()
        {
            var angularAcceleration = 3 * ((Input.KeyDown(OpenTK.Input.Key.Left) ? 1 : 0) - (Input.KeyDown(OpenTK.Input.Key.Right) ? 1 : 0));
            var positionalAcceleration = 100 * ((Input.KeyDown(OpenTK.Input.Key.Up) ? 1 : 0) - (Input.KeyDown(OpenTK.Input.Key.Down) ? 1 : 0));
            Angle += angularAcceleration * Game.Delta;
            Speed += positionalAcceleration * Game.Delta;
            Position += Speed * Engine.Utils.UnitVector2(Angle) * Game.Delta;
        }
    }

    class FighterGame : Game
    {
        private Fighter actor;

        public FighterGame() : base(1280, 720, "Fighter")
        { }

        public override void Start()
        {
            var actor = new Fighter(10);
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
            new FighterGame().Run();
        }
    }
}