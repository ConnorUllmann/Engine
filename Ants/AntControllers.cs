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

    public class AIAntController : StateMachine, IAntController
    {
        private enum AntStates
        {
            Return,
            Grow,
            Gather,
            Attack
        }

        private class ReturnState : IState
        {
            public AIAntController Controller;
            public ReturnState(AIAntController _controller) => Controller = _controller;

            public void Start()
            {
                Controller.Stop = false;
            }

            public void Update()
            {
                var tile = Controller.ant.Grid.Get(Controller.ant.X, Controller.ant.Y);
                var signalAngle = tile?.GetSignalAngleForResourceType(SignalType.Home);
                if (signalAngle.HasValue)
                    Controller.TargetAngle = signalAngle.Value;
            }

            public void Finish() { }
        }

        private class GrowState : IState
        {
            public AIAntController Controller;
            public GrowState(AIAntController _controller) => Controller = _controller;

            private const float GrowthTime = 60;
            private float growthTimeRemaining = GrowthTime;

            public void Start()
            {
                Controller.TargetAngle = (float)Basics.Utils.RandomAngleRad();
            }

            public void Update()
            {
                Controller.Stop = true;
                growthTimeRemaining -= Game.Delta;
                if (growthTimeRemaining <= 0)
                    Controller.ChangeState(AntStates.Return);
            }

            public void Finish() { }
        }

        private class GatherState : IState
        {
            public AIAntController Controller;
            public GatherState(AIAntController _controller) => Controller = _controller;

            private ResourceType targetResource;
            private SignalType targetSignal => Signal.ForResource(targetResource);
            
            public void Start()
            {
                Controller.Stop = false;
            }

            public void Update()
            {
                if (Controller.ant.CarryResource.HasValue)
                {
                    Controller.ChangeState(AntStates.Return);
                    return;
                }

                /********************************************************************************************************/
                //TODO: Get the nearest resource that matches our targetResource within visible range and pathfind to it//
                /********************************************************************************************************/

                var tile = Controller.ant.Tile;
                foreach (var resourceType in Ant.Gatherables[Controller.ant.Type].Shuffled())
                {
                    if (tile.HasSignal(Signal.ForResource(resourceType)))
                    {
                        targetResource = resourceType;
                        break;
                    }
                }

                var targetAngle = tile.GetSignalAngleForResourceType(targetSignal);
                if(targetAngle.HasValue)
                    Controller.TargetAngle = targetAngle.Value;
            }

            public void Finish() { }
        }

        private class AttackState : IState
        {
            public AIAntController Controller;
            public AttackState(AIAntController _controller) => Controller = _controller;

            public void Start() { }
            public void Update() { }
            public void Finish() { }
        }

        private Ant ant;
        private Dictionary<AntStates, IState> states;

        public void Start(Ant _ant)
        {
            ant = _ant;
            states = new Dictionary<AntStates, IState>
            {
                [AntStates.Return] = new ReturnState(this),
                [AntStates.Grow] = new GrowState(this),
                [AntStates.Gather] = new GatherState(this),
                [AntStates.Attack] = new AttackState(this)
            };
            ChangeState(AntStates.Grow);
        }

        private void ChangeState(AntStates _newState) => ChangeState(states[_newState]);

        public float TargetAngle { get; set; }
        public bool Stop { get; set; }
    }

    public class KeyboardAntController : IAntController
    {
        private const Key LeftKey = Key.Left;
        private const Key RightKey = Key.Right;
        private const Key UpKey = Key.Up;
        private const Key DownKey = Key.Down;

        private Ant ant;

        public void Start(Ant _ant) => ant = _ant;

        public void Update()
        {
            TargetAngle = ant.Angle + ((Input.KeyDown(LeftKey) ? 1 : 0) - (Input.KeyDown(RightKey) ? 1 : 0));
            Stop = (Input.KeyDown(UpKey) ? 1 : 0) - (Input.KeyDown(DownKey) ? 1 : 0) <= 0;
        }

        public float TargetAngle { get; set; }
        public bool Stop { get; set; }
    }
}
