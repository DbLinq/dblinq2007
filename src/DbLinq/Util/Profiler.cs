using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace DbLinq.Util
{
    public static class Profiler
    {
        private static Stopwatch timer = new Stopwatch();
        private static long prevTicks;

        [Conditional("DEBUG")]
        public static void Start()
        {
            timer.Reset();
            timer.Start();
            prevTicks = 0;
        }

        [Conditional("DEBUG")]
        public static void At(string format, params object[] args)
        {
            timer.Stop();
            Console.Write("#AT(time={0:D10}, elapsed={1:D10}) ", timer.ElapsedTicks, timer.ElapsedTicks - prevTicks);
            prevTicks = timer.ElapsedTicks;
            Console.WriteLine(format, args);
            timer.Start();
        }

        [Conditional("DEBUG")]
        public static void Stop()
        {
            timer.Stop();
        }
    }
}