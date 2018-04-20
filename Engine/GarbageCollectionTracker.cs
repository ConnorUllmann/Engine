using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Engine
{
    internal class GarbageCollectionTracker
    {
        private static readonly HashSet<int> generations = new HashSet<int> { 0, 1, 2 };
        private readonly Dictionary<int, (long count, bool garbageCollected)> garbageCollectionsByGeneration;

        public bool Active = true;

        internal GarbageCollectionTracker()
        {
            garbageCollectionsByGeneration = new Dictionary<int, (long count, bool garbageCollected)>();
            foreach (var generation in generations)
                garbageCollectionsByGeneration.Add(generation, (0, false));
        }

        public void Update()
        {
            if (!Active)
                return;

            foreach (var gen in generations)
            {
                var newCount = GC.CollectionCount(gen);
                var oldCount = garbageCollectionsByGeneration[gen].count;
                garbageCollectionsByGeneration[gen] = (newCount, newCount > oldCount);
            }
        }

        /// <summary>
        /// Determines whether a generation 0, 1, or 2 garbage collection has occurred between the last two calls of Update().
        /// </summary>
        /// <returns>True if a generation 0, 1, or 2 garbage collection has occurred between the last two calls of Update()</returns>
        public bool GarbageCollectionsByGeneration(int? generation = null)
        {
            if (generation != null && !generations.Contains(generation.Value))
                throw new ArgumentOutOfRangeException($"The 'generation' argument for GarbageCollectionTracker.GarbageCollectionsByGeneration() must be one of (null, {string.Join(", ", generations)})");

            return generation == null
                ? garbageCollectionsByGeneration.Values.Any(o => o.garbageCollected)
                : garbageCollectionsByGeneration[generation.Value].garbageCollected;
        }
    }
}
