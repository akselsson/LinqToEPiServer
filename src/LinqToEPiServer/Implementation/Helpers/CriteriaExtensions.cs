using System.Collections.Generic;
using System.Linq;
using EPiServer;

namespace LinqToEPiServer.Implementation.Helpers
{
    public static class CriteriaExtensions
    {
        public static EquatableCriteria[] EquatableWithFormatting(this IEnumerable<PropertyCriteria> criteria)
        {
            return criteria.Select(c =>EquatableWithFormatting((PropertyCriteria) c)).ToArray();
        }

        public static EquatableCriteria EquatableWithFormatting(this PropertyCriteria criteria)
        {
            return new EquatableCriteria
                       {
                           Condition = criteria.Condition,
                           IsNull = criteria.IsNull,
                           Name = criteria.Name,
                           Required = criteria.Required,
                           Type = criteria.Type,
                           Value = criteria.Value
                       };
        }
    }
}