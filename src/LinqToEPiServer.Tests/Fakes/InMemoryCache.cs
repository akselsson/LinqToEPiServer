using System;
using System.Collections;
using System.Web.Caching;
using EPiServer.BaseLibrary;

namespace LinqToEPiServer.Tests.Fakes
{
    public class InMemoryCache : IRuntimeCache
    {
        private readonly Hashtable _objects = new Hashtable();

        public void Insert(string key, object value, CacheDependency dependencies, DateTime absoluteExpiration, TimeSpan slidingExpiration, CacheItemPriority priority)
        {
            Insert(key,value,dependencies,absoluteExpiration,slidingExpiration,priority, delegate { });
        }

        public void Insert(string key, object value, CacheDependency dependencies, DateTime absoluteExpiration, TimeSpan slidingExpiration, CacheItemPriority priority, CacheItemRemovedCallback onRemoveCallback)
        {
            _objects[key] = value;
        }

        public object Add(string key, object value, CacheDependency dependencies, DateTime absoluteExpiration, TimeSpan slidingExpiration, CacheItemPriority priority)
        {
            return Add(key,value,dependencies,absoluteExpiration,slidingExpiration,priority,delegate { });
        }

        public object Add(string key, object value, CacheDependency dependencies, DateTime absoluteExpiration, TimeSpan slidingExpiration, CacheItemPriority priority, CacheItemRemovedCallback onRemoveCallback)
        {
            Insert(key, value, dependencies, absoluteExpiration, slidingExpiration, priority, delegate { });
            return value;
        }

        public void Remove(string key)
        {
            _objects.Remove(key);
        }

        public object Get(string key)
        {
            return _objects[key];
        }

        public void Clear()
        {
            _objects.Clear();
        }
    }
}