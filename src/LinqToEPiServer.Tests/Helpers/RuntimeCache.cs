using System;
using System.Web.Caching;
using EPiServer.BaseLibrary;

namespace LinqToEPiServer.Tests.Helpers
{
    public class RuntimeCache : IRuntimeCache
    {
        public static IRuntimeCache Inner { get; set; }

        public void Insert(string key, object value, CacheDependency dependencies, DateTime absoluteExpiration, TimeSpan slidingExpiration, CacheItemPriority priority)
        {
            Inner.Insert(key, value, dependencies, absoluteExpiration, slidingExpiration, priority);
        }

        public void Insert(string key, object value, CacheDependency dependencies, DateTime absoluteExpiration, TimeSpan slidingExpiration, CacheItemPriority priority, CacheItemRemovedCallback onRemoveCallback)
        {
            Inner.Insert(key, value, dependencies, absoluteExpiration, slidingExpiration, priority, onRemoveCallback);
        }

        public object Add(string key, object value, CacheDependency dependencies, DateTime absoluteExpiration, TimeSpan slidingExpiration, CacheItemPriority priority)
        {
            return Inner.Add(key, value, dependencies, absoluteExpiration, slidingExpiration, priority);
        }

        public object Add(string key, object value, CacheDependency dependencies, DateTime absoluteExpiration, TimeSpan slidingExpiration, CacheItemPriority priority, CacheItemRemovedCallback onRemoveCallback)
        {
            return Inner.Add(key, value, dependencies, absoluteExpiration, slidingExpiration, priority, onRemoveCallback);
        }

        public void Remove(string key)
        {
            Inner.Remove(key);
        }

        public object Get(string key)
        {
            return Inner.Get(key);
        }

        public void Clear()
        {
            Inner.Clear();
        }
    }
}