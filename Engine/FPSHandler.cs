using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Engine
{
    //TODO rename to FPSTracker
    internal class FPSHandler
    {
        public static FPSHandler Singleton { get; private set; }

        private long millisecondsSinceStartPrevious;
        private Stopwatch stopwatch;
        private List<float> FPSSamples = new List<float>();
        public float timeSinceStart => stopwatch.ElapsedMilliseconds;
        public float fps => FPSSamples.Average();
        public float deltaMillis { get; private set; }

        public static int FramesSinceStart;
        public static float MillisecondsSinceStart => Singleton.stopwatch.ElapsedMilliseconds;
        public static float FPS => Singleton.FPSSamples?.Count > 0 ? Singleton.FPSSamples.Average() : 0;
        public static float Delta => Singleton.deltaMillis / 1000f;

        internal FPSHandler()
        {
            Singleton = this;
            millisecondsSinceStartPrevious = 0;
            stopwatch = new Stopwatch();
            stopwatch.Start();
        }

        public void Update()
        {
            var time = stopwatch.ElapsedMilliseconds;
            deltaMillis = time - millisecondsSinceStartPrevious;
            millisecondsSinceStartPrevious = time;
            FPSSamples.Add(1000f / deltaMillis);
            while (FPSSamples.Count > 10)
                FPSSamples.RemoveAt(0);

            FramesSinceStart++;
        }
    }
}
