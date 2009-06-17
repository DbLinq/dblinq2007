using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace DbLinq.Util
{
#if !MONO_STRICT
    public
#endif
    static class Profiler
    {
        [ThreadStatic]
        private static Stopwatch timer = new Stopwatch();
        [ThreadStatic]
        private static long prevTicks;
        [ThreadStatic]
        private static bool profiling;

        [Conditional("DEBUG")]
        public static void Start()
        {
            profiling = true;
            prevTicks = 0;
            timer.Reset();
            timer.Start();
        }

        [Conditional("DEBUG")]
        public static void At(string format, params object[] args)
        {
            if (profiling)
            {
                timer.Stop();
                Console.Write("#AT(time={0:D10}, elapsed={1:D10}) ", timer.ElapsedTicks, timer.ElapsedTicks - prevTicks);
                prevTicks = timer.ElapsedTicks;
                Console.WriteLine(format, args);
                timer.Start();
            }
        }

        [Conditional("DEBUG")]
        public static void Stop()
        {
            profiling = false;
            timer.Stop();
        }
    }
}