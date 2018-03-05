using System;
using System.Collections.Generic;
using System.Text;

namespace Engine
{
    public class Timer
    {
        private Action Action;
        private float Period;
        private float Current;
        private bool Repeat;

        public Timer(Action _action, float _period, bool _repeat=true)
        {
            Action = _action;
            Period = _period;
            Current = 0;
            Repeat = _repeat;
        }

        public float Percent => Current / Period;

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
                Action();
            }
        }
    }
}
