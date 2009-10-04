using System;
using EPiServer;
using LinqToEPiServer.Implementation.Expressions;

namespace LinqToEPiServer.Implementation.Visitors
{
    public class CriteriaFactory
    {
        public PropertyCriteria GetCriteria(PropertyComparison comparison)
        {
            return new PropertyCriteria
                       {
                           Condition = comparison.CompareCondition,
                           IsNull = comparison.ComparisonValue == null,
                           Name = comparison.PropertyName,
                           Required = true,
                           Type = comparison.GetPropertyDataType(),
                           Value = ConvertToString(comparison.ComparisonValue)
                       };
        }

        private static string ConvertToString(object value)
        {
            if (value == null)
                return null;
            return Convert.ToString(value);
        }
    }
}