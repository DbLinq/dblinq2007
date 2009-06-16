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

        [Conditional("DEBUG")]
        public static void Start()
        {
            timer.Reset();
            timer.Start();
        }

        [Conditional("DEBUG")]
        public static void At(string format, params object[] args)
        {
            timer.Stop();
            Console.Write("#AT({0:D16}) ", timer.ElapsedTicks);
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