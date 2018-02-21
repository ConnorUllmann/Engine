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
    public class Ant : PolygonActor
    {
        private IAntController controller = null;

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

        public AntGrid Grid;

        public Ant(AntGrid grid, float _radius) : base(new ConvexPolygon(new List<Vector2> { new Vector2(-_radius, _radius), new Vector2(-_radius, -_radius), new Vector2(2 * _radius, 0) }))
        {
            Grid = grid;
            anglePrevious = 0;
            angle = 0;
            Speed = 0;
            SpeedMin = 0;
            SpeedMax = 60;
        }

        public void AttachController(IAntController _controller)
        {
            controller = _controller;
            controller.Start(this);
        }

        public override void Update()
        {
            controller?.Update();
            UpdatePhysics();
            Position = Game.ScreenClamp(Position);
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
            var angularAcceleration = 6 * directionToTargetAnglePre;
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
