using System;
using System.Collections.Generic;
using System.Text;
using Basics;

namespace Engine
{
    public class ActorGroup
    {
        private int nextID = 0;
        public int GetNextID() => nextID++;

        public static ActorGroup World = new ActorGroup();

        private Dictionary<int, Actor> actors;
        public Dictionary<int, Actor> Actors => actors;
        private HashSet<Actor> actorsToAdd;
        private HashSet<Actor> actorsToRemove;

        public ActorGroup()
        {
            Reset();
        }

        public void Reset()
        {
            actors = new Dictionary<int, Actor>();
            actorsToAdd = new HashSet<Actor>();
            actorsToRemove = new HashSet<Actor>();
        }

        public void Update()
        {
            UpdateRemoveActors();
            UpdateAddActors();
            foreach(var actor in actors.Values)
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
            foreach(var actor in actors.Values)
                if (actor.Visible)
                    actor.Render();
        }

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
                actors[actorToAdd.ID] = actorToAdd;
            }
            actorsToAdd.Clear();
        }
        private void UpdateRemoveActors()
        {
            foreach (var actorToRemove in actorsToRemove)
            {
                actorToRemove.OnRemove();
                actors.Remove(actorToRemove.ID);
            }
            actorsToRemove.Clear();
        }
    }

    public class Actor
    {
        public Actor AddToWorld() => ActorGroup.World.Add(this);

        public virtual float X { get; set; }
        public virtual float Y { get; set; }

        private bool destroyed;
        public bool Destroyed => destroyed;

        public virtual OpenTK.Vector2 Position
        {
            get => new OpenTK.Vector2(X, Y);
            set { X = value.X; Y = value.Y; }
        }

        public virtual bool Visible { get; set; } = true;
        public virtual bool Active { get; set; } = true;
        public readonly int ID = ActorGroup.World.GetNextID();
        
        public Actor(float _x=0, float _y=0)
        {
            X = _x;
            Y = _y;
        }

        public void ScreenWrap(float _margin=0) => Position = Game.ScreenWrap(Position, _margin);
        public void ScreenClamp(float _margin=0) => Position = Game.ScreenClamp(Position, _margin);

        internal Action DestroyHandler;
        public void Destroy()
        {
            if (Destroyed)
                return;
            DestroyHandler();
            destroyed = true;
        }

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
        
        public ShellActor(float _x, float _y) : base(_x, _y) { }

        public override void OnRemove() => OnRemoveHandler?.Invoke();
        public override void Start() => StartHandler?.Invoke();
        public override void PreUpdate() => PreUpdateHandler?.Invoke();
        public override void Update() => UpdateHandler?.Invoke();
        public override void PostUpdate() => PostUpdateHandler?.Invoke();
        public override void Render() => RenderHandler?.Invoke();
    }
}
