using System;
using System.Collections.Generic;
using System.Text;

namespace Engine
{
    public class Timer
    {
        private float Period;
        private Action Action;
        private bool Repeat;
        private float Current;

        public Timer(float _period, Action _action = null, bool _repeat = true)
        {
            Period = _period;
            Action = _action;
            Repeat = _repeat;
            Restart();
        }

        public void Restart() => Current = 0;

        public float NormalizedPercent => Current / Period;

        public void Update()
        {
            if (!Repeat && Current >= Period)
                return;

            Current += Game.Delta;
            if(Current >= Period)
            {
                if (Repeat)
                    Current = Basics.Utils.Clamp(Current - Period, 0, Period);
                else
                    Current = Period;
                Action?.Invoke();
            }
        }
    }
}
