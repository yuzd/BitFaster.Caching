﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitFaster.Caching
{
    /// <summary>
    /// Represents a time based cache policy.
    /// </summary>
    public interface ITimePolicy
    {
        /// <summary>
        /// Gets the time to live for items in the cache.
        /// </summary>
        TimeSpan TimeToLive { get; }

        /// <summary>
        /// Remove all expired items from the cache.
        /// </summary>
        void TrimExpired();
    }
}
