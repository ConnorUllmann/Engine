using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Input;
using Basics;
using Engine;

namespace Ants.AntControllers
{
    public interface IAntController
    {
        void Start(Ant _ant);
        void Update();
        float TargetAngle { get; }
        bool Stop { get; }
    }

    public class AIAntController : IAntController
    {
        private Ant ant;

        public void Start(Ant _ant) => ant = _ant;

        public void Update()
        {
            //var tile = ant.Grid.Get(ant.X, ant.Y);
            TargetAngle = Input.AngleToMouse(ant.X, ant.Y);
            Stop = false;
        }

        public float TargetAngle { get; set; }
        public bool Stop { get; set; }
    }

    public class KeyboardAntController : IAntController
    {
        public const Key LeftKey = Key.Left;
        public const Key RightKey = Key.Right;
        public const Key UpKey = Key.Up;
        public const Key DownKey = Key.Down;

        private Ant ant;

        public void Start(Ant _ant) => ant = _ant;

        public void Update() { }

        public float TargetAngle => ant.Angle + ((Input.KeyDown(LeftKey) ? 1 : 0) - (Input.KeyDown(RightKey) ? 1 : 0));
        public bool Stop => (Input.KeyDown(UpKey) ? 1 : 0) - (Input.KeyDown(DownKey) ? 1 : 0) <= 0;
    }
}
