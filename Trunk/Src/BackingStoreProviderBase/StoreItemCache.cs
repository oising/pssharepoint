using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

// web cache is ok for .net 2.0:
// see http://support.microsoft.com/kb/917411
using System.Web.Caching;

namespace Nivot.PowerShell
{
    class StoreItemCache
    {        
        private static TimeSpan s_slidingExpiration;
        private readonly object m_syncRoot;
        private Cache m_cache;

        public static readonly StoreItemCache Instance;

        private StoreItemCache()
        {
            m_syncRoot = new Object();
            m_cache = new Cache();
        }

        static StoreItemCache()
        {
            s_slidingExpiration = new TimeSpan(0, 5, 0); // default: 5 mins after last access
            Instance = new StoreItemCache();
        }

        internal static TimeSpan SlidingExpiration
        {
            get
            {
                return s_slidingExpiration;
            }
            set
            {
                s_slidingExpiration = value;
            }
        }

        internal IStoreItem this[string key]
        {
            get
            {
                return m_cache[key] as IStoreItem;
            }
        }

        internal void Add(string key, IStoreItem value)
        {
            Add(key, value, CacheItemPriority.Normal);
        }

        internal void Add(string key, IStoreItem value, CacheItemPriority priority)
        {
            object returned;

            lock (m_syncRoot)
            {
                returned = m_cache.Add(key, value, null, Cache.NoAbsoluteExpiration, StoreItemCache.SlidingExpiration,
                    priority, new CacheItemRemovedCallback(StoreItemRemoved));
            }
            
            // should be null for new inserts.
            Debug.Assert(returned == null, "returned == null");
        }

        internal static void StoreItemRemoved(string key, object value, CacheItemRemovedReason reason)
        {
            if (value is IDisposable)
            {
                ((IDisposable) value).Dispose();
            }
        }
    }
}
