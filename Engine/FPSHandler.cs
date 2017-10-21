using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics;
using System.Threading;

namespace Engine
{
    internal class FPSHandler
    {
        public static FPSHandler Singleton { get; private set; }

        private long millisecondsSinceStartPrevious;
        private Stopwatch stopwatch;
        private List<float> FPSSamples = new List<float>();
        public float timeSinceStart => stopwatch.ElapsedMilliseconds;
        public float fps => FPSSamples.Average();
        public float delta { get; private set; }

        public static int FramesSinceStart;
        public static float MillisecondsSinceStart => Singleton.stopwatch.ElapsedMilliseconds;
        public static float FPS => Singleton.FPSSamples.Average();
        public static float Delta => Singleton.delta;

        private Window window;

        internal FPSHandler(Window _window)
        {
            Singleton = this;
            window = _window;
            millisecondsSinceStartPrevious = 0;
            stopwatch = new Stopwatch();
            stopwatch.Start();
        }

        public void Update()
        {
            var time = stopwatch.ElapsedMilliseconds;
            delta = time - millisecondsSinceStartPrevious;
            millisecondsSinceStartPrevious = time;
            FPSSamples.Add(1000f / delta);
            while (FPSSamples.Count > 10)
                FPSSamples.RemoveAt(0);

            FramesSinceStart++;
        }
    }
}
