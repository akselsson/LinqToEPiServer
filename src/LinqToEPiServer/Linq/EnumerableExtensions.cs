using System.Collections.Generic;
using EPiServer;
using EPiServer.Core;

namespace LinqToEPiServer.Linq
{
    public static class EnumerableExtensions
    {
        public static PageDataCollection ToPageDataCollection<T>(this IEnumerable<T> enumerable) where T : PageData
        {
            return new PageDataCollection(enumerable);
        }

        public static PropertyCriteriaCollection ToCriteriaCollection(this IEnumerable<PropertyCriteria> criterion)
        {
            var collection = new PropertyCriteriaCollection();
            foreach (var criteria in criterion)
            {
                collection.Add(criteria);
            }
            return collection;
        }
    }
}