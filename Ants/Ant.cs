using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Input;
using Basics;
using Engine;
using Ants.AntControllers;

namespace Ants
{
    public enum AntType
    {
        Digger,
        Forager,
        Breeder,
        Queen
    }

    public class Ant : PolygonActor
    {
        //The types of resources that each ant type can gather
        public static readonly Dictionary<AntType, List<ResourceType>> Gatherables = new Dictionary<AntType, List<ResourceType>>
        {
            [AntType.Digger] = new List<ResourceType>
            {
                ResourceType.Dirt
            },
            [AntType.Forager] = new List<ResourceType>
            {
                ResourceType.Food
            },
            [AntType.Breeder] = new List<ResourceType>(),
            [AntType.Queen] = new List<ResourceType>()
        };

        private IAntController controller = null;
        private ResourceAccount account = null;
        public ResourceType? CarryResource => account?.Type;

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
        private float speed;
        public float Speed
        {
            get => speed;
            set
            {
                if (!SpeedMin.HasValue)
                {
                    if (!SpeedMax.HasValue)
                        speed = value;
                    else
                        speed = Basics.Utils.Min(value, SpeedMax.Value);
                }
                else if (!SpeedMax.HasValue)
                    speed = Basics.Utils.Max(value, SpeedMin.Value);
                else
                    speed = Basics.Utils.Clamp(value, SpeedMin.Value, SpeedMax.Value);
            }
        }
        public float? SpeedMin = null;
        public float? SpeedMax = null;

        private AntType type;
        public AntType Type => type;
        public WorldType Location = WorldType.Underworld;
        public AntWorld World;
        public AntGrid Grid => World.WorldFromType(Location);
        
        private static AntType RandomAntType() => (AntType)Basics.Utils.RandomInt(Enum.GetNames(typeof(AntType)).Length);

        public Ant(AntWorld _world, WorldType _location)
            : this(_world, _location, Game.RandomPosition())
        { }
        public Ant(AntWorld _world, WorldType _location, Vector2 _position)
            : this(RandomAntType(), _world, _location, _position.X, _position.Y, 4)
        { }
        public Ant(AntType _type, AntWorld _world, WorldType _location, float _x, float _y, float _radius) 
            : base(new ConvexPolygon(new List<Vector2> { new Vector2(-_radius, _radius), new Vector2(-_radius, -_radius), new Vector2(2 * _radius, 0) }), _x, _y)
        {
            type = _type;
            World = _world;
            World.AddAnt(this);
            Location = _location;
            anglePrevious = 0;
            angle = 0;
            Speed = 0;
            SpeedMin = 0;
            SpeedMax = 60;
        }

        public override void OnRemove() => World.RemoveAnt(this);

        public void AttachController(IAntController _controller)
        {
            controller = _controller;
            controller.Start(this);
        }

        public bool TakeResource(Resource _resource)
        {
            if (account == null)
                account = new ResourceAccount(_resource.Account.Type);
            else if (_resource.Account.Type != account.Type)
                return false;

            _resource.Account.TransferAll(account);
            return true;
        }

        public float DepositResource() => account?.Withdraw() ?? 0;

        public override void Update()
        {
            UpdateAccount();
            UpdateTile();
            controller?.Update();
            UpdatePhysics();
            Position = Game.ScreenClamp(Position);
        }

        private void UpdateAccount()
        {
            if (account?.Amount <= 0)
                account = null;
        }

        private Tile tile;
        public Tile Tile => Grid.Get(X, Y);

        private void UpdateTile()
        {
            tile = Grid.Get(X, Y);
            if (tile == null)
                return;

            if (account == null)
                tile.AddSignal(SignalType.Home, 10, (float)(Angle + Math.PI));
        }

        private void UpdatePhysics()
        {
            UpdateAngle();
            UpdateSpeed();
            UpdatePosition();
        }

        //Returns the sign of the difference between the controller's target angle and the given angle
        private int DirectionToTargetAngle(float _angle) => Math.Sign(Basics.Utils.AngleDifferenceRadians(_angle, controller.TargetAngle));
        private int DirectionToTargetAngle() => DirectionToTargetAngle(Angle);

        private void UpdateAngle()
        {
            var directionToTargetAnglePre = DirectionToTargetAngle();
            var angularAcceleration = 5 * directionToTargetAnglePre;
            var angleDelta = angularAcceleration * Game.Delta;
            var directionToTargetAnglePost = DirectionToTargetAngle(Angle + angleDelta);
            var movedBeyondTargetAngle = directionToTargetAnglePre != directionToTargetAnglePost;
            if (movedBeyondTargetAngle)
                Angle = controller.TargetAngle;
            else
                Angle += angleDelta;
        }

        private void UpdateSpeed()
        {
            Speed += (controller.Stop ? -1 : 1) * 100 * Game.Delta;
        }

        private void UpdatePosition()
        {
            Position += Speed * Engine.Utils.UnitVector2(Angle) * Game.Delta;
        }
    }
}
