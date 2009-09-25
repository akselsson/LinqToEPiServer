using System;
using LinqToEPiServer.Implementation.Expressions;

namespace LinqToEPiServer.Implementation.Visitors.PropertyDataTypeMappers
{
    public class ComparisonValuePropertyDataTypeMapper : TypeBasedPropertyDataTypeMapperBase
    {
        protected override Type GetType(PropertyComparison propertyComparison)
        {
            return propertyComparison.ComparisonValue != null ? propertyComparison.ComparisonValue.GetType() : null;
        }
    }
}