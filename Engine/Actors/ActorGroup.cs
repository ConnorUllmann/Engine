using System;
using System.Collections.Generic;
using System.Linq;
using Basics;

namespace Engine.Actors
{
    public class ActorGroup
    {
        public static ActorGroup World = new ActorGroup();

        private int nextID = 0;
        public int GetNextID() => nextID++;
        private Dictionary<int, Actor> actorsByID;
        public Dictionary<int, Actor> Actors => actorsByID;

        private int nextTypeID = 0;
        public int GetNextTypeID() => nextTypeID++;
        public int GetTypeID(Actor _actor) => typeIDByName.GetOrAdd(_actor.GetType().Name, GetNextTypeID);
        private Dictionary<string, int> typeIDByName;
        private Dictionary<int, HashSet<Actor>> actorsByTypeID;

        private HashSet<Actor> actorsToAdd;
        private HashSet<Actor> actorsToRemove;

        private ActorDepthSorter depthSorter;

        private ActorGroup()
        {
            Reset();
        }

        public void Reset()
        {
            actorsByID = new Dictionary<int, Actor>();
            actorsByTypeID = new Dictionary<int, HashSet<Actor>>();
            typeIDByName = new Dictionary<string, int>();
            actorsToAdd = new HashSet<Actor>();
            actorsToRemove = new HashSet<Actor>();
            depthSorter = new ActorDepthSorter();
        }

        public void Update()
        {
            UpdateRemoveActors();
            UpdateAddActors();

            depthSorter.ForEach(actor =>
            {
                if (actor.Active)
                    actor.PreUpdate();
            });
            depthSorter.ForEach(actor =>
            {
                if (actor.Active)
                    actor.Update();
            });
            depthSorter.ForEach(actor =>
            {
                if (actor.Active)
                    actor.PostUpdate();
            });
        }

        public void Render()
        {
            depthSorter.ForEach(actor =>
            {
                if (actor.Visible)
                    actor.Render();
            });
        }

        private void UpdateAddActors()
        {
            actorsToAdd.ForEach(AddActorNow);
            actorsToAdd.Clear();
        }

        private void UpdateRemoveActors()
        {
            actorsToRemove.ForEach(RemoveActorNow);
            actorsToRemove.Clear();
        }

        private void AddActorNow(Actor _actor)
        {
            //Add to depthSorter so that 1) the actor's depth in the constructor is used, and 2) if it is overridden in the actor's Start(), 
            depthSorter.Add(_actor);
            _actor.Start();
            actorsByID[_actor.ID] = _actor;
            actorsByTypeID.GetOrAddNew(_actor.TypeID).Add(_actor);
        }

        private void RemoveActorNow(Actor _actor)
        {
            depthSorter.Remove(_actor);
            _actor.OnRemove();
            actorsByID.Remove(_actor.ID);
            if (actorsByTypeID.TryGetValue(_actor.TypeID, out var set))
            {
                set.Remove(_actor);
                if (set.Count == 0)
                    actorsByTypeID.Remove(_actor.TypeID);
            }
        }

        //Slightly more appealing name for the externally-facing version of AddOnNextFrame
        public Actor AddToWorld(Actor _actor) => AddOnNextFrame(_actor);
        private Actor AddOnNextFrame(Actor _actor)
        {
            _actor.DestroyHandler += () => RemoveOnNextFrame(_actor);
            actorsToRemove.Remove(_actor);
            actorsToAdd.Add(_actor);
            return _actor;
        }

        private void RemoveOnNextFrame(Actor _actor)
        {
            actorsToAdd.Remove(_actor);
            actorsToRemove.Add(_actor);
        }

        /// <summary>
        /// Sorts actors by their depth value
        /// </summary>
        internal class ActorDepthSorter
        {
            private SortedSet<Actor> actors;
            private Dictionary<Actor, float> toUpdate = new Dictionary<Actor, float>();

            public ActorDepthSorter() => actors = new SortedSet<Actor>(actorDepthComparer);

            public void Add(Actor _actor)
            {
                if (actors.Add(_actor))
                    _actor.changeDepth += ChangeDepth;
            }

            public void Remove(Actor _actor)
            {
                if (actors.Remove(_actor))
                    _actor.changeDepth -= ChangeDepth;
            }

            /// <summary>
            /// Enqueues an update to an actor's depth that will execute before the next iteration over the set.
            /// Actor.Depth is updated instantly, but the dictionary brings the actors set into sync on the next UpdateActors call.
            /// </summary>
            /// <param name="_actor"></param>
            /// <param name="_newDepth"></param>
            internal void ChangeDepth(Actor _actor, float _newDepth)
            {
                if (_actor.Depth == _newDepth)
                    return;
                toUpdate[_actor] = _actor.Depth;
                _actor.depth = _newDepth; //So that any calls to _actor.Depth will return the new depth value
            }

            /// <summary>
            /// Called before each iteration over the set to bring it into sync with current actor depth values
            /// </summary>
            internal void UpdateActors()
            {
                //Execute each change to each actors' place within the actors set
                foreach(var kv in toUpdate)
                {
                    var actor = kv.Key;
                    var oldDepth = kv.Value;
                    var newDepth = actor.depth;
                    actor.depth = oldDepth;
                    actors.Remove(actor);
                    actor.depth = newDepth;
                    actors.Add(actor);
                }
                toUpdate.Clear();
            }
            
            public void ForEach(Action<Actor> _action)
            {
                UpdateActors();
                foreach (var actor in actors)
                    _action(actor);
            }

            private static IComparer<Actor> actorDepthComparer => Comparer<Actor>.Create((a, b) => actorDepthComparison(a, b));
            private static Comparison<Actor> actorDepthComparison = new Comparison<Actor>(compareActorDepths);

            /// <summary>
            /// -1 if _a has a lower depth than _b (or they have the same depth and _a has a lower ID)
            /// 1 if _a has a higher depth than _b (or they have the same depth and _a has a higher ID)
            /// </summary>
            /// <param name="_a">actor to compare</param>
            /// <param name="_b">actor to compare against</param>
            /// <returns>relative depths of _a and _b = (-1, 0, 1) if _a is (less than, equal to, greater than) _b)</returns>
            private static int compareActorDepths(Actor _a, Actor _b)
            {
                return _a.Depth > _b.Depth
                    ? -1
                    : _a.Depth < _b.Depth
                        ? 1
                        : _a.ID < _b.ID
                            ? -1
                            : _a.ID > _b.ID
                                ? 1
                                : 0;
            }
        }
    }
}
