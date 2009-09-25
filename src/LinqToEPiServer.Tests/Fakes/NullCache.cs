using System;
using System.Web.Caching;
using EPiServer.BaseLibrary;

namespace LinqToEPiServer.Tests.Fakes
{
    public class NullCache : IRuntimeCache
    {
        #region IRuntimeCache Members

        public void Insert(string key, object value, CacheDependency dependencies, DateTime absoluteExpiration,
                           TimeSpan slidingExpiration, CacheItemPriority priority)
        {
        }

        public void Insert(string key, object value, CacheDependency dependencies, DateTime absoluteExpiration,
                           TimeSpan slidingExpiration, CacheItemPriority priority,
                           CacheItemRemovedCallback onRemoveCallback)
        {
        }

        public object Add(string key, object value, CacheDependency dependencies, DateTime absoluteExpiration,
                          TimeSpan slidingExpiration, CacheItemPriority priority)
        {
            return value;
        }

        public object Add(string key, object value, CacheDependency dependencies, DateTime absoluteExpiration,
                          TimeSpan slidingExpiration, CacheItemPriority priority,
                          CacheItemRemovedCallback onRemoveCallback)
        {
            return value;
        }

        public void Remove(string key)
        {
        }

        public object Get(string key)
        {
            return null;
        }

        public void Clear()
        {
        }

        #endregion
    }
}