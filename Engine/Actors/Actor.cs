using System;
using Basics;
using Basics.QuadTree;
using OpenTK;
using Rectangle = Basics.Rectangle;

namespace Engine.Actors
{
    public static class QuadTreeExtensions
    {
        public static void Insert<T>(this QuadTree<T> _tree, T _actor) where T : Actor
            => _tree.Insert(_actor, _actor.CollisionBox);
    }

    public class Actor : IPosition
    {
        public Actor AddToGroup() => Game.Group.AddToGroup(this);
        
        public virtual float X { get; set; }
        public virtual float Y { get; set; }

        /// <summary>
        /// The actor with the lowest depth will update and render last
        /// while the actor with the highest depth will update and render first.
        /// Ties are decided by the actors' IDs (lowest will update/render first).
        /// </summary>
        public float Depth
        {
            get => depth;
            set
            {
                if (changeDepth != null)
                    changeDepth(this, value);
                else
                    depth = value;
            }
        }

        /// <summary>
        /// ActorGroups utilize this with their depth sorter to tell when there is a change it needs to act on.
        /// </summary>
        internal Action<Actor, float> changeDepth;
        internal float depth = 0;

        //Coordinates relative to the Actor's position
        //(e.g. 40x60 box centered around the player = new BoundingBox(-20, -30, 40, 60))
        public BoundingBox BoundingBox;
        public Rectangle CollisionBox => new Rectangle(X + BoundingBox.X, Y + BoundingBox.Y, BoundingBox.W, BoundingBox.H);
        public bool Collides(Actor _actor) => _actor == null ? false : CollisionBox.Collides(_actor.CollisionBox);

        private bool destroyed;
        public bool Destroyed => destroyed;

        public virtual Vector2 Position
        {
            get => new Vector2(X, Y);
            set { X = value.X; Y = value.Y; }
        }

        public virtual bool Visible { get; set; } = true;
        public virtual bool Active { get; set; } = true;

        public readonly int ID;
        public readonly int TypeID;
        
        public Actor(float _x = 0, float _y = 0, float _w = 0, float _h = 0, float _depth = 0,
            Align.Horizontal _halign = Align.Horizontal.Center, 
            Align.Vertical _valign = Align.Vertical.Middle, 
            ActorGroup _group=null)
        {
            _group = _group ?? Game.Group;
            ID = _group.GetNextID();
            TypeID = _group.GetTypeID(this);

            X = _x;
            Y = _y;
            Depth = _depth;

            BoundingBox = new BoundingBox(_w, _h, _halign, _valign);
        }
        
        public void ScreenWrap(float _margin=0) => Position = Game.ScreenWrap(Position, _margin);
        public void ScreenClamp(float _margin=0) => Position = Game.ScreenClamp(Position, _margin);

        internal Action DestroyHandler;

        /// <summary>
        /// Adds an action to the list of functions which should be called on the frame the actor's Destroy() function is called
        /// </summary>
        /// <param name="_action">new action to trigger when the actor is destroyed</param>
        public void AddDestroyTrigger(Action _action)
        {
            DestroyHandler += _action;
        }

        public void Destroy()
        {
            if (Destroyed)
                return;
            destroyed = true;
            DestroyHandler();
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
        
        public ShellActor(float _x=0, float _y=0) : base(_x, _y) { }

        public override void OnRemove() => OnRemoveHandler?.Invoke();
        public override void Start() => StartHandler?.Invoke();
        public override void PreUpdate() => PreUpdateHandler?.Invoke();
        public override void Update() => UpdateHandler?.Invoke();
        public override void PostUpdate() => PostUpdateHandler?.Invoke();
        public override void Render() => RenderHandler?.Invoke();
    }
}
