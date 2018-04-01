using System;
using System.Collections.Generic;
using System.Linq;
using Basics;
using OpenTK;
using Rectangle = Basics.Rectangle;

namespace Engine
{
    /// <summary>
    /// An interface which defines an object which can be used with SwarmInstinct
    /// </summary>
    public interface ISwarmer : IPosition
    {
        /// <summary>
        /// Angle that the swarmer is currently facing
        /// </summary>
        float Angle { get; }

        /// <summary>
        /// The fellow swarmers that this swarmer should consider when determining which angle to face
        /// </summary>
        IEnumerable<ISwarmer> Swarmers { get; }
    }

    public class SwarmInstinct
    {
        public float RadiusOfRepulsion;
        public float RadiusOfAlignment;
        public float RadiusOfAttraction;

        public float RadiusOfRepulsionSquared;
        public float RadiusOfAlignmentSquared;
        public float RadiusOfAttractionSquared;

        public float RepulsionMultiplier;
        public float AlignmentMultiplier;
        public float AttractionMultiplier;

        private ISwarmer swarmer;

        public SwarmInstinct(ISwarmer _swarmer,
            float _radiusOfRepulsion = 20,
            float _radiusOfAlignment = 40,
            float _radiusOfAttraction = 80,
            float _repulsionMultiplier = 50,
            float _alignmentMultiplier = 1,
            float _attractionMultiplier = 1)
        {
            swarmer = _swarmer;

            //Verify arguments
            if (_radiusOfRepulsion > _radiusOfAlignment)
                throw new ArgumentException("_radiusOfRepulsion must be <= _radiusOfAlignment");
            if (_radiusOfAlignment > _radiusOfAttraction)
                throw new ArgumentException("_radiusOfAlignment must be <= _radiusOfAttraction");

            RadiusOfRepulsion = _radiusOfRepulsion;
            RadiusOfAlignment = _radiusOfAlignment;
            RadiusOfAttraction = _radiusOfAttraction;

            RadiusOfRepulsionSquared = RadiusOfRepulsion * RadiusOfRepulsion;
            RadiusOfAlignmentSquared = RadiusOfAlignment * RadiusOfAlignment;
            RadiusOfAttractionSquared = RadiusOfAttraction * RadiusOfAttraction;

            RepulsionMultiplier = _repulsionMultiplier;
            AlignmentMultiplier = _alignmentMultiplier;
            AttractionMultiplier = _attractionMultiplier;
        }

        /// <summary>
        /// Bounding box of the swarm instinct's circle of perception
        /// </summary>
        public Rectangle VisibleRectangle => new Rectangle(swarmer.X - RadiusOfAttraction, swarmer.Y - RadiusOfAttraction, 2 * RadiusOfAttraction, 2 * RadiusOfAttraction);

        /// <summary>
        /// Returns the angle that the swarm instinct is indicating to move (if any)
        /// </summary>
        /// <returns>The angle the swarm instinct is indicating (if any)</returns>
        public float? Angle() => swarmer.Swarmers.Select(swarmVectorForNeighbor).Sum().Radians();

        /// <summary>
        /// Determines what this swarmer should do in response to the given neighbor within the radius of attraction
        /// </summary>
        /// <param name="_neighbor"></param>
        /// <returns>The vector for what direction this swarmer should do in response to the given neighbor</returns>
        private Vector2 swarmVectorForNeighbor(ISwarmer _neighbor)
        {
            if (swarmer == _neighbor)
                return Vector2.Zero;
            var position = new Vector2(swarmer.X, swarmer.Y);
            var neighborPosition = new Vector2(_neighbor.X, _neighbor.Y);
            var distanceSquared = neighborPosition.DistanceSquared(position);
            return distanceSquared < 0.001f
                ? Vector2.Zero
                : distanceSquared <= RadiusOfRepulsionSquared
                    ? RepulsionMultiplier * (position - neighborPosition).Normalized()
                    : distanceSquared <= RadiusOfAlignmentSquared
                        ? AlignmentMultiplier * Engine.Utils.Vector2(_neighbor.Angle)
                        : distanceSquared <= RadiusOfAttractionSquared
                            ? AttractionMultiplier * (neighborPosition - position).Normalized()
                            : Vector2.Zero;
        }
    }
}
