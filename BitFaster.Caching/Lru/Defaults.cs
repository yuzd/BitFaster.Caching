﻿using System;
using System.Collections.Generic;
using System.Text;

namespace BitFaster.Caching.Lru
{
    internal static class Defaults
    {
        public static int ConcurrencyLevel
        {
            get { return Environment.ProcessorCount; }
        }

        public static readonly TimeSpan Infinite = new TimeSpan(0, 0, 0, 0, -1);
    }
}
