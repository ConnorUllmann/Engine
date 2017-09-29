using System;
using System.Collections.Generic;
using System.Text;
using Basics;

namespace Engine
{
    public class ActorGroup
    {
        public static ActorGroup World = new ActorGroup();

        private HashSet<Actor> actors;
        private HashSet<Actor> actorsToAdd;
        private HashSet<Actor> actorsToRemove;

        public ActorGroup()
        {
            Reset();
        }

        public void Reset()
        {
            actors = new HashSet<Actor>();
            actorsToAdd = new HashSet<Actor>();
            actorsToRemove = new HashSet<Actor>();
        }

        public void Update()
        {
            UpdateRemoveActors();
            UpdateAddActors();
            actors.ForEach(o => { if (o.Active) o.PreUpdate(); });
            actors.ForEach(o => { if (o.Active) o.Update(); });
            actors.ForEach(o => { if (o.Active) o.PostUpdate(); });
        }
        public void Render() => actors.ForEach(o => { if (o.Visible) o.Render(); });

        public Actor Add(Actor _actor)
        {
            _actor.DestroyHandler += () => Remove(_actor);
            actorsToRemove.Remove(_actor);
            actorsToAdd.Add(_actor);
            return _actor;
        }
        public void Remove(Actor _actor)
        {
            actorsToAdd.Remove(_actor);
            actorsToRemove.Add(_actor);
        }

        private void UpdateAddActors()
        {
            foreach (var actorToAdd in actorsToAdd)
            {
                actorToAdd.Start();
                actors.Add(actorToAdd);
            }
            actorsToAdd.Clear();
        }
        private void UpdateRemoveActors()
        {
            foreach (var actorToRemove in actorsToRemove)
            {
                actorToRemove.OnRemove();
                actors.Remove(actorToRemove);
            }
            actorsToRemove.Clear();
        }
    }

    public class Actor
    {
        public Actor AddToWorld() => ActorGroup.World.Add(this);

        public virtual float X { get; set; }
        public virtual float Y { get; set; }

        public virtual bool Visible { get; set; }
        public virtual bool Active { get; set; }

        public Actor()
        {
            Visible = true;
            Active = true;
        }
        public Actor(float _x, float _y) : this()
        {
            X = _x;
            Y = _y;
        }

        internal Action DestroyHandler;
        public void Destroy() => DestroyHandler();

        public virtual void OnRemove() { }
        public virtual void Start() { }
        public virtual void PreUpdate() { }
        public virtual void Update() { }
        public virtual void PostUpdate() { }
        public virtual void Render() { }
    }

    public class ShellActor : Actor
    {
        public Action OnRemoveHandler;
        public Action StartHandler;
        public Action PreUpdateHandler;
        public Action UpdateHandler;
        public Action PostUpdateHandler;
        public Action RenderHandler;

        public ShellActor() : base() { }
        public ShellActor(float _x, float _y) : base(_x, _y) { }

        public override void OnRemove() => OnRemoveHandler?.Invoke();
        public override void Start() => StartHandler?.Invoke();
        public override void PreUpdate() => PreUpdateHandler?.Invoke();
        public override void Update() => UpdateHandler?.Invoke();
        public override void PostUpdate() => PostUpdateHandler?.Invoke();
        public override void Render() => RenderHandler?.Invoke();
    }
}
