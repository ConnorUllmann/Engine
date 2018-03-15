using System;
using System.Collections.Generic;
using System.Text;

namespace Engine
{
    public class Timer
    {
        private float period;
        private Action action;
        private bool repeat;
        private float current;

        /// <summary>
        /// Simple class to use in combination with an update loop to time events
        /// </summary>
        /// <param name="_period">seconds between calls to _action</param>
        /// <param name="_action">function to call when each _period of time has elapsed</param>
        /// <param name="_repeat">whether to continue counting after _action is called for the first time</param>
        public Timer(float _period, Action _action = null, bool _repeat = true)
        {
            period = _period;
            action = _action;
            repeat = _repeat;
            Restart();
        }

        public void Restart() => current = 0;

        public float NormalizedPercent => current / period;

        public void Update()
        {
            if (!repeat && current >= period)
                return;

            current += Game.Delta;
            if(current >= period)
            {
                if (repeat)
                    current = Basics.Utils.Clamp(current - period, 0, period);
                else
                    current = period;
                action?.Invoke();
            }
        }
    }
}
