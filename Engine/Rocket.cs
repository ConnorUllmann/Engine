using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Graphics;

namespace Engine.Actors
{
    public class Rocket
    {
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
                rotate?.Invoke(angle - anglePrevious);
            }
        }
        private float speed;
        public float Speed
        {
            get => speed;
            set => speed = Basics.Utils.Clamp(value, SpeedMin, SpeedMax);
        }
        public float SpeedMin;
        public float SpeedMax;

        private Action<float> rotate;

        public Rocket(Action<float> _rotate=null, float _speedMin=0, float _speedMax=float.MaxValue)
        {
            rotate = _rotate;
            SpeedMin = _speedMin;
            SpeedMax = _speedMax;
            Speed = 0; // To make sure speed ends up within the bounds
        }
        
        public Vector2 DeltaPosition() => Speed * Utils.Vector2(Angle) * Game.Delta;
    }
}
