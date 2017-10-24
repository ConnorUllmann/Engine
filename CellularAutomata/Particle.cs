using System;
using Engine;
using System.Linq;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;
using Basics;
using System.Threading.Tasks;


namespace CellularAutomata
{
    class Particle : PolygonActor
    {
        public float Radius { get; private set; }
        public float Mass { get; private set; }
        public Vector2 Acceleration { get; private set; }
        public Vector2 Velocity { get; private set; }

        private float AngularVelocity = (float)(Basics.Utils.RandomDouble() * 2 - 1) * 0.01f;

        private static float GetRadius(float _mass) => 4 * (float)Math.Sqrt(_mass);

        public Particle(float _mass, int _x, int _y, int _sides=4) : base(ConvexPolygon.Regular(_sides, GetRadius(_mass)).Move(new Vector3(_x - Game.Width / 2, _y - Game.Height / 2, 0)))
        {
            Mass = _mass;
            Radius = GetRadius(Mass);
            FillVisible = false;
        }

        public override void Update()
        {
            Velocity += Acceleration * Game.Delta;
            Position += Velocity * Game.Delta;

            //ScreenWrap(Radius);
            ScreenClamp(-Radius);

            Rotate(AngularVelocity * Game.Delta);
        }

        public override void PostUpdate()
        {
            Acceleration = Vector2.Zero;
        }

        public Vector2 AirResistanceForce()
        {
            if (Velocity.LengthSquared <= 0)
                return Vector2.Zero;
            return Radius * Velocity.LengthSquared * Game.Delta * -Velocity.Normalized();
        }
        public Vector2 GravityForce(Particle other)
        {
            if (X == other.X && Y == other.Y)
                return Vector2.Zero;
            var diff = other.Position - Position;
            var radii = Radius + other.Radius;
            return Mass * other.Mass / Basics.Utils.Max(diff.LengthSquared, radii * radii) * diff.Normalized();
        }
        
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
                var mass = (float)Basics.Utils.RandomDouble() * 99 + 1;
                var x = (int)(Basics.Utils.RandomDouble() * Game.Width / 2 + Game.Width / 4);
                var y = (int)(Basics.Utils.RandomDouble() * Game.Height / 2 + Game.Height / 4);
                var particle = new Particle(mass, x, y);
                particles.Add(particle);
                particle.AddToWorld();
            }
        }

        public void ApplyVectorGridSynchronously(VectorGrid _vectorGrid)
        {
            foreach(var particle in particles)
            {
                var tile = _vectorGrid.Get(_vectorGrid.WorldPositionToGridPosition(particle.Position));
                if(tile != null)
                    particle.ApplyForce(0.001f * tile.Vector * Game.Delta);
            }
        }

        public void ApplyAirResistanceSynchronously() => particles.ForEach(ApplyAirResistance);
        private void ApplyAirResistance(Particle _particle) => ApplyAirResistance(_particle, 0.001f);
        private void ApplyAirResistance(Particle _particle, float _drag)
        {
            _particle.ApplyForce(_drag * _particle.AirResistanceForce());
        }

        public void ApplyGravitySynchronously() => particles.ForEach(ApplyGravity);
        public void ApplyGravityAsynchronously()
        {
            var tasks = new List<Task>();
            foreach (var currentParticle in particles)
                tasks.Add(Task.Factory.StartNew(() => ApplyGravity(currentParticle)));
            Task.WaitAll(tasks.ToArray());
        }

        private void ApplyGravity(Particle _particle) => ApplyGravity(_particle, 0.0001f);
        private void ApplyGravity(Particle _particle, float _gravity)
        {
            foreach (var otherParticle in particles)
                _particle.ApplyForce(_gravity * _particle.GravityForce(otherParticle));
        }
    }
}
