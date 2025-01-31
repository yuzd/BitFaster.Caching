﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace BitFaster.Caching
{
    /// <summary>
    /// A cache decorator for working with Scoped IDisposable values. The Scoped methods (e.g. ScopedGetOrAdd)
    /// are threadsafe and create lifetimes that guarantee the value will not be disposed until the
    /// lifetime is disposed.
    /// </summary>
    /// <typeparam name="K">The type of keys in the cache.</typeparam>
    /// <typeparam name="V">The type of values in the cache.</typeparam>
    [DebuggerTypeProxy(typeof(ScopedCacheDebugView<,>))]
    [DebuggerDisplay("Count = {Count}")]
    public sealed class ScopedCache<K, V> : IScopedCache<K, V> where V : IDisposable
    {
        private readonly ICache<K, Scoped<V>> cache;

        /// <summary>
        /// Initializes a new instance of the ScopedCache class with the specified inner cache.
        /// </summary>
        /// <param name="cache">The decorated cache.</param>
        public ScopedCache(ICache<K, Scoped<V>> cache)
        {
            if (cache == null)
            {
                Ex.ThrowArgNull(ExceptionArgument.cache);
            }

            this.cache = cache;
        }

        ///<inheritdoc/>
        public int Count => this.cache.Count;

        ///<inheritdoc/>
        public Optional<ICacheMetrics> Metrics => this.cache.Metrics;

        ///<inheritdoc/>
        public Optional<ICacheEvents<K, Scoped<V>>> Events => this.cache.Events;

        ///<inheritdoc/>
        public CachePolicy Policy => this.cache.Policy;

        ///<inheritdoc/>
        public ICollection<K> Keys => this.cache.Keys;

        ///<inheritdoc/>
        public void AddOrUpdate(K key, V value)
        {
            this.cache.AddOrUpdate(key, new Scoped<V>(value));
        }

        ///<inheritdoc/>
        public void Clear()
        {
            this.cache.Clear();
        }

        ///<inheritdoc/>
        public Lifetime<V> ScopedGetOrAdd(K key, Func<K, Scoped<V>> valueFactory)
        {
            int c = 0;
            var spinwait = new SpinWait();
            while (true)
            {
                var scope = cache.GetOrAdd(key, k => valueFactory(k));

                if (scope.TryCreateLifetime(out var lifetime))
                {
                    return lifetime;
                }

                spinwait.SpinOnce();

                if (c++ > ScopedCacheDefaults.MaxRetry)
                {
                    Ex.ThrowScopedRetryFailure();
                }
            }
        }

        ///<inheritdoc/>
        public bool ScopedTryGet(K key, out Lifetime<V> lifetime)
        {
            if (this.cache.TryGet(key, out var scope))
            {
                if (scope.TryCreateLifetime(out lifetime))
                {
                    return true;
                }
            }

            lifetime = default;
            return false;
        }

        ///<inheritdoc/>
        public bool TryRemove(K key)
        {
            return this.cache.TryRemove(key);
        }

        ///<inheritdoc/>
        public bool TryUpdate(K key, V value)
        {
            return this.cache.TryUpdate(key, new Scoped<V>(value));
        }

        ///<inheritdoc/>
        public IEnumerator<KeyValuePair<K, Scoped<V>>> GetEnumerator()
        {
            foreach (var kvp in this.cache)
            {
                yield return kvp;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((ScopedCache<K, V>)this).GetEnumerator();
        }
    }
}
