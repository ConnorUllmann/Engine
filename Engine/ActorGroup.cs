using System;
using System.Collections.Generic;
using System.Text;
using Basics;

namespace Engine
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
        }

        public void Update()
        {
            UpdateRemoveActors();
            UpdateAddActors();
            foreach (var actor in actorsByID.Values)
            {
                if (actor.Active)
                    actor.PreUpdate();
                if (actor.Active)
                    actor.Update();
                if (actor.Active)
                    actor.PostUpdate();
            }
        }

        public void Render()
        {
            foreach (var actor in actorsByID.Values)
                if (actor.Visible)
                    actor.Render();
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
            _actor.Start();
            actorsByID[_actor.ID] = _actor;
            actorsByTypeID.GetOrAddNew(_actor.TypeID).Add(_actor);
        }

        private void RemoveActorNow(Actor _actor)
        {
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
    }
}
